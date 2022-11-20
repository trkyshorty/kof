using System;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using KOF.Common;
using KOF.Models;
using System.Drawing;
using System.IO;
using System.Text.Json;

namespace KOF.Core
{
    public class Processor : AddressFinder
    {
        #region "Variables"
        public App _App { get; set; }
        public Process _Process { get; private set; }
        public IntPtr _Handle { get; private set; }
        public int _ProcessId { get; private set; }
        public IntPtr _MailslotRecvPtr { get; private set; }
        public IntPtr _MailslotRecvFuncPtr { get; private set; }
        public IntPtr _MailslotRecvHookPtr { get; private set; }
        public IntPtr _MailslotSendPtr { get; private set; }
        public IntPtr _MailslotSendHookPtr { get; private set; }
        private EPhase _Phase { get; set; }
        private EAction _Action { get; set; }
        private int _LoginTime { get; set; }
        private int _DisconnectTime { get; set; }
        private int _EnterGameTime { get; set; }
        private AddressEnum.Platform _Platform { get; set; }
        private List<int> _PartyAllowedCollection { get; set; } = new List<int>();
        public List<LootInfo> _AutoLootCollection { get; set; } = new List<LootInfo>();
        public int _PartyRequestTime { get; set; }
        private Zone _Zone { get; set; }
        private Image _MiniMapImage { get; set; }
        public Dictionary<int, int> _GroupHealCooldown { get; set; } = new Dictionary<int, int>();
        private bool _MovingLoot { get; set; }
        public Account _AccountData { get; set; }
        private bool _RouteSaving { get; set; }
        private List<RouteData> _RouteSaveData { get; set; } = new List<RouteData>();
        private List<Control> ControlCollection { get; set; } = new List<Control>();
        private  List<Skill> SkillCollection { get; set; } = new List<Skill>();
        private List<SkillBar> SkillBarCollection { get; set; } = new List<SkillBar>();
        private List<Loot> LootCollection { get; set; } = new List<Loot>();
        private List<Sell> SellCollection { get; set; } = new List<Sell>();
        private  List<Target> TargetCollection { get; set; } = new List<Target>();

        public enum EPhase : short
        {
            None = 0,
            Disconnected = 1, //Deprecated
            Authentication = 2,
            Loggining = 3,
            Selecting = 4,
            Selected = 5,
            Warping = 6,
            Playing = 7,
        }
        public enum EAction : short
        {
            None = 0,
            Routing = 1,
        }
        #endregion

        #region "Processor"

        public void HandleProcess(Process Process, ManagementObject Management, Account Account)
        {
            _Process = Process;

            _Handle = Process.Handle;
            _ProcessId = Process.Id;

            string CommandLine = Process.StartInfo.Arguments;

            if (Management != null)
                CommandLine = Management["CommandLine"].ToString();

            var Args = ParseArguments(CommandLine);

            if (Process.MainWindowTitle == "ÆïÊ¿3.0" || Process.MainWindowTitle == "骑士3.0" || Process.MainWindowTitle == "ﾆ・ｿ3.0")
                _Platform = AddressEnum.Platform.CNKO;
            else
            {
                if (Args.Length >= 1 && (Args[0] == "MGAMEJP" || Args[1] == "MGAMEJP"))
                {
                    _Platform = AddressEnum.Platform.JPKO;

                    if (Process.MainWindowTitle == "Knight OnLine Client")
                        PatchMutant();
                }
                else
                    _Platform = AddressEnum.Platform.PVP;
            }

            if (Storage.AddressCollection.ContainsKey(_Platform) == false)
                Storage.AddressCollection.Add(_Platform, LoadAddressList(_Handle, _Platform));

            foreach (AddressStorage Address in GetAddressList())
                Debug.WriteLine(Address.Name + " : " + Address.Address);


            PatchSendMailslot();
        }

        public void LoadCollection(string name, string job)
        {
            ControlCollection = Database().GetControlList(name);
            SkillCollection = Database().GetSkillList(job);
            SkillBarCollection = Database().GetSkillBarList(name, GetPlatform().ToString());
            LootCollection = Database().GetLootList(name, GetPlatform().ToString());
            SellCollection = Database().GetSellList(name, GetPlatform().ToString());
            TargetCollection = Database().GetTargetList(name, GetPlatform().ToString());
        }

        public void ClearCollection()
        {

            ControlCollection.Clear();
            SkillCollection.Clear();
            SkillBarCollection.Clear();
            LootCollection.Clear();
            SellCollection.Clear();
            TargetCollection.Clear();
        }

        public AddressEnum.Platform GetPlatform()
        {
            return _Platform;
        }

        public bool HasExited()
        {
            if (_Process == null) return true;

            return _Process.HasExited;
        }

        public int GetAddress(string Name)
        {
            if (_Process == null) return 0;
            List<AddressStorage> AddressList;
            if (Storage.AddressCollection.TryGetValue(_Platform, out AddressList))
                return int.Parse(AddressList.Where(x => x.Name == Name)?.SingleOrDefault().Address, System.Globalization.NumberStyles.HexNumber);

            return 0;
        }

        public List<AddressStorage> GetAddressList()
        {
            if (_Process == null) return null;
            List<AddressStorage> AddressList;
            if (Storage.AddressCollection.TryGetValue(_Platform, out AddressList))
                return AddressList;

            return null;
        }

        public int GetAddressListSize()
        {
            if (_Process == null) return 0;
            List<AddressStorage> AddressList;
            if (Storage.AddressCollection.TryGetValue(_Platform, out AddressList))
                return AddressList.Count;

            return 0;
        }

        public int GetControlSize()
        {
            if (_Process == null) return 0;
            return ControlCollection.Count();
        }

        public Database Database()
        {
            return _App.Database();
        }

        public string GetControl(string name, string defaultValue = "")
        {
            Control control = ControlCollection.SingleOrDefault(x => x.Name == name);

            if (control == null)
            {
                if (defaultValue != "")
                    SetControl(name, defaultValue);

                return defaultValue;
            }

            return control.Value;
        }

        public void SetControl(string name, string value)
        {
            Control control = ControlCollection.SingleOrDefault(x => x.Name == name);

            if (control == null)
            {
                control = new Control();

                control.Form = GetName();
                control.Name = name;
                control.Value = value;
                control.Platform = GetPlatform().ToString();

                control.Id = Database().SetControl(control);
                ControlCollection.Add(control);
            }
            else
            {
                control.Value = value;
                Database().SetControl(control);
            }   
        }

        public List<Skill> GetSkillList()
        {
            return SkillCollection;
        }

        public Skill GetSkillData(int skillId)
        {
            return SkillCollection.SingleOrDefault(x => x.Id == skillId);
        }

        public Skill GetSkillData(string skillName)
        {
            return SkillCollection.SingleOrDefault(x => x.Name == skillName);
        }

        public List<SkillBar> GetSkillBarList()
        {
            return SkillBarCollection;
        }

        public SkillBar GetSkillBar(int skillId)
        {
            return SkillBarCollection.SingleOrDefault(x => x.SkillId == skillId);
        }

        public void SetSkillBar(int skillId, int skillType)
        {
            SkillBar skillBarData = SkillBarCollection.SingleOrDefault(x => x.SkillId == skillId);

            if (skillBarData == null)
            {
                skillBarData = new SkillBar();

                skillBarData.User = GetName();
                skillBarData.SkillId = skillId;
                skillBarData.SkillType = skillType;
                skillBarData.Platform = GetPlatform().ToString();
                skillBarData.UseTime = 0;

                skillBarData.Id = Database().SetSkillBar(skillBarData);

                SkillBarCollection.Add(skillBarData);
            }         
        }

        public void DeleteSkillBar(int skillId)
        {
            SkillBar skillBarData = SkillBarCollection.SingleOrDefault(x => x.SkillId == skillId);

            if(skillBarData != null)
            {
                Database().DeleteSkillBar(skillBarData);
                SkillBarCollection.RemoveAll(x => x.SkillId == skillId);
            }
        }

        public List<Loot> GetLootList()
        {
            return LootCollection;
        }

        public int GetLootSize()
        {
            if (_Process == null) return 0;
            return LootCollection.Count();
        }

        public Loot GetLoot(int itemId)
        {
            return LootCollection.SingleOrDefault(x => x.ItemId == itemId);
        }

        public void SetLoot(int itemId, string itemName)
        {
            Loot loot = LootCollection.SingleOrDefault(x => x.ItemId == itemId);

            if (loot == null)
            {
                loot = new Loot();

                loot.User = GetName();
                loot.ItemId = itemId;
                loot.ItemName = itemName;
                loot.Platform = GetPlatform().ToString();

                loot.Id = Database().SetLoot(loot);

                LootCollection.Add(loot);
            }
        }

        public void DeleteLoot(int itemId)
        {
            Loot loot = LootCollection.SingleOrDefault(x => x.ItemId == itemId);

            if (loot != null)
            {
                Database().DeleteLoot(loot);
                LootCollection.RemoveAll(x => x.ItemId == itemId);
            }
        }

        public void ClearLoot()
        {
            Database().ClearLoot(LootCollection);
            LootCollection.Clear();
        }

        public List<Sell> GetSellList()
        {
            return SellCollection;
        }

        public int GetSellListSize()
        {
            if (_Process == null) return 0;
            return SellCollection.Count();
        }

        public Sell GetSell(int itemId)
        {
            return SellCollection.SingleOrDefault(x => x.ItemId == itemId);
        }

        public void SetSell(int itemId, string itemName)
        {
            Sell sell = SellCollection.SingleOrDefault(x => x.ItemId == itemId);

            if (sell == null)
            {
                sell = new Sell();

                sell.User = GetName();
                sell.ItemId = itemId;
                sell.ItemName = itemName;
                sell.Platform = GetPlatform().ToString();

                sell.Id = Database().SetSell(sell);

                SellCollection.Add(sell);
            }
        }

        public void DeleteSell(int itemId)
        {
            Sell sell = SellCollection.SingleOrDefault(x => x.ItemId == itemId);

            if (sell != null)
            {
                Database().DeleteSell(sell);
                LootCollection.RemoveAll(x => x.ItemId == itemId);
            }
        }

        public void ClearSell()
        {
            Database().ClearSell(SellCollection);
            SellCollection.Clear();
        }

        public List<Target> GetTargetList()
        {
            return TargetCollection;
        }

        public int GetTargetSize()
        {
            if (_Process == null) return 0;
            return TargetCollection.Count();
        }

        public Target GetTargetList(string name)
        {
            return TargetCollection.SingleOrDefault(x => x.Name == name);
        }

        public void SetTargetList(string name, int targetChecked)
        {
            Target targetData = TargetCollection.SingleOrDefault(x => x.Name == name);

            if (targetData == null)
            {
                targetData = new Target();

                targetData.User = GetName();
                targetData.Name = name;
                targetData.Checked = targetChecked;
                targetData.Platform = GetPlatform().ToString();

                targetData.Id = Database().SetTarget(targetData);

                TargetCollection.Add(targetData);
            }
            else
            {
                targetData.Checked = targetChecked;
                Database().SetTarget(targetData);
            }
        }

        public void ClearTargetList()
        {
            Database().ClearTargetList(TargetCollection);
            TargetCollection.Clear();
        }

        public bool IsDisconnected()
        {
            return (GetDisconnectTime() > 0 && Environment.TickCount - GetDisconnectTime() >= 15000) || HasExited();
        }

        public Process GetProcess()
        {
            return _Process;
        }

        public int GetProcessId()
        {
            return _ProcessId;
        }

        public void SetPhase(EPhase s)
        {
            _Phase = s;
        }

        public EPhase GetPhase()
        {
            return _Phase;
        }
        public void SetAction(EAction s)
        {
            _Action = s;
        }

        public EAction GetAction()
        {
            return _Action;
        }

        public void SetLoginTime(int Time)
        {
            _LoginTime = Time;
        }

        public int GetLoginTime()
        {
            return _LoginTime;
        }

        public bool IsInEnterGame()
        {
            return _EnterGameTime > 0 && Environment.TickCount - GetEnterGameTime() <= 16000;
        }

        public void SetEnterGameTime(int Time)
        {
            _EnterGameTime = Time;
        }

        public int GetEnterGameTime()
        {
            return _EnterGameTime;
        }

        public void SetDisconnectTime(int Time)
        {
            _DisconnectTime = Time;
        }

        public int GetDisconnectTime()
        {
            return _DisconnectTime;
        }

        public bool IsCharacterAvailable()
        {
            if (GetPhase() == EPhase.Playing 
                && GetControlSize() > 0 
                && Storage.ClientCollection.Where(x => x.GetProcessId() == GetProcessId())?.SingleOrDefault() != null)
                return true;

            return false;
        }

        public bool IsFollowOwner()
        {
            return Storage.FollowedClient != null && Storage.FollowedClient.GetProcessId() == GetProcessId();
        }

        public int GetAttackableTargetSize()
        {
            var TargetList = Database().GetTargetList(GetName(), GetPlatform().ToString()).ToList();

            int AllowedCount = 0;

            TargetList.ForEach(x =>
            {
                if (x.Checked == 1)
                    AllowedCount++;
            });

            return AllowedCount;
        }

        public bool IsInAttackableTargetList(string Name)
        {
            Target Target = GetTargetList(Name);
            if (Target == null) return false;
            return Target.Checked == 1;
        }

        public int GetPartyAllowedSize()
        {
            return _PartyAllowedCollection.Count;
        }

        public bool GetPartyAllowed(int Id)
        {
            return _PartyAllowedCollection.Find(x => x == Id) == Id;
        }

        public void AddPartyAllowed(int Id)
        {
            if (_PartyAllowedCollection.Contains(Id) == false)
                _PartyAllowedCollection.Add(Id);
        }

        public void RemovePartyAllowed(int Id)
        {
            if (_PartyAllowedCollection.Contains(Id))
                _PartyAllowedCollection.Remove(Id);
        }

        public bool IsMovingLoot()
        {
            return _MovingLoot;
        }

        public void SetMovingLoot(bool MovingLoot)
        {
            _MovingLoot = MovingLoot;
        }

        protected void ProcessRecvPacketEvent(byte[] Packet)
        {
            /**
             * @reference https://github.com/srmeier/KnightOnline/blob/master/Server/shared/packets.h
             */
            string Message = ByteToHex(Packet);

            Console.WriteLine("RECV -> " + Message);

            switch (Message.Substring(0, 2))
            {
                case "01": //WIZ_LOGIN
                    SetPhase(EPhase.Selecting);
                    SetLoginTime(Environment.TickCount);
                    Debug.WriteLine("Login " + GetLoginTime());
                    break;

                case "F3": //WIZ_UNKNOWN -> Maybe Authentication response
                    {
                        string Ret = Message.Substring(2, 2);

                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            Ret = Message.Substring(6, 2);

                        switch (Ret)
                        {
                            case "01":
                                Debug.WriteLine("Auth Response " + Ret + " - Success");
                                break;
                            case "02":
                                Debug.WriteLine("Auth Response " + Ret + " - User not found");
                                break;
                            case "03":
                                Debug.WriteLine("Auth Response " + Ret + " - Password does not match");
                                break;
                            case "05":
                                Debug.WriteLine("Auth Response " + Ret + " - Already");
                                break;
                            default:
                                Debug.WriteLine("Auth Response " + Ret);
                                break;

                        }
                    }
                    break;

                case "10": //WIZ_CHAT
                    {
                        if (GetControlSize() == 0)
                            return;

                        string chatMode = Message.Substring(2, 2);
                        string nation = Message.Substring(4, 2);
                        string playerId = Message.Substring(6, 4);
                        string nameLen = Message.Substring(10, 2);
                        string name = Message.Substring(12, int.Parse(nameLen, System.Globalization.NumberStyles.HexNumber) * 2);
                        string messageLen = Message.Substring(12 + (int.Parse(nameLen, System.Globalization.NumberStyles.HexNumber) * 2), 2);
                        string message = Message.Substring(12 + (int.Parse(nameLen, System.Globalization.NumberStyles.HexNumber) * 2) + 4, int.Parse(messageLen, System.Globalization.NumberStyles.HexNumber) * 2);

                        switch (chatMode)
                        {
                            case "01": //Normal Chat
                                {
                                    if (Convert.ToBoolean(GetControl("CommandSwift")) && GetJob() == "Rogue")
                                    {
                                        string nameString = Encoding.Default.GetString(StringToByte(name));
                                        string messageString = Encoding.Default.GetString(StringToByte(message));

                                        if (messageString == GetControl("CommandSwiftValue"))
                                        {
                                            Skill skillData = GetSkillData("Swift");

                                            if (skillData != null)
                                            {
                                                if (UseSkill(skillData, BitConverter.ToInt16(StringToByte(playerId), 0)))
                                                    Thread.Sleep(1250);
                                            }
                                        }
                                    }
                                }
                                break;
                            case "02": //Whisper Chat
                                {
                                    if (Convert.ToBoolean(GetControl("CommandPartyRequest")))
                                    {
                                        string nameString = Encoding.Default.GetString(StringToByte(name));
                                        string messageString = Encoding.Default.GetString(StringToByte(message));

                                        if (messageString == GetControl("CommandPartyRequestValue"))
                                            SendParty(nameString);
                                    }

                                    if (Convert.ToBoolean(GetControl("CommandSwift")) && GetJob() == "Rogue")
                                    {
                                        string nameString = Encoding.Default.GetString(StringToByte(name));
                                        string messageString = Encoding.Default.GetString(StringToByte(message));

                                        if (messageString == GetControl("CommandSwiftValue"))
                                        {
                                            Skill skillData = GetSkillData("Swift");

                                            if (skillData != null)
                                            {
                                                if (UseSkill(skillData, BitConverter.ToInt16(StringToByte(playerId), 0)))
                                                    Thread.Sleep(1250);
                                            }
                                        }
                                    }

                                }
                                break;
                            case "03": //Party Chat
                                {
                                    if (Convert.ToBoolean(GetControl("CommandSwift")) && GetJob() == "Rogue")
                                    {
                                        string nameString = Encoding.Default.GetString(StringToByte(name));
                                        string messageString = Encoding.Default.GetString(StringToByte(message));

                                        if (messageString == GetControl("CommandSwiftValue"))
                                        {
                                            Skill skillData = GetSkillData("Swift");

                                            if (skillData != null)
                                            {
                                                if (UseSkill(skillData, BitConverter.ToInt16(StringToByte(playerId), 0)))
                                                    Thread.Sleep(1250);
                                            }
                                        }
                                    }

                                    if (Convert.ToBoolean(GetControl("CommandBuff")) && GetJob() == "Priest")
                                    {
                                        string nameString = Encoding.Default.GetString(StringToByte(name));
                                        string messageString = Encoding.Default.GetString(StringToByte(message));

                                        if (messageString == GetControl("CommandBuffValue"))
                                        {
                                            List<Player> partyList = new List<Player>();

                                            if (GetPartyList(ref partyList) > 0)
                                            {
                                                partyList.ForEach(x =>
                                                {
                                                    if (x.Name != nameString) return;
                                                    PartyPriestAction(x, true);
                                                });
                                            }
                                        }
                                    }

                                    if (Convert.ToBoolean(GetControl("CommandTeleport")) && GetJob() == "Mage")
                                    {
                                        string nameString = Encoding.Default.GetString(StringToByte(name));
                                        string messageString = Encoding.Default.GetString(StringToByte(message));

                                        if (messageString == GetControl("CommandTeleportValue"))
                                        {
                                            Skill skillData = GetSkillData("Summon Friend");

                                            if (skillData != null)
                                            {
                                                if (UseMageSkill(skillData, BitConverter.ToInt16(StringToByte(playerId), 0)))
                                                    Thread.Sleep(1250);
                                            }
                                        }
                                    }
                                }
                                break;
                            case "05": //Shout
                                break;

                            case "06": //Clan
                                break;

                            case "0F": //Alliance
                                break;
                        }
                    }
                    break;

                case "23": //WIZ_ITEM_DROP
                    {
                        if (GetControlSize() == 0)
                            return;

                        if (Convert.ToBoolean(GetControl("OnlyNoah")) || Convert.ToBoolean(GetControl("LootOnlyList")) || Convert.ToBoolean(GetControl("LootOnlySell")))
                        {
                            LootInfo AutoLootData = new LootInfo();

                            AutoLootData.Id = Message.Substring(6, 8);
                            AutoLootData.MobId = BitConverter.ToInt16(StringToByte(Message.Substring(2, 4)), 0);
                            
                            AutoLootData.DropTime = Environment.TickCount;

                            int TargetBase = GetTargetBase(AutoLootData.MobId);

                            if (TargetBase == 0)
                            {
                                AutoLootData.X = 0;
                                AutoLootData.Y = 0;
                            }
                            else
                            {
                                AutoLootData.X = (int)Math.Round(ReadFloat(TargetBase + GetAddress("KO_OFF_X")));
                                AutoLootData.Y = (int)Math.Round(ReadFloat(TargetBase + GetAddress("KO_OFF_Y")));
                            }

                            _AutoLootCollection.Add(AutoLootData);
                        }

                        Debug.WriteLine("Chest drop " + Message.Substring(6, 8));
                    }

                    break;

                case "24": //WIZ_BUNDLE_OPEN_REQ
                    if (GetControlSize() == 0)
                        return;

                    if (Message.Length != 156) break;

                    for (int i = 0; i < 4; i++)
                    {
                        string ItemHex = Message.Substring(12 + (12 * i), 8);

                        if (ItemHex != "00000000")
                        {
                            int Item = BitConverter.ToInt32(StringToByte(ItemHex), 0);
                            Item ItemData = Storage.ItemCollection.Find(x => x.Id == Item);

                            Debug.WriteLine("Loot info " + i + " " + Item);

                            bool Loot = false;

                            if (Item == 900000000 || (Convert.ToBoolean(GetControl("OnlyNoah")) && Item == 900000000))
                                Loot = true;

                            if (Convert.ToBoolean(GetControl("LootOnlyList")) && GetLoot(Item) != null)
                                Loot = true;

                            if (Convert.ToBoolean(GetControl("LootOnlySell")) && GetSell(Item) != null)
                                Loot = true;

                            if (ItemData != null)
                            {
                                if (Convert.ToBoolean(GetControl("LootOnlyList")) && Convert.ToBoolean(GetControl("LootConsumable")) && ItemData.Extension == 22)
                                    Loot = true;

                                if (Convert.ToBoolean(GetControl("LootOnlyList")) && Convert.ToBoolean(GetControl("LootOther")) && ItemData.Extension != 22)
                                {
                                    int SellPrice = ItemData.BuyPrice / 6; //Non-Premium User

                                    if (SellPrice < 0)
                                        SellPrice = 0;

                                    if (SellPrice >= Convert.ToInt32(GetControl("LootPrice")))
                                        Loot = true;
                                }
                            }
                            else
                                Loot = true;

                            if (Loot)
                                SendPacket("26" + Message.Substring(2, 4) + Message.Substring(6, 4) + ItemHex + "0" + i + "00");

                        }
                    }
                    break;

                case "5B": //WIZ_ITEM_UPGRADE
                    switch (Message.Substring(2, 2))
                    {
                        case "0C": //CharacterSeal //Only JPKO 
                            {
                                switch (Message.Substring(4, 2))
                                {
                                    case "01":
                                        Debug.WriteLine("Seal password validation");
                                        break;
                                    case "03":
                                        if (Message.Substring(6, 2) == "01")
                                            Debug.WriteLine("Seal password validate success");
                                        else
                                            Debug.WriteLine("Seal password validate failed");
                                        break;
                                }
                            }
                            break;
                    }
                    break;

                case "27": //WIZ_ZONE_CHANGE
                    switch (Message.Substring(2, 2))
                    {
                        case "02": //ZoneChangeLoaded
                            SetPhase(EPhase.Playing);
                            SetEnterGameTime(Environment.TickCount);
                            Debug.WriteLine("Zone change loaded");
                            break;

                        case "03": //ZoneChangeTeleport
                            SetEnterGameTime(Environment.TickCount);
                            int ZoneId = int.Parse(Message.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                            Debug.WriteLine("Zone change teleport(" + ZoneId + ")");
                            break;
                    }
                    break;

                case "1E": //WIZ_WARP
                    //SetFallbackTime(Environment.TickCount);
                    Debug.WriteLine("Warp start (/town, fallback or gamemaster)");
                    break;

                case "4B": //WIZ_WARP_LIST
                    switch (Message.Substring(2, 2))
                    {
                        case "01": //GetWarpList
                            Debug.WriteLine("Warp list loaded");
                            break;
                    }
                    break;

                case "2F": //WIZ_PARTY
                    if (GetControlSize() == 0)
                        return;

                    switch (Message.Substring(2, 2))
                    {
                        case "02": //PartyPermit
                            if (Storage.FollowedClient != null
                                && Storage.FollowedClient.GetProcessId() != GetProcessId()
                                && Convert.ToBoolean(GetControl("FollowDisable")) == false
                                && Storage.AutoPartyAccept)
                            {
                                string Name = Encoding.Default.GetString(StringToByte(Message.Substring(12)));

                                if (Storage.ClientCollection.Any(x => x.GetName() == Storage.FollowedClient.GetName()))
                                    _PartyRequestTime = Environment.TickCount;
                            }
                            break;
                    }
                    break;
            }
        }

        protected void ProcessSendPacketEvent(byte[] Packet)
        {
            /**
             * @reference https://github.com/srmeier/KnightOnline/blob/master/Server/shared/packets.h
             */
            string Message = ByteToHex(Packet);

            Debug.WriteLine("SEND -> " + Message);

            switch (Message.Substring(0, 2))
            {
                case "F2": //WIZ_AUTH
                    Debug.WriteLine("Auth " + Environment.TickCount);
                    SetPhase(EPhase.Authentication);
                    break;

                case "01": //WIZ_LOGIN
                    SetPhase(EPhase.Loggining);
                    Debug.WriteLine("Login " + Environment.TickCount);
                    break;

                case "04": //WIZ_SEL_CHAR
                    SetPhase(EPhase.Selected);
                    Debug.WriteLine("Character selected " + Environment.TickCount);
                    break;

                case "0D": //WIZ_GAMESTART
                    SetPhase(EPhase.Playing);
                    SetEnterGameTime(Environment.TickCount);
                    LoadZone();

                    Debug.WriteLine("Game start " + Environment.TickCount);
                    break;

                case "5B": //WIZ_ITEM_UPGRADE
                    switch (Message.Substring(2, 2))
                    {
                        case "0C": //CharacterSealPacket
                            {
                                switch (Message.Substring(4, 2))
                                {
                                    case "01":
                                        Debug.WriteLine("Seal password validation");
                                        break;
                                    case "03":
                                        if (GetControl("CharacterSealPacket") != Message)
                                            SetControl("CharacterSealPacket", Message);

                                        Debug.WriteLine("Seal password validate request");
                                        break;
                                }
                            }
                            break;
                    }
                    break;

                case "20": //WIZ_NPC_EVENT
                    {
                        Debug.WriteLine("Npc event");
                        ForwardPacketToAllFollower(Message);
                    }
                    break;

                case "33": //WIZ_OBJECT_EVENT
                    {
                        if (GetControlSize() == 0)
                            return;

                        Debug.WriteLine("Object event");

                        if (Convert.ToBoolean(GetControl("RouteSave")) && _RouteSaveData != null)
                        {
                            if (_RouteSaveData.Count() > 0)
                            {
                                RouteData routeData = _RouteSaveData.Last();

                                routeData.Action = RouteData.Event.OBJECT;

                                _RouteSaveData.Add(routeData);

                                Debug.WriteLine("Route Save - Action : {0}", routeData.Action);
                            }
                        }

                        ForwardPacketToAllFollower(Message);
                    }
                    break;

                case "24": //WIZ_BUNDLE_OPEN_REQ
                    {
                        if (GetControlSize() == 0)
                            return;

                        if (Convert.ToBoolean(GetControl("OnlyNoah")) || Convert.ToBoolean(GetControl("LootOnlyList")) || Convert.ToBoolean(GetControl("LootOnlySell")))
                        {
                            LootInfo LootInfo = _AutoLootCollection.Find(x => x.Id == Message.Substring(2, 8));

                            if (LootInfo != null)
                                _AutoLootCollection.RemoveAll(x => x.Id == LootInfo.Id);

                            if (Convert.ToBoolean(GetControl("MoveToLoot"))
                                && (Convert.ToBoolean(GetControl("FollowDisable")) == true || (Convert.ToBoolean(GetControl("FollowDisable")) == false && IsFollowOwner() == true)))
                                SetMovingLoot(false);
                        }

                        Debug.WriteLine("Chest open");
                    }
                    break;

                case "26": //WIZ_ITEM_GET
                    Debug.WriteLine("Loot get " + BitConverter.ToInt32(StringToByte(Message.Substring(10, 8)), 0));
                    break;

                case "79": //WIZ_SKILLDATA
                    if (GetControlSize() == 0)
                        return;

                    switch (Message.Substring(2, 2))
                    {
                        case "02":
                            {
                                if (GetControl("CharacterSealPacket") != "")
                                    SendPacket(GetControl("CharacterSealPacket"));
                            }
                            break;
                    }
                    break;

                case "27": //WIZ_ZONE_CHANGE
                    switch (Message.Substring(2, 2))
                    {
                        case "01": //ZoneChangeLoaading
                            LoadZone();
                            Debug.WriteLine("Zone change load");
                            break;
                    }
                    break;

                case "4B": //WIZ_WARP_LIST
                    {
                        if (GetControlSize() == 0)
                            return;

                        SetPhase(EPhase.Warping);

                        if (Convert.ToBoolean(GetControl("RouteSave")) && _RouteSaveData != null)
                        {
                            if (_RouteSaveData.Count() > 0)
                            {
                                RouteData routeData = _RouteSaveData.Last();

                                routeData.Action = RouteData.Event.GATE;
                                routeData.Packet = Message;

                                RouteSetAction(routeData);
                            }
                        }

                        ForwardPacketToAllFollower(Message);
                    }
                    break;

                case "48": //WIZ_WARP_HOME
                    {
                        Debug.WriteLine("/town");

                        if (GetControlSize() == 0)
                            return;

                        ForwardPacketToAllFollower(Message);
                        _AutoLootCollection.Clear();

                        if (Convert.ToBoolean(GetControl("RouteSave")) && _RouteSaveData != null)
                        {
                            if (_RouteSaveData.Count() > 0)
                            {
                                RouteData routeData = _RouteSaveData.Last();

                                routeData.Action = RouteData.Event.TOWN;

                                RouteSetAction(routeData);
                            }
                        }
                    }
                    break;

                case "55": //WIZ_SELECT_MSG
                    {
                        Debug.WriteLine("Select message");
                        ForwardPacketToAllFollower(Message);
                    }
                    break;

                case "56": //WIZ_NPC_SAY
                    {
                        Debug.WriteLine("Say message");
                        ForwardPacketToAllFollower(Message);
                    }
                    break;

                case "64": //WIZ_QUEST
                    {
                        Debug.WriteLine("Select quest");
                        ForwardPacketToAllFollower(Message);
                    }
                    break;
            }
        }
        #endregion

        #region "Read Memory Functions"

        public byte[] ReadByteArray(int address, int length)
        {
            return ReadByteArray(_Handle, address, length);
        }

        public Int32 Read4Byte(IntPtr Address)
        {
            return Read4Byte(_Handle, Address);
        }

        public Int32 Read4Byte(long Address)
        {
            return Read4Byte(new IntPtr(Address));
        }

        public Int16 ReadByte(IntPtr Address)
        {
            return ReadByte(_Handle, Address);
        }

        public Int16 ReadByte(long Address)
        {
            return ReadByte(new IntPtr(Address));
        }

        public Single ReadFloat(IntPtr Address)
        {
            return ReadFloat(_Handle, Address);
        }

        public Single ReadFloat(long Address)
        {
            return ReadFloat(new IntPtr(Address));
        }

        public String ReadString(IntPtr Address, Int32 Size)
        {
            return ReadString(_Handle, Address, Size);
        }

        public String ReadString(long Address, Int32 Size)
        {
            return ReadString(new IntPtr(Address), Size);
        }
        #endregion

        #region "Write Memory Functions"
        public void WriteFloat(IntPtr Address, float Value)
        {
            WriteFloat(_Handle, Address, Value);
        }

        public void WriteFloat(long Address, float Value)
        {
            WriteFloat(_Handle, new IntPtr(Address), Value);
        }

        public void Write4Byte(IntPtr Address, Int32 Value)
        {
            Write4Byte(_Handle, Address, Value);
        }

        public void Write4Byte(long Address, Int32 Value)
        {
            Write4Byte(_Handle, new IntPtr(Address), Value);
        }

        public void WriteByte(IntPtr Address, Int32 Value)
        {
            WriteByte(_Handle, Address, Value);
        }

        public void WriteByte(long Address, Int32 Value)
        {
            WriteByte(_Handle, new IntPtr(Address), Value);
        }

        public void WriteString(IntPtr Address, string Value)
        {
            byte[] data = Encoding.Default.GetBytes(Value + "\0");
            IntPtr Zero = IntPtr.Zero;
            WriteProcessMemory(_Handle, Address, data, data.Length, (int)Zero);
        }

        public void ExecuteRemoteCode(String Code)
        {
            byte[] CodeByte = StringToByte(Code);
            IntPtr CodePtr = VirtualAllocEx(_Handle, IntPtr.Zero, CodeByte.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            if(CodePtr != IntPtr.Zero)
            {
                WriteProcessMemory(_Handle, CodePtr, CodeByte, CodeByte.Length, 0);
                IntPtr Thread = CreateRemoteThread(_Handle, IntPtr.Zero, 0, CodePtr, IntPtr.Zero, 0, IntPtr.Zero);

                if (Thread != IntPtr.Zero)
                    WaitForSingleObject(Thread, uint.MaxValue);

                CloseHandle(Thread);
            }

            VirtualFreeEx(_Handle, CodePtr, 0, MEM_RELEASE);

        }
        #endregion

        #region "Delete Mutant (Multi Client - JPKO - knight.mgame.jp)"
        private static string GetObjectName(SYSTEM_HANDLE_INFORMATION shHandle, Process process)
        {
            IntPtr m_ipProcessHwnd = process.Handle;
            IntPtr ipHandle = IntPtr.Zero;
            var objBasic = new OBJECT_BASIC_INFORMATION();
            IntPtr ipBasic = IntPtr.Zero;
            IntPtr ipObjectType = IntPtr.Zero;
            var objObjectName = new OBJECT_NAME_INFORMATION();
            IntPtr ipObjectName = IntPtr.Zero;
            string strObjectName = "";
            int nLength = 0;
            int nReturn = 0;
            IntPtr ipTemp = IntPtr.Zero;

            if (!DuplicateHandle(m_ipProcessHwnd, shHandle.Handle, GetCurrentProcess(),
                                          out ipHandle, 0, false, DUPLICATE_SAME_ACCESS))
            {
                return null;
            }


            ipBasic = Marshal.AllocHGlobal(Marshal.SizeOf(objBasic));
            NtQueryObject(ipHandle, (int)ObjectInformationClass.ObjectBasicInformation,
                                   ipBasic, Marshal.SizeOf(objBasic), ref nLength);
            objBasic = (OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(ipBasic, objBasic.GetType());
            Marshal.FreeHGlobal(ipBasic);

            nLength = objBasic.NameInformationLength;

            ipObjectName = Marshal.AllocHGlobal(nLength);
            while ((uint)(nReturn = NtQueryObject(
                     ipHandle, (int)ObjectInformationClass.ObjectNameInformation,
                     ipObjectName, nLength, ref nLength))
                   == STATUS_INFO_LENGTH_MISMATCH)
            {
                Marshal.FreeHGlobal(ipObjectName);
                ipObjectName = Marshal.AllocHGlobal(nLength);
            }
            objObjectName = (OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(ipObjectName, objObjectName.GetType());

            if (Is64Bit())
                ipTemp = new IntPtr(Convert.ToInt64(objObjectName.Name.Buffer.ToString(), 10) >> 32);
            else
                ipTemp = objObjectName.Name.Buffer;

            if (ipTemp != IntPtr.Zero)
            {

                byte[] baTemp2 = new byte[nLength];
                try
                {
                    Marshal.Copy(ipTemp, baTemp2, 0, nLength);

                    strObjectName = Marshal.PtrToStringUni(Is64Bit() ?
                                                           new IntPtr(ipTemp.ToInt64()) :
                                                           new IntPtr(ipTemp.ToInt32()));
                    return strObjectName;
                }
                catch (AccessViolationException)
                {
                    return null;
                }
                finally
                {
                    Marshal.FreeHGlobal(ipObjectName);
                    CloseHandle(ipHandle);
                }
            }
            return null;
        }

        private static string GetObjectTypeName(SYSTEM_HANDLE_INFORMATION shHandle, Process process)
        {
            IntPtr m_ipProcessHwnd = process.Handle;
            IntPtr ipHandle = IntPtr.Zero;
            var objBasic = new OBJECT_BASIC_INFORMATION();
            IntPtr ipBasic = IntPtr.Zero;
            var objObjectType = new OBJECT_TYPE_INFORMATION();
            IntPtr ipObjectType = IntPtr.Zero;
            IntPtr ipObjectName = IntPtr.Zero;
            string strObjectTypeName = "";
            int nLength = 0;
            int nReturn = 0;
            IntPtr ipTemp = IntPtr.Zero;

            if (!DuplicateHandle(m_ipProcessHwnd, shHandle.Handle,
                                          GetCurrentProcess(), out ipHandle,
                                          0, false, DUPLICATE_SAME_ACCESS))
            {
                return null;
            }

            ipBasic = Marshal.AllocHGlobal(Marshal.SizeOf(objBasic));
            NtQueryObject(ipHandle, (int)ObjectInformationClass.ObjectBasicInformation,
                                   ipBasic, Marshal.SizeOf(objBasic), ref nLength);
            objBasic = (OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(ipBasic, objBasic.GetType());
            Marshal.FreeHGlobal(ipBasic);

            ipObjectType = Marshal.AllocHGlobal(objBasic.TypeInformationLength);
            nLength = objBasic.TypeInformationLength;
            while ((uint)(nReturn = NtQueryObject(
                ipHandle, (int)ObjectInformationClass.ObjectTypeInformation, ipObjectType,
                  nLength, ref nLength)) ==
                STATUS_INFO_LENGTH_MISMATCH)
            {
                Marshal.FreeHGlobal(ipObjectType);
                ipObjectType = Marshal.AllocHGlobal(nLength);
            }

            objObjectType = (OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(ipObjectType, objObjectType.GetType());
            if (Is64Bit())
            {
                ipTemp = new IntPtr(Convert.ToInt64(objObjectType.Name.Buffer.ToString(), 10) >> 32);
            }
            else
            {
                ipTemp = objObjectType.Name.Buffer;
            }

            strObjectTypeName = Marshal.PtrToStringUni(ipTemp, objObjectType.Name.Length >> 1);
            Marshal.FreeHGlobal(ipObjectType);
            CloseHandle(ipHandle);

            return strObjectTypeName;
        }
        private static List<SYSTEM_HANDLE_INFORMATION> GetHandles(Process process = null, String[] IN_strObjectTypeName = null, String[] IN_strObjectName = null)
        {
            uint nStatus;
            int nHandleInfoSize = 0x10000;
            IntPtr ipHandlePointer = Marshal.AllocHGlobal(nHandleInfoSize);
            int nLength = 0;
            IntPtr ipHandle = IntPtr.Zero;

            while ((nStatus = NtQuerySystemInformation(CNST_SYSTEM_HANDLE_INFORMATION, ipHandlePointer,
                                                                nHandleInfoSize, ref nLength)) ==
                    STATUS_INFO_LENGTH_MISMATCH)
            {
                nHandleInfoSize = nLength;
                Marshal.FreeHGlobal(ipHandlePointer);
                ipHandlePointer = Marshal.AllocHGlobal(nLength);
            }

            byte[] baTemp = new byte[nLength];
            Marshal.Copy(ipHandlePointer, baTemp, 0, nLength);

            long lHandleCount = 0;

            if (Is64Bit())
            {
                lHandleCount = Marshal.ReadInt64(ipHandlePointer);
                ipHandle = new IntPtr(ipHandlePointer.ToInt64() + 8);
            }
            else
            {
                lHandleCount = Marshal.ReadInt32(ipHandlePointer);
                ipHandle = new IntPtr(ipHandlePointer.ToInt32() + 4);
            }

            SYSTEM_HANDLE_INFORMATION shHandle;
            List<SYSTEM_HANDLE_INFORMATION> lstHandles = new List<SYSTEM_HANDLE_INFORMATION>();

            for (long lIndex = 0; lIndex < lHandleCount; lIndex++)
            {
                if (process == null || process.HasExited)
                    return lstHandles;

                shHandle = new SYSTEM_HANDLE_INFORMATION();

                if (Is64Bit())
                {
                    shHandle = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ipHandle, shHandle.GetType());
                    ipHandle = new IntPtr(ipHandle.ToInt64() + Marshal.SizeOf(shHandle) + 8);
                }
                else
                {
                    ipHandle = new IntPtr(ipHandle.ToInt64() + Marshal.SizeOf(shHandle));
                    shHandle = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ipHandle, shHandle.GetType());
                }

                if (shHandle.ProcessID != process.Id) continue;

                string strObjectTypeName = "";
                if (IN_strObjectTypeName != null)
                {
                    strObjectTypeName = GetObjectTypeName(shHandle, process);
                    if (!IN_strObjectTypeName.Contains(strObjectTypeName)) continue;
                }

                string strObjectName = "";
                if (IN_strObjectName != null)
                {
                    strObjectName = GetObjectName(shHandle, process);
                    if (!IN_strObjectName.Contains(strObjectName)) continue;
                }

                string strObjectTypeName2 = GetObjectTypeName(shHandle, process);
                string strObjectName2 = GetObjectName(shHandle, process);

                Console.WriteLine("{0}   {1}   {2}", shHandle.ProcessID, strObjectTypeName2, strObjectName2);

                lstHandles.Add(shHandle);
            }
            return lstHandles;
        }

        public void PatchMutant()
        {
            Debug.WriteLine("Mutant patching");

            String[] Names =
            {
                "Mutant"
            };

            String[] Values =
            {
                "\\Sessions\\0\\BaseNamedObjects\\Knight OnLine Client",
                "\\Sessions\\1\\BaseNamedObjects\\Knight OnLine Client",
                "\\Sessions\\2\\BaseNamedObjects\\Knight OnLine Client",
                "\\Sessions\\3\\BaseNamedObjects\\Knight OnLine Client",
                "\\Sessions\\4\\BaseNamedObjects\\Knight OnLine Client",
                "\\Sessions\\5\\BaseNamedObjects\\Knight OnLine Client",
                "\\Sessions\\6\\BaseNamedObjects\\Knight OnLine Client",
                "\\Sessions\\7\\BaseNamedObjects\\Knight OnLine Client",
                "\\Sessions\\8\\BaseNamedObjects\\Knight OnLine Client",
                "\\Sessions\\9\\BaseNamedObjects\\Knight OnLine Client",
                "\\Sessions\\10\\BaseNamedObjects\\Knight OnLine Client"
            };

            var handles = GetHandles(_Process, Names, Values);

            if (handles.Count == 0) return;

            foreach (var handle in handles)
            {
                IntPtr ipHandle = IntPtr.Zero;

                try
                {
                    Process Process = Process.GetProcessById(handle.ProcessID);

                    if (DuplicateHandle(Process.Handle, handle.Handle, GetCurrentProcess(), out ipHandle, 0, false, DUPLICATE_CLOSE_SOURCE))
                    {
                        CloseHandle(ipHandle);
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace); //Except -- Process(PID) is not running
                }
            }
        }
        #endregion

        public int GetRecvPointer()
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                return GetAddress("KO_PTR_DLG");

            return GetAddress("KO_PTR_DLG");
        }

        public int GetRecvHookPointer()
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                return Read4Byte(Read4Byte(GetRecvPointer())) + 0x8;

            return Read4Byte(Read4Byte(GetRecvPointer())) + 0x8;
        }

        public void PatchRecvMailslot()
        {
            if (_MailslotRecvFuncPtr == IntPtr.Zero)
            {
                UnicodeEncoding UnicodeEncoding = new UnicodeEncoding();

                IntPtr CreateFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "CreateFileW");
                IntPtr WriteFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "WriteFile");
                IntPtr CloseFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "CloseHandle");

                String MailslotRecvName = @"\\.\mailslot\KNIGHTONLINE_RECV\" + Environment.TickCount;

                if (_MailslotRecvPtr == IntPtr.Zero)
                    _MailslotRecvPtr = CreateMailslot(MailslotRecvName, 0, 0, IntPtr.Zero);

                _MailslotRecvFuncPtr = VirtualAllocEx(_Handle, _MailslotRecvFuncPtr, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

                byte[] MailslotRecvNameByte = UnicodeEncoding.GetBytes(MailslotRecvName);

                WriteProcessMemory(_Handle, _MailslotRecvFuncPtr + 0x400, MailslotRecvNameByte, MailslotRecvNameByte.Length, 0);

                Patch(_Handle, _MailslotRecvFuncPtr,
                    "55" +
                    "8BEC" +
                    "83C4F4" +
                    "33C0" +
                    "8945FC" +
                    "33D2" +
                    "8955F8" +
                    "6A00" +
                    "6880000000" +
                    "6A03" +
                    "6A00" +
                    "6A01" +
                    "6800000040" +
                    "68" + AlignDWORD(_MailslotRecvFuncPtr + 0x400) +
                    "E8" + AlignDWORD(AddressDistance(_MailslotRecvFuncPtr + 0x27, CreateFilePtr)) +
                    "8945F8" +
                    "6A00" +
                    "8D4DFC" +
                    "51" +
                    "FF750C" +
                    "FF7508" +
                    "FF75F8" +
                    "E8" + AlignDWORD(AddressDistance(_MailslotRecvFuncPtr + 0x3E, WriteFilePtr)) +
                    "8945F4" +
                    "FF75F8" +
                    "E8" + AlignDWORD(AddressDistance(_MailslotRecvFuncPtr + 0x49, CloseFilePtr)) +
                    "8BE5" +
                    "5D" +
                    "C3");
            }

            _MailslotRecvHookPtr = VirtualAllocEx(_Handle, _MailslotRecvHookPtr, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            Patch(_Handle, _MailslotRecvHookPtr,
                "55" +
                "8BEC" +
                "83C4F8" +
                "53" +
                "8B4508" +
                "83C004" +
                "8B10" +
                "8955FC" +
                "8B4D08" +
                "83C108" +
                "8B01" +
                "8945F8" +
                "FF75FC" +
                "FF75F8" +
                "E8" + AlignDWORD(AddressDistance(_MailslotRecvHookPtr + 0x23, _MailslotRecvFuncPtr)) +
                "83C408" +
                "8B0D" + AlignDWORD(GetRecvPointer()) +
                "FF750C" +
                "FF7508" +
                "B8" + AlignDWORD(Read4Byte(GetRecvHookPointer())) +
                "FFD0" +
                "5B" +
                "59" +
                "59" +
                "5D" +
                "C20800");

            uint MemoryProtection;
            VirtualProtectEx(_Handle, new IntPtr(GetRecvHookPointer()), 4, PAGE_EXECUTE_READWRITE, out MemoryProtection);
            Write4Byte(GetRecvHookPointer(), _MailslotRecvHookPtr.ToInt32());
            VirtualProtectEx(_Handle, new IntPtr(GetRecvHookPointer()), 4, MemoryProtection, out MemoryProtection);

            Debug.WriteLine("Recv packet hooked. Address: {0}", _MailslotRecvHookPtr);
        }

        public void PatchSendMailslot()
        {
            UnicodeEncoding UnicodeEncoding = new UnicodeEncoding();

            IntPtr CreateFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "CreateFileW");
            IntPtr WriteFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "WriteFile");
            IntPtr CloseFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "CloseHandle");

            String MailslotSendName = @"\\.\mailslot\KNIGHTONLINE_SEND\" + Environment.TickCount;

            _MailslotSendPtr = CreateMailslot(MailslotSendName, 0, 0, IntPtr.Zero);

            _MailslotSendHookPtr = VirtualAllocEx(_Handle, _MailslotSendHookPtr, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            byte[] MailslotSendNameByte = UnicodeEncoding.GetBytes(MailslotSendName);

            WriteProcessMemory(_Handle, _MailslotSendHookPtr + 0x400, MailslotSendNameByte, MailslotSendNameByte.Length, 0);

            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
            {
                Patch(_Handle, _MailslotSendHookPtr, "608B4424248905" +
                    AlignDWORD(_MailslotSendHookPtr + 0x100) + "8B4424288905" +
                    AlignDWORD(_MailslotSendHookPtr + 0x104) + "3D004000007D3D6A0068800000006A036A006A01680000004068" +
                    AlignDWORD(_MailslotSendHookPtr + 0x400) + "E8" +
                    AlignDWORD(AddressDistance(_MailslotSendHookPtr + 0x33, CreateFilePtr)) + "83F8FF741C6A005490FF35" +
                    AlignDWORD(_MailslotSendHookPtr + 0x104) + "FF35" +
                    AlignDWORD(_MailslotSendHookPtr + 0x100) + "50E8" +
                    AlignDWORD(AddressDistance(_MailslotSendHookPtr + 0x4E, WriteFilePtr)) + "50E8" +
                    AlignDWORD(AddressDistance(_MailslotSendHookPtr + 0x54, CloseFilePtr)) + "61558BEC6AFF68" +
                    AlignDWORD(Read4Byte(GetAddress("KO_PTR_SND") + 0x4)) + "E9" +
                    AlignDWORD(AddressDistance(_MailslotSendHookPtr + 0x61, new IntPtr(GetAddress("KO_PTR_SND") + 0x7))));
            }
            else
            {
                Patch(_Handle, _MailslotSendHookPtr, "608B4424248905" +
                    AlignDWORD(_MailslotSendHookPtr + 0x100) + "8B4424288905" +
                    AlignDWORD(_MailslotSendHookPtr + 0x104) + "3D004000007D3D6A0068800000006A036A006A01680000004068" +
                    AlignDWORD(_MailslotSendHookPtr + 0x400) + "E8" +
                    AlignDWORD(AddressDistance(_MailslotSendHookPtr + 0x33, CreateFilePtr)) + "83F8FF741C6A005490FF35" +
                    AlignDWORD(_MailslotSendHookPtr + 0x104) + "FF35" +
                    AlignDWORD(_MailslotSendHookPtr + 0x100) + "50E8" +
                    AlignDWORD(AddressDistance(_MailslotSendHookPtr + 0x4E, WriteFilePtr)) + "50E8" +
                    AlignDWORD(AddressDistance(_MailslotSendHookPtr + 0x54, CloseFilePtr)) + "616AFF68" +
                    AlignDWORD(Read4Byte(GetAddress("KO_PTR_SND") + 0x4)) + "E9" +
                    AlignDWORD(AddressDistance(_MailslotSendHookPtr + 0x61, new IntPtr(GetAddress("KO_PTR_SND") + 0x7))));
            }

            Patch(_Handle, new IntPtr(GetAddress("KO_PTR_SND")), "E9" + AlignDWORD(AddressDistance(new IntPtr(GetAddress("KO_PTR_SND")), _MailslotSendHookPtr)));

            Debug.WriteLine("Send packet hooked. Address: {0}", _MailslotSendHookPtr);
        }

        #region "Game Functions"

        public string GetName()
        {
            int NameLen = Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_NAME_LENGTH"));

            if (NameLen > 15)
                return ReadString(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_NAME")), NameLen);

            return ReadString(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_NAME"), NameLen);
        }

        protected bool IsConnectionLost()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_PKT")) + 0xA0) == 0 ? true : false;
        }

        public int GetId()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_ID"));
        }

        public int GetLevel()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_LEVEL"));
        }

        public int GetHp()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_HP"));
        }
        public int GetMaxHp()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MAX_HP"));
        }

        public int GetMp()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MP"));
        }

        public int GetMaxMp()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MAX_MP"));
        }

        public int GetX()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_X")));
        }

        public int GetY()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_Y")));
        }

        public int GetZ()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_Z")));
        }

        public string GetJob(int ClassId = -1)
        {
            if (ClassId == -1)
                ClassId = GetClass();

            string Job = "Undefined";

            switch (ClassId)
            {
                case 102: Job = "Rogue"; break;
                case 107: Job = "Rogue"; break;
                case 108: Job = "Rogue"; break;
                case 202: Job = "Rogue"; break;
                case 207: Job = "Rogue"; break;
                case 208: Job = "Rogue"; break;
                case 104: Job = "Priest"; break;
                case 111: Job = "Priest"; break;
                case 112: Job = "Priest"; break;
                case 204: Job = "Priest"; break;
                case 211: Job = "Priest"; break;
                case 212: Job = "Priest"; break;
                case 101: Job = "Warrior"; break;
                case 105: Job = "Warrior"; break;
                case 106: Job = "Warrior"; break;
                case 201: Job = "Warrior"; break;
                case 205: Job = "Warrior"; break;
                case 206: Job = "Warrior"; break;
                case 103: Job = "Mage"; break;
                case 109: Job = "Mage"; break;
                case 110: Job = "Mage"; break;
                case 203: Job = "Mage"; break;
                case 209: Job = "Mage"; break;
                case 210: Job = "Mage"; break;
            }

            return Job;
        }

        public int GetClass()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_CLASS"));
        }

        public int GetNation()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_NATION"));
        }

        public int GetTargetId()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOB"));
        }

        public string GetTargetName()
        {
            if (GetTargetId() == 0) return "";
            int Base = GetTargetBase();
            if (Base == 0) return "";
            int NameLen = Read4Byte(Base + GetAddress("KO_OFF_NAME_LENGTH"));
            if (NameLen > 15)
                return ReadString(Read4Byte(Base + GetAddress("KO_OFF_NAME")), NameLen);

            return ReadString(Base + GetAddress("KO_OFF_NAME"), NameLen);
        }

        public int GetTargetX()
        {
            if (GetTargetId() == 0) return 0;
            int Base = GetTargetBase();
            if (Base == 0) return 0;
            return (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_X")));
        }

        public int GetTargetY()
        {
            if (GetTargetId() == 0) return 0;
            int Base = GetTargetBase();
            if (Base == 0) return 0;
            return (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_Y")));
        }

        public int GetTargetZ()
        {
            if (GetTargetId() == 0) return 0;
            int Base = GetTargetBase();
            if (Base == 0) return 0;
            return (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_Z")));
        }

        protected short GetState()
        {
            return ReadByte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_STATE"));
        }

        public int GetZoneId()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_ZONE"));
        }

        protected int GetExp()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_EXP"));
        }

        protected int GetMaxExp()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MAX_EXP"));
        }

        public int GetGoX()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GO_X")));
        }

        public int GetGoY()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GO_Y")));
        }

        public int GetGoZ()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GO_Y")));
        }

        public int GetSkill(int Slot)
        {
            return Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_SKILL_TREE_BASE")) + 0x184 + (Slot * 4) + 0x68)));
        }

        public int GetSkillPoint(int Slot)
        {
            return Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_SKILL_TREE_BASE")) + GetAddress("KO_OFF_SKILL_TREE_POINT") + (Slot * 4));
        }

        public void Oreads(bool Enable)
        {
            if (Enable)
            {
                ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) + "6A0168" + AlignDWORD(700039000) + "B8" + AlignDWORD(GetAddress("KO_FAKE_ITEM")) + "FFD061C3");

                Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C4, 1);
                Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C6, 1);
                Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C7, 1);
            }
            else
            {
                ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) + "6A0068" + AlignDWORD(700039000) + "B8" + AlignDWORD(GetAddress("KO_FAKE_ITEM")) + "FFD061C3");

                Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C4, 0);
                Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C6, 0);
                Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C7, 0);
            }
        }

        public void Wallhack(bool Enable)
        {
            WriteByte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_WH"), Enable ? 0 : 1);
        }

        public void MoveCoordinate(int GoX, int GoY)
        {
            if (GoX <= 0 || GoY <= 0) return;
            if (GoX == GetX() && GoY == GetY()) return;

            WriteByte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOVE_TYPE"), 2);
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GO_X"), GoX);
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GO_Y"), GoY);
            WriteByte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOVE"), 1);
        }

        public void StartRouteEvent(int GoX, int GoY, int GoZ = 0)
        {
            if (GoX <= 0 || GoY <= 0) return;
            if (GoX == GetX() && GoY == GetY()) return;
            if (CoordinateDistance(GoX, GoY, GetX(), GetY()) <= 50)
                MoveCoordinate(GoX, GoY);
            else
            {
                ExecuteRemoteCode("60" +
                       "8B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) +
                       "C700" + FloatToHex(GoX) +
                       "C74004" + FloatToHex(GoZ) +
                       "C74008" + FloatToHex(GoY) +
                       "50" +
                       "B8" + AlignDWORD(GetAddress("KO_PTR_ROUTE_START")) +
                       "FFD0" +
                       "61" +
                       "C3");
            }
        }

        public void StopRouteEvent()
        {
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GO_X"), GetX());
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GO_Y"), GetY());

            ExecuteRemoteCode("60" +
                   "8B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) +
                   "B8" + AlignDWORD(GetAddress("KO_PTR_ROUTE_STOP")) +
                   "FFD0" +
                   "61" +
                   "C3");
        }

        public void SetCoordinate(int GoX, int GoY, int ExecutionAfterWait = 0)
        {
            if (GoX <= 0 || GoY <= 0) return;
            if (GoX == GetX() && GoY == GetY()) return;

            int StartX = GetX();
            int StartY = GetY();

            int DistanceX = Math.Abs(GoX - StartX);
            int DistanceY = Math.Abs(GoY - StartY);

            int DirectionX = 1; int DirectionY = 1;

            if (StartX > GoX)
                DirectionX = -1;

            if (StartY > GoY)
                DirectionY = -1;

            int Distance = Convert.ToInt32(Math.Sqrt((Math.Pow(DistanceX, 2) + Math.Pow(DistanceY, 2))));

            for (int Step = 0; Step < Distance; Step++)
            {
                int AngleX = Convert.ToInt32(Math.Sqrt((Math.Pow(Step, 2) * Math.Pow(DistanceX, 2)) / (Math.Pow(DistanceX, 2) + Math.Pow(DistanceY, 2))));
                int AngleY = Convert.ToInt32(Math.Sqrt(Math.Pow(Step, 2) - Math.Pow(AngleX, 2)));

                int NextX = StartX + DirectionX * AngleX;
                int NextY = StartY + DirectionY * AngleY;

                SendPacket("06" + AlignDWORD(NextX * 10).Substring(0, 4) + AlignDWORD(NextY * 10).Substring(0, 4) + AlignDWORD(GetZ() * 10).Substring(0, 4) + "2B0003");
            }

            SendPacket("06" + AlignDWORD(GoX * 10).Substring(0, 4) + AlignDWORD(GoY * 10).Substring(0, 4) + AlignDWORD(GetZ() * 10).Substring(0, 4) + "2B0003");

            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_X"), GoX);
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_Y"), GoY);

            Write4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOVE_TYPE"), 2);
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GO_X"), GoX);
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GO_Y"), GoY);
            Write4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOVE"), 1);

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public int ReadAffectedSkill(int Skill)
        {
            int SkillBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_SKILL_BASE"));

            SkillBase = Read4Byte(SkillBase + 0x4);
            SkillBase = Read4Byte(SkillBase + GetAddress("KO_OFF_SKILL_SLOT"));

            for (int i = 1; i < Skill; i++)
                SkillBase = Read4Byte(SkillBase + 0x0);

            SkillBase = Read4Byte(SkillBase + 0x8);

            if (SkillBase > 0)
                return Read4Byte(SkillBase + 0x0);

            SkillBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_SKILL_BASE"));

            SkillBase = Read4Byte(SkillBase + 0x4);
            SkillBase = Read4Byte(SkillBase + GetAddress("KO_OFF_SKILL_SLOT"));

            for (int i = 1; i < Skill; i++)
                SkillBase = Read4Byte(SkillBase + 0x0);

            SkillBase = Read4Byte(SkillBase + 0x8);

            if (SkillBase > 0)
                return Read4Byte(SkillBase + 0x0);

            return 0;
        }

        public bool IsBuffAffected()
        {
            string[] Skills = { "606", "615", "624", "633", "642", "654", "655", "657", "670", "675" };

            for (int i = 0; i < Skills.Length; i++)
                if (IsSkillRightAffected(Skills[i])) return true;

            return false;
        }

        public bool IsAcAffected()
        {
            string[] Skills = { "603", "612", "621", "630", "639", "651", "660", "674" };

            for (int i = 0; i < Skills.Length; i++)
                if (IsSkillRightAffected(Skills[i])) return true;

            return false;
        }

        public bool IsMindAffected()
        {
            string[] Skills = { "609", "627", "636", "645" };

            for (int i = 0; i < Skills.Length; i++)
                if (IsSkillRightAffected(Skills[i])) return true;

            return false;
        }

        public bool IsSkillAffected(int Skill)
        {
            for (int i = 1; i < 20; i++)
            {
                if (ReadAffectedSkill(i) == Skill) return true;
            }

            return false;
        }

        protected bool IsSkillRightAffected(string Skill)
        {
            string SkillRight = Skill.Substring(Skill.Length - 3, 3);

            for (int i = 1; i < 20; i++)
            {
                int ExistSkill = ReadAffectedSkill(i);
                if (ExistSkill == 0) continue;

                string ExistSkillRight = ExistSkill.ToString().Substring(ExistSkill.ToString().Length - 3, 3);

                if (ExistSkillRight == SkillRight) return true;
            }

            return false;
        }

        protected int GetInventoryItemDurability(int SlotId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEM_BASE"));
            int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEM_SLOT") + (4 * SlotId)));
            return Read4Byte(Length + 0x74);
        }

        protected void SetInventoryItemDurability(int SlotId, int iDurability)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEM_BASE"));
            int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEM_SLOT") + (4 * SlotId)));
            Write4Byte(Length + 0x74, iDurability);
        }

        protected bool IsInventoryFull()
        {
            bool Full = true;
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEM_BASE"));

            for (int i = 14; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEM_SLOT") + (4 * i)));

                if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == 0)
                    return false;
            }

            return Full;
        }

        protected int GetInventoryAvailableSlotCount()
        {
            int Count = 0;
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEM_BASE"));

            for (int i = 14; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEM_SLOT") + (4 * i)));

                if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == 0)
                    Count++;
            }

            return Count;
        }

        protected int GetInventoryItemCount(int ItemId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEM_BASE"));

            for (int i = 0; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEM_SLOT") + (4 * i)));
                if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == ItemId)
                    return Read4Byte(Length + 0x70);
            }

            return 0;
        }

        protected int GetInventoryItemId(int SlotId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEM_BASE"));
            int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEM_SLOT") + (4 * SlotId)));
            return Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C));
        }


        protected string GetInventoryItemName(int SlotId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEM_BASE"));
            int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEM_SLOT") + (4 * SlotId)));

            int Name = Read4Byte(Length + 0x68);
            int NameLength = Read4Byte(Name + 0x1C);

            if (NameLength > 15)
                return ReadString(Read4Byte(Name + 0xC), NameLength);
            else
                return ReadString(Name + 0xC, NameLength);
        }

        protected int GetInventoryEmptySlot()
        {
            for (int i = 14; i < 42; i++)
            {
                if (GetInventoryItemId(i) == 0)
                    return i;
            }
            return -1;
        }

        protected bool IsInventorySlotEmpty(int Slot)
        {
            if (GetInventoryItemId(Slot) > 0)
                return false;

            return true;
        }

        protected bool IsInventoryItemExist(int ItemId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEM_BASE"));

            for (int i = 14; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEM_SLOT") + (4 * i)));

                if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == ItemId)
                    return true;
            }

            return false;
        }

        protected int GetInventoryItemSlot(int ItemId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEM_BASE"));

            for (int i = 14; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEM_SLOT") + (4 * i)));

                if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == ItemId)
                    return i;
            }

            return -1;
        }

        public int GetAllInventoryItem(ref List<Inventory> refInventory)
        {
            for (int i = 0; i < 42; i++)
            {
                int Item = GetInventoryItemId(i);

                if (Item > 0)
                {
                    Inventory Inventory = new Inventory();

                    Inventory.Id = Item;

                    Item ItemData = Storage.ItemCollection.Find(x => x.Id == Item);

                    if (ItemData != null)
                        Inventory.Name = ItemData.Name;
                    else
                        Inventory.Name = GetInventoryItemName(i);

                    refInventory.Add(Inventory);
                }
            }

            return refInventory.Count;
        }

        public bool IsNeedRepair()
        {
            for (int i = 0; i < 14; i++)
            {
                switch (i)
                {
                    case 1:
                    case 4:
                    case 6:
                    case 8:
                    case 10:
                    case 12:
                    case 13:
                        {
                            if (IsInventorySlotEmpty(i) == false && GetInventoryItemDurability(i) == 0)
                                return true;
                        }
                        break;
                }
            }

            return false;
        }

        public void RepairAllEquipment(int NpcId, int ExecutionAfterWait = 1250)
        {
            for (int i = 0; i < 14; i++)
            {
                switch (i)
                {
                    case 1:
                    case 4:
                    case 6:
                    case 8:
                    case 10:
                    case 12:
                    case 13:
                        {
                            if (IsInventorySlotEmpty(i) == false)
                            {
                                SendPacket("3B01" + AlignDWORD(i).Substring(0, 2) + AlignDWORD(NpcId).Substring(0, 4) + AlignDWORD(GetInventoryItemId(i)));
                                Thread.Sleep(500);

                                if (Convert.ToBoolean(GetControl("MiningEnable")) == true)
                                    SetInventoryItemDurability(i, 30000);
                            }

                        }
                        break;
                }
            }

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public bool IsNeedSupply(ref List<Supply> refSupply, bool force)
        {
            foreach (var x in Storage.SupplyCollection)
            {
                if (Convert.ToBoolean(GetControl(x.Control)) == true)
                {
                    string ItemName = x.ControlItem != null ? GetControl(x.ControlItem) : x.ItemConst;
                    Item Item = Storage.ItemCollection.Find(y => y.Name == ItemName);

                    if (Item == null)
                        continue;

                    if((Item.Name == "Arrow" && GetInventoryItemCount(Item.Id) <= 30) 
                        || (Item.Name != "Arrow" && GetInventoryItemCount(Item.Id) <= 1 && GetInventoryItemCount(Item.Id) < Convert.ToInt32(GetControl(x.ControlCount)))
                        || (force))
                    {
                        Supply Supply = new Supply();

                        Supply.Item = Item;

                        Supply.Count = Convert.ToInt32(GetControl(x.ControlCount));

                        refSupply.Add(Supply);
                    }
                }
            }

            return refSupply.Count > 0 ? true : false;
        }

        protected bool UseMinorHealing(int TargetId)
        {
            if (GetMp() >= 15)
            {
                SendPacket("3103" +
                    AlignDWORD(Convert.ToInt32(GetClass().ToString() + "705")).Substring(0, 6) +
                    "00" +
                    AlignDWORD(GetId()).Substring(0, 4) +
                    AlignDWORD(TargetId).Substring(0, 4) +
                    "0000000000000000000000000000");

                return true;
            }

            return false;
        }

        public void SelectTarget(int TargetId)
        {
            if (TargetId > 0)
                SendPacket("22" + AlignDWORD(TargetId).Substring(0, 4) + "01");

            Write4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOB"), TargetId);
        }

        public bool IsMoving()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOVE")) == 1 || GetState() == 2 ? true : false;
        }

        public void SendPacket(byte[] Packet, int ExecutionAfterWait = 0)
        {
            if (IsDisconnected()) return;

            IntPtr PacketPtr = VirtualAllocEx(_Handle, IntPtr.Zero, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            WriteProcessMemory(_Handle, PacketPtr, Packet, Packet.Length, 0);
            ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_PKT")) + "68" + AlignDWORD(Packet.Length) + "68" + AlignDWORD(PacketPtr) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD7C605" + AlignDWORD(GetAddress("KO_PTR_PKT") + 0xC5) + "0061C3");
            VirtualFreeEx(_Handle, PacketPtr, 0, MEM_RELEASE);

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public void ForwardPacketToAll(String Packet)
        {
            foreach (Client ClientData in Storage.ClientCollection)
            {
                if (ClientData == null) continue;
                if (GetProcessId() == ClientData.GetProcessId()) continue;

                if (ClientData.GetPhase() == Processor.EPhase.Playing || ClientData.GetPhase() == Processor.EPhase.Warping)
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false && Storage.FollowedClient.GetZoneId() == ClientData.GetZoneId())
                        ClientData.SendPacket(Packet);
                }
            }
        }

        public void ForwardPacketToAllFollower(String Packet)
        {
            if (Storage.FollowedClient == null) return;
            if (Storage.FollowedClient.GetProcessId() != GetProcessId()) return;
            if (GetAction() != EAction.None) return;

            foreach (Client ClientData in Storage.ClientCollection)
            {
                if (ClientData == null || Storage.FollowedClient == null) continue;
                if (Storage.FollowedClient.GetProcessId() == ClientData.GetProcessId()) continue;

                if (ClientData.GetPhase() == Processor.EPhase.Playing || ClientData.GetPhase() == Processor.EPhase.Warping)
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false && Storage.FollowedClient.GetZoneId() == ClientData.GetZoneId())
                    {
                        ClientData.SendPacket(Packet);
                    }
                }
            }
        }

        public void SendPacket(String Packet, int ExecutionAfterWait = 0)
        {
            SendPacket(StringToByte(Packet), ExecutionAfterWait);
        }

        protected void SetCharacterState(UInt16 State)
        {
            /*
             * 0 = Idle
             * 1 = Walking
             * 2 = Running
             */

            ExecuteRemoteCode("60" +
                          "6A00" + 
                          "6A" + AlignDWORD(State).Substring(0, 2) +
                          "B9" + AlignDWORD(GetAddress("KO_PTR_DLG")) +
                          "8B09B8" +
                          AlignDWORD(GetAddress("KO_PTR_STATE")) +
                          "FFD061C3");
        }

        protected void SendAttackPacket(int TargetId)
        {
            SendPacket("080101" + AlignDWORD(TargetId).Substring(0, 4) + "FF00000000");
        }

        public bool UseSkill(Skill Skill, int TargetId = 0)
        {
            switch (GetJob())
            {
                case "Warrior": return UseWarriorSkill(Skill, TargetId);
                case "Rogue": return UseRogueSkill(Skill, TargetId);
                case "Priest": return UsePriestSkill(Skill, TargetId);
                case "Mage": return UseMageSkill(Skill, TargetId);
            }

            return false;
        }

        private void UseAttackSkill(int SkillId, int TargetId = 0)
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                SetCharacterState(0);

            SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");
        }

        public void UseSelfSkill(int SkillId, int TargetId = 0)
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                SetCharacterState(0);

            SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");
        }

        private void UseTargetSkill(int SkillId, int TargetId)
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                SetCharacterState(0);

            SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000F00");
            Thread.Sleep(10);
            SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");
        }

        private void UseTargetAreaSkill(int SkillId, int TargetX, int TargetY, int TargetZ)
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                SetCharacterState(0);

            SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "00000000000000000F00");
            Thread.Sleep(10);
            SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "000000000000");
        }

        public bool IsSelectableTarget(int TargetId = 0)
        {
            
            TargetId = TargetId > 0 ? TargetId : GetTargetId();
            if (TargetId == 0) return false;
            int TargetBase = GetTargetBase(TargetId);
            if (TargetBase == 0) return false;
            if (Read4Byte(TargetBase + GetAddress("KO_OFF_MAX_HP")) != 0 && Read4Byte(TargetBase + GetAddress("KO_OFF_HP")) == 0) return false;
            if (CoordinateDistance(GetX(), GetY(), (int)Math.Round(ReadFloat(TargetBase + GetAddress("KO_OFF_X"))), (int)Math.Round(ReadFloat(TargetBase + GetAddress("KO_OFF_Y")))) > Convert.ToInt32(GetControl("AttackDistance"))) return false;
            if (Convert.ToBoolean(GetControl("TargetOpponentNation")) == false && Read4Byte(TargetBase + GetAddress("KO_OFF_NATION")) != 0) return false;
            if (Convert.ToBoolean(GetControl("TargetOpponentNation")) == true && Read4Byte(TargetBase + GetAddress("KO_OFF_NATION")) >= 3) return false;
            //if (ReadByte(TargetBase + GetAddress("KO_OFF_STATE")) == 10 || ReadByte(TargetBase + GetAddress("KO_OFF_STATE")) == 11) return false;
            return true;
        }

        public bool IsAttackableTarget(int TargetId = 0)
        {
            TargetId = TargetId > 0 ? TargetId : GetTargetId();
            if (TargetId == 0) return false;
            int TargetBase = GetTargetBase(TargetId);
            if (TargetBase == 0) return false;
            if (Read4Byte(TargetBase + GetAddress("KO_OFF_HP")) == 0) return false;
            if (CoordinateDistance(GetX(), GetY(), (int)Math.Round(ReadFloat(TargetBase + GetAddress("KO_OFF_X"))), (int)Math.Round(ReadFloat(TargetBase + GetAddress("KO_OFF_Y")))) > Convert.ToInt32(GetControl("AttackDistance"))) return false;
            if (Convert.ToBoolean(GetControl("TargetOpponentNation")) == false && Read4Byte(TargetBase + GetAddress("KO_OFF_NATION")) != 0) return false;
            if (Convert.ToBoolean(GetControl("TargetOpponentNation")) == true && Read4Byte(TargetBase + GetAddress("KO_OFF_NATION")) >= 3) return false;
            //if (ReadByte(TargetBase + GetAddress("KO_OFF_STATE")) == 10 || ReadByte(TargetBase + GetAddress("KO_OFF_STATE")) == 11) return false;
            return true;
        }

        public bool UseWarriorSkill(Skill Skill, int TargetId = 0)
        {
            if (IsCharacterAvailable() == false) return false;
            if (GetHp() == 0) return false;
            if (Skill.Mana > 0 && GetMp() < Skill.Mana) return false;
            if (Skill.Type == 1 && TargetId <= 0) return false;

            int SkillId = Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"));

            if (Skill.Type == 2 && (TargetId == 0 || TargetId == GetId()) && IsSkillAffected(SkillId)) return false;
            if (Skill.Item > 0 && IsInventoryItemExist(Skill.Item) == false) return false;
            if (Skill.ItemCount > 0 && GetInventoryItemCount(Skill.Item) < Skill.ItemCount) return false;

            switch (Skill.Name)
            {
                case "Stroke":
                case "Slash":
                case "Crash":
                case "Piercing":
                case "Whipping":
                case "Hash":
                case "Hoodwink":
                case "Shear":
                case "Leg Cutting":
                case "Pierce":
                case "Carwing":
                case "Sever":
                case "Prick":
                case "Multiple Shock":
                case "Cleave":
                case "Mangling":
                case "Thrust":
                case "Sword Aura":
                case "Sword Dancing":
                case "Howling Sword":
                case "Blooding":
                case "Hell Blade":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (Convert.ToBoolean(GetControl("RAttack")) == true)
                        {
                            SendAttackPacket(TargetId);
                            Thread.Sleep(250);
                        }

                        UseAttackSkill(SkillId, TargetId);

                        return true;
                    }

                case "Gain":
                    {
                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "0000000000000000000000001400");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "000000000000000000000000");

                        return true;
                    }

                case "Sprint":
                case "Defense":
                    {
                        UseSelfSkill(SkillId, GetId());
                        return true;
                    }
            }

            return false;
        }

        public bool UseRogueSkill(Skill Skill, int TargetId = 0)
        {
            if (IsCharacterAvailable() == false) return false;
            if (GetHp() == 0) return false;
            if (Skill.Mana > 0 && GetMp() < Skill.Mana) return false;
            if (Skill.Type == 1 && TargetId <= 0) return false;

            int SkillId = Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"));

            if (Skill.Type == 2 && (TargetId == 0 || TargetId == GetId()) && IsSkillAffected(SkillId)) return false;
            if (Skill.Item > 0 && IsInventoryItemExist(Skill.Item) == false) return false;
            if (Skill.ItemCount > 0 && GetInventoryItemCount(Skill.Item) < Skill.ItemCount) return false;

            int TargetX = GetTargetX(); int TargetY = GetTargetY(); int TargetZ = GetTargetZ();

            switch (Skill.Name)
            {
                case "Archery":
                case "Through Shot":
                case "Fire Arrow":
                case "Poison Arrow":
                case "Guided Arrow":
                case "Perfect Shot":
                case "Fire Shot":
                case "Poison Shot":
                case "Arc Shot":
                case "Explosive Shot":
                case "Viper":
                case "Shadow Shot":
                case "Shadow Hunter":
                case "Ice Shot":
                case "Lightning Shot":
                case "Dark Pursuer":
                case "Blow Arrow":
                case "Blinding Strafe":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000D00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000010000000000");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000001000000000000000000");

                        return true;
                    }

                case "Counter Strike":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000A00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000010000000000");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000001000000000000000000");

                        return true;
                    }

                case "Multiple Shot":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000D00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000010000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0100000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000002000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0200000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000003000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0300000000000000");

                        return true;
                    }

                case "Arrow Shower":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000F00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000010000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0100000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000002000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0200000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000003000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0300000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000004000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0400000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000005000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0500000000000000");

                        return true;
                    }

                case "Süper Archer":
                    {
                        if (IsAttackableTarget() == false) return false;
                        if (CoordinateDistance(GetX(), GetY(), TargetX, TargetY) > 1) return false;

                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000D00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000010000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0100000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000002000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0200000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000003000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0300000000000000");

                        if (IsAttackableTarget() == false) return false;
                        if (CoordinateDistance(GetX(), GetY(), TargetX, TargetY) > 1) return false;

                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000F00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000010000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0100000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000002000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0200000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000003000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0300000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000004000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0400000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000005000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "0200000000009BFF0500000000000000");

                        return true;
                    }

                case "Stroke":
                case "Stab":
                case "Stab2":
                case "Jab":
                case "Pierce":
                case "Shock":
                case "Thrust":
                case "Cut":
                case "Spike":
                case "Blody Beast":
                case "Blinding":
                case "Blood Drain":
                case "Vampiric Touch":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        if (Skill.Name == "Blood Drain" || Skill.Name == "Vampiric Touch")
                        {
                            SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000001000");
                            Thread.Sleep(10);
                            SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");
                        }
                        else
                        {
                            if (Convert.ToBoolean(GetControl("RAttack")) == true)
                            {
                                SendAttackPacket(TargetId);
                                Thread.Sleep(250);
                            }

                            UseAttackSkill(SkillId, TargetId);
                        }

                        return true;
                    }

                case "Sprint":
                case "Light Feet":
                case "Evade":
                case "Safety":
                    {
                        if (Skill.Name == "Evade" && IsSkillAffected(Int32.Parse(GetClass().ToString() + "730"))) return false;
                        if (Skill.Name == "Safety" && IsSkillAffected(Int32.Parse(GetClass().ToString() + "710"))) return false;

                        UseSelfSkill(SkillId, GetId());

                        return true;
                    }

                case "Hide":
                    {
                        UseTargetSkill(SkillId, GetId());

                        return true;
                    }

                case "Stealth":
                    {
                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "00000000000000000000000000001E00");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "000000000000000000000000");

                        return true;
                    }

                case "Blood of wolf":
                    {
                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3106" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "000000000000000000000000");
                        Thread.Sleep(10);
                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(GetX()).Substring(0, 4) + AlignDWORD(GetZ()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + "00000000000000001100");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(GetX()).Substring(0, 4) + AlignDWORD(GetZ()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + "000000000000");

                        return true;
                    }
                case "Swift":
                    {
                        UseTargetSkill(SkillId, TargetId > 0 ? TargetId : GetId());

                        return true;
                    }
                case "Lupine Eyes":
                    {
                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "00000000000000000000000000001400");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "000000000000000000000000");

                        return true;
                    }
            }

            return false;
        }

        public bool UsePriestSkill(Skill Skill, int TargetId = 0)
        {
            if (IsCharacterAvailable() == false) return false;
            if (GetHp() == 0) return false;
            if (Skill.Mana > 0 && GetMp() < Skill.Mana) return false;
            if (Skill.Type == 1 && TargetId <= 0) return false;

            int SkillId = Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"));

            if (Skill.Type == 2 && (TargetId == 0 || TargetId == GetId()) && IsSkillAffected(SkillId)) return false;
            if (Skill.Item > 0 && IsInventoryItemExist(Skill.Item) == false) return false;
            if (Skill.ItemCount > 0 && GetInventoryItemCount(Skill.Item) < Skill.ItemCount) return false;

            int TargetX = GetTargetX(); int TargetY = GetTargetY(); int TargetZ = GetTargetZ();

            switch (Skill.Name)
            {
                case "Stroke":
                case "Holy Attack":
                case "Wrath":
                case "Wield":
                case "Harsh":
                case "Collision":
                case "Collapse":
                case "Shuddering":
                case "Ruin":
                case "Hellish":
                case "Tilt":
                case "Bloody":
                case "Raving Edge":
                case "Hades":
                case "Judgement":
                case "Helis":
                    {
                        if (IsAttackableTarget() == false) return false;
                        if (CoordinateDistance(GetX(), GetY(), TargetX, TargetY) > 13) return false;

                        if (Convert.ToBoolean(GetControl("RAttack")) == true)
                        {
                            SendAttackPacket(TargetId);
                            Thread.Sleep(250);
                        }

                        UseAttackSkill(SkillId, TargetId);

                        return true;
                    }

                case "Malice":
                case "Confusion":
                case "Slow":
                case "Reverse Life":
                case "Sleep Wing":
                case "Parasite":
                case "Massive":
                    {
                        if (IsAttackableTarget() == false) return false;
                        if (CoordinateDistance(GetX(), GetY(), TargetX, TargetY) > 30) return false;

                        UseTargetSkill(SkillId, TargetId);

                        return true;
                    }

                case "Light Healing":
                case "Tiny Healing":
                    {
                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000001300");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");

                        return true;
                    }

                case "Superior Healing":
                case "Massive Healing":
                case "Great Healing":
                case "Major Healing":
                case "Healing":
                case "Minor Healing":
                    {
                        UseTargetSkill(SkillId, TargetId > 0 ? TargetId : GetId());

                        return true;
                    }

                case "Group Massive Healing":
                case "Group Complete Healing":
                    {
                        UseTargetAreaSkill(SkillId, GetX(), GetY(), GetZ());

                        return true;
                    }

                case "Superioris":
                case "Insensibility Guard":
                case "Imposingness":
                case "Insensibility Peel":
                case "Massiveness":
                case "Heapness":
                case "Undying":
                case "Insensibility Protector":
                case "Fresh Mind":
                case "Mightness":
                case "Insensibility Barrier":
                case "Calm Mind":
                case "Hardness":
                case "Insensibility Shield":
                case "Bright Mind":
                case "Strong":
                case "Insensibility Armor":
                case "Brave":
                case "Insensibility Shell":
                case "Resist All":
                case "Grace":
                case "Insensibility Skin":
                case "Strength":
                case "Cure Curse":
                case "Cure Disease":
                    {
                        UseTargetSkill(SkillId, TargetId > 0 ? TargetId : GetId());

                        return true;
                    }

                case "Sleep Carpet":
                case "Torment":
                    {
                        if (IsAttackableTarget() == false) return false;

                        UseTargetAreaSkill(SkillId, GetTargetX(), GetTargetY(), GetTargetZ());

                        return true;
                    }

                case "Prayer of Cronos":
                case "Prayer of god’s power":
                case "Blasting":
                case "Wildness":
                case "Eruption":
                    {
                        UseSelfSkill(SkillId, GetId());

                        return true;
                    }
            }

            return false;
        }

        public bool UseMageSkill(Skill Skill, int TargetId = 0)
        {
            if (IsCharacterAvailable() == false) return false;
            if (GetHp() == 0) return false;
            if (Skill.Mana > 0 && GetMp() < Skill.Mana) return false;
            if (Skill.Type == 1 && TargetId <= 0) return false;

            int SkillId = Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"));

            if (Skill.Type == 2 && (TargetId == 0 || TargetId == GetId()) && IsSkillAffected(SkillId)) return false;
            if (Skill.Item > 0 && IsInventoryItemExist(Skill.Item) == false) return false;
            if (Skill.ItemCount > 0 && GetInventoryItemCount(Skill.Item) < Skill.ItemCount) return false;

            int TargetX = GetTargetX(); int TargetY = GetTargetY(); int TargetZ = GetTargetZ();

            switch (Skill.Name)
            {
                case "Burn":
                case "Ignition":
                case "Specter Of Fire":
                case "Manes Of Fire":
                case "Manes Of Ice":
                case "Specter Of Ice":
                case "Specter Of Thunder":
                case "Incineration":
                case "Manes Of Thunder":
                case "Charge":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000A00");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");

                        return true;
                    }

                case "Fire Blast":
                case "Blaze":
                case "Freeze":
                case "Chill":
                case "Solid":
                case "Hell Fire":
                case "Pillar Of Fire":
                case "Fire Thorn":
                case "Fire Impact":
                case "Frostbite":
                case "Ice Comet":
                case "Ice Impact":
                case "Ice Blast":
                case "Counter Spell":
                case "Lightining":
                case "Static Hemispher":
                case "Thunder":
                case "Thunder Blast":
                case "Static Thorn":
                    {
                        if (IsAttackableTarget() == false) return false;

                        UseTargetSkill(SkillId, TargetId);

                        return true;
                    }

                case "Fire Spear":
                case "Fire Ball":
                case "Ice Orb":
                case "Static Orb":
                case "Thunder Impact":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000F00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "9BFF0000000000000000");

                        return true;
                    }

                case "Stroke":
                case "Flame Blade":
                case "Frozen Blade":
                case "Charged Blade":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (Convert.ToBoolean(GetControl("RAttack")) == true)
                        {
                            SendAttackPacket(TargetId);
                            Thread.Sleep(250);
                        }

                        UseAttackSkill(SkillId, TargetId);

                        return true;
                    }

                case "Ice Burst":
                case "Fire Burst":
                case "Thunder Burst":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                            SetCharacterState(0);

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "00000000000000000F00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "00000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "9BFF0000000000000000");

                        return true;
                    }

                case "Inferno":
                case "Blizzard":
                case "Thundercloud":
                case "Super Nova":
                case "Frost Nova":
                case "Static Nova":
                case "Meteor Fall":
                case "Ice Storm":
                case "Chain Lightning":
                    {
                        if (IsAttackableTarget() == false) return false;

                        UseTargetAreaSkill(SkillId, TargetX, TargetY, TargetZ);

                        return true;
                    }

                case "Resist Fire":
                case "Endure Fire":
                case "Immunity Fire":
                case "Resist Cold":
                case "Frozen Armor":
                case "Endure Cold":
                case "Immunity Cold":
                case "Resist Lightning":
                case "Endure Lightning":
                case "Immunity Lightning":
                case "Summon Friend":
                    {
                        UseTargetSkill(SkillId, TargetId > 0 ? TargetId : GetId());

                        return true;
                    }
            }

            return false;
        }



        public int GetWarehouseItemId(int Slot)
        {
            int WarehouseBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + 0x204); //0x204 KO_OFF_BANKB
            int WarehouseSlot = Read4Byte(WarehouseBase + (0x128 + (4 * Slot))); //0x128 KO_OFF_BANKS
            int ItemId = Read4Byte(Read4Byte(WarehouseSlot + 0x68));
            int ItemExt = Read4Byte(Read4Byte(WarehouseSlot + 0x6C));
            return ItemId + ItemExt;
        }

        public string GetWarehouseItemName(int Slot)
        {
            int WarehouseBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + 0x204); //0x204 KO_OFF_BANKB
            int WarehouseSlot = Read4Byte(WarehouseBase + (0x128 + (4 * Slot))); //0x128 KO_OFF_BANKS
            int ItemId = Read4Byte(WarehouseSlot + 0x68);
            int ItemNameLen = Read4Byte(ItemId + 0x1C);

            if (ItemNameLen > 15)
                return ReadString(Read4Byte(ItemId + 0xC), ItemNameLen);
            else
                return ReadString(ItemId + 0xC, ItemNameLen);
        }

        public int GetWarehouseItemCount(int Slot)
        {
            int WarehouseBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + 0x204); //0x204 KO_OFF_BANKB
            int WarehouseSlot = Read4Byte(WarehouseBase + (0x128 + (4 * Slot))); //0x128 KO_OFF_BANKS
            int ItemCount = Read4Byte(WarehouseSlot + 0x70);
            return ItemCount;
        }

        public void WarehouseItemCheckOut(Item Item, Npc Npc, int Count, int ExecutionAfterWait = 0)
        {
            int InventoryItemSlot = GetInventoryItemSlot(Item.Id);

            InventoryItemSlot = InventoryItemSlot != -1 ? InventoryItemSlot : GetInventoryEmptySlot();

            if (InventoryItemSlot == -1) return;

            SendPacket("2001" + AlignDWORD(Npc.RealId).Substring(0, 4) + "FFFFFFFF");
            Thread.Sleep(125);
            SendPacket("4501" + AlignDWORD(Npc.RealId).Substring(0, 4));
            Thread.Sleep(2500);

            for (int i = 0; i <= 191; i++)
            {
                if (GetWarehouseItemId(i) != Item.Id)
                    continue;

                if (GetWarehouseItemCount(i) < Count)
                    return;

                int Slot = 0;
                int Page = 0;

                if (i >= 0 && i <= 23)
                {
                    Page = 0;
                    Slot = i;
                }
                else if (i >= 24 && i <= 47)
                {
                    Page = 1;
                    Slot = i - 24;
                }
                else if (i >= 48 && i <= 71)
                {
                    Page = 2;
                    Slot = i - 48;
                }
                else if (i >= 72 && i <= 95)
                {
                    Page = 3;
                    Slot = i - 72;
                }
                else if (i >= 96 && i <= 119)
                {
                    Page = 4;
                    Slot = i - 96;
                }
                else if (i >= 120 && i <= 143)
                {
                    Page = 5;
                    Slot = i - 120;
                }
                else if (i >= 144 && i <= 167)
                {
                    Page = 6;
                    Slot = i - 144;
                }
                else if (i >= 168 && i <= 191)
                {
                    Page = 7;
                    Slot = i - 168;
                }

                SendPacket("4503" + AlignDWORD(Npc.RealId).Substring(0, 4) + AlignDWORD(Item.Id) + AlignDWORD(Page).Substring(0, 2) + AlignDWORD(Slot).Substring(0, 2) + AlignDWORD(InventoryItemSlot - 14).Substring(0, 2) + AlignDWORD(Count).Substring(0, 4) + "0000");
            }

            Thread.Sleep(10);
            SendPacket("2001" + AlignDWORD(Npc.RealId).Substring(0, 4) + "FFFFFFFF");
            Thread.Sleep(50);
            SendPacket("6A02");

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public void DeleteItem(int Slot, int ItemId)
        {
            SendPacket("3F02" + AlignDWORD(Slot - 14).Substring(0, 2) + AlignDWORD(ItemId));
            Thread.Sleep(250);
            SendPacket("6A02");
        }

        public void BuyItem(Item item, int npcId, int npcType, int Count, int ExecutionAfterWait = 0)
        {
            if (Count == 0) return;

            int InventoryItemSlot = GetInventoryItemSlot(item.Id);

            InventoryItemSlot = InventoryItemSlot != -1 ? InventoryItemSlot : GetInventoryEmptySlot();

            if (InventoryItemSlot == -1) return;

            string ItemCount = item.BuyPacketCountSize == 0 ? AlignDWORD(Count) : AlignDWORD(Count).Substring(0, item.BuyPacketCountSize);

            if (npcType == 0) // 0 = Sunderies - 1 = Potion
                SendPacket("2101" + "18E40300" + AlignDWORD(npcId).Substring(0, 4) + "01" + AlignDWORD(item.Id) + AlignDWORD(InventoryItemSlot - 14).Substring(0, 2) + ItemCount + item.BuyPacketEnd);
            else
                SendPacket("2101" + "48DC0300" + AlignDWORD(npcId).Substring(0, 4) + "01" + AlignDWORD(item.Id) + AlignDWORD(InventoryItemSlot - 14).Substring(0, 2) + ItemCount + item.BuyPacketEnd);

            Thread.Sleep(50);
            SendPacket("6A02");

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public void SellItem(int itemId, int npcId, int npcType, int count, int executionAfterWait = 0)
        {
            if (count == 0) return;

            int InventoryItemSlot = GetInventoryItemSlot(itemId);
            int InventoryItemCount = GetInventoryItemCount(itemId);

            if (InventoryItemCount == 0 || InventoryItemSlot == -1 || count > InventoryItemCount) return;

            if (npcType == 0) // 0 = Sunderies - 1 = Potion
                SendPacket("2102" + "18E40300" + AlignDWORD(npcId).Substring(0, 4) + "01" + AlignDWORD(itemId) + AlignDWORD(InventoryItemSlot - 14).Substring(0, 2) + AlignDWORD(count).Substring(0, 4));
            else
                SendPacket("2102" + "48DC0300" + AlignDWORD(npcId).Substring(0, 4) + "01" + AlignDWORD(itemId) + AlignDWORD(InventoryItemSlot - 14).Substring(0, 2) + AlignDWORD(count).Substring(0, 4));

            Thread.Sleep(50);
            SendPacket("6A02");

            if (executionAfterWait > 0)
                Thread.Sleep(executionAfterWait);
        }

        public int SearchTarget(Int32 Address, ref List<TargetInfo> OutTargetList)
        {
            int Ebp = Read4Byte(Read4Byte(GetAddress("KO_PTR_FLDB")) + Address);
            int Fend = Read4Byte(Read4Byte(Ebp + 0x4) + 0x4);
            int Esi = Read4Byte(Ebp);
            int Tick = Environment.TickCount;

            while (Esi != Ebp && Environment.TickCount - Tick < 50)
            {
                int Base = 0;

                if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                    Base = Read4Byte(Esi + 0x14);
                else
                    Base = Read4Byte(Esi + 0x10);

                if (Base == 0) break;

                if (OutTargetList.Any(x => x.Base == Base) == false)
                {
                    TargetInfo Target = new TargetInfo();

                    int NameLen = Read4Byte(Base + GetAddress("KO_OFF_NAME_LENGTH"));

                    Target.Base = Base;

                    Target.Id = Read4Byte(Base + GetAddress("KO_OFF_ID"));

                    if (NameLen > 15)
                        Target.Name = ReadString(Read4Byte(Base + GetAddress("KO_OFF_NAME")), NameLen);
                    else
                        Target.Name = ReadString(Base + GetAddress("KO_OFF_NAME"), NameLen);

                    Target.Nation = Read4Byte(Base + GetAddress("KO_OFF_NATION"));

                    Target.State = ReadByte(Base + GetAddress("KO_OFF_STATE"));
                    Target.X = (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_X")));
                    Target.Y = (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_Y")));

                    OutTargetList.Add(Target);
                }

                int Eax = Read4Byte(Esi + 0x8);

                if (Eax != Fend)
                {
                    while (Read4Byte(Eax) != Fend && Environment.TickCount - Tick < 50)
                        Eax = Read4Byte(Eax);

                    Esi = Eax;
                }
                else
                {
                    Eax = Read4Byte(Esi + 0x4);

                    while (Esi == Read4Byte(Eax + 0x8) && Environment.TickCount - Tick < 50)
                    {
                        Esi = Eax;
                        Eax = Read4Byte(Eax + 0x4);
                    }

                    if (Read4Byte(Esi + 0x8) != Eax)
                        Esi = Eax;
                }
            }

            return OutTargetList.Count;
        }

        public int SearchMob(ref List<TargetInfo> OutTargetList)
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                return SearchTarget(0x28, ref OutTargetList);

            return SearchTarget(0x34, ref OutTargetList);
        }

        public int SearchPlayer(ref List<TargetInfo> OutTargetList)
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                return SearchTarget(0x30, ref OutTargetList);

            return SearchTarget(0x40, ref OutTargetList);
        }

        public int GetTargetBase(int TargetId = 0)
        {
            TargetId = TargetId > 0 ? TargetId : GetTargetId();

            if (TargetId > 0)
            {
                if (TargetId > 9999)
                    return GetMobBase(TargetId);
                else
                    return GetPlayerBase(TargetId);
            }

            return 0;
        }

        public int GetMobBase(int MobId)
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
            {
                IntPtr Addr = VirtualAllocEx(_Handle, IntPtr.Zero, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_FLDB")) + "6A0168" + AlignDWORD(MobId) + "BF" + AlignDWORD(GetAddress("KO_PTR_FMBS")) + "FFD7A3" + AlignDWORD(Addr) + "61C3");
                int Base = Read4Byte(Addr);
                VirtualFreeEx(_Handle, Addr, 0, MEM_RELEASE);
                return Base;
            }
            else
            {
                List<TargetInfo> SearchTargetList = new List<TargetInfo>();
                if (SearchMob(ref SearchTargetList) > 0)
                {
                    TargetInfo Target = SearchTargetList
                        .Find(x => x.Id == MobId);

                    if (Target != null)
                        return Target.Base;
                }
            }

            return 0;
        }

        public int GetPlayerBase(int PlayerId)
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
            {
                IntPtr Addr = VirtualAllocEx(_Handle, IntPtr.Zero, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_FLDB")) + "6A0168" + AlignDWORD(PlayerId) + "BF" + AlignDWORD(GetAddress("KO_PTR_FPBS")) + "FFD7A3" + AlignDWORD(Addr) + "61C3");
                int Base = Read4Byte(Addr);
                VirtualFreeEx(_Handle, Addr, 0, MEM_RELEASE);
                return Base;
            }
            else
            {
                List<TargetInfo> SearchTargetList = new List<TargetInfo>();
                if (SearchPlayer(ref SearchTargetList) > 0)
                {
                    TargetInfo Target = SearchTargetList
                        .Find(x => x.Id == PlayerId);

                    if (Target != null)
                        return Target.Base;
                }
            }

            return 0;
        }

        public void ClearAllMob()
        {
            List<TargetInfo> SearchTargetList = new List<TargetInfo>();
            if (SearchMob(ref SearchTargetList) > 0)
            {
                SearchTargetList.FindAll(x => x.Nation == 0)
                    .ForEach(x =>
                    {
                        WriteFloat(x.Base + GetAddress("KO_OFF_X"), 0);
                        WriteFloat(x.Base + GetAddress("KO_OFF_Y"), 0);
                    });
            }
        }

        public int GetPartyList(ref List<Player> PartyList)
        {
            int Base = Read4Byte(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_PARTY_BASE")) + GetAddress("KO_OFF_PARTY_LIST")));

            for (int i = 0; i <= GetPartyCount() - 1; i++)
            {
                Player Party = new Player();

                if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    Party.Id = Read4Byte(Base + 0x8);
                    Party.Class = Read4Byte(Base + 0x10);
                    Party.Hp = Read4Byte(Base + 0x18);
                    Party.MaxHp = Read4Byte(Base + 0x1C);
                    Party.Cure1 = Read4Byte(Base + 0x24);
                    Party.Cure2 = Read4Byte(Base + 0x25);
                    Party.Cure3 = Read4Byte(Base + 0x26);
                    Party.Cure4 = Read4Byte(Base + 0x27);

                    int MemberNameLen = Read4Byte(Base + 0x40);

                    if (MemberNameLen > 15)
                        Party.Name = ReadString(Read4Byte(Base + 0x30), MemberNameLen);
                    else
                        Party.Name = ReadString(Base + 0x30, MemberNameLen);
                }
                else
                {
                    Party.Id = Read4Byte(Base + 0x8);
                    Party.Class = Read4Byte(Base + 0x10);
                    Party.Hp = Read4Byte(Base + 0x14);
                    Party.MaxHp = Read4Byte(Base + 0x18);
                    Party.Cure1 = Read4Byte(Base + 0x24);
                    Party.Cure2 = Read4Byte(Base + 0x25);
                    Party.Cure3 = Read4Byte(Base + 0x26);
                    Party.Cure4 = Read4Byte(Base + 0x27);

                    int MemberNameLen = Read4Byte(Base + 0x40);

                    if (MemberNameLen > 15)
                        Party.Name = ReadString(Read4Byte(Base + 0x30), MemberNameLen);
                    else
                        Party.Name = ReadString(Base + 0x30, MemberNameLen);
                }

                PartyList.Add(Party);

                Base = Read4Byte(Base);
            }

            return PartyList.Count;
        }

        public int GetPartyMemberMaxHp(int Id)
        {
            int Base = Read4Byte(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_PARTY_BASE")) + GetAddress("KO_OFF_PARTY_LIST")));

            for (int i = 0; i <= GetPartyCount() - 1; i++)
            {
                if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    if (Read4Byte(Base + 0x8) == Id)
                        return Read4Byte(Base + 0x1C);
                }
                else
                {
                    if (Read4Byte(Base + 0x8) == Id)
                        return Read4Byte(Base + 0x18);
                }

                Base = Read4Byte(Base);
            }

            return 0;
        }

        public int GetPartyCount()
        {
            return Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_PARTY_BASE")) + GetAddress("KO_OFF_PARTY_COUNT"));
        }

        public bool IsPartyMember(string Name)
        {
            int Base = Read4Byte(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_PARTY_BASE")) + GetAddress("KO_OFF_PARTY_LIST")));

            for (int i = 0; i <= GetPartyCount() - 1; i++)
            {
                string MemberName = "";

                if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    int MemberNameLen = Read4Byte(Base + 0x40);

                    if (MemberNameLen > 15)
                        MemberName = ReadString(Read4Byte(Base + 0x30), MemberNameLen);
                    else
                        MemberName = ReadString(Base + 0x30, MemberNameLen);
                }
                else
                {
                    int MemberNameLen = Read4Byte(Base + 0x40);

                    if (MemberNameLen > 15)
                        MemberName = ReadString(Read4Byte(Base + 0x30), MemberNameLen);
                    else
                        MemberName = ReadString(Base + 0x30, MemberNameLen);
                }

                if (Name == MemberName)
                    return true;

                Base = Read4Byte(Base);
            }

            return false;
        }

        public void SendParty(string Name)
        {
            SendPacket("2F01" + AlignDWORD(Name.Length).Substring(0, 2) + "00" + StringToHex(Name));
            Thread.Sleep(10);
            SendPacket("2F03" + AlignDWORD(Name.Length).Substring(0, 2) + "00" + StringToHex(Name));
        }

        public string GetProperHealthBuff(int maxHp)
        {
            int undyHp = (int)Math.Round((float)(maxHp * 60.0f) / 100.0f);

            if (GetSkill(117) > 0 && GetSkillPoint(1) >= 78)
            {
                if (undyHp >= 2500)
                    return "Undying";
                else
                    return "Superioris";
            }
            else if (GetSkill(112) > 0 && GetSkillPoint(1) >= 70)
            {
                if (undyHp >= 2000)
                    return "Undying";
                else
                    return "Imposingness";
            }
            else if (GetSkillPoint(1) >= 57)
            {
                if (undyHp >= 1500)
                    return "Undying";
                else
                    return "Massiveness";
            }
            else if (GetSkillPoint(1) >= 54)
            {
                if (undyHp >= 1200)
                    return "Undying";
                else
                    return "Heapness";
            }
            else if (GetSkillPoint(1) >= 42)
                return "Mightness";
            else if (GetSkillPoint(1) >= 33)
                return "Hardness";
            else if (GetSkillPoint(1) >= 24)
                return "Strong";
            else if (GetSkillPoint(1) >= 15)
                return "Brave";
            else if (GetSkillPoint(1) >= 6)
                return "Grace";

            return "";
        }

        public string GetProperDefenseBuff()
        {
            if (GetSkill(116) > 0 && GetSkillPoint(1) >= 76)
                return "Insensibility Guard";
            else if (GetSkillPoint(1) >= 60)
                return "Insensibility Peel";
            else if (GetSkillPoint(1) >= 51)
                return "Insensibility Protector";
            else if (GetSkillPoint(1) >= 39)
                return "Insensibility Barrier";
            else if (GetSkillPoint(1) >= 30)
                return "Insensibility Shield";
            else if (GetSkillPoint(1) >= 21)
                return "Insensibility Armor";
            else if (GetSkillPoint(1) >= 12)
                return "Insensibility Shell";
            else if (GetSkillPoint(1) >= 3)
                return "Insensibility Skin";

            return "";
        }

        public string GetProperMindBuff()
        {
            if (GetSkillPoint(1) >= 45 && GetSkillPoint(1) <= 80)
                return "Fresh Mind";
            else if (GetSkillPoint(1) >= 36 && GetSkillPoint(1) <= 44)
                return "Calm Mind";
            else if (GetSkillPoint(1) >= 27 && GetSkillPoint(1) <= 35)
                return "Bright Mind";
            else if (GetSkillPoint(1) >= 9 && GetSkillPoint(1) <= 26)
                return "Resist All";

            return "";
        }

        public string GetProperHeal()
        {
            if (GetSkillPoint(0) >= 45)
                return "Superior Healing";
            else if (GetSkillPoint(0) >= 36)
                return "Massive Healing";
            else if (GetSkillPoint(0) >= 27)
                return "Great Healing";
            else if (GetSkillPoint(0) >= 18)
                return "Major Healing";
            else if (GetSkillPoint(0) >= 9)
                return "Healing";
            else if (GetSkillPoint(0) >= 0)
                return "Minor Healing";
            else if (GetLevel() >= 5)
                return "Light Healing";
            else if (GetLevel() >= 1)
                return "Tiny Healing";

            return "";
        }

        public bool IsTransformationAvailableZone()
        {
            if (GetZoneId() == 30 || GetZoneId() == 71 || GetZoneId() == 73 || GetZoneId() == 75 || GetZoneId() == 81 || GetZoneId() == 82 || GetZoneId() == 83)
                return false;

            return true;
        }

        public void SendNotice(string Text)
        {
            Debug.WriteLine(Text);
            if (GetPlatform() == AddressEnum.Platform.CNKO || GetPlatform() == AddressEnum.Platform.USKO) return;
            SendPacket("1013" + AlignDWORD(Text.Length).Substring(0, 2) + "00" + StringToHex(Text));
        }

        #endregion

        public void UpgradeEvent()
        {
            var UpgradeThread = new Thread(() =>
            {
                Item UpgradeScroll = Storage.ItemCollection.Find(y => y.Name == GetControl("UpgradeScroll"));

                if (UpgradeScroll == null) return;

                int UpgradeScrollSlot = GetInventoryItemSlot(UpgradeScroll.Id);

                if (UpgradeScrollSlot == -1) return;

                for (int i = 14; i < 42; i++)
                {
                    int ItemId = GetInventoryItemId(i);

                    if (ItemId == -1) continue;
                    if (ItemId < 100000000 || ItemId > 370000000) continue;

                    int ItemSlot = GetInventoryItemSlot(ItemId);

                    SendPacket("5B02" + "01" + "1427" + AlignDWORD(ItemId) + AlignDWORD(ItemSlot - 14).Substring(0, 2) + AlignDWORD(UpgradeScroll.Id) + AlignDWORD(UpgradeScrollSlot - 14).Substring(0, 2) + "00000000FF00000000FF00000000FF00000000FF00000000FF00000000FF00000000FF00000000FF");

                    Thread.Sleep(Convert.ToInt32(GetControl("UpgradeWait")));
                }

                SendPacket("790101001E7A0700");
            });

            UpgradeThread.IsBackground = true;
            UpgradeThread.Start();
        }

        public bool IsMining()
        {
            return GetState() == 63;
        }

        public void RemoveAllMiningTrashItem()
        {
            Storage.MiningTrashItemCollection.ForEach(x =>
            {
                int ItemSlot = GetInventoryItemSlot(x);

                if (ItemSlot != -1)
                    DeleteItem(ItemSlot, x);
            });
        }

        public bool IsInMonsterStoneZone()
        {
            return GetZoneId() == 81 || GetZoneId() == 82 || GetZoneId() == 83;
        }

        public void IntroSkip()
        {
            if (GetPlatform() == AddressEnum.Platform.CNKO)
            {
                ExecuteRemoteCode("60" +
                    "C6810C01000001" +
                    "FF35" + AlignDWORD(GetAddress("KO_PTR_LOGIN")) +
                    "BF" + AlignDWORD(GetAddress("KO_PTR_INTRO_SKIP_CALL")) +
                    "FFD7" +
                    "83C404" +
                    "B001" +
                    "61" +
                    "C20400"
                    );
            }
            else if (GetPlatform() == AddressEnum.Platform.JPKO)
            {
                ExecuteRemoteCode("60" +
                    "C6812401000001" +
                    "FF35" + AlignDWORD(GetAddress("KO_PTR_LOGIN")) +
                    "BF" + AlignDWORD(GetAddress("KO_PTR_INTRO_SKIP_CALL")) +
                    "FFD7" +
                    "83C404" +
                    "B001" +
                    "61" +
                    "C20400"
                    );
            }
        }

        public void LoginAlready()
        {
            string CodeString = "60" +
                "8B0D" + AlignDWORD(GetAddress("KO_PTR_LOGIN")) +
                "BF" + AlignDWORD(int.Parse("006B11A0", System.Globalization.NumberStyles.HexNumber)) +
                "FFD7" +
                "61" +
                "C3";

            ExecuteRemoteCode(CodeString);
        }

        public void Login(string AccountId, string Password)
        {
            if (GetPlatform() == AddressEnum.Platform.CNKO)
            {
                IntPtr AccountIdBase = new IntPtr(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_LOGIN")) + 0x2C) + 0x10C) + 0x13C);
                IntPtr AccountIdLengthBase = AccountIdBase + 0x10;
                IntPtr PasswordBase = new IntPtr(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_LOGIN")) + 0x2C) + 0x110) + 0x124);
                IntPtr PasswordLengthBase = PasswordBase + 0x10;

                Write4Byte(AccountIdLengthBase, AccountId.Length);
                WriteString(AccountIdBase, AccountId);
                Write4Byte(PasswordLengthBase, Password.Length);
                WriteString(PasswordBase, Password);

                Thread.Sleep(1000);

                /* string CodeString = "60" +
                     "8B15" + AlignDWORD(GetAddress("KO_PTR_LOGIN_BTN")) +
                     "8B8AA8000000" +
                     "8B01" +
                     "8B35" + AlignDWORD(GetAddress("KO_PTR_LOGIN_BTN_BASE")) +
                     "8B762C" +
                     "8BB610010000" +
                     "8B36" +
                     "6800100000" +
                     "56" +
                     "BF" + AlignDWORD(GetAddress("KO_PTR_LOGIN_BTN_CALL")) +
                     "FFD7" +
                     "61" +
                     "C3";

                 ExecuteRemoteCode(CodeString);*/

                string CodeString = "60" +
                    "8B0D" + AlignDWORD(GetAddress("KO_PTR_LOGIN")) +
                    "BF" + AlignDWORD(int.Parse("006B0CF0", System.Globalization.NumberStyles.HexNumber)) +
                    "FFD7" +
                    "8B0D" + AlignDWORD(GetAddress("KO_PTR_LOGIN")) +
                    "BF" + AlignDWORD(int.Parse("006B3200", System.Globalization.NumberStyles.HexNumber)) +
                    "FFD7" +

                    "61" +
                    "C3";

                ExecuteRemoteCode(CodeString);
            }

            Thread.Sleep(1000);

            while (true)
            {
                if (_Process.HasExited)
                    return;

                short LoginState = ReadByte(Read4Byte(Read4Byte(GetAddress("KO_PTR_PKT")) + 0x3C));

                switch(LoginState)
                {
                    case 0:
                        {
                            if (GetPlatform() == AddressEnum.Platform.CNKO)
                            {
                                if (Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_LOGIN")) + 0x2C) + 0x250) == 0) //Server List
                                    SelectServer(_AccountData.ServerId - 1);
                                else
                                {
                                    LoginAlready(); //Already Login Disconnect Packet
                                    Login(AccountId, Password);
                                    return;
                                }
                            }
                            else if (GetPlatform() == AddressEnum.Platform.JPKO)
                            {
                                if (Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_LOGIN")) + 0x28) + 0x43C) == 1)
                                    SelectServer(_AccountData.ServerId - 1);
                            }
                        }
                        break;
                    case 49: 
                        {
                            if((GetPlatform() == AddressEnum.Platform.CNKO && Read4Byte(Read4Byte(GetAddress("KO_PTR_CHARACTER_SELECT")) + 0x3C) != 0)
                                || (GetPlatform() == AddressEnum.Platform.JPKO && Read4Byte(Read4Byte(GetAddress("KO_PTR_CHARACTER_SELECT")) + 0x2C) != 0))
                            {
                                Thread.Sleep(3000);

                                for (int i = 0; i < _AccountData.CharacterId - 1; i++)
                                {
                                    SelectCharacterTurnLeft();
                                    Thread.Sleep(3000);
                                }

                                SelectCharacter();

                                SetPhase(EPhase.Selected);

                                return;
                            }
                        }
                        break;
                }

                Thread.Sleep(250);
            }

            /*if (GetPlatform() == AddressEnum.Platform.CNKO)
            {
                while (Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_LOGIN")) + 0x2C) + 0x250) != 0)
                {
                    if (_Process.HasExited)
                        return;

                    Thread.Sleep(100);
                }
                    
            }
            else if (GetPlatform() == AddressEnum.Platform.JPKO)
            {
                while (Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_LOGIN")) + 0x28) + 0x43C) != 1)
                {
                    if (_Process.HasExited)
                        return;

                    Thread.Sleep(100);
                }
            }

            if(!_Process.HasExited)
            {
                Thread.Sleep(500);

                SelectServer(_AccountData.ServerId - 1);

                if (GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    while (Read4Byte(Read4Byte(GetAddress("KO_PTR_CHARACTER_SELECT")) + 0x3C) == 0)
                    {
                        if (_Process.HasExited)
                            return;

                        Thread.Sleep(100);
                    }
                }
                else if (GetPlatform() == AddressEnum.Platform.JPKO)
                {
                    while (Read4Byte(Read4Byte(GetAddress("KO_PTR_CHARACTER_SELECT")) + 0x2C) == 0)
                    {
                        if (_Process.HasExited)
                            return;

                        Thread.Sleep(100);
                    }
                }

                if (!_Process.HasExited)
                {
                    Thread.Sleep(3000);

                    for (int i = 0; i < _AccountData.CharacterId - 1; i++)
                    {
                        SelectCharacterTurnLeft();
                        Thread.Sleep(3000);
                    }

                    SelectCharacter();

                    SetPhase(EPhase.Loggining);
                }
            }*/
        }

        public void SelectServer(int ServerId)
        {
            if (GetPlatform() == AddressEnum.Platform.JPKO)
            {
                ExecuteRemoteCode("60" +
                    "A1" + AlignDWORD(GetAddress("KO_PTR_LOGIN")) +
                    "8B4028" +
                    "C78004040000" + AlignDWORD(ServerId) +
                    "8B01" +
                    "8B0D" + AlignDWORD(GetAddress("KO_PTR_LOGIN")) +
                    "BF" + AlignDWORD(GetAddress("KO_PTR_SERVER_SELECT")) +
                    "FFD7" +
                    "61" +
                    "C3");
            } 
            else if(GetPlatform() == AddressEnum.Platform.CNKO)
            {
                ExecuteRemoteCode("60" +
                    "A1" + AlignDWORD(GetAddress("KO_PTR_LOGIN")) +
                    "8B402C" +
                    "C7800C040000" + AlignDWORD(ServerId) +
                    "8B01" +
                    "8B0D" + AlignDWORD(GetAddress("KO_PTR_LOGIN")) +
                    "BF" + AlignDWORD(GetAddress("KO_PTR_SERVER_SELECT")) +
                    "FFD7" +
                    "61" +
                    "C3");
            }

            SetPhase(EPhase.Selecting);
        }

        public void SelectCharacterTurnLeft()
        {
            ExecuteRemoteCode("60" +
                "8B0D" + AlignDWORD(GetAddress("KO_PTR_CHARACTER_SELECT")) +
                "BF" + AlignDWORD(GetAddress("KO_PTR_CHARACTER_SELECT_LEFT")) +
                "FFD7" +
                "61" +
                "C3");
        }

        public void SelectCharacter()
        {
            ExecuteRemoteCode("60" +
                "8B0D" + AlignDWORD(GetAddress("KO_PTR_CHARACTER_SELECT")) +
                "BF" + AlignDWORD(GetAddress("KO_PTR_CHARACTER_SELECT_ENTER")) +
                "FFD7" +
                "61" +
                "C3");
        }

        public void SelectCharacterTurnRight()
        {
            ExecuteRemoteCode("60" +
                "8B0D" + AlignDWORD(GetAddress("KO_PTR_CHARACTER_SELECT")) +
                "BF" + AlignDWORD(GetAddress("KO_PTR_CHARACTER_SELECT_RIGHT")) +
                "FFD7" +
                "61" +
                "C3");
        }

        public int SkillBase(int SkillId)
        {
            IntPtr Addr = VirtualAllocEx(_Handle, IntPtr.Zero, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            ExecuteRemoteCode(
                "60" +
                "8B0D" + AlignDWORD(0xEFE114) +
                "68" + AlignDWORD(SkillId) +
                "BF" + AlignDWORD(0x51D1F0) +
                "FFD7" +
                "A3" + AlignDWORD(Addr) +
                "61" +
                "C3");

            int Base = Read4Byte(Addr);

            return Base;
        }

        public void LegalAttack(int SkillId)
        {

            ExecuteRemoteCode(
                "60" +
                "8B0D" + AlignDWORD(GetAddress("KO_PTR_DLG")) +
                "8B89" + AlignDWORD(0x458) +
                "68 " + AlignDWORD(SkillBase(207003)) + //207003 - Archery 
                "68 " + AlignDWORD(GetTargetId()).Substring(0, 4) + "0000" +
                "B8" + AlignDWORD(0x10624DD3) + "FFD0" +
                "61" +
                "C3");
        }

        public void LoadZone()
        {
            _Zone = Database().GetZoneById(GetZoneId());

            if (_Zone != null)
                _MiniMapImage = GetImageFromFile(string.Format("{0}\\Resource\\Map\\{1}", Directory.GetCurrentDirectory(), _Zone.Image));
        }

        public Zone GetZone()
        {
            return _Zone;
        }

        public Image GetMiniMapImage()
        {
            return _MiniMapImage;
        }

        public Item FindHpPotion(int PotionId = 0)
        {
            if (PotionId != 0)
            {
                if (IsInventoryItemExist(PotionId))
                    return Storage.ItemCollection.Find(x => x.Id == PotionId);
            }
            else
            {
                int[] HpPotion = {
                    389015000, 389014000, 389013000,
                    389012000, 389011000, 389010000
                };

                HpPotion = HpPotion.Reverse().ToArray();

                for (int i = 0; i < HpPotion.Length; i++)
                {
                    if (IsInventoryItemExist(HpPotion[i]))
                        return Storage.ItemCollection.Find(x => x.Id == HpPotion[i]);
                }
            }

            return null;
        }

        public Item FindMpPotion(int PotionId = 0)
        {
            if (PotionId != 0)
            {
                if (IsInventoryItemExist(PotionId))
                    return Storage.ItemCollection.Find(x => x.Id == PotionId);
            }
            else
            {
                int[] MpPotion = {
                    389020000, 389019000, 389018000,
                    389017000, 389016000
                };

                MpPotion = MpPotion.Reverse().ToArray();

                for (int i = 0; i < MpPotion.Length; i++)
                {
                    if (IsInventoryItemExist(MpPotion[i]))
                        return Storage.ItemCollection.Find(x => x.Id == MpPotion[i]);
                }
            }

            return null;
        }

        public Item FindIbexPotion()
        {
            int[] IbexPotion = {
                389070000, 389071000, 800124000,
                800126000, 810189000, 810247000,
                811006000, 811008000, 814679000,
                900486000
            };

            IbexPotion = IbexPotion.Reverse().ToArray();

            for (int i = 0; i < IbexPotion.Length; i++)
            {
                if (IsInventoryItemExist(IbexPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == IbexPotion[i]);
            }

            return null;
        }

        public Item FindCrisisPotion()
        {
            int[] CrisisPotion = {
                389072000, 800125000, 800127000,
                810192000, 810248000, 900487000,
                811006000, 811008000, 814679000,
                900486000
            };

            CrisisPotion = CrisisPotion.Reverse().ToArray();

            for (int i = 0; i < CrisisPotion.Length; i++)
            {
                if (IsInventoryItemExist(CrisisPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == CrisisPotion[i]);
            }

            return null;
        }

        public Item FindPremiumHpPotion()
        {
            int[] PremiumHpPotion = {
                389310000, 389320000, 389330000,
                389390000, 900817000
            };

            PremiumHpPotion = PremiumHpPotion.Reverse().ToArray();

            for (int i = 0; i < PremiumHpPotion.Length; i++)
            {
                if (IsInventoryItemExist(PremiumHpPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == PremiumHpPotion[i]);
            }

            return null;
        }

        public Item FindPremiumMpPotion()
        {
            int[] PremiumMpPotion = {
                389340000, 389350000, 389360000,
                389400000, 900818000
            };

            PremiumMpPotion = PremiumMpPotion.Reverse().ToArray();

            for (int i = 0; i < PremiumMpPotion.Length; i++)
            {
                if (IsInventoryItemExist(PremiumMpPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == PremiumMpPotion[i]);
            }

            return null;
        }

        public Item FindQuestHpPotion()
        {
            int[] QuestHpPotion = {
                389064000, 910005000, 389063000, 399014000,
                810265000, 810267000, 810269000, 810272000,
                890229000, 899996000, 910004000, 930665000,
                931786000, 389062000, 900790000, 910003000,
                930664000, 389061000, 900780000, 910002000,
                389060000, 900770000, 910001000, 910012000
            };

            QuestHpPotion = QuestHpPotion.Reverse().ToArray();

            for (int i = 0; i < QuestHpPotion.Length; i++)
            {
                if (IsInventoryItemExist(QuestHpPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == QuestHpPotion[i]);
            }

            return null;
        }

        public Item FindQuestMpPotion()
        {
            int[] QuestMpPotion = {
                910006000, 389078000, 910007000, 900800000,
                389079000, 910008000, 900810000, 389080000,
                910009000, 900820000, 389081000, 910010000,
                899997000, 399020000, 389082000
            };

            QuestMpPotion = QuestMpPotion.Reverse().ToArray();

            for (int i = 0; i < QuestMpPotion.Length; i++)
            {
                if (IsInventoryItemExist(QuestMpPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == QuestMpPotion[i]);
            }

            return null;
        }

        public void PartyPriestAction(Player player, bool bForce = false)
        {
            if (Convert.ToBoolean(GetControl("PartyBuff")) || bForce)
            {
                string buff = GetControl("PartyBuffSelect");

                if (buff == "Otomatik")
                    buff = GetProperHealthBuff(player.MaxHp);

                if (player.Id != GetId() || (player.Id == GetId() && IsBuffAffected() == false))
                {
                    Skill skillData = GetSkillData(buff);

                    if (skillData != null)
                    {
                        if (UsePriestSkill(skillData, player.Id))
                            Thread.Sleep(1250);
                    }
                }
            }

            if (Convert.ToBoolean(GetControl("PartyAc")) || bForce)
            {
                string selectedDefenseBuff = GetControl("PartyAcSelect");

                if (selectedDefenseBuff == "Otomatik")
                    selectedDefenseBuff = GetProperDefenseBuff();

                if (player.Id != GetId() || (player.Id == GetId() && IsAcAffected() == false))
                {
                    Skill skillData = GetSkillData(selectedDefenseBuff);

                    if (skillData != null)
                    {
                        if (UsePriestSkill(skillData, player.Id))
                            Thread.Sleep(1250);
                    }
                }
            }

            if (Convert.ToBoolean(GetControl("PartyMind")) || bForce)
            {
                string selectedMindBuff = GetControl("PartyMindSelect");

                if (selectedMindBuff == "Otomatik")
                    selectedMindBuff = GetProperMindBuff();

                if (player.Id != GetId() || (player.Id == GetId() && IsMindAffected() == false))
                {
                    Skill skillData = GetSkillData(selectedMindBuff);

                    if (skillData != null)
                    {
                        if (UsePriestSkill(skillData, player.Id))
                            Thread.Sleep(1250);
                    }
                }
            }

            if ((Convert.ToBoolean(GetControl("PartyStr")) || bForce) && GetJob(player.Class) == "Warrior")
            {
                Skill SkillData = GetSkillData("Strength");

                if (SkillData != null)
                {
                    if (UsePriestSkill(SkillData, player.Class))
                        Thread.Sleep(1250);
                }
            }
        }

        public void PartyPriestHealAction(Player player)
        {
            if (Convert.ToBoolean(GetControl("PartyHeal")))
            {
                double healPercent = Math.Round(((double)player.Hp * 100) / player.MaxHp);

                if (healPercent <= Convert.ToInt32(GetControl("PartyHealValue")))
                {
                    string selectedHeal = GetControl("PartyHealSelect");

                    if (selectedHeal == "Otomatik")
                        selectedHeal = GetProperHeal();

                    Skill skillData = GetSkillData(selectedHeal);

                    if (skillData != null)
                    {
                        if (UsePriestSkill(skillData, player.Id))
                            Thread.Sleep(1250);
                    }
                }
            }

            if (Convert.ToBoolean(GetControl("PartyGroupHeal")))
            {
                double healPercent = Math.Round(((double)player.Hp * 100) / player.MaxHp);

                if (healPercent <= Convert.ToInt32(GetControl("PartyGroupHealValue"))
                    && GetPartyCount() >= Convert.ToInt32(GetControl("PartyGroupHealMemberCount")))
                {
                    string[] skills = { "Group Complete Healing", "Group Massive Healing" };

                    for (int i = 0; i < skills.Length; i++)
                    {
                        Skill skillData = GetSkillData(skills[i]);

                        if (skillData != null)
                        {
                            int lastUseTime;

                            bool coolDown = _GroupHealCooldown.TryGetValue(skillData.RealId, out lastUseTime);

                            if (lastUseTime == 0 || Environment.TickCount > lastUseTime + (skillData.Cooldown * 1000))
                            {
                                if (UsePriestSkill(skillData, player.Id))
                                {
                                    if (coolDown == false)
                                        _GroupHealCooldown.Add(skillData.RealId, Environment.TickCount);
                                    else
                                        _GroupHealCooldown[skillData.RealId] = Environment.TickCount;

                                    Thread.Sleep(1250);
                                }
                            }
                        }
                    }
                }
            }

            if (Convert.ToBoolean(GetControl("PartyCure")))
            {
                if (player.Cure1 == 256)
                {
                    Skill skillData = GetSkillData("Cure Curse");

                    if (skillData != null)
                    {
                        if (UsePriestSkill(skillData, player.Id))
                            Thread.Sleep(1250);
                    }
                }
            }

            if (Convert.ToBoolean(GetControl("PartyCureDisease")))
            {
                if (player.Cure1 == 257 || player.Cure1 == 1 || player.Cure1 == 65536)
                {
                    Skill skillData = GetSkillData("Cure Disease");

                    if (skillData != null)
                    {
                        if (UsePriestSkill(skillData, player.Id))
                            Thread.Sleep(1250);
                    }
                }
            }
        }

        public void PartyRogueAction(Player player)
        {
            if (Convert.ToBoolean(GetControl("PartySwift")) && player.Id != GetId())
            {
                Skill skillData = GetSkillData("Swift");

                if (skillData != null)
                {
                    if (UseRogueSkill(skillData, player.Id))
                        Thread.Sleep(1250);
                }
            }
        }

        public void PartyRogueHealAction(Player player)
        {
            if (Convert.ToBoolean(GetControl("PartyMinor")) && player.Id != GetId())
            {
                double minorPercent = Math.Round(((double)player.Hp * 100) / player.MaxHp);

                if (minorPercent <= Convert.ToInt32(GetControl("PartyMinorPercent")))
                {
                    if (UseMinorHealing(player.Id))
                        Thread.Sleep(30);
                }
            }
        }

        public void PartyMageAction(Player player)
        {
            if (Convert.ToBoolean(GetControl("PullAway")) && player.Id != GetId())
            {
                Skill skillData = GetSkillData("Summon Friend");

                if (skillData != null)
                {
                    if (UseMageSkill(skillData, player.Id))
                        Thread.Sleep(1250);
                }
            }
        }

        public void RepairEquipmentAction(int npcId)
        {
            if (GetAction() != EAction.Routing) return;

            SendNotice("Repair action start.");

            RepairAllEquipment(npcId, 1250);

            SendNotice("Repair action stop.");
        }

        public void BuyItemAction(int npcId, int npcType)
        {
            if (GetAction() != EAction.Routing) return;

            SendNotice("Buy item action started.");

            SendPacket("2001" + AlignDWORD(npcId).Substring(0, 4) + "FFFFFFFF");

            Thread.Sleep(300);

            List<Supply> Supply = new List<Supply>();

            if (IsNeedSupply(ref Supply, true))
            {
                Supply.ForEach(x =>
                {
                    BuyItem(x.Item, npcId, npcType, Math.Abs(GetInventoryItemCount(x.Item.Id) - x.Count), 1250);
                });
            }
            else
                SendNotice("Not needed supply action.");

            SendNotice("Buy item action stopped.");

            Supply.Clear();
        }

        public void SellItemAction(int npcId, int npcType)
        {
            if (GetAction() != EAction.Routing) return;

            SendNotice("Sell item action started.");

            SendPacket("2001" + AlignDWORD(npcId).Substring(0, 4) + "FFFFFFFF");

            Thread.Sleep(300);

            if (GetSellListSize() > 0)
            {
                for (int i = 14; i < 42; i++)
                {
                    int itemId = GetInventoryItemId(i);

                    if (GetSell(itemId) != null)
                        SellItem(itemId, npcId, npcType, GetInventoryItemCount(itemId), 1250);
                }
            }
            else
                SendNotice("Not needed selling item action.");

            SendNotice("Sell item action stopped.");
        }

        public void SetRouteSaving(bool State)
        {
            _RouteSaving = State;
        }

        public bool IsRouteSaving()
        {
            return _RouteSaving;
        }

        public void RouteSetAction(RouteData routeData)
        {
            _RouteSaveData.Add(routeData);
        }

        public void RouteSaveEvent()
        {           
            if(_RouteSaveData.Count() == 0)
            {
                RouteData routeData = new RouteData();

                routeData.Action = RouteData.Event.START;
                routeData.X = GetX();
                routeData.Y = GetY();

                RouteSetAction(routeData);
            }
            else
            {
                RouteData routeData = _RouteSaveData.Last();

                if (CoordinateDistance(routeData.X, routeData.Y, GetX(), GetY()) >= 1)
                {
                    RouteData newRouteData = new RouteData();

                    newRouteData.Action = RouteData.Event.MOVE;
                    newRouteData.X = GetX();
                    newRouteData.Y = GetY();

                    RouteSetAction(newRouteData);
                }
            }
        }

        public void RouteSave()
        {
            if (_RouteSaveData == null)
                return;

            Database().SetRoute(GetControl("RouteName"), GetZoneId(), JsonSerializer.Serialize(_RouteSaveData));

            _RouteSaveData.Clear();

            SetRouteSaving(false);
        }
    }
}
