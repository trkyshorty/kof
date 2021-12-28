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
        private int _FallbackTime { get; set; }
        private int _EnterGameTime { get; set; }
        private AddressEnum.Platform _Platform { get; set; }
        private List<string> _TargetAllowedCollection { get; set; } = new List<string>();
        public int _SupplyEventAfterWaitTime { get; set; } = 0;
        public int _RepairEventAfterWaitTime { get; set; } = 0;
        public List<Party> _PartyCollection { get; set; } = new List<Party>();
        private string _CharacterName { get; set; } = "";
        private string _AccountName { get; set; }
        private List<string> _PartyAllowedCollection { get; set; } = new List<string>();
        public List<AutoLoot> _AutoLootCollection { get; set; } = new List<AutoLoot>();
        public int _PartyRequestTime { get; set; }
        private Zone _Zone { get; set; }
        private Image _MiniMapImage { get; set; }
        public Dictionary<int, int> _GroupHealCooldown { get; set; } = new Dictionary<int, int>();

        public enum EPhase : short
        {
            None = 0,
            Disconnected = 1,
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
            Repairing = 1,
            Supplying = 2,
            MineExchanging = 3,
        }
        #endregion

        #region "Processor"

        public void HandleProcess(Process Process, ManagementObject Management)
        {
            _Process = Process;

            _Handle = Process.Handle;
            _ProcessId = Process.Id;

            string CommandLine = Process.StartInfo.Arguments;

            if (Management != null)
                CommandLine = Management["CommandLine"].ToString();

            var Args = ParseArguments(CommandLine);

            if (Process.MainWindowTitle == "ÆïÊ¿3.0" || (Management != null && Args.Length >= 2 && Args[2] == "CNKO"))
            {
                _Platform = AddressEnum.Platform.CNKO;

                if (Args.Length >= 2)
                {
                    int Index = Management != null ? 3 : 2;
                    SetAccountName(Args[Index]);
                }
            }
            else
            {
                if (Args.Length >= 1 && (Args[0] == "MGAMEJP" || Args[1] == "MGAMEJP"))
                {
                    _Platform = AddressEnum.Platform.JPKO;

                    int Index = Management != null ? 2 : 1;
                    SetAccountName(Args[Index]);

                    if (Process.MainWindowTitle == "Knight OnLine Client")
                        PatchMutant();
                }
                else
                    _Platform = AddressEnum.Platform.USKO;
            }

            if (Storage.AddressCollection.ContainsKey(_Platform) == false)
                Storage.AddressCollection.Add(_Platform, LoadAddressList(_Handle, _Platform));

            foreach (AddressStorage Address in GetAddressList())
            {
                Debug.WriteLine(Address.Name + " : " + Address.Address);
            }

            if(GetAccountName() != "")
                SetWindowText(_Process.MainWindowHandle, GetAccountName());
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
            List<Control> ControlList;
            if (Storage.ControlCollection.TryGetValue(GetNameConst(), out ControlList))
                return ControlList.Count;

            return 0;
        }
        public void SetNameConst(string Name)
        {
            _CharacterName = Name;
        }

        public string GetNameConst()
        {
            return _CharacterName;
        }

        public void SetAccountName(string Name)
        {
            _AccountName = Name;
        }

        public string GetAccountName()
        {
            return _AccountName;
        }
        public Database Database()
        {
            return _App.Database();
        }

        public string GetControl(string Name, string DefaultValue = "")
        {
            return Database().GetControl(GetNameConst(), GetPlatform().ToString(), Name, DefaultValue);
        }

        public void SetControl(string Name, string Value)
        {
            Database().SetControl(GetNameConst(), GetPlatform().ToString(), Name, Value);
        }

        public SkillBar GetSkillBar(int SkillId)
        {
            return Database().GetSkillBar(GetNameConst(), SkillId, GetPlatform().ToString());
        }

        public void DeleteSkillBar(int SkillId)
        {
            Database().DeleteSkillBar(GetNameConst(), SkillId, GetPlatform().ToString());
        }

        public void SetSkillBar(int SkillId, int SkillType)
        {
            Database().SetSkillBar(GetNameConst(), SkillId, SkillType, GetPlatform().ToString());
        }

        public bool IsDisconnected()
        {
            return (GetPhase() == EPhase.Disconnected && Environment.TickCount - GetDisconnectTime() >= 5000) || HasExited();
        }

        public IntPtr GetHandle()
        {
            return _Handle;
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

        public bool IsInFallback()
        {
            return _FallbackTime > 0 && Environment.TickCount - GetFallbackTime() <= 5000;
        }

        public void SetFallbackTime(int Time)
        {
            _FallbackTime = Time;
        }

        public int GetFallbackTime()
        {
            return _FallbackTime;
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
            if (GetPhase() == EPhase.Playing && GetControlSize() > 0 && Storage.ClientCollection.ContainsKey(GetProcessId()))
                return true;

            return false;
        }

        public int GetTargetAllowedSize()
        {
            return _TargetAllowedCollection.Count;
        }

        public bool GetTargetAllowed(string Name)
        {
            return _TargetAllowedCollection.Find(x => x == Name) == Name;
        }

        public void AddTargetAllowed(string Name)
        {
            if (_TargetAllowedCollection.Contains(Name) == false)
                _TargetAllowedCollection.Add(Name);
        }

        public void RemoveTargetAllowed(string Name)
        {
            if (_TargetAllowedCollection.Contains(Name))
                _TargetAllowedCollection.Remove(Name);
        }

        public void ClearTargetAllowed()
        {
            _TargetAllowedCollection.Clear();
        }

        public int GetPartyAllowedSize()
        {
            return _PartyAllowedCollection.Count;
        }

        public bool GetPartyAllowed(string Name)
        {
            return _PartyAllowedCollection.Find(x => x == Name) == Name;
        }

        public void AddPartyAllowed(string Name)
        {
            if (_PartyAllowedCollection.Contains(Name) == false)
                _PartyAllowedCollection.Add(Name);
        }

        public void RemovePartyAllowed(string Name)
        {
            if (_PartyAllowedCollection.Contains(Name))
                _PartyAllowedCollection.Remove(Name);
        }

        public void ClearPartyAllowed()
        {
            _PartyAllowedCollection.Clear();
        }

        protected void ProcessRecvPacketEvent(byte[] Packet)
        {
            /**
             * @reference https://github.com/srmeier/KnightOnline/blob/master/Server/shared/packets.h
             */
            string Message = ByteToHex(Packet);

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
                                SetPhase(EPhase.Loggining);
                                Debug.WriteLine("Auth Response " + Ret + " - Success");
                                break;
                            case "02":
                                SetPhase(EPhase.Disconnected);
                                Debug.WriteLine("Auth Response " + Ret + " - User not found");
                                break;
                            case "03":
                                SetPhase(EPhase.Disconnected);
                                Debug.WriteLine("Auth Response " + Ret + " - Password does not match");
                                break;
                            case "05":
                                SetPhase(EPhase.Loggining);
                                Debug.WriteLine("Auth Response " + Ret + " - Already");
                                break;
                            default:
                                Debug.WriteLine("Auth Response " + Ret);
                                break;

                        }
                    }
                    break;

                case "23": //WIZ_ITEM_DROP
                    {
                        AutoLoot AutoLootData = new AutoLoot();

                        AutoLootData.Id = Message.Substring(6, 8);
                        AutoLootData.DropTime = Environment.TickCount;

                        _AutoLootCollection.Add(AutoLootData);

                        Debug.WriteLine("Chest drop " + Message.Substring(6, 8));
                    }

                    break;

                case "24": //WIZ_BUNDLE_OPEN_REQ
                    if (Message.Length != 156) break;

                    for (int i = 0; i < 4; i++)
                    {
                        string ItemHex = Message.Substring(12 + (12 * i), 8);

                        if (ItemHex != "00000000")
                        {
                            int Item = BitConverter.ToInt32(StringToByte(ItemHex), 0);

                            Debug.WriteLine("Loot info " + i + " " + Item);

                            bool Loot = false;

                            if (Item == 900000000 || (Convert.ToBoolean(GetControl("OnlyNoah")) && Item == 900000000))
                                Loot = true;

                            if (Convert.ToBoolean(GetControl("LootOnlyList")) && Database().GetLoot(GetNameConst(), Item, GetPlatform().ToString()) != null)
                                Loot = true;

                            if (Convert.ToBoolean(GetControl("LootOnlyList")) && Convert.ToBoolean(GetControl("LootConsumable")) && Item >= 370000000 && Item <= 1931768000)
                                Loot = true;

                            if (Convert.ToBoolean(GetControl("LootOnlyList")) && Convert.ToBoolean(GetControl("LootOther")) && Item >= 100000000 && Item < 370000000)
                                Loot = true;

                            if (Convert.ToBoolean(GetControl("LootOnlySell")) && Database().GetSell(GetNameConst(), Item, GetPlatform().ToString()) != null)
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
                    SetFallbackTime(Environment.TickCount);
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
                    switch (Message.Substring(2, 2))
                    {
                        case "02": //PartyPermit
                            if (Storage.FollowedClient != null
                                && Storage.FollowedClient.GetProcessId() != GetProcessId()
                                && Convert.ToBoolean(GetControl("FollowDisable")) == false
                                && Storage.AutoPartyAccept)
                            {
                                string Name = Encoding.Default.GetString(StringToByte(Message.Substring(12)));

                                if (Storage.ClientCollection.Any(x => x.Value.GetNameConst() == Storage.FollowedClient.GetNameConst()))
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

            //Debug.WriteLine(Message);

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
                        Debug.WriteLine("Object event");
                        ForwardPacketToAllFollower(Message);
                    }
                    break;

                case "24": //WIZ_BUNDLE_OPEN_REQ
                    {
                        Debug.WriteLine("Chest open");
                        _AutoLootCollection.RemoveAll(x => x.Id == Message.Substring(2, 8));
                    }
                    break;

                case "26": //WIZ_ITEM_GET
                    Debug.WriteLine("Loot get " + BitConverter.ToInt32(StringToByte(Message.Substring(10, 8)), 0));
                    break;

                case "79": //WIZ_SKILLDATA
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
                        SetPhase(EPhase.Warping);
                        ForwardPacketToAllFollower(Message);
                    }
                    break;

                case "48": //WIZ_WARP_HOME
                    {
                        Debug.WriteLine("/town");
                        SetFallbackTime(Environment.TickCount);
                        ForwardPacketToAllFollower(Message);
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

        public void ExecuteRemoteCode(String Code)
        {
            IntPtr CodePtr = VirtualAllocEx(_Handle, IntPtr.Zero, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            byte[] CodeByte = StringToByte(Code);
            WriteProcessMemory(_Handle, CodePtr, CodeByte, CodeByte.Length, 0);
            IntPtr Thread = CreateRemoteThread(_Handle, IntPtr.Zero, 0, CodePtr, IntPtr.Zero, 0, IntPtr.Zero);
            if (Thread != IntPtr.Zero)
                WaitForSingleObject(Thread, uint.MaxValue);
            CloseHandle(Thread);
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
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG") - 0x14)) + 0x8;
        }

        public void PatchMailslot()
        {
            UnicodeEncoding UnicodeEncoding = new UnicodeEncoding();

            IntPtr CreateFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "CreateFileW");
            IntPtr WriteFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "WriteFile");
            IntPtr CloseFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "CloseHandle");

            if (_MailslotRecvFuncPtr == IntPtr.Zero)
            {
                String MailslotRecvName = @"\\.\mailslot\KNIGHTONLINE_RECV\" + Environment.TickCount;

                if (_MailslotRecvPtr == IntPtr.Zero)
                    _MailslotRecvPtr = CreateMailslot(MailslotRecvName, 0, 0, IntPtr.Zero);

                _MailslotRecvFuncPtr = VirtualAllocEx(_Handle, _MailslotRecvFuncPtr, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

                byte[] MailslotRecvNameByte = UnicodeEncoding.GetBytes(MailslotRecvName);

                WriteProcessMemory(_Handle, _MailslotRecvFuncPtr + 0x400, MailslotRecvNameByte, MailslotRecvNameByte.Length, 0);

                Patch(_Handle, _MailslotRecvFuncPtr,
                    "55" + //push ebp
                    "8BEC" + //mov ebp,esp
                    "83C4F4" + //add esp,-0C
                    "33C0" + //xor eax,eax
                    "8945FC" + //mov [ebp-04],eax
                    "33D2" + //xor edx,edx
                    "8955F8" + //mov [ebp-08],edx
                    "6A00" + //push 00
                    "6880000000" + //push 00000080
                    "6A03" + //push 03
                    "6A00" + //push 00
                    "6A01" + //push 01
                    "6800000040" + //push 40000000
                    "68" + AlignDWORD(_MailslotRecvFuncPtr + 0x400) + //push
                    "E8" + AlignDWORD(AddressDistance(_MailslotRecvFuncPtr + 0x27, CreateFilePtr)) + //call
                    "8945F8" + //mov [ebp-08],eax
                    "6A00" + //push 00
                    "8D4DFC" + //lea ecx,[ebp-04]
                    "51" + //push ecx
                    "FF750C" + //push [ebp+0C]
                    "FF7508" + //push [ebp+08]
                    "FF75F8" + //push [ebp-08]
                    "E8" + AlignDWORD(AddressDistance(_MailslotRecvFuncPtr + 0x3E, WriteFilePtr)) + //call
                    "8945F4" + //mov [ebp-0C],eax
                    "FF75F8" + //push [ebp-08]
                    "E8" + AlignDWORD(AddressDistance(_MailslotRecvFuncPtr + 0x49, CloseFilePtr)) + //call
                    "8BE5" + //mov esp,ebp
                    "5D" + //pop ebp
                    "C3"); //ret 
            }

            _MailslotRecvHookPtr = VirtualAllocEx(_Handle, _MailslotRecvHookPtr, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            Patch(_Handle, _MailslotRecvHookPtr,
                "55" + //push ebp
                "8BEC" + //mov ebp,esp
                "83C4F8" + //add esp,-08
                "53" + //push ebx
                "8B4508" + //mov eax,[ebp+08]
                "83C004" + //add eax,04
                "8B10" + //mov edx,[eax]
                "8955FC" + //mov [ebp-04],edx
                "8B4D08" + //mov ecx,[ebp+08]
                "83C108" + //add ecx,08
                "8B01" + //mov eax,[ecx]
                "8945F8" + //mov [ebp-08],eax
                "FF75FC" + //push [ebp-04]
                "FF75F8" + //push [ebp-08]
                "E8" + AlignDWORD(AddressDistance(_MailslotRecvHookPtr + 0x23, _MailslotRecvFuncPtr)) + //call
                "83C408" + //add esp,08
                "8B0D" + AlignDWORD(GetAddress("KO_PTR_DLG") - 0x14) + //mov ecx
                "FF750C" + //push [ebp+0C]
                "FF7508" + //push [ebp+08]
                "B8" + AlignDWORD(Read4Byte(GetRecvPointer())) + //mov eax
                "FFD0" + //call eax
                "5B" + //pop ebx
                "59" + //pop ecx
                "59" + //pop ecx
                "5D" + //pop ebp
                "C20800"); //ret 0008

            uint MemoryProtection;

            VirtualProtectEx(_Handle, new IntPtr(GetRecvPointer()), 4, PAGE_EXECUTE_READWRITE, out MemoryProtection);
            Write4Byte(GetRecvPointer(), _MailslotRecvHookPtr.ToInt32());
            VirtualProtectEx(_Handle, new IntPtr(GetRecvPointer()), 4, MemoryProtection, out MemoryProtection);

            Debug.WriteLine("Recv packet hooked.");

            String MailslotSendName = @"\\.\mailslot\KNIGHTONLINE_SEND\" + Environment.TickCount;

            _MailslotSendPtr = CreateMailslot(MailslotSendName, 0, 0, IntPtr.Zero);

            _MailslotSendHookPtr = VirtualAllocEx(_Handle, _MailslotSendHookPtr, 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

            byte[] MailslotSendNameByte = UnicodeEncoding.GetBytes(MailslotSendName);

            WriteProcessMemory(_Handle, _MailslotSendHookPtr + 0x400, MailslotSendNameByte, MailslotSendNameByte.Length, 0);

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

            Patch(_Handle, new IntPtr(GetAddress("KO_PTR_SND")), "E9" + AlignDWORD(AddressDistance(new IntPtr(GetAddress("KO_PTR_SND")), _MailslotSendHookPtr)));

            Debug.WriteLine("Send packet hooked.");
        }

        #region "Game Functions"

        public string GetName()
        {
            int NameLen = Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_NAME_LEN"));

            if (NameLen > 15)
                return ReadString(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_NAME")), NameLen);

            return ReadString(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_NAME"), NameLen);
        }

        protected bool IsConnectionLost()
        {
            int StateAddress = 0x40064;

            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                StateAddress = 0x400DC;

            return Read4Byte(Read4Byte(GetAddress("KO_PTR_PKT")) + StateAddress) == 0 ? true : false;
        }

        public int GetId()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_ID"));
        }

        public int GetLevel()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_LEVEL"));
        }

        protected int GetHp()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_HP"));
        }
        protected int GetMaxHp()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MAX_HP"));
        }

        protected int GetMp()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MP"));
        }

        protected int GetMaxMp()
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
            int NameLen = Read4Byte(Base + GetAddress("KO_OFF_NAME_LEN"));
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
            //return (int)Math.Round(ReadFloat(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_MCOR")) + GetAddress("KO_OFF_MCORX")));
        }

        public int GetTargetY()
        {
            if (GetTargetId() == 0) return 0;
            int Base = GetTargetBase();
            if (Base == 0) return 0;
            return (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_Y")));
            //return (int)Math.Round(ReadFloat(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_MCOR")) + GetAddress("KO_OFF_MCORY")));
        }

        public int GetTargetZ()
        {
            if (GetTargetId() == 0) return 0;
            int Base = GetTargetBase();
            if (Base == 0) return 0;
            return (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_Z")));
            //return (int)Math.Round(ReadFloat(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_MCOR")) + GetAddress("KO_OFF_MCORZ")));
        }

        protected short GetState()
        {
            int StateOffset = 0x2A0;

            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                StateOffset = 0x2EC;

            return ReadByte(Read4Byte(GetAddress("KO_PTR_CHR")) + StateOffset);
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
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoX")));
        }

        public int GetGoY()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoY")));
        }

        public int GetGoZ()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoY")));
        }

        protected int GetSkill(int Slot)
        {
            return Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_SBARBase")) + 0x184 + (Slot * 4) + 0x68)));
        }

        public int GetSkillPoint(int Slot)
        {
            return Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_SBARBase")) + GetAddress("KO_OFF_BSkPoint") + (Slot * 4));
        }

        protected bool IsOreadsOn()
        {
            return Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C4) != 0 ? true : false;
        }

        protected void Oreads(bool Enable)
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

        protected bool IsWallhackOn()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_WH")) == 0 ? true : false;
        }

        protected void Wallhack(bool Enable)
        {
            Write4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_WH"), Enable ? 0 : 1);
        }

        public void MoveCoordinate(int GoX, int GoY)
        {
            if (GoX <= 0 || GoY <= 0) return;
            if (GoX == GetX() && GoY == GetY()) return;
            Write4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOVE"), 1);
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoX"), GoX);
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoY"), GoY);
            Write4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOVEType"), 2);
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
                       "B8" + AlignDWORD(GetAddress("KO_ROTA_START")) +
                       "FFD0" +
                       "61" +
                       "C3");
            }
        }

        public void StopRouteEvent()
        {
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoX"), GetX());
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoZ"), GetZ());
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoY"), GetY());

            ExecuteRemoteCode("60" +
                   "8B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) +
                   "B8" + AlignDWORD(GetAddress("KO_ROTA_STOP")) +
                   "FFD0" +
                   "61" +
                   "C3");
        }

        public void SetCoordinate(int GoX, int GoY, int ExecutionAfterWait = 0)
        {
            if (GoX <= 0 || GoY <= 0 || IsInFallback() || IsInEnterGame()) return;
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

                if (IsInFallback() || IsInEnterGame())
                    return;

                SendPacket("06" + AlignDWORD(NextX * 10).Substring(0, 4) + AlignDWORD(NextY * 10).Substring(0, 4) + AlignDWORD(GetZ() * 10).Substring(0, 4) + "2B0003");
            }

            SendPacket("06" + AlignDWORD(GoX * 10).Substring(0, 4) + AlignDWORD(GoY * 10).Substring(0, 4) + AlignDWORD(GetZ() * 10).Substring(0, 4) + "2B0003");

            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_X"), GoX);
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_Y"), GoY);

            MoveCoordinate(GetX(), GetY());

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        private int ReadAffectedSkill(int Skill)
        {
            int SkillBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_SKILLBASE"));

            SkillBase = Read4Byte(SkillBase + 0x4);
            SkillBase = Read4Byte(SkillBase + GetAddress("KO_OFF_SKILLID"));

            for (int i = 1; i < Skill; i++)
                SkillBase = Read4Byte(SkillBase + 0x0);

            SkillBase = Read4Byte(SkillBase + 0x8);

            if (SkillBase > 0)
                return Read4Byte(SkillBase + 0x0);

            SkillBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_SKILLBASE"));

            SkillBase = Read4Byte(SkillBase + 0x4);
            SkillBase = Read4Byte(SkillBase + GetAddress("KO_OFF_SKILLID"));

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

        protected bool IsSkillAffected(int Skill)
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
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * SlotId)));
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                return Read4Byte(Length + 0x7C);
            else
                return Read4Byte(Length + 0x74);
        }

        protected void SetInventoryItemDurability(int SlotId, int iDurability)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * SlotId)));

            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                Write4Byte(Length + 0x7C, iDurability);
            else
                Write4Byte(Length + 0x74, iDurability);
        }

        protected bool IsInventoryFull()
        {
            bool Full = true;
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            for (int i = 14; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * i)));
                if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    if (Read4Byte(Read4Byte(Length + 0x70)) + Read4Byte(Read4Byte(Length + 0x74)) == 0)
                        return false;
                }
                else
                {
                    if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == 0)
                        return false;
                }
            }
            return Full;
        }

        public int GetInventoryAvailableSlotCount()
        {
            int Count = 0;
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            for (int i = 14; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * i)));
                if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    if (Read4Byte(Read4Byte(Length + 0x70)) + Read4Byte(Read4Byte(Length + 0x74)) == 0)
                        Count++;
                }
                else
                {
                    if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == 0)
                        Count++;
                }
            }
            return Count;
        }

        protected int GetInventoryItemCount(int ItemId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            for (int i = 0; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * i)));
                if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    if (Read4Byte(Read4Byte(Length + 0x70)) + Read4Byte(Read4Byte(Length + 0x74)) == ItemId)
                        return Read4Byte(Length + 0x78);
                }
                else
                {
                    if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == ItemId)
                        return Read4Byte(Length + 0x70);
                }
            }
            return 0;
        }

        protected int GetInventoryItemId(int SlotId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * SlotId)));
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
            {
                return Read4Byte(Read4Byte(Length + 0x70)) + Read4Byte(Read4Byte(Length + 0x74));
            }
            else
            {
                return Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C));
            }
        }


        protected string GetInventoryItemName(int SlotId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * SlotId)));

            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
            {
                int Name = Read4Byte(Length + 0x70);
                int NameLength = Read4Byte(Name + 0x1C);

                if (NameLength > 15)
                    return ReadString(Read4Byte(Name + 0xC), NameLength);
                else
                    return ReadString(Name + 0xC, NameLength);
            }
            else
            {
                int Name = Read4Byte(Length + 0x68);
                int NameLength = Read4Byte(Name + 0x1C);

                if (NameLength > 15)
                    return ReadString(Read4Byte(Name + 0xC), NameLength);
                else
                    return ReadString(Name + 0xC, NameLength);
            }


           
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
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            for (int i = 0; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * i)));
                if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    if (Read4Byte(Read4Byte(Length + 0x70)) + Read4Byte(Read4Byte(Length + 0x74)) == ItemId)
                        return true;
                }
                else
                {
                    if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == ItemId)
                        return true;
                }
            }
            return false;
        }

        protected int GetInventoryItemSlot(int ItemId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));

            for (int i = 14; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * i)));

                if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    if (Read4Byte(Read4Byte(Length + 0x70)) + Read4Byte(Read4Byte(Length + 0x74)) == ItemId)
                        return i;
                }
                else
                {
                    if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == ItemId)
                        return i;
                }
            }
            return -1;
        }

        public int GetAllInventoryItem(ref List<Inventory> refInventory)
        {
            for (int i = 14; i < 42; i++)
            {
                int Item = GetInventoryItemId(i);

                if (Item > 0)
                {
                    Inventory Inventory = new Inventory();
                    Inventory.Id = Item;
                    Inventory.Name = GetInventoryItemName(i);

                    refInventory.Add(Inventory);
                }
            }

            return refInventory.Count;
        }

        public bool IsNeedRepair()
        {
            for (int i = 1; i < 14; i++)
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

        public void RepairAllEquipment(int NpcId, bool Force, int ExecutionAfterWait = 1250)
        {
            for (int i = 1; i < 14; i++)
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
                            if (Force == true || (IsInventorySlotEmpty(i) == false && GetInventoryItemDurability(i) == 0))
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

        public bool IsNeedSupply(ref List<Supply> refSupply, bool Force = false)
        {
            foreach (var x in Storage.SupplyCollection)
            {
                if (Convert.ToBoolean(GetControl(x.Control)) == true)
                {
                    string ItemName = x.ControlItem != null ? GetControl(x.ControlItem) : x.ItemConst;
                    Item Item = Storage.ItemCollection.Find(y => y.Name == ItemName);

                    if (Item == null)
                        continue;

                    if (Force == true || (GetInventoryItemCount(Item.Id) < 8 && GetInventoryItemCount(Item.Id) < Convert.ToInt32(GetControl(x.ControlCount))))
                    {
                        Npc Npc = Storage.NpcCollection
                            .FindAll(y => y.Type == x.Type && y.Zone == GetZoneId() && (y.Nation == 0 || y.Nation == GetNation()))
                            .GroupBy(y => Math.Pow((GetX() - y.X), 2) + Math.Pow((GetY() - y.Y), 2))
                            .OrderBy(y => y.Key)
                            ?.FirstOrDefault()
                            ?.FirstOrDefault();

                        if (Npc != null)
                        {
                            Supply Supply = new Supply();

                            Supply.Item = Item;
                            Supply.Npc = Npc;
                            Supply.Count = Convert.ToInt32(GetControl(x.ControlCount));

                            refSupply.Add(Supply);
                        }
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

        public void SelectTarget(int TargetId, int ExecutionAfterWait = 0)
        {
            /*if (TargetId > 0)
                WriteByte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + 0x1D0) + 0xFC, 1);
            else
                WriteByte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + 0x1D0) + 0xFC, 0);*/

            Write4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOB"), TargetId);

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public bool IsMoving()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOVE")) == 1 ? true : false;
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
            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
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

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
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
            SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");
        }

        public void UseSelfSkill(int SkillId, int TargetId = 0)
        {
            SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");
        }

        public void ClearSkill(int SkillId)
        {
            SendPacket("3106" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "000000000000000000000000");
        }

        private void UseTargetSkill(int SkillId, int TargetId)
        {
            SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000F00");
            Thread.Sleep(10);
            SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");
        }

        private void UseTargetAreaSkill(int SkillId, int TargetX, int TargetY, int TargetZ)
        {
            SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "00000000000000000F00");
            Thread.Sleep(10);
            SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "000000000000");
        }

        public bool IsAttackableTarget(int TargetId = 0)
        {
            TargetId = TargetId > 0 ? TargetId : GetTargetId();
            if (TargetId == 0) return false;
            int TargetBase = GetTargetBase(TargetId);
            if (TargetBase == 0) return false;
            if (CoordinateDistance(GetX(), GetY(), (int)Math.Round(ReadFloat(TargetBase + GetAddress("KO_OFF_X"))), (int)Math.Round(ReadFloat(TargetBase + GetAddress("KO_OFF_Y")))) > Convert.ToInt32(GetControl("AttackDistance"))) return false;
            if (Read4Byte(TargetBase + GetAddress("KO_OFF_NATION")) != 0 && Read4Byte(TargetBase + GetAddress("KO_OFF_NATION")) == GetNation()) return false;
            if (Read4Byte(TargetBase + GetAddress("KO_OFF_NATION")) == 3) return false;
            if (Read4Byte(TargetBase + GetAddress("KO_OFF_MAX_HP")) != 0 && Read4Byte(TargetBase + GetAddress("KO_OFF_HP")) == 0) return false;

            int StateOffset = 0x2A0;

            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                StateOffset = 0x2EC;

            if (ReadByte(TargetBase + StateOffset) == 10 || ReadByte(TargetBase + StateOffset) == 11) return false;

            return true;
        }

        public bool UseWarriorSkill(Skill Skill, int TargetId = 0)
        {
            if (IsCharacterAvailable() == false) return false;
            if (Skill.Mana > 0 && GetMp() < Skill.Mana) return false;
            if (Skill.Type == 1 && TargetId <= 0) return false;

            int SkillId = Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"));

            if (Skill.Type == 2 && (TargetId == 0 || TargetId == GetId()) && IsSkillAffected(SkillId)) return false;
            if (Skill.Item > 0 && IsInventoryItemExist(Skill.Item) == false) return false;
            if (Skill.ItemCount > 0 && GetInventoryItemCount(Skill.Item) < Skill.ItemCount) return false;

            switch (Skill.Name)
            {
                case "Slash":
                case "Crash":
                case "Piercing":
                case "Whipping":
                case "Hash":
                case "Hoodwink":
                case "Shear":
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

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000D00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0100000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000002000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0200000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000003000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0300000000000000");

                        return true;
                    }

                case "Arrow Shower":
                    {
                        if (IsAttackableTarget() == false) return false;

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000F00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0100000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000002000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0200000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000003000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0300000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000004000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0400000000000000");
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000005000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0500000000000000");

                        return true;
                    }

                case "Süper Archer":
                    {
                        if (IsAttackableTarget() == false) return false;
                        if (CoordinateDistance(GetX(), GetY(), TargetX, TargetY) > 1) return false;

                        SendPacket("3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000D00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0100000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000002000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0200000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000003000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0300000000000000");

                        if (IsAttackableTarget() == false) return false;
                        if (CoordinateDistance(GetX(), GetY(), TargetX, TargetY) > 1) return false;

                        SendPacket("3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000F00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0100000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000002000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0200000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000003000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0300000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000004000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0400000000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000005000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "0200000000009BFF0500000000000000");

                        return true;
                    }

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
                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "00000000000000000000000000001E00");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "000000000000000000000000");

                        return true;
                    }

                case "Blood of wolf":
                    {
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
            if (Skill.Mana > 0 && GetMp() < Skill.Mana) return false;
            if (Skill.Type == 1 && TargetId <= 0) return false;

            int SkillId = Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"));

            if (Skill.Type == 2 && (TargetId == 0 || TargetId == GetId()) && IsSkillAffected(SkillId)) return false;
            if (Skill.Item > 0 && IsInventoryItemExist(Skill.Item) == false) return false;
            if (Skill.ItemCount > 0 && GetInventoryItemCount(Skill.Item) < Skill.ItemCount) return false;

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

                        UseTargetSkill(SkillId, TargetId);

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

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000F00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");
                        SendPacket("3104" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "9BFF0000000000000000");

                        return true;
                    }

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

                        SendPacket("3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "00000000000000000F00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "000000000000");
                        Thread.Sleep(10);
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

        public void RepairEquipmentAction(bool Force = false)
        {
            if (GetAction() != EAction.None) return;

            Npc Sunderies = Storage.NpcCollection
                .FindAll(x => x.Type == "Sunderies" && x.Zone == GetZoneId() && (x.Nation == 0 || x.Nation == GetNation()))
                .GroupBy(x => Math.Pow((GetX() - x.X), 2) + Math.Pow((GetY() - x.Y), 2))
                .OrderBy(x => x.Key)
                ?.FirstOrDefault()
                ?.FirstOrDefault();

            if (Sunderies != null)
            {
                SendNotice("Repair işlemi başladı.");

                SetAction(EAction.Repairing);

                SelectTarget(0);

                Thread.Sleep(1250);

                int iLastX = GetX(); int iLastY = GetY();

                if (Sunderies.Town == 1)
                    SendPacket("4800", 2500);

                while (GetAction() == EAction.Repairing)
                {
                    if (CoordinateDistance(GetX(), GetY(), Sunderies.X, Sunderies.Y) > 5)
                    {
                        if (GetPlatform() == AddressEnum.Platform.CNKO || GetPlatform() == AddressEnum.Platform.USKO)
                        {
                            if (GetState() == 0)
                                StartRouteEvent(Sunderies.X, Sunderies.Y);
                        }
                        else
                            SetCoordinate(Sunderies.X, Sunderies.Y, 2500);
                    }
                    else
                    {
                        RepairAllEquipment(Sunderies.RealId, Force, 5000);

                        while (GetAction() == EAction.Repairing)
                        {
                            if (CoordinateDistance(GetX(), GetY(), iLastX, iLastY) > 5)
                            {
                                if (GetPlatform() == AddressEnum.Platform.CNKO || GetPlatform() == AddressEnum.Platform.USKO)
                                {
                                    if(GetState() == 0)
                                        StartRouteEvent(iLastX, iLastY);
                                }  
                                else
                                    SetCoordinate(iLastX, iLastY, 2500);
                            }
                            else
                                SetAction(EAction.None);

                            Thread.Sleep(1250);
                        }
                    }

                    Thread.Sleep(1250);
                }

                _RepairEventAfterWaitTime = Environment.TickCount;

                SendNotice("Repair işlemi sona erdi.");
            }
            else
            {
                Debug.WriteLine("Sunderies does not exist (" + GetZoneId() + ")");
            }
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

        public void BuyItem(Item Item, Npc Npc, int Count, int ExecutionAfterWait = 0)
        {
            if (Count == 0) return;

            int InventoryItemSlot = GetInventoryItemSlot(Item.Id);

            InventoryItemSlot = InventoryItemSlot != -1 ? InventoryItemSlot : GetInventoryEmptySlot();

            if (InventoryItemSlot == -1) return;

            string ItemCount = Item.BuyPacketCountSize == 0 ? AlignDWORD(Count) : AlignDWORD(Count).Substring(0, Item.BuyPacketCountSize);

            if (Npc.Type == "Sunderies")
                SendPacket("2101" + "18E40300" + AlignDWORD(Npc.RealId).Substring(0, 4) + "01" + AlignDWORD(Item.Id) + AlignDWORD(InventoryItemSlot - 14).Substring(0, 2) + ItemCount + Item.BuyPacketEnd);
            else
                SendPacket("2101" + "48DC0300" + AlignDWORD(Npc.RealId).Substring(0, 4) + "01" + AlignDWORD(Item.Id) + AlignDWORD(InventoryItemSlot - 14).Substring(0, 2) + ItemCount + Item.BuyPacketEnd);

            Thread.Sleep(50);
            SendPacket("6A02");

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public void SellItem(int ItemId, Npc Npc, int Count, int ExecutionAfterWait = 0)
        {
            if (Count == 0) return;

            int InventoryItemSlot = GetInventoryItemSlot(ItemId);
            int InventoryItemCount = GetInventoryItemCount(ItemId);

            if (InventoryItemCount == 0 || InventoryItemSlot == -1 || Count > InventoryItemCount) return;

            if (Npc.Type == "Sunderies")
                SendPacket("2102" + "18E40300" + AlignDWORD(Npc.RealId).Substring(0, 4) + "01" + AlignDWORD(ItemId) + AlignDWORD(InventoryItemSlot - 14).Substring(0, 2) + AlignDWORD(Count).Substring(0, 4));
            else
                SendPacket("2102" + "48DC0300" + AlignDWORD(Npc.RealId).Substring(0, 4) + "01" + AlignDWORD(ItemId) + AlignDWORD(InventoryItemSlot - 14).Substring(0, 2) + AlignDWORD(Count).Substring(0, 4));

            Thread.Sleep(50);
            SendPacket("6A02");

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public void SupplyItemAction(List<Supply> Supply)
        {
            if (GetAction() != EAction.None) return;

            SendNotice("Tedarik işlemi başladı.");

            SetAction(EAction.Supplying);

            Thread.Sleep(1250);

            List<Supply> OrderedSupply = Supply.OrderBy(x => x.Npc.Id).ToList();

            int iLastX = GetX(); int iLastY = GetY();

            OrderedSupply.ForEach(x =>
            {
                if (x.Npc.Town == 1)
                    SendPacket("4800", 2500);

                while (CoordinateDistance(GetX(), GetY(), x.Npc.X, x.Npc.Y) > 5)
                {
                    if (GetPlatform() == AddressEnum.Platform.CNKO || GetPlatform() == AddressEnum.Platform.USKO)
                    {
                        if (GetState() == 0)
                            StartRouteEvent(x.Npc.X, x.Npc.Y);
                    }
                    else
                        SetCoordinate(x.Npc.X, x.Npc.Y, 2500);

                    Thread.Sleep(1250);
                }

                if (x.Npc.Type == "Inn")
                    WarehouseItemCheckOut(x.Item, x.Npc, Math.Abs(GetInventoryItemCount(x.Item.Id) - x.Count), 2500);
                else
                {
                    BuyItem(x.Item, x.Npc, Math.Abs(GetInventoryItemCount(x.Item.Id) - x.Count), 2500);

                    for (int i = 14; i < 42; i++)
                    {
                        int ItemId = GetInventoryItemId(i);

                        if (Database().GetSell(GetNameConst(), ItemId, GetPlatform().ToString()) != null)
                            SellItem(ItemId, x.Npc, GetInventoryItemCount(ItemId), 2500);
                    }
                }
            });

            while (CoordinateDistance(GetX(), GetY(), iLastX, iLastY) > 5)
            {
                if (GetPlatform() == AddressEnum.Platform.CNKO || GetPlatform() == AddressEnum.Platform.USKO)
                {
                    if (GetState() == 0)
                        StartRouteEvent(iLastX, iLastY);
                }
                else
                    SetCoordinate(iLastX, iLastY, 2500);

                Thread.Sleep(1250);
            }

            SetAction(EAction.None);

            SendNotice("Tedarik işlemi sona erdi.");

            _SupplyEventAfterWaitTime = Environment.TickCount;

            Supply.Clear();
        }

        public int SearchTarget(Int32 Address, ref List<Target> OutTargetList)
        {
            int Ebp = Read4Byte(Read4Byte(GetAddress("KO_FLDB")) + Address);
            int Fend = Read4Byte(Read4Byte(Ebp + 0x4) + 0x4);
            int Esi = Read4Byte(Ebp);
            int Tick = Environment.TickCount;

            while (Esi != Ebp && Environment.TickCount - Tick < 50)
            {
                int Base = Read4Byte(Esi + 0x10);

                if (Base == 0) break;

                if (OutTargetList.Any(x => x.Base == Base) == false)
                {
                    Target Target = new Target();

                    int NameLen = Read4Byte(Base + GetAddress("KO_OFF_NAME_LEN"));

                    Target.Base = Base;

                    Target.Id = Read4Byte(Base + GetAddress("KO_OFF_ID"));

                    if (NameLen > 15)
                        Target.Name = ReadString(Read4Byte(Base + GetAddress("KO_OFF_NAME")), NameLen);
                    else
                        Target.Name = ReadString(Base + GetAddress("KO_OFF_NAME"), NameLen);

                    Target.Nation = Read4Byte(Base + GetAddress("KO_OFF_NATION"));

                    int StateOffset = 0x2A0;

                    if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                        StateOffset = 0x2EC;

                    Target.State = ReadByte(Base + StateOffset);
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

        public int SearchMob(ref List<Target> OutTargetList)
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                return SearchTarget(0x5C, ref OutTargetList);

            return SearchTarget(0x34, ref OutTargetList);
        }

        public int SearchPlayer(ref List<Target> OutTargetList)
        {
            if (GetPlatform() == AddressEnum.Platform.USKO || GetPlatform() == AddressEnum.Platform.CNKO)
                return SearchTarget(0x7C, ref OutTargetList);

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
                ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_FLDB")) + "6A0168" + AlignDWORD(MobId) + "BF" + AlignDWORD(GetAddress("KO_FMBS")) + "FFD7A3" + AlignDWORD(Addr) + "61C3");
                int Base = Read4Byte(Addr);
                VirtualFreeEx(_Handle, Addr, 0, MEM_RELEASE);
                return Base;
            }
            else
            {
                List<Target> SearchTargetList = new List<Target>();
                if (SearchMob(ref SearchTargetList) > 0)
                {
                    Target Target = SearchTargetList
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
                ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_FLDB")) + "6A0168" + AlignDWORD(PlayerId) + "BF" + AlignDWORD(GetAddress("KO_FPBS")) + "FFD7A3" + AlignDWORD(Addr) + "61C3");
                int Base = Read4Byte(Addr);
                VirtualFreeEx(_Handle, Addr, 0, MEM_RELEASE);
                return Base;
            }
            else
            {
                List<Target> SearchTargetList = new List<Target>();
                if (SearchPlayer(ref SearchTargetList) > 0)
                {
                    Target Target = SearchTargetList
                        .Find(x => x.Id == PlayerId);

                    if (Target != null)
                        return Target.Base;
                }
            }

            return 0;
        }

        public void ClearAllMob()
        {
            List<Target> SearchTargetList = new List<Target>();
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

        public int GetPartyList(ref List<Party> PartyList)
        {
            int Base = Read4Byte(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_PtBase")) + GetAddress("KO_OFF_Pt")));

            for (int i = 0; i <= GetPartyCount() - 1; i++)
            {
                Party Party = new Party();

                Party.MemberId = Read4Byte(Base + 0x8);
                Party.MemberClass = Read4Byte(Base + 0x10);

                if (GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    Party.MemberHp = Read4Byte(Base + 0x18);
                    Party.MemberMaxHp = Read4Byte(Base + 0x1C);
                    Party.MemberBuffHp = 0;

                    Party.MemberCure1 = Read4Byte(Base + 0x28);
                    Party.MemberCure2 = Read4Byte(Base + 0x29);
                    Party.MemberCure3 = Read4Byte(Base + 0x2A);
                    Party.MemberCure4 = Read4Byte(Base + 0x2B);

                    int MemberNickLen = Read4Byte(Base + 0x44);

                    if (MemberNickLen > 15)
                        Party.MemberName = ReadString(Read4Byte(Base + 0x34), MemberNickLen);
                    else
                        Party.MemberName = ReadString(Base + 0x34, MemberNickLen);
                }
                else
                {
                    Party.MemberHp = Read4Byte(Base + 0x14);
                    Party.MemberMaxHp = Read4Byte(Base + 0x18);
                    Party.MemberBuffHp = 0;

                    Party.MemberCure1 = Read4Byte(Base + 0x24);
                    Party.MemberCure2 = Read4Byte(Base + 0x25);
                    Party.MemberCure3 = Read4Byte(Base + 0x26);
                    Party.MemberCure4 = Read4Byte(Base + 0x27);

                    int MemberNickLen = Read4Byte(Base + 0x40);

                    if (MemberNickLen > 15)
                        Party.MemberName = ReadString(Read4Byte(Base + 0x30), MemberNickLen);
                    else
                        Party.MemberName = ReadString(Base + 0x30, MemberNickLen);
                }

                if (PartyList.Contains(Party) == false)
                    PartyList.Add(Party);

                Base = Read4Byte(Base);
            }

            return PartyList.Count;
        }

        public int GetPartyCount()
        {
            return Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_PtBase")) + GetAddress("KO_OFF_PtCount"));
        }

        public bool IsPartyMember(string Name)
        {
            int Base = Read4Byte(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_PtBase")) + GetAddress("KO_OFF_Pt")));

            for (int i = 0; i <= GetPartyCount() - 1; i++)
            {
                string MemberName = "";

                if (GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    int MemberNickLen = Read4Byte(Base + 0x44);

                    if (MemberNickLen > 15)
                        MemberName = ReadString(Read4Byte(Base + 0x34), MemberNickLen);
                    else
                        MemberName = ReadString(Base + 0x34, MemberNickLen);
                }
                else
                {
                    int MemberNickLen = Read4Byte(Base + 0x40);

                    if (MemberNickLen > 15)
                        MemberName = ReadString(Read4Byte(Base + 0x30), MemberNickLen);
                    else
                        MemberName = ReadString(Base + 0x30, MemberNickLen);
                }

                if (Name == MemberName)
                    return true;

                Base = Read4Byte(Base);
            }

            return false;
        }

        public bool IsPartyMemberNeedHeal()
        {
            int Base = Read4Byte(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_PtBase")) + GetAddress("KO_OFF_Pt")));

            for (int i = 0; i <= GetPartyCount() - 1; i++)
            {
                int MemberHp = Read4Byte(Base + 0x14);
                int MemberMaxHp = Read4Byte(Base + 0x18);

                if (GetPlatform() == AddressEnum.Platform.CNKO)
                {
                    MemberHp = Read4Byte(Base + 0x18);
                    MemberMaxHp = Read4Byte(Base + 0x1C);
                }

                if (MemberHp < MemberMaxHp)
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

        public string GetProperHealthBuff(int MaxHp)
        {
            int UndyHp = (int)Math.Round((float)(MaxHp * 60.0f) / 100.0f);

            if (GetSkill(117) > 0 && GetSkillPoint(1) >= 78)
            {
                if (UndyHp >= 2500)
                    return "Undying";
                else
                    return "Superioris";
            }
            else if (GetSkill(112) > 0 && GetSkillPoint(1) >= 70)
            {
                if (UndyHp >= 2000)
                    return "Undying";
                else
                    return "Imposingness";
            }
            else if (GetSkillPoint(1) >= 57)
            {
                if (UndyHp >= 1500)
                    return "Undying";
                else
                    return "Massiveness";
            }
            else if (GetSkillPoint(1) >= 54)
            {
                if (UndyHp >= 1200)
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
            if (GetSkillPoint(1) >= 45)
                return "Superior Healing";
            else if (GetSkillPoint(1) >= 36)
                return "Massive Healing";
            else if (GetSkillPoint(1) >= 27)
                return "Great Healing";
            else if (GetSkillPoint(1) >= 18)
                return "Major Healing";
            else if (GetSkillPoint(1) >= 9)
                return "Healing";
            else if (GetSkillPoint(1) >= 0)
                return "Minor Healing";

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

        public void Test()
        {
            //ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) + "6A0068" + AlignDWORD(700039000) + "B8" + AlignDWORD(GetAddress("KO_FAKE_ITEM")) + "FFD061C3");

            //Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C4, 1);
            //Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C6, 1);
            //Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C7, 1);

            //Debug.WriteLine(GetInventoryItemId(14));

            //ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) + "6A0068" + AlignDWORD(160210195) + "B8" + AlignDWORD(GetAddress("KO_FAKE_ITEM")) + "FFD061C3");

            //ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) + "6A0068" + AlignDWORD(84738048) + "B8" + AlignDWORD(GetAddress("KO_FAKE_ITEM")) + "FFD061C3");
            //ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) + "6A0068" + AlignDWORD(700038000) + "B8" + AlignDWORD(GetAddress("KO_FAKE_ITEM")) + "FFD061C3");

            //StartRouteEvent(807, 460);

            //ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) + "6A0068" + AlignDWORD(700038000) + "B8" + AlignDWORD(GetAddress("KO_FAKE_ITEM")) + "FFD061C3");

            //SendPacket("4800");
        }

        public void ChannelList()
        {
            ExecuteRemoteCode(
                "60" +
                "8B0D" + AlignDWORD(GetAddress("KO_OTO_LOGIN_PTR")) +
                "8B89" + AlignDWORD(0x12C) +
                "68" + AlignDWORD(0xDD) +
                "BF" + AlignDWORD(GetAddress("KO_OTO_LOGIN_01")) +
                "FFD761C3");
        }

        public void SelectChannel(int ChannelId)
        {
            ExecuteRemoteCode(
                "60" +
                "8B0D" + AlignDWORD(GetAddress("KO_OTO_LOGIN_PTR")) +
                "8B89" + AlignDWORD(0x15C) +
                "6A" + AlignDWORD(ChannelId).Substring(0, 2) +
                "BF" + AlignDWORD(GetAddress("KO_OTO_LOGIN_02")) +
                "FFD761C3");
        }

        public void SelectServer(int ServerId)
        {
            ExecuteRemoteCode(
                "60" +
                "8B0D" + AlignDWORD(GetAddress("KO_OTO_LOGIN_PTR")) +
                "8B89" + AlignDWORD(0x15C) +
                "BF" + AlignDWORD(GetAddress("KO_OTO_LOGIN_03")) + "FFD731C931FF" +
                "8B0D" + AlignDWORD(GetAddress("KO_OTO_LOGIN_PTR")) +
                "8B89" + AlignDWORD(0x15C) +
                "6A" + AlignDWORD(ServerId).Substring(0, 2) +
                "BF" + AlignDWORD(GetAddress("KO_OTO_LOGIN_04")) +
                "FFD761C3");
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
                int[] HpPotion = { 389014000, 389013000, 389012000, 389011000, 389010000 };

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
            if(PotionId != 0)
            {
                if (IsInventoryItemExist(PotionId))
                    return Storage.ItemCollection.Find(x => x.Id == PotionId);
            }
            else
            {
                int[] MpPotion = { 389020000, 389019000, 389018000, 389017000, 389016000 };

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
            int[] IbexPotion = { 389070000, 389071000, 800124000, 800126000, 810189000, 810247000, 811006000, 811008000, 814679000, 900486000 };

            for (int i = 0; i < IbexPotion.Length; i++)
            {
                if (IsInventoryItemExist(IbexPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == IbexPotion[i]);
            }

            return null;
        }

        public Item FindCrisisPotion()
        {
            int[] CrisisPotion = { 389072000, 800125000, 800127000, 810192000, 810248000, 900487000, 811006000, 811008000, 814679000, 900486000 };

            for (int i = 0; i < CrisisPotion.Length; i++)
            {
                if (IsInventoryItemExist(CrisisPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == CrisisPotion[i]);
            }

            return null;
        }

        public Item FindPremiumHpPotion()
        {
            int[] PremiumHpPotion = { 389310000, 389320000, 389330000, 389390000, 900817000 };

            for (int i = 0; i < PremiumHpPotion.Length; i++)
            {
                if (IsInventoryItemExist(PremiumHpPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == PremiumHpPotion[i]);
            }

            return null;
        }

        public Item FindPremiumMpPotion()
        {
            int[] PremiumMpPotion = { 389340000, 389350000, 389360000, 389400000, 900818000 };

            for (int i = 0; i < PremiumMpPotion.Length; i++)
            {
                if (IsInventoryItemExist(PremiumMpPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == PremiumMpPotion[i]);
            }

            return null;
        }

        public Item FindQuestHpPotion()
        {
            int[] QuestHpPotion = { 931786000 };

            for (int i = 0; i < QuestHpPotion.Length; i++)
            {
                if (IsInventoryItemExist(QuestHpPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == QuestHpPotion[i]);
            }

            return null;
        }

        public Item FindQuestMpPotion()
        {
            int[] QuestMpPotion = { 931787000 };

            for (int i = 0; i < QuestMpPotion.Length; i++)
            {
                if (IsInventoryItemExist(QuestMpPotion[i]))
                    return Storage.ItemCollection.Find(x => x.Id == QuestMpPotion[i]);
            }

            return null;
        }
    }
}
