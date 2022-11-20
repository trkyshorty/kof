using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using KOF.Common;
using KOF.Models;
using KOF.UI;
using System.Diagnostics;
using System.Text.Json;

namespace KOF.Core
{
    public class Client : Processor
    {
        private Dispatcher _DispatcherInterface { get; set; }
        private int _CharacterEventTime { get; set; } = Environment.TickCount;
        private int _ProtectionEventTime { get; set; } = Environment.TickCount;
        private int _AttackEventTime { get; set; } = Environment.TickCount;
        private int _TimedSkillEventTime { get; set; } = Environment.TickCount;
        private bool _Started { get; set; } = false;
        private List<Thread> _ThreadPool { get; set; } = new List<Thread>();
        public List<PartyData> _PartyDataCollection { get; set; } = new List<PartyData>();
        public Thread _RouteThread { get; set; }

        public Client(App App, Dispatcher DispatcherInterface)
        {
            _App = App;

            _DispatcherInterface = DispatcherInterface;

            StartThread(ClientEvent);
            StartThread(RecvPacketEvent);
            StartThread(SendPacketEvent);
        }

        private void StartThread(Action p)
        {
            Thread t = new Thread(new ThreadStart(p));

            t.IsBackground = true;
            t.Start();

            _ThreadPool.Add(t);
        }

        public Dispatcher GetDispatcherInterface()
        {
            return _DispatcherInterface;
        }

        public void ReloadCollection()
        {
            LoadCollection(GetName(), GetJob());

            _DispatcherInterface.InitializeControl();
        }

        public void Start()
        {
            LoadZone();
            LoadCollection(GetName(), GetJob());

            StartThread(CharacterEvent);
            StartThread(ProtectionEvent);
            StartThread(AttackEvent);
            StartThread(TimedSkillEvent);
            StartThread(PartyEvent);
            StartThread(TargetAndActionEvent);
            StartThread(AutoLootEvent);
            StartThread(FollowEvent);

            _DispatcherInterface.InitializeControl();

            _Started = true;
        }

        public void Login(Account Account)
        {
            _AccountData = Account;
            Login(_AccountData.AccountId, _AccountData.Password);
        }

        public void Destroy()
        {
            Storage.ClientCollection.RemoveAll(x => x.GetProcessId() == GetProcessId());

            ClearCollection();

        }

        public bool IsStarted()
        {
            return _Started;
        }

        private void ClientEvent()
        {
            try
            {
                while (true)
                {
                    if (GetProcessId() > 0 && HasExited())
                        return;

                    if(GetAddressListSize() > 0)
                    {
                        //if (GetPhase() >= EPhase.Playing && Read4Byte(GetRecvHookPointer()) != _MailslotRecvHookPtr.ToInt32())
                           //PatchRecvMailslot();

                        if (GetPhase() == EPhase.None)
                        {
                            if (GetName() != "")
                                SetPhase(EPhase.Playing);
                        }

                        if (GetDisconnectTime() == 0 && IsConnectionLost())
                            SetDisconnectTime(Environment.TickCount);

                        if (!IsConnectionLost())
                            SetDisconnectTime(0);

                        _DispatcherInterface.RenderMiniMap();
                    }

                    Thread.Sleep(1250);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void RecvPacketEvent()
        {
            try
            {
                while (true)
                {
                    if (GetProcessId() > 0 && HasExited())
                        return;

                    Int32 MailslotRecvMessageSize = 0; Int32 MailslotRecvMessageLeft = 0;

                    GetMailslotInfo(_MailslotRecvPtr, IntPtr.Zero, out MailslotRecvMessageSize, out MailslotRecvMessageLeft, IntPtr.Zero);

                    if (MailslotRecvMessageSize > 0)
                    {

                        byte[] MessageBuffer = new byte[MailslotRecvMessageSize];

                        do
                        {
                            Int32 MessageReadByte = 0;

                            ReadFile(_MailslotRecvPtr, MessageBuffer, MailslotRecvMessageSize, out MessageReadByte, IntPtr.Zero);

                            if (MessageReadByte > 0)
                                ProcessRecvPacketEvent(MessageBuffer);

                            MailslotRecvMessageLeft -= 1;

                        } while (MailslotRecvMessageLeft != 0);
                    }

                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void SendPacketEvent()
        {
            try
            {
                while (true)
                {
                    if (GetProcessId() > 0 && HasExited())
                        return;

                    Int32 MailslotSendMessageSize = 0; Int32 MailslotSendMessageLeft = 0;

                    GetMailslotInfo(_MailslotSendPtr, IntPtr.Zero, out MailslotSendMessageSize, out MailslotSendMessageLeft, IntPtr.Zero);

                    if (MailslotSendMessageSize > 0)
                    {
                        byte[] MessageBuffer = new byte[MailslotSendMessageSize];

                        do
                        {
                            Int32 MessageReadByte = 0;

                            ReadFile(_MailslotSendPtr, MessageBuffer, MailslotSendMessageSize, out MessageReadByte, IntPtr.Zero);

                            if (MessageReadByte > 0)
                                ProcessSendPacketEvent(MessageBuffer);

                            MailslotSendMessageLeft -= 1;

                        } while (MailslotSendMessageLeft != 0);
                    }

                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void CharacterEvent()
        {
            try
            {
                while (true)
                {
                    if (HasExited())
                        return;

                    if (!IsCharacterAvailable())
                    {
                        Thread.Sleep(1250);
                        continue;
                    }

                    if (Convert.ToBoolean(GetControl("SpeedHack")) && GetAsyncKeyState(System.Windows.Forms.Keys.G) != 0)
                    {
                        if (GetTargetId() > 0)
                            SetCoordinate(GetTargetX(), GetTargetY());
                        else
                        {
                            if (CoordinateDistance(GetX(), GetY(), GetGoX(), GetGoY()) <= 300)
                                SetCoordinate(GetGoX(), GetGoY());
                        }
                    }

                    if (Convert.ToBoolean(GetControl("Suicide")))
                    {
                        double SuicidePercent = Math.Round(((double)GetHp() * 100) / GetMaxHp());

                        if (SuicidePercent <= Convert.ToInt32(GetControl("SuicidePercent")))
                        {
                            SendPacket("290103");
                            SendPacket("1200");

                            SetEnterGameTime(Environment.TickCount);
                        }
                    }

                    if (_PartyRequestTime > 0 && Environment.TickCount - _PartyRequestTime > 3000)
                    {
                        if (GetPartyCount() == 0)
                            SendPacket("2F0201");

                        _PartyRequestTime = 0;
                    }

                    if (_CharacterEventTime % 250 == 0)
                    {
                        if (Convert.ToBoolean(GetControl("DeathOnBorn")))
                        {
                            if (GetHp() == 0)
                                SendPacket("1200");
                        }
                    }

                    if (_CharacterEventTime % 1250 == 0 && Convert.ToBoolean(GetControl("Bot")) && GetAction() != EAction.Routing && !IsInEnterGame())
                    {
                        if (IsNeedRepair())
                        {
                            if(Convert.ToBoolean(GetControl("RepairMagicHammer")))
                                SendPacket("3103" + AlignDWORD(490202) + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "00000000000000000000000000000000");
                            else if (Convert.ToBoolean(GetControl("RepairSunderies")))
                            {
                                Route route = Database().GetRoute(Convert.ToInt32(GetControl("SupplyRoute")));

                                if (route != null)
                                {
                                    var routeData = JsonSerializer.Deserialize<List<RouteData>>(route.Data);

                                    var sunderiesCount = routeData.Where(p => p.Action == RouteData.Event.SUNDERIES).Count();

                                    if (sunderiesCount > 0)
                                        Route(route);
                                }
                            }
                        }

                        if(IsInventoryFull() && Convert.ToBoolean(GetControl("SellInventoryFull")))
                        {
                            Route route = Database().GetRoute(Convert.ToInt32(GetControl("SupplyRoute")));

                            if (route != null)
                            {
                                var routeData = JsonSerializer.Deserialize<List<RouteData>>(route.Data);

                                var npcCount = routeData.Where(p => p.Action == RouteData.Event.SUNDERIES || p.Action == RouteData.Event.POTION).Count();

                                if (npcCount > 0)
                                    Route(route); 
                            }
                        }

                        List<Supply> Supply = new List<Supply>();

                        if (IsNeedSupply(ref Supply, false))
                        {
                            Route route = Database().GetRoute(Convert.ToInt32(GetControl("SupplyRoute")));

                            if (Supply.Count() > 0)
                            {
                                var routeData = JsonSerializer.Deserialize<List<RouteData>>(route.Data);

                                var npcCount = routeData.Where(p => p.Action == RouteData.Event.SUNDERIES || p.Action == RouteData.Event.POTION).Count();

                                if (npcCount > 0)
                                    Route(route);
                            }
                        }
                    }

                    if (Convert.ToBoolean(GetControl("Wallhack")) && !IsInEnterGame())
                    {
                        if (Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_WH")) == 1)
                            Wallhack(true);
                    }
                    else
                    {
                        if (Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_WH")) == 0)
                            Wallhack(false);
                    }

                    if (Convert.ToBoolean(GetControl("RouteSave")))
                        RouteSaveEvent();

                    _CharacterEventTime = Environment.TickCount;

                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void ProtectionEvent()
        {
            try
            {
                while (true)
                {
                    if (HasExited())
                        return;

                    if (!IsCharacterAvailable())
                    {
                        Thread.Sleep(1250);
                        continue;
                    }

                    if (_ProtectionEventTime % 30 == 0)
                    {
                        if (Convert.ToBoolean(GetControl("Minor")))
                        {
                            double MinorPercent = Math.Round(((double)GetHp() * 100) / GetMaxHp());

                            if (MinorPercent <= Convert.ToInt32(GetControl("MinorPercent")))
                                UseMinorHealing(GetId());
                        }
                    }

                    if (_ProtectionEventTime % 250 == 0)
                    {
                        if (!IsInEnterGame() && Convert.ToBoolean(GetControl("AreaControl")) && GetAction() == EAction.None)
                        {
                            if (Convert.ToBoolean(GetControl("FollowDisable")) || (!Convert.ToBoolean(GetControl("FollowDisable")) && IsFollowOwner()))
                            {
                                if (Convert.ToInt32(GetControl("AreaControlX")) > 0 && Convert.ToInt32(GetControl("AreaControlY")) > 0
                                && CoordinateDistance(GetX(), GetY(), Convert.ToInt32(GetControl("AreaControlX")), Convert.ToInt32(GetControl("AreaControlY"))) > Convert.ToInt32(GetControl("AttackDistance")))
                                {
                                    if (GetPlatform() == AddressEnum.Platform.CNKO || GetPlatform() == AddressEnum.Platform.USKO)
                                        MoveCoordinate(Convert.ToInt32(GetControl("AreaControlX")), Convert.ToInt32(GetControl("AreaControlY")));
                                    else
                                        SetCoordinate(Convert.ToInt32(GetControl("AreaControlX")), Convert.ToInt32(GetControl("AreaControlY")));

                                    Thread.Sleep(1250);
                                }
                            }
                        }
                    }

                    if (_ProtectionEventTime % 1250 == 0)
                    {
                        if (!IsInEnterGame() && Convert.ToBoolean(GetControl("Transformation")) && IsTransformationAvailableZone())
                        {
                            int TransformationId = 0;

                            switch (GetControl("TransformationName"))
                            {
                                case "Kecoon":
                                    TransformationId = 472020;
                                    break;
                                case "Orc Bowman":
                                    TransformationId = 472310;
                                    break;
                                case "Death Knight":
                                    TransformationId = 472150;
                                    break;
                                case "Burning Skeleton":
                                    TransformationId = 472202;
                                    break;
                                case "Bulture":
                                    TransformationId = 472040;
                                    break;
                                case "Human Hera (L) 1":
                                    TransformationId = 520530;
                                    break;
                                case "Human Hera (L) 2":
                                    TransformationId = 520531;
                                    break;
                                case "Karus Hera (PUS)":
                                    TransformationId = 500512;
                                    break;
                                case "Karus Cougar (PUS)":
                                    TransformationId = 500511;
                                    break;
                                case "Karus Menicia (PUS)":
                                    TransformationId = 500510;
                                    break;
                                case "Karus Patrick (PUS)":
                                    TransformationId = 500509;
                                    break;
                                case "Human Hera (PUS)":
                                    TransformationId = 500508;
                                    break;
                                case "Human Cougar (PUS)":
                                    TransformationId = 500507;
                                    break;
                                case "Human Menicia (PUS)":
                                    TransformationId = 500506;
                                    break;
                                case "Human Patrick (PUS)":
                                    TransformationId = 500505;
                                    break;
                            }

                            if (IsInventoryItemExist(379091000) && !IsSkillAffected(TransformationId))
                            {
                                SendPacket("3103" +
                                 AlignDWORD(TransformationId).Substring(0, 6) +
                                 "00" +
                                 AlignDWORD(GetId()).Substring(0, 4) +
                                 AlignDWORD(GetId()).Substring(0, 4) +
                                 "000000000000000000000000");
                            }

                        }
                    }

                    if (_ProtectionEventTime % 2250 == 0)
                    {
                        if (Convert.ToBoolean(GetControl("AreaHeal")))
                        {
                            {
                                SendPacket("3103" +
                                  AlignDWORD(492060) +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  "FFFF00000000000000000000000000000000");
                            }

                        }
                    }

                    if (_ProtectionEventTime % 2000 == 0)
                    {
                        double HpPotionPercent = Math.Round(((double)GetHp() * 100) / GetMaxHp());

                        if (Convert.ToBoolean(GetControl("HpPotion")) && HpPotionPercent <= Convert.ToInt32(GetControl("HpPotionPercent")))
                        {
                            Item HpPotionItem = null;

                            switch (GetControl("HpPotionItem"))
                            {
                                case "Otomatik":
                                    HpPotionItem = FindQuestHpPotion();

                                    if (HpPotionItem == null)
                                        HpPotionItem = FindHpPotion();

                                    break;
                                case "Water of favors":
                                    HpPotionItem = FindHpPotion(389014000);
                                    break;
                                case "Water of grace":
                                    HpPotionItem = FindHpPotion(389013000);
                                    break;
                                case "Water of love":
                                    HpPotionItem = FindHpPotion(389012000);
                                    break;
                                case "Water of life":
                                    HpPotionItem = FindHpPotion(389011000);
                                    break;
                                case "Holy water":
                                    HpPotionItem = FindHpPotion(389010000);
                                    break;
                                case "Water of Ibexs":
                                    HpPotionItem = FindIbexPotion();
                                    break;
                                case "Premium Potion HP":
                                    HpPotionItem = FindPremiumHpPotion();
                                    break;
                                case "Quest HP Potion":
                                    HpPotionItem = FindQuestHpPotion();
                                    break;
                            }

                            if (HpPotionItem != null)
                            {
                                SendPacket("3103" +
                                  AlignDWORD(HpPotionItem.CastSkill).Substring(0, 6) +
                                  "00" +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  "0000000000000000000000000000");

                                Thread.Sleep(100);
                            }
                        }

                        double MpPotionPercent = Math.Round(((double)GetMp() * 100) / GetMaxMp());

                        if (Convert.ToBoolean(GetControl("MpPotion")) && MpPotionPercent <= Convert.ToInt32(GetControl("MpPotionPercent")))
                        {
                            Item MpPotionItem = null;

                            switch (GetControl("MpPotionItem"))
                            {
                                case "Otomatik":
                                    MpPotionItem = FindQuestMpPotion();
                                    if (MpPotionItem == null)
                                        MpPotionItem = FindMpPotion();
                                    break;
                                case "Potion of Soul":
                                    MpPotionItem = FindMpPotion(389020000);
                                    break;
                                case "Potion of Wisdom":
                                    MpPotionItem = FindMpPotion(389019000);
                                    break;
                                case "Potion of Sagacity":
                                    MpPotionItem = FindMpPotion(389018000);
                                    break;
                                case "Potion of Intelligence":
                                    MpPotionItem = FindMpPotion(389017000);
                                    break;
                                case "Potion of Spirit":
                                    MpPotionItem = FindMpPotion(389016000);
                                    break;
                                case "Potion of Crisis":
                                    MpPotionItem = FindCrisisPotion();
                                    break;
                                case "Premium Potion MP":
                                    MpPotionItem = FindPremiumMpPotion();
                                    break;
                                case "Quest MP Potion":
                                    MpPotionItem = FindQuestMpPotion();
                                    break;
                            }

                            if (MpPotionItem != null)
                            {
                                SendPacket("3103" +
                                  AlignDWORD(MpPotionItem.CastSkill).Substring(0, 6) +
                                  "00" +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  "0000000000000000000000000000");

                                Thread.Sleep(100);
                            }
                        }
                    }

                    if (_ProtectionEventTime % 3000 == 0)
                    {
                        if (Convert.ToBoolean(GetControl("StatScroll")))
                        {
                            if (!IsSkillAffected(492059))
                                SendPacket("3103" +
                                  AlignDWORD(492059) +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  "FFFF00000000000000000000000000000000");
                        }

                        if (Convert.ToBoolean(GetControl("AttackScroll")))
                        {
                            if (!IsSkillAffected(500271))
                                SendPacket("3103" +
                                  AlignDWORD(500271) +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  "00000000000000000000000000000000");
                        }

                        if (Convert.ToBoolean(GetControl("AcScroll")))
                        {
                            if (!IsSkillAffected(492061) && !IsSkillAffected(492024))
                                SendPacket("3103" +
                                  AlignDWORD(492061) +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  "FFFF00000000000000000000000000000000");
                        }

                        if (Convert.ToBoolean(GetControl("DropScroll")))
                        {
                            if (!IsSkillAffected(492024) && !IsSkillAffected(492023))
                                SendPacket("3103" +
                                  AlignDWORD(492023) +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  "00000000000000000000000000000000");

                            if (!IsSkillAffected(492023) && !IsSkillAffected(492024) && !IsSkillAffected(492061))
                                SendPacket("3103" +
                                  AlignDWORD(492024) +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  AlignDWORD(GetId()).Substring(0, 4) +
                                  "00000000000000000000000000000000");
                        }
                    }

                    _ProtectionEventTime = Environment.TickCount;

                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void AttackEvent()
        {
            try
            {
                while (true)
                {
                    if (HasExited())
                        return;

                    if (!IsCharacterAvailable() || GetAction() != EAction.None || IsInEnterGame())
                    {
                        Thread.Sleep(1250);
                        continue;
                    }

                    if (!Convert.ToBoolean(GetControl("Attack")))
                    {
                        Thread.Sleep(1250);
                        continue;
                    }

                    if (Convert.ToBoolean(GetControl("Attack")) && _AttackEventTime % Convert.ToInt32(GetControl("AttackSpeed")) == 0)
                    {
                        var SkillBarList = GetSkillBarList().FindAll(x => x.SkillType == 1);

                        for (int i = 0; i < SkillBarList.Count; i++)
                        {
                            SkillBar SkillBarData = SkillBarList[i];

                            if (SkillBarData == null) continue;

                            Skill SkillData = GetSkillData(SkillBarData.SkillId);

                            if (SkillData == null) continue;

                            if (SkillData.Cooldown == 0 || Environment.TickCount > SkillBarData.UseTime + (SkillData.Cooldown * 1000))
                            {
                                if (UseSkill(SkillData, GetTargetId()))
                                {
                                    SkillBarData.UseTime = Environment.TickCount;
                                    Thread.Sleep(800);
                                }
                            }
                        }
                    }

                    _AttackEventTime = Environment.TickCount;

                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void TimedSkillEvent()
        {
            try
            {
                while (true)
                {
                    if (HasExited())
                        return;

                    if (!IsCharacterAvailable() || !Convert.ToBoolean(GetControl("TimedSkill")))
                    {
                        Thread.Sleep(1250);
                        continue;
                    }

                    if (_TimedSkillEventTime % 2250 == 0)
                    {
                        var SkillBarList = GetSkillBarList().FindAll(x => x.SkillType == 2);

                        for (int i = 0; i < SkillBarList.Count; i++)
                        {
                            SkillBar SkillBarData = SkillBarList[i];

                            if (SkillBarData == null) continue;

                            Skill SkillData = GetSkillData(SkillBarData.SkillId);

                            if (SkillData == null) continue;

                            if (!Convert.ToBoolean(GetControl("WaitTime")) || SkillData.Cooldown == 0 || Environment.TickCount > SkillBarData.UseTime + (SkillData.Cooldown * 1000))
                            {
                                if (UseSkill(SkillData))
                                {
                                    SkillBarData.UseTime = Environment.TickCount;
                                    Thread.Sleep(800);
                                }
                            }
                        }
                    }

                    _TimedSkillEventTime = Environment.TickCount;

                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void PartyEvent()
        {
            try
            {
                while (true)
                {
                    if (HasExited() || GetJob(GetClass()) == "Warrior")
                        return;

                    if (!IsCharacterAvailable() || IsInEnterGame())
                    {
                        if (_PartyDataCollection.Count > 0)
                            _PartyDataCollection.Clear();

                        Thread.Sleep(1250);
                        continue;
                    }

                    List<Player> partyList = new List<Player>();

                    if (GetPartyList(ref partyList) > 0)
                    {
                        partyList = partyList.OrderBy(x => Math.Round(((double)x.Hp * 100) / x.MaxHp)).ToList();

                        partyList.ForEach(x =>
                        {
                            switch (GetJob(GetClass()))
                            {
                                case "Rogue":
                                    {
                                        if (GetPartyAllowedSize() > 0 && !GetPartyAllowed(x.Id))
                                            return;

                                        int playerBase = Read4Byte(GetAddress("KO_PTR_CHR"));

                                        if (GetId() != x.Id)
                                            playerBase = GetPlayerBase(x.Id);

                                        PartyRogueHealAction(x);

                                        PartyData partyData = _PartyDataCollection.Where(p => p.Id == x.Id).SingleOrDefault();

                                        if (partyData == null)
                                        {
                                            int memberX = (int)Math.Round(ReadFloat(playerBase + GetAddress("KO_OFF_X")));
                                            int memberY = (int)Math.Round(ReadFloat(playerBase + GetAddress("KO_OFF_Y")));

                                            int memberDistance = CoordinateDistance(GetX(), GetY(), memberX, memberY);

                                            if (playerBase > 0 && memberDistance <= 30)
                                            {
                                                PartyRogueAction(x);

                                                partyData = new PartyData();

                                                partyData.Id = x.Id;
                                                partyData.SwiftedAt = Environment.TickCount;

                                                _PartyDataCollection.Add(partyData);
                                            }
                                        }
                                        else
                                        {
                                            if (Environment.TickCount - partyData.SwiftedAt > (1000 * 600))
                                            {
                                                int memberX = (int)Math.Round(ReadFloat(playerBase + GetAddress("KO_OFF_X")));
                                                int memberY = (int)Math.Round(ReadFloat(playerBase + GetAddress("KO_OFF_Y")));

                                                int memberDistance = CoordinateDistance(GetX(), GetY(), memberX, memberY);

                                                if (playerBase > 0 && memberDistance <= 30)
                                                {
                                                    PartyRogueAction(x);

                                                    partyData.SwiftedAt = Environment.TickCount;
                                                }
                                            }
                                        }
                                    }
                                    break;

                                case "Mage":
                                    {
                                        if (GetPartyAllowedSize() > 0 && !GetPartyAllowed(x.Id))
                                            return;

                                        int playerBase = Read4Byte(GetAddress("KO_PTR_CHR"));

                                        if (GetId() != x.Id)
                                            playerBase = GetPlayerBase(x.Id);

                                        int memberX = (int)Math.Round(ReadFloat(playerBase + GetAddress("KO_OFF_X")));
                                        int memberY = (int)Math.Round(ReadFloat(playerBase + GetAddress("KO_OFF_Y")));

                                        int memberDistance = CoordinateDistance(GetX(), GetY(), memberX, memberY);

                                        if (playerBase == 0 || (playerBase > 0 && memberDistance > 30))
                                            PartyMageAction(x);
                                    }
                                    break;

                                case "Priest":
                                    {
                                        if (GetPartyAllowedSize() > 0 && !GetPartyAllowed(x.Id))
                                            return;

                                        int playerBase = Read4Byte(GetAddress("KO_PTR_CHR"));

                                        if (GetId() != x.Id)
                                            playerBase = GetPlayerBase(x.Id);

                                        int memberX = (int)Math.Round(ReadFloat(playerBase + GetAddress("KO_OFF_X")));
                                        int memberY = (int)Math.Round(ReadFloat(playerBase + GetAddress("KO_OFF_Y")));

                                        int memberOldMaxHp = GetPartyMemberMaxHp(x.Id);

                                        int memberDistance = CoordinateDistance(GetX(), GetY(), memberX, memberY);

                                        if (playerBase > 0 && memberDistance <= 25)
                                            PartyPriestHealAction(x);

                                        PartyData partyData = _PartyDataCollection.Where(p => p.Id == x.Id).SingleOrDefault();

                                        if (partyData == null)
                                        {
                                            PartyPriestAction(x);

                                            partyData = new PartyData();


                                            partyData.Id = x.Id;
                                            partyData.LastHp = GetPartyMemberMaxHp(x.Id) == memberOldMaxHp ? 0 : GetPartyMemberMaxHp(x.Id);
                                            partyData.BuffedAt = Environment.TickCount;

                                            _PartyDataCollection.Add(partyData);
                                        }
                                        else
                                        {
                                            if (partyData.LastHp != GetPartyMemberMaxHp(x.Id)
                                                 || Environment.TickCount - partyData.BuffedAt > (1000 * 600))
                                            {
                                                PartyPriestAction(x);

                                                partyData.LastHp = GetPartyMemberMaxHp(x.Id) == memberOldMaxHp ? 0 : GetPartyMemberMaxHp(x.Id);
                                                partyData.BuffedAt = Environment.TickCount;
                                            }
                                        }
                                    }
                                    break;
                            }
                        });
                    }
                    else
                    {
                        switch (GetJob(GetClass()))
                        {
                            case "Priest":
                                {
                                    Player player = new Player();

                                    player.Id = GetId();
                                    player.Class = GetClass();
                                    player.Name = GetName();
                                    player.Hp = GetHp();
                                    player.MaxHp = GetMaxHp();

                                    PartyPriestHealAction(player);
                                    PartyPriestAction(player);
                                }
                                break;
                        }
                    }

                    Thread.Sleep(100);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void TargetAndActionEvent()
        {
            try
            {
                while (true)
                {
                    if (HasExited())
                        return;

                    if (!IsCharacterAvailable() || GetAction() != EAction.None || IsInEnterGame() || IsMovingLoot() || !Convert.ToBoolean(GetControl("Attack")))
                    {
                        Thread.Sleep(1250);
                        continue;
                    }

                    if (!Convert.ToBoolean(GetControl("FollowDisable")) && !IsFollowOwner())
                    {
                        Thread.Sleep(1250);
                        continue;
                    }

                    if (Convert.ToBoolean(GetControl("TargetAutoSelect")) || GetAttackableTargetSize() > 0)
                    {
                        if (IsFollowOwner() || Convert.ToBoolean(GetControl("FollowDisable")))
                        {
                            int TargetId = GetTargetId();

                            if (TargetId > 0)
                            {
                                int Base = GetTargetBase(TargetId);

                                if (Base == 0 ||
                                    (!Convert.ToBoolean(GetControl("TargetWaitDown")) &&
                                    (ReadByte(Base + GetAddress("KO_OFF_STATE")) == 10 || ReadByte(Base + GetAddress("KO_OFF_STATE")) == 11 ||
                                    (Read4Byte(Base + GetAddress("KO_OFF_MAX_HP")) != 0 && Read4Byte(Base + GetAddress("KO_OFF_HP")) == 0))))
                                    SelectTarget(0);
                            }
                            else
                            {
                                List<TargetInfo> TargetList = new List<TargetInfo>();
                                SearchMob(ref TargetList);

                                if (TargetList.Count == 0)
                                    SelectTarget(0);
                                else
                                {
                                    if (GetAttackableTargetSize() > 0)
                                    {
                                        if (Convert.ToBoolean(GetControl("TargetOpponentNation")))
                                            SearchPlayer(ref TargetList);

                                        if (TargetList.Count == 0)
                                            SelectTarget(0);
                                        else
                                        {
                                            TargetInfo Target = TargetList
                                            .FindAll(x => IsSelectableTarget(x.Id)
                                                        && IsInAttackableTargetList(x.Name)
                                                        && CoordinateDistance(x.X, x.Y, GetX(), GetY()) < Convert.ToInt32(GetControl("AttackDistance")))
                                            .GroupBy(x => Math.Pow((GetX() - x.X), 2) + Math.Pow((GetY() - x.Y), 2))
                                            .OrderBy(x => x.Key)
                                            ?.FirstOrDefault()
                                            ?.FirstOrDefault();

                                            if (Target != null)
                                                SelectTarget(Read4Byte(Target.Base + GetAddress("KO_OFF_ID")));
                                            else
                                                SelectTarget(0);
                                        }
                                    }
                                    else if (Convert.ToBoolean(GetControl("TargetOpponentNation")))
                                    {
                                        SearchPlayer(ref TargetList);

                                        if (TargetList.Count == 0)
                                            SelectTarget(0);
                                        else
                                        {
                                            TargetInfo Target = TargetList
                                            .FindAll(x => IsSelectableTarget(x.Id)
                                                        && (Convert.ToBoolean(GetControl("TargetOpponentNation")) && x.Nation != 3 && x.Nation != GetNation())
                                                        && CoordinateDistance(x.X, x.Y, GetX(), GetY()) < Convert.ToInt32(GetControl("AttackDistance")))
                                            .GroupBy(x => Math.Pow((GetX() - x.X), 2) + Math.Pow((GetY() - x.Y), 2))
                                            .OrderBy(x => x.Key)
                                            ?.FirstOrDefault()
                                            ?.FirstOrDefault();

                                            if (Target != null)
                                                SelectTarget(Read4Byte(Target.Base + GetAddress("KO_OFF_ID")));
                                            else
                                                SelectTarget(0);
                                        }
                                    }
                                    else
                                    {
                                        TargetInfo Target = TargetList
                                            .FindAll(x => IsSelectableTarget(x.Id)
                                                        && x.Nation == 0
                                                        && CoordinateDistance(x.X, x.Y, GetX(), GetY()) < Convert.ToInt32(GetControl("AttackDistance")))
                                            .GroupBy(x => Math.Pow((GetX() - x.X), 2) + Math.Pow((GetY() - x.Y), 2))
                                            .OrderBy(x => x.Key)
                                            ?.FirstOrDefault()
                                            ?.FirstOrDefault();

                                        if (Target != null)
                                            SelectTarget(Read4Byte(Target.Base + GetAddress("KO_OFF_ID")));
                                        else
                                            SelectTarget(0);
                                    }
                                }
                            }
                        }
                    }

                    if (IsFollowOwner() || (Convert.ToBoolean(GetControl("FollowDisable"))))
                    {
                        if (Convert.ToBoolean(GetControl("Attack")) && IsAttackableTarget(GetTargetId()))
                        {
                            int TargetX = GetTargetX(); int TargetY = GetTargetY();
                            if ((GetTargetId() > 0) && (TargetX != GetX() || TargetY != GetY()))
                            {
                                if (Convert.ToBoolean(GetControl("ActionMove")))
                                    MoveCoordinate(TargetX, TargetY);
                                else if (Convert.ToBoolean(GetControl("ActionSetCoordinate")))
                                    SetCoordinate(TargetX, TargetY);
                            }
                        }
                    }

                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void AutoLootEvent()
        {
            try
            {
                while (true)
                {
                    if (HasExited())
                        return;

                    if (!IsCharacterAvailable())
                    {
                        Thread.Sleep(1250);
                        continue;
                    }

                    if (Convert.ToBoolean(GetControl("OnlyNoah")) || Convert.ToBoolean(GetControl("LootOnlyList")) || Convert.ToBoolean(GetControl("LootOnlySell")))
                    {
                        var AutoLootPool = _AutoLootCollection.ToList();

                        foreach (var LootData in AutoLootPool)
                        {
                            if (LootData == null) continue;

                            if (GetTargetBase(LootData.MobId) != 0) continue;

                            if (Convert.ToBoolean(GetControl("MoveToLoot")))
                            {
                                while (CoordinateDistance(GetX(), GetY(), LootData.X, LootData.Y) >= 3 
                                    && CoordinateDistance(GetX(), GetY(), LootData.X, LootData.Y) <= 50
                                    && Environment.TickCount - LootData.DropTime <= 20000)
                                {
                                    SetMovingLoot(true);

                                    if (Convert.ToBoolean(GetControl("ActionMove")))
                                        MoveCoordinate(LootData.X, LootData.Y);
                                    else if (Convert.ToBoolean(GetControl("ActionSetCoordinate")))
                                        SetCoordinate(LootData.X, LootData.Y);
                                    else
                                        MoveCoordinate(LootData.X, LootData.Y);

                                    Thread.Sleep(1);
                                }
                            }

                            if(CoordinateDistance(GetX(), GetY(), LootData.X, LootData.Y) <= 3 && Environment.TickCount - LootData.DropTime <= 20000)
                                SendPacket("24" + LootData.Id);

                            _AutoLootCollection.RemoveAll(x => x.Id == LootData.Id);
                            SetMovingLoot(false);
                        }
                    }

                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void FollowEvent()
        {
            try
            {
                while (true)
                {
                    if (HasExited())
                        return;

                    if (Storage.FollowedClient == null || !IsCharacterAvailable() || GetAction() != EAction.None 
                        || IsInEnterGame() || Convert.ToBoolean(GetControl("FollowDisable")) || IsFollowOwner() || IsMovingLoot())
                    {
                        Thread.Sleep(1250);
                        continue;
                    }

                    if ((GetX() != Storage.FollowedClient.GetX() || GetY() != Storage.FollowedClient.GetY()))
                    {
                        if (GetPlatform() == AddressEnum.Platform.JPKO)
                            SetCoordinate(Storage.FollowedClient.GetX(), Storage.FollowedClient.GetY());
                        else
                        {
                            if(CoordinateDistance(Storage.FollowedClient.GetX(), Storage.FollowedClient.GetY(), GetX(), GetY()) <= 100)
                                MoveCoordinate(Storage.FollowedClient.GetX(), Storage.FollowedClient.GetY());
                        }           
                    }

                    if (Storage.FollowedClient.GetTargetId() != GetTargetId() 
                        && CoordinateDistance(Storage.FollowedClient.GetX(), Storage.FollowedClient.GetY(), GetX(), GetY()) <= 100)
                        SelectTarget(Storage.FollowedClient.GetTargetId());

                    Thread.Sleep(1);
                };
            }
            catch (ThreadAbortException ex)
            {
                Debug.WriteLine("Thread is aborted and the code is "
                                                 + ex.ExceptionState);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        public void Route(Route route)
        {
            if (_RouteThread != null && _RouteThread.IsAlive)
            {
                SetAction(EAction.None);
                _RouteThread.Abort();
                return;
            }

            _RouteThread = new Thread(() =>
            {
                SetAction(EAction.Routing);

                var routeData = JsonSerializer.Deserialize<List<RouteData>>(route.Data);

                routeData.ForEach(x =>
                {
                    switch (x.Action)
                    {
                        case RouteData.Event.START:
                        case RouteData.Event.MOVE:
                            {
                                while (CoordinateDistance(x.X, x.Y, GetX(), GetY()) > 1)
                                {
                                    if (GetPlatform() == AddressEnum.Platform.JPKO)
                                        SetCoordinate(x.X, x.Y);
                                    else
                                        MoveCoordinate(x.X, x.Y);

                                    Thread.Sleep(10);
                                }
                            }
                            break;

                        case RouteData.Event.TOWN:
                            SendPacket("4800", 1250);
                            break;

                        case RouteData.Event.GATE:
                        case RouteData.Event.OBJECT:
                            SendPacket(x.Packet, 1250);
                            break;

                        case RouteData.Event.SUNDERIES:
                            {
                                while (CoordinateDistance(x.X, x.Y, GetX(), GetY()) > 1)
                                {
                                    if (GetPlatform() == AddressEnum.Platform.JPKO)
                                        SetCoordinate(x.X, x.Y);
                                    else
                                        MoveCoordinate(x.X, x.Y);

                                    Thread.Sleep(10);
                                }

                                if (Convert.ToBoolean(GetControl("SellInventoryFull")))
                                    SellItemAction(x.Npc, 0);

                                BuyItemAction(x.Npc, 0);
                                RepairEquipmentAction(x.Npc);
                            }
                            break;

                        case RouteData.Event.POTION:
                            {
                                while (CoordinateDistance(x.X, x.Y, GetX(), GetY()) > 1)
                                {
                                    if (GetPlatform() == AddressEnum.Platform.JPKO)
                                        SetCoordinate(x.X, x.Y);
                                    else
                                        MoveCoordinate(x.X, x.Y);

                                    Thread.Sleep(10);
                                }

                                if (Convert.ToBoolean(GetControl("SellInventoryFull")))
                                    SellItemAction(x.Npc, 1);

                                BuyItemAction(x.Npc, 1);
                            }
                            break;

                        case RouteData.Event.INN:
                            {
                                while (CoordinateDistance(x.X, x.Y, GetX(), GetY()) > 1)
                                {
                                    if (GetPlatform() == AddressEnum.Platform.JPKO)
                                        SetCoordinate(x.X, x.Y);
                                    else
                                        MoveCoordinate(x.X, x.Y);

                                    Thread.Sleep(10);
                                }

                                if (Convert.ToBoolean(GetControl("SellInventoryFull")))
                                    SellItemAction(x.Npc, 1);

                                BuyItemAction(x.Npc, 1);
                            }
                            break;
                    }
                });

                SetAction(EAction.None);
            });

            _RouteThread.IsBackground = true;
            _RouteThread.Start();
        }
    }
}