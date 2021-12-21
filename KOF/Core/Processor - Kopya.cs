using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using KOF.Services;
using KOF.Models;
using KOF.Common;

namespace KOF.Core
{
    public class Processor : AddressFinder
    {
        #region "Variables"
        public App App { get; private set; }
        public Process Process { get; private set; }
        public IntPtr Handle { get; private set; }
        public int ProcessId { get; private set; }
        public IntPtr MailslotRecvPtr { get; private set; }
        public IntPtr MailslotRecvFuncPtr { get; private set; }
        public IntPtr MailslotRecvHookPtr { get; private set; }
        public IntPtr MailslotSendPtr { get; private set; }
        public IntPtr MailslotSendHookPtr { get; private set; }
        private EPhase Phase { get; set; }
        private EAction Action { get; set; }
        private int LoginTime { get; set; }
        private int DisconnectTime { get; set; }
        private int FallbackTime { get; set; }
        private int EnterGameTime { get; set; }
        private AddressEnum.Platform Platform { get; set; }
        private IntPtr PacketPtr { get; set; }
        private IntPtr CodePtr { get; set; }
        private IntPtr PercentageSkillPtr { get; set; }
        private IntPtr ArcheryPtr { get; set; }
        private IntPtr MultipleShotPtr { get; set; }
        private IntPtr CounterStrikePtr { get; set; }
        private IntPtr ArrowShowerPtr { get; set; }
        private IntPtr StealthPtr { get; set; }
        private IntPtr WolfPtr { get; set; }
        private IntPtr LupineEyesPtr { get; set; }
        private IntPtr GainPtr { get; set; }
        private IntPtr TargetSkillPtr { get; set; }
        private IntPtr MageSkillType1Ptr { get; set; }
        private IntPtr MageSkillType3Ptr { get; set; }
        private IntPtr MageSkillType4Ptr { get; set; }
        private IntPtr TargetAreaSkillPtr { get; set; }
        private List<string> TargetAllowedCollection { get; set; } = new List<string>();
        public int SupplyEventAfterWaitTime { get; set; } = 0;
        public int RepairEventAfterWaitTime { get; set; } = 0;
        public List<Party> PartyCollection { get; set; } = new List<Party>();
        private string CharacterName { get; set; } = "";
        private string AccountName { get; set; }
        private List<string> PartyAllowedCollection { get; set; } = new List<string>();

        public enum EPhase : short
        {
            None = 0,
            Disconnected = 1,
            Loggining = 2,
            Selecting = 3,
            Selected = 4,
            Warping = 5,
            Playing = 6,
        }
        public enum EAction : short
        {
            None = 0,
            Repairing = 1,
            Supplying = 2,
        }
        #endregion

        #region "Processor"
        public void HandleProcess(App App, Process Process, AddressEnum.Platform Platform)
        {
            this.App = App;
            this.Process = Process;

            this.Handle = Process.Handle;
            this.ProcessId = Process.Id;
            this.Platform = Platform;

            if (this.Process.MainWindowTitle == "Knight OnLine Client")
            {
                Console.WriteLine("Client -> Patch Mutant");
                PatchMutant(GetAccountName());
            }

            if (Storage.AddressCollection.ContainsKey(this.Platform) == false)
                Storage.AddressCollection.Add(this.Platform, LoadAddressList(this.Handle, Platform));

            foreach (AddressStorage Address in GetAddressList())
            {
                //Console.WriteLine(Address.Name + " : " + Address.Address);
            }

        }

        public bool HasExited()
        {
            return this.Process != null && this.Process.HasExited;
        }

        public int GetAddress(string Name)
        {
            if (this.Process == null) return 0;
            List<AddressStorage> AddressList;
            if (Storage.AddressCollection.TryGetValue(this.Platform, out AddressList))
                return int.Parse(AddressList.Where(x => x.Name == Name)?.SingleOrDefault().Address, System.Globalization.NumberStyles.HexNumber);

            return 0;
        }

        public List<AddressStorage> GetAddressList()
        {
            if (this.Process == null) return null;
            List<AddressStorage> AddressList;
            if (Storage.AddressCollection.TryGetValue(this.Platform, out AddressList))
                return AddressList;

            return null;
        }

        public int GetAddressListSize()
        {
            if (this.Process == null) return 0;
            List<AddressStorage> AddressList;
            if (Storage.AddressCollection.TryGetValue(this.Platform, out AddressList))
                return AddressList.Count;

            return 0;
        }

        public int GetControlSize()
        {
            if (this.Process == null) return 0;
            List<Control> ControlList;
            if (Storage.ControlCollection.TryGetValue(GetNameConst(), out ControlList))
                return ControlList.Count;

            return 0;
        }
        public void SetNameConst(string Name)
        {
            this.CharacterName = Name;
        }

        public string GetNameConst()
        {
            return this.CharacterName;
        }

        public void SetAccountName(string Name)
        {
            this.AccountName = Name;
        }

        public string GetAccountName()
        {
            return this.AccountName;
        }

        public string GetControl(string Name, string DefaultValue = "")
        {
            return this.GetControl(GetNameConst(), Name, DefaultValue);
        }

        public void SetControl(string Name, string Value)
        {
            this.SetControl(GetNameConst(), Name, Value);
        }

        public SkillBar GetSkillBar(int SkillId)
        {
            return this.GetSkillBar(GetNameConst(), SkillId);
        }

        public void DeleteSkillBar(int SkillId)
        {
            this.DeleteSkillBar(GetNameConst(), SkillId);
        }

        public void SetSkillBar(int SkillId, int SkillType)
        {
            this.SetSkillBar(GetNameConst(), SkillId, SkillType);
        }

        public bool IsDisconnected()
        {
            return (GetPhase() == EPhase.Disconnected && Environment.TickCount - GetDisconnectTime() >= 15000) || HasExited();
        }

        public IntPtr GetHandle()
        {
            return this.Handle;
        }

        public Process GetProcess()
        {
            return this.Process;
        }

        public int GetProcessId()
        {
            return this.ProcessId;
        }

        public void SetPhase(EPhase s)
        {
            this.Phase = s;
        }

        public EPhase GetPhase()
        {
            return Phase;
        }
        public void SetAction(EAction s)
        {
            this.Action = s;
        }

        public EAction GetAction()
        {
            return Action;
        }

        public void SetLoginTime(int Time)
        {
            this.LoginTime = Time;
        }

        public int GetLoginTime()
        {
            return this.LoginTime;
        }

        public bool IsInFallback()
        {
            return this.FallbackTime > 0 && Environment.TickCount - GetFallbackTime() <= 5000;
        }

        public void SetFallbackTime(int Time)
        {
            this.FallbackTime = Time;
        }

        public int GetFallbackTime()
        {
            return this.FallbackTime;
        }

        public bool IsInEnterGame()
        {
            return this.EnterGameTime > 0 && Environment.TickCount - GetEnterGameTime() <= 16000;
        }

        public void SetEnterGameTime(int Time)
        {
            this.EnterGameTime = Time;
        }

        public int GetEnterGameTime()
        {
            return this.EnterGameTime;
        }

        public void SetDisconnectTime(int Time)
        {
            this.DisconnectTime = Time;
        }

        public int GetDisconnectTime()
        {
            return DisconnectTime;
        }

        public bool IsCharacterAvailable()
        {
            if (GetPhase() == EPhase.Playing && GetControlSize() > 0 && Storage.ClientCollection.ContainsKey(GetProcessId()) && IsInEnterGame() == false)
                return true;

            return false;
        }

        public int GetTargetAllowedSize()
        {
            return this.TargetAllowedCollection.Count;
        }

        public bool GetTargetAllowed(string Name)
        {
            return this.TargetAllowedCollection.Find(x => x == Name) == Name;
        }

        public void AddTargetAllowed(string Name)
        {
            if (this.TargetAllowedCollection.Contains(Name) == false)
                this.TargetAllowedCollection.Add(Name);
        }

        public void RemoveTargetAllowed(string Name)
        {
            if (this.TargetAllowedCollection.Contains(Name))
                this.TargetAllowedCollection.Remove(Name);
        }

        public void ClearTargetAllowed()
        {
            this.TargetAllowedCollection.Clear();
        }

        public int GetPartyAllowedSize()
        {
            return this.PartyAllowedCollection.Count;
        }

        public bool GetPartyAllowed(string Name)
        {
            return this.PartyAllowedCollection.Find(x => x == Name) == Name;
        }

        public void AddPartyAllowed(string Name)
        {
            if (this.PartyAllowedCollection.Contains(Name) == false)
                this.PartyAllowedCollection.Add(Name);
        }

        public void RemovePartyAllowed(string Name)
        {
            if (this.PartyAllowedCollection.Contains(Name))
                this.PartyAllowedCollection.Remove(Name);
        }

        public void ClearPartyAllowed()
        {
            this.PartyAllowedCollection.Clear();
        }

        protected void ProcessRecvPacketEvent(byte[] Packet)
        {
            /**
             * @reference https://github.com/srmeier/KnightOnline/blob/master/Server/shared/packets.h
             */
            string Message = ByteToHex(Packet);

            //Console.WriteLine(Message);

            switch (Message.Substring(0, 2))
            {
                case "01": //WIZ_LOGIN
                    SetPhase(EPhase.Selecting);
                    SetLoginTime(Environment.TickCount);
                    Console.WriteLine("Recv -> Login " + GetLoginTime());

                    break;

                case "23": //WIZ_ITEM_DROP
                    Console.WriteLine("Recv -> Chest open " + Message.Substring(6, 8));
                    SendPacket("24" + Message.Substring(6, 8));
                    break;

                case "24": //WIZ_BUNDLE_OPEN_REQ
                    if (Message.Length != 156) break;

                    for (int i = 0; i < 4; i++)
                    {
                        string ItemHex = Message.Substring(12 + (12 * i), 8);

                        if (ItemHex != "00000000")
                        {
                            int Item = BitConverter.ToInt32(StringToByte(ItemHex), 0);

                            Console.WriteLine("Recv -> Loot info " + i + " " + Item);

                            bool Loot = false;

                            if (Item == 900000000 || (Convert.ToBoolean(GetControl("OnlyNoah")) && Item == 900000000))
                                Loot = true;
                            
                            if (Convert.ToBoolean(GetControl("LootOnlyList")) && GetLoot(GetNameConst(), Item) != null)
                                Loot = true;

                            if (Convert.ToBoolean(GetControl("LootOnlyList")) && Convert.ToBoolean(GetControl("LootConsumable")) && Item >= 370000000 && Item <= 1931768000)
                                Loot = true;

                            if (Convert.ToBoolean(GetControl("LootOnlyList")) && Convert.ToBoolean(GetControl("LootOther")) && Item >= 100000000 && Item < 370000000)
                                Loot = true;

                            if (Convert.ToBoolean(GetControl("LootOnlySell")) && GetSell(GetNameConst(), Item) != null)
                                Loot = true;

                            if(Loot)
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
                                        Console.WriteLine("Recv -> Seal password validation");
                                        break;
                                    case "03":
                                        if (Message.Substring(6, 2) == "01")
                                            Console.WriteLine("Recv -> Seal password validate success");
                                        else
                                            Console.WriteLine("Recv -> Seal password validate failed");
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
                            Console.WriteLine("Recv -> Zone change loaded");
                            break;

                        case "03": //ZoneChangeTeleport
                            SetEnterGameTime(Environment.TickCount);
                            int ZoneId = int.Parse(Message.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                            Console.WriteLine("Recv -> Zone change teleport(" + ZoneId + ")");
                            break;
                    }
                    break;

                case "1E": //WIZ_WARP
                    SetFallbackTime(Environment.TickCount);
                    Console.WriteLine("Recv -> Warp start (/town, fallback or gamemaster)");
                    break;

                case "4B": //WIZ_WARP_LIST
                    switch (Message.Substring(2, 2))
                    {
                        case "01": //GetWarpList
                            Console.WriteLine("Recv -> Warp list loaded");
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
                                    SendPacket("2F0201");  
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

            //Console.WriteLine(Message);

            switch (Message.Substring(0, 2))
            {
                case "F2": //WIZ_AUTH
                    Console.WriteLine("Send -> Auth " + Environment.TickCount);
                    break;

                case "01": //WIZ_LOGIN
                    SetPhase(EPhase.Loggining);
                    Console.WriteLine("Send -> Login " + Environment.TickCount);
                    break;
   
                case "06": //WIZ_MOVE
                    //Console.WriteLine("Real -> " + Message);
                    break;

                case "04": //WIZ_SEL_CHAR
                    SetPhase(EPhase.Selected);
                    Console.WriteLine("Send -> Character Selected " + Environment.TickCount);
                    break;

                case "0D": //WIZ_GAMESTART
                    SetPhase(EPhase.Playing);
                    SetEnterGameTime(Environment.TickCount);
                    Console.WriteLine("Send -> Game start " + Environment.TickCount);
                    break;

                case "5B": //WIZ_ITEM_UPGRADE
                    switch (Message.Substring(2, 2))
                    {
                        case "0C": //CharacterSealPacket
                            {
                                switch (Message.Substring(4, 2))
                                {
                                    case "01":
                                        Console.WriteLine("Send -> Seal password validation");
                                        break;
                                    case "03":
                                        if (GetControl("CharacterSealPacket") != Message)
                                            SetControl("CharacterSealPacket", Message);

                                        Console.WriteLine("Send -> Seal password validate request");
                                        break;
                                }
                            }
                            break;
                    }
                    break;

                case "20": //WIZ_NPC_EVENT
                    {
                        Console.WriteLine("Send -> Npc Event");
                        ForwardPacketToAllFollower(Message);
                    }
                    break;

                case "33": //WIZ_OBJECT_EVENT
                    {
                        Console.WriteLine("Send -> Object Event");
                        ForwardPacketToAllFollower(Message);
                    }
                    break;

                case "26": //WIZ_ITEM_GET
                    int ItemId = BitConverter.ToInt32(StringToByte(Message.Substring(10, 8)), 0);
                    Console.WriteLine("Send -> Loot get " + ItemId);
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
                            Console.WriteLine("Send -> Zone change load");
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
                    Console.WriteLine("Send -> /town");
                    SetFallbackTime(Environment.TickCount);
                    ForwardPacketToAllFollower(Message);
                    break;

                case "55": //WIZ_SELECT_MSG
                    {
                        Console.WriteLine("Send -> Select Message");
                        ForwardPacketToAllFollower(Message);
                    }
                    break;

                case "56": //WIZ_NPC_SAY
                    {
                        Console.WriteLine("Send -> Say Message");
                        ForwardPacketToAllFollower(Message);
                    }
                    break;

                case "64": //WIZ_QUEST
                    {
                        Console.WriteLine("Send -> Select Quest");
                        ForwardPacketToAllFollower(Message);
                    }
                    break;
            }
        }
        #endregion

        #region "Read Memory Functions"

        public byte[] ReadByteArray(int address, int length)
        {
            return ReadByteArray(this.Handle, address, length);
        }

        public Int32 Read4Byte(IntPtr Address)
        {
            return Read4Byte(this.Handle, Address);
        }

        public Int32 Read4Byte(long Address)
        {
            return Read4Byte(new IntPtr(Address));
        }

        public Int16 ReadByte(IntPtr Address)
        {
            return ReadByte(this.Handle, Address);
        }

        public Int16 ReadByte(long Address)
        {
            return ReadByte(new IntPtr(Address));
        }

        public Single ReadFloat(IntPtr Address)
        {
            return ReadFloat(this.Handle, Address);
        }

        public Single ReadFloat(long Address)
        {
            return ReadFloat(new IntPtr(Address));
        }

        public String ReadString(IntPtr Address, Int32 Size)
        {
            return ReadString(this.Handle, Address, Size);
        }

        public String ReadString(long Address, Int32 Size)
        {
            return ReadString(new IntPtr(Address), Size);
        }
        #endregion

        #region "Write Memory Functions"
        public void WriteFloat(IntPtr Address, float Value)
        {
            WriteFloat(this.Handle, Address, Value);
        }

        public void WriteFloat(long Address, float Value)
        {
            WriteFloat(this.Handle, new IntPtr(Address), Value);
        }

        public void Write4Byte(IntPtr Address, Int32 Value)
        {
            Write4Byte(this.Handle, Address, Value);
        }

        public void Write4Byte(long Address, Int32 Value)
        {
            Write4Byte(this.Handle, new IntPtr(Address), Value);
        }

        public void WriteByte(IntPtr Address, Int32 Value)
        {
            WriteByte(this.Handle, Address, Value);
        }

        public void WriteByte(long Address, Int32 Value)
        {
            WriteByte(this.Handle, new IntPtr(Address), Value);
        }

        public void ExecuteRemoteCode(String Code)
        {
            if(this.CodePtr == IntPtr.Zero)
                this.CodePtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            byte[] CodeByte = StringToByte(Code);
            WriteProcessMemory(this.Handle, this.CodePtr, CodeByte, CodeByte.Length, 0);
            IntPtr Thread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.CodePtr, IntPtr.Zero, 0, IntPtr.Zero);
            if (Thread != IntPtr.Zero)
                WaitForSingleObject(Thread, uint.MaxValue);
            CloseHandle(Thread);
            VirtualFreeEx(this.Handle, this.CodePtr, 1, MEM_RESET);
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

        public void PatchMutant(string NewWindowTitle)
        {
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

            var handles = GetHandles(this.Process, Names, Values);

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
                    Console.WriteLine(ex.StackTrace); //Except -- Process(PID) is not running
                }
            }

            SetWindowText(this.Process.MainWindowHandle, NewWindowTitle);
        }
        #endregion

        #region "Patch Mailslot"

        public IntPtr GetRecvPointer()
        {
            return new IntPtr(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG") - 0x14)) + 0x8);
        }

        public void PatchMailslot()
        {
            UnicodeEncoding UnicodeEncoding = new UnicodeEncoding();

            IntPtr CreateFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "CreateFileW");
            IntPtr WriteFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "WriteFile");
            IntPtr CloseFilePtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "CloseHandle");

            #region "Mailslot Recv Hook"

            if (this.MailslotRecvFuncPtr == IntPtr.Zero)
            {
                String MailslotRecvName = @"\\.\mailslot\KNIGHTONLINE_RECV\" + Environment.TickCount;

                if (this.MailslotRecvPtr == IntPtr.Zero)
                    this.MailslotRecvPtr = CreateMailslot(MailslotRecvName, 0, 0, IntPtr.Zero);

                if (this.MailslotRecvFuncPtr == IntPtr.Zero)
                    this.MailslotRecvFuncPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                byte[] MailslotRecvNameByte = UnicodeEncoding.GetBytes(MailslotRecvName);

                WriteProcessMemory(this.Handle, MailslotRecvFuncPtr + 0x400, MailslotRecvNameByte, MailslotRecvNameByte.Length, 0);

                Patch(this.Handle, MailslotRecvFuncPtr, "558BEC83C4F433C08945FC33D28955F86A0068800000006A036A006A01680000004068" +
                    AlignDWORD(MailslotRecvFuncPtr + 0x400) + "E8" +
                    AlignDWORD(AddressDistance(MailslotRecvFuncPtr + 0x27, CreateFilePtr)) + "8945F86A008D4DFC51FF750CFF7508FF75F8E8" +
                    AlignDWORD(AddressDistance(MailslotRecvFuncPtr + 0x3E, WriteFilePtr)) + "8945F4FF75F8E8" +
                    AlignDWORD(AddressDistance(MailslotRecvFuncPtr + 0x49, CloseFilePtr)) + "8BE55DC3");
            }

            if(this.MailslotRecvHookPtr != IntPtr.Zero)
                VirtualFreeEx(this.Handle, this.MailslotRecvHookPtr, 1, MEM_RESET);

            if (this.MailslotRecvHookPtr == IntPtr.Zero)
                this.MailslotRecvHookPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            Patch(this.Handle, this.MailslotRecvHookPtr, "558BEC83C4F8538B450883C0048B108955FC8B4D0883C1088B018945F8FF75FCFF75F8E8" +
                AlignDWORD(AddressDistance(this.MailslotRecvHookPtr + 0x23, MailslotRecvFuncPtr)) + "83C4088B0D" +
                AlignDWORD(GetAddress("KO_PTR_DLG") - 0x14) + "FF750CFF7508B8" +
                AlignDWORD(new IntPtr(Read4Byte(GetRecvPointer()))) + "FFD05B59595DC20800");

            Patch(this.Handle, GetRecvPointer(), AlignDWORD(this.MailslotRecvHookPtr));

            #endregion

            #region "Mailslot Send Hook"
            String MailslotSendName = @"\\.\mailslot\KNIGHTONLINE_SEND\" + Environment.TickCount;

            this.MailslotSendPtr = CreateMailslot(MailslotSendName, 0, 0, IntPtr.Zero);

            if (this.MailslotSendHookPtr != IntPtr.Zero)
                VirtualFreeEx(this.Handle, this.MailslotSendHookPtr, 1, MEM_RESET);

            if (this.MailslotSendHookPtr == IntPtr.Zero)
                this.MailslotSendHookPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            byte[] MailslotSendNameByte = UnicodeEncoding.GetBytes(MailslotSendName);

            WriteProcessMemory(this.Handle, MailslotSendHookPtr + 0x400, MailslotSendNameByte, MailslotSendNameByte.Length, 0);

            Patch(this.Handle, MailslotSendHookPtr, "608B4424248905" +
                AlignDWORD(MailslotSendHookPtr + 0x100) + "8B4424288905" +
                AlignDWORD(MailslotSendHookPtr + 0x104) + "3D004000007D3D6A0068800000006A036A006A01680000004068" +
                AlignDWORD(MailslotSendHookPtr + 0x400) + "E8" +
                AlignDWORD(AddressDistance(MailslotSendHookPtr + 0x33, CreateFilePtr)) + "83F8FF741C6A005490FF35" +
                AlignDWORD(MailslotSendHookPtr + 0x104) + "FF35" +
                AlignDWORD(MailslotSendHookPtr + 0x100) + "50E8" +
                AlignDWORD(AddressDistance(MailslotSendHookPtr + 0x4E, WriteFilePtr)) + "50E8" +
                AlignDWORD(AddressDistance(MailslotSendHookPtr + 0x54, CloseFilePtr)) + "616AFF68" +
                AlignDWORD(Read4Byte(GetAddress("KO_PTR_SND") + 0x4)) + "E9" +
                AlignDWORD(AddressDistance(MailslotSendHookPtr + 0x61, new IntPtr(GetAddress("KO_PTR_SND") + 0x7))));

            Patch(this.Handle, new IntPtr(GetAddress("KO_PTR_SND")), "E9" + AlignDWORD(AddressDistance(new IntPtr(GetAddress("KO_PTR_SND")), MailslotSendHookPtr)));

            #endregion

            Console.WriteLine("Mailslot -> Recv & Send packet hooked.");
        }
        #endregion

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
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_PKT")) + 0x40064) == 0 ? true : false;
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

        public int GetTargetX()
        {
            if (GetTargetId() == 0 || GetTargetId() == -1) return 0;
            int Base = GetTargetBase();
            if (Base == 0) return 0;
            return (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_X")));
            //return (int)Math.Round(ReadFloat(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_MCOR")) + GetAddress("KO_OFF_MCORX")));
        }

        public int GetTargetY()
        {
            if (GetTargetId() == 0 || GetTargetId() == -1) return 0;
            int Base = GetTargetBase();
            if (Base == 0) return 0;
            return (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_Y")));
            //return (int)Math.Round(ReadFloat(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_MCOR")) + GetAddress("KO_OFF_MCORY")));
        }

        public int GetTargetZ()
        {
            if (GetTargetId() == 0 || GetTargetId() == -1) return 0;
            int Base = GetTargetBase();
            if (Base == 0) return 0;
            return (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_Z")));
            //return (int)Math.Round(ReadFloat(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_MCOR")) + GetAddress("KO_OFF_MCORZ")));
        }

        protected short GetState()
        {
            return ReadByte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_STATE"));
        }

        public int GetZone()
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

        protected int GetGoX()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoX")));
        }

        protected int GetGoY()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoY")));
        }

        protected int GetGoZ()
        {
            return (int)Math.Round(ReadFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoZ")));
        }

        protected int GetSkill(int Slot)
        {
            return Read4Byte(Read4Byte(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_SBARBase")) + 0x184 + (Slot * 4) + 0x68)));
        }

        protected int GetSkillPoint(int Slot)
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
                ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) + "6A006858BFB929B8" + AlignDWORD(GetAddress("KO_FAKE_ITEM")) + "FFD061C3");

                Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C4, 1);
                Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C6, 1);
                Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x58) + 0x5C7, 1);
            }
            else
            {
                ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_CHR")) + "6A006858BFB929B8" + AlignDWORD(GetAddress("KO_FAKE_ITEM")) + "FFD061C3");

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
            Write4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOVE"), 1);
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoX"), GoX);
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_GoY"), GoY);
            Write4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + 0x3F0, 2);
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

            for (int Step = 0; Step < Distance; Step += 1)
            {
                int AngleX = Convert.ToInt32(Math.Sqrt((Math.Pow(Step, 2) * Math.Pow(DistanceX, 2)) / (Math.Pow(DistanceX, 2) + Math.Pow(DistanceY, 2))));
                int AngleY = Convert.ToInt32(Math.Sqrt(Math.Pow(Step, 2) - Math.Pow(AngleX, 2)));

                int NextX = StartX + DirectionX * AngleX;
                int NextY = StartY + DirectionY * AngleY;

                if (IsInFallback() || IsInEnterGame())
                    return;

                SendPacket("06" + AlignDWORD(NextX * 10).Substring(0, 4) + AlignDWORD(NextY * 10).Substring(0, 4) + AlignDWORD(GetZ() * 10).Substring(0, 4) + "2B0000");
            }

            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_X"), GoX);
            WriteFloat(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_Y"), GoY);

            Thread.Sleep(1);

            MoveCoordinate(GoX, GoY);

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        private int ReadAffectedSkill(int Skill)
        {
            int SkillBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_SKILLBASE"));

            SkillBase = Read4Byte(SkillBase + 0x4); //first line
            SkillBase = Read4Byte(SkillBase + GetAddress("KO_OFF_SKILLID"));

            for (int i = 1; i < Skill; i++)
                SkillBase = Read4Byte(SkillBase + 0x0);

            SkillBase = Read4Byte(SkillBase + 0x8);

            if (SkillBase > 0)
                return Read4Byte(SkillBase + 0x0);

            SkillBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_SKILLBASE"));

            SkillBase = Read4Byte(SkillBase + 0x4); //second line
            SkillBase = Read4Byte(SkillBase + GetAddress("KO_OFF_SKILLID"));

            for (int i = 1; i < Skill; i++)
                SkillBase = Read4Byte(SkillBase + 0x0);

            SkillBase = Read4Byte(SkillBase + 0x8);

            if (SkillBase > 0)
                return Read4Byte(SkillBase + 0x0);

            return 0;
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
            return Read4Byte(Length + 0x74);
        }

        protected int GetInventoryItemCount(int ItemId)
        {
            int ItemCount = 0;
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            for (int i = 14; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * i)));
                if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == ItemId) ItemCount += Read4Byte(Length + 0x70);
            }
            return ItemCount;
        }

        protected int GetInventoryItemId(int SlotId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * SlotId)));
            return Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C));
        }

        protected string GetInventoryItemName(int SlotId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * SlotId)));
            int Name = Read4Byte(Length + 0x68);
            int NameLength = Read4Byte(Name + 0x1C);
            if(NameLength > 15)
                return ReadString(Read4Byte(Name + 0xC), NameLength);
            else
                return ReadString(Name + 0xC, NameLength);
        }

        protected int GetInventoryEmptySlot()
        {
            for (int i = 14; i < 42; i++)
                if (GetInventoryItemId(i) == 0) return i;
            return -1;
        }

        protected bool IsInventorySlotEmpty(int Slot)
        {
            if (GetInventoryItemId(Slot) > 0) return false;
            return true;
        }

        protected bool IsInventoryItemExist(int ItemId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            for (int i = 0; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * i)));
                if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == ItemId) return i != -1;
            }
            return false;
        }

        protected int GetInventoryItemSlot(int ItemId)
        {
            int InventoryBase = Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_ITEMB"));
            for (int i = 14; i < 42; i++)
            {
                int Length = Read4Byte(InventoryBase + (GetAddress("KO_OFF_ITEMS") + (4 * i)));
                if (Read4Byte(Read4Byte(Length + 0x68)) + Read4Byte(Read4Byte(Length + 0x6C)) == ItemId) return i;
            }
            return -1;
        }

        public int GetAllInventoryItem(ref List<Inventory> refInventory)
        {
            for (int i = 14; i < 42; i++)
            {
                int Item = GetInventoryItemId(i);

                if(Item > 0)
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

        public void RepairAllEquipment(int NpcId, bool Force, int ExecutionAfterWait = 0)
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
                            if(Force == true || (IsInventorySlotEmpty(i) == false && GetInventoryItemDurability(i) == 0))
                            {
                                SendPacket("3B01" + AlignDWORD(i).Substring(0, 2) + AlignDWORD(NpcId).Substring(0, 4) + AlignDWORD(GetInventoryItemId(i)));
                                Thread.Sleep(300);
                            }
                                
                        }
                        break;
                }
            }

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public bool IsNeedSupply(ref List<Supply>refSupply, bool Force = false)
        {
            foreach (var x in Storage.SupplyCollection)
            {
                if (Convert.ToBoolean(GetControl(x.Control)) == true)
                {
                    string ItemName = x.ControlItem != null ? GetControl(x.ControlItem) : x.ItemConst;
                    Item Item = Storage.ItemCollection.Find(y => y.Name == ItemName);

                    if (Item == null)
                        continue;

                    if (Force == true || (GetInventoryItemCount((int)Item.Id) < 8 && GetInventoryItemCount((int)Item.Id) < Convert.ToInt32(GetControl(x.ControlCount))))
                    {
                        Npc Npc = Storage.NpcCollection
                            .FindAll(y => y.Type == x.Type && y.Zone == GetZone() && (y.Nation == 0 || y.Nation == GetNation()))
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

        public void SelectTarget(int TargetId, int ExecuteAfterWait = 0)
        {
            if (TargetId > 0)
                Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + 0x1D0) + 0xFC, 1);
            else
                Write4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + 0x1D0) + 0xFC, 0);

            Write4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOB"), TargetId);
        }

        public bool IsMoving()
        {
            return Read4Byte(Read4Byte(GetAddress("KO_PTR_CHR")) + GetAddress("KO_OFF_MOVE")) == 1 ? true : false;
        }

        public void SendPacket(byte[] Packet, int ExecutionAfterWait = 0)
        {
            if (IsDisconnected()) return;

            if (this.PacketPtr == IntPtr.Zero)
                this.PacketPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            WriteProcessMemory(this.Handle, this.PacketPtr, Packet, Packet.Length, 0);
            ExecuteRemoteCode("608B0D" + AlignDWORD(GetAddress("KO_PTR_PKT")) + "68" + AlignDWORD(Packet.Length) + "68" + AlignDWORD(this.PacketPtr) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD7C605" + AlignDWORD(GetAddress("KO_PTR_PKT") + 0xC5) + "0061C3");
            VirtualFreeEx(this.Handle, this.PacketPtr, 1, MEM_RESET);

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
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false && Storage.FollowedClient.GetZone() == ClientData.GetZone())
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
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false && Storage.FollowedClient.GetZone() == ClientData.GetZone())
                        ClientData.SendPacket(Packet);
                }
            }
        }

        public void SendPacket(String Packet, int ExecutionAfterWait = 0)
        {
            SendPacket(StringToByte(Packet), ExecutionAfterWait);
        }

        protected void SendAttackPacket()
        {
            SendPacket("080101" + AlignDWORD(GetTargetId()).Substring(0, 4) + "FF00000000");
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

        private void UseTargetSkill(int SkillId, int TargetId = 0)
        {
            if (this.TargetSkillPtr == IntPtr.Zero)
                this.TargetSkillPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            Patch(this.Handle, this.TargetSkillPtr, "608B0D" +
                AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.TargetSkillPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.TargetSkillPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

            Patch(this.Handle, this.TargetSkillPtr + 0x100, "3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "00000000000000000000000000000F00");
            Patch(this.Handle, this.TargetSkillPtr + 0x120, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(TargetId).Substring(0, 4) + "000000000000000000000000");

            IntPtr TargetSkillThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.TargetSkillPtr, IntPtr.Zero, 0, IntPtr.Zero);

            if (TargetSkillThread != IntPtr.Zero)
                WaitForSingleObject(TargetSkillThread, uint.MaxValue);

            CloseHandle(TargetSkillThread);

            VirtualFreeEx(this.Handle, this.TargetSkillPtr, 1, MEM_RESET);
        }

        private void UseTargetAreaSkill(int SkillId, int TargetX, int TargetY, int TargetZ)
        {
            if (this.TargetAreaSkillPtr == IntPtr.Zero)
                this.TargetAreaSkillPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            Patch(this.Handle, this.TargetAreaSkillPtr, "608B0D" +
                AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.TargetAreaSkillPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.TargetAreaSkillPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

            Patch(this.Handle, this.TargetAreaSkillPtr + 0x100, "3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "00000000000000000F00");
            Patch(this.Handle, this.TargetAreaSkillPtr + 0x120, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(TargetX).Substring(0, 4) + AlignDWORD(TargetZ).Substring(0, 4) + AlignDWORD(TargetY).Substring(0, 4) + "000000000000");

            IntPtr TargetAreaSkillThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.TargetAreaSkillPtr, IntPtr.Zero, 0, IntPtr.Zero);

            if (TargetAreaSkillThread != IntPtr.Zero)
                WaitForSingleObject(TargetAreaSkillThread, uint.MaxValue);

            CloseHandle(TargetAreaSkillThread);

            VirtualFreeEx(this.Handle, this.TargetAreaSkillPtr, 1, MEM_RESET);
        }

        public bool IsAttackableTarget()
        {
            if(GetTargetId() == 0 || GetTargetId() == -1) return false;
            int TargetBase = GetTargetBase();
            if (TargetBase == 0) return false;
            if (Read4Byte(TargetBase + GetAddress("KO_OFF_MAX_HP")) > 0 && Read4Byte(TargetBase + GetAddress("KO_OFF_HP")) == 0) return false;
            if (ReadByte(TargetBase + 0x2A0) == 10) return false;
            return true;
        }

        public bool UseWarriorSkill(Skill Skill, int TargetId = 0)
        {
            if (IsCharacterAvailable() == false) return false;
            if (Skill.Mana > 0 && GetMp() < Skill.Mana) return false;
            if (Skill.Type == 1 && GetTargetId() <= 0) return false;

            int SkillId = Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"));

            if ((Skill.Type == 2 && TargetId == 0 && IsSkillAffected(SkillId)) || (Skill.Item > 0 && IsInventoryItemExist((int)Skill.Item) == false)) return false;
            if (Skill.ItemCount > 0 && GetInventoryItemCount((int)Skill.Item) < Skill.ItemCount) return false;

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
                            SendAttackPacket();
                            Thread.Sleep(250);
                        }

                        UseAttackSkill(SkillId, TargetId > 0 ? TargetId : GetTargetId());

                        return true;
                    }

                case "Gain":
                    {
                        if (this.GainPtr == IntPtr.Zero)
                            this.GainPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.GainPtr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.GainPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.GainPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.GainPtr + 0x100, "3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "0000000000000000000000001400");
                        Patch(this.Handle, this.GainPtr + 0x120, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "000000000000000000000000");

                        IntPtr GainThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.GainPtr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (GainThread != IntPtr.Zero)
                            WaitForSingleObject(GainThread, uint.MaxValue);

                        CloseHandle(GainThread);

                        VirtualFreeEx(this.Handle, this.GainPtr, 1, MEM_RESET);

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
            if (Skill.Type == 1 && GetTargetId() <= 0) return false;

            int SkillId = Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"));

            if ((Skill.Type == 2 && TargetId == 0 && IsSkillAffected(SkillId)) || (Skill.Item > 0 && IsInventoryItemExist((int)Skill.Item) == false)) return false;
            if (Skill.ItemCount > 0 && GetInventoryItemCount((int)Skill.Item) < Skill.ItemCount) return false;

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

                        if (this.ArcheryPtr == IntPtr.Zero)
                            this.ArcheryPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.ArcheryPtr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArcheryPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArcheryPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArcheryPtr + 0x140) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.ArcheryPtr + 0x100, "3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000D00");
                        Patch(this.Handle, this.ArcheryPtr + 0x120, "3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        Patch(this.Handle, this.ArcheryPtr + 0x140, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");

                        IntPtr ArcheryThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.ArcheryPtr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (ArcheryThread != IntPtr.Zero)
                            WaitForSingleObject(ArcheryThread, uint.MaxValue);

                        CloseHandle(ArcheryThread);

                        VirtualFreeEx(this.Handle, this.ArcheryPtr, 1, MEM_RESET);

                        return true;
                    }

                case "Counter Strike":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (this.CounterStrikePtr == IntPtr.Zero)
                            this.CounterStrikePtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.CounterStrikePtr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.CounterStrikePtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.CounterStrikePtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.CounterStrikePtr + 0x140) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.CounterStrikePtr + 0x100, "3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000A00");
                        Patch(this.Handle, this.CounterStrikePtr + 0x120, "3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        Patch(this.Handle, this.CounterStrikePtr + 0x140, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");

                        IntPtr CounterStrikeThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.CounterStrikePtr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (CounterStrikeThread != IntPtr.Zero)
                            WaitForSingleObject(CounterStrikeThread, uint.MaxValue);

                        CloseHandle(CounterStrikeThread);

                        VirtualFreeEx(this.Handle, this.CounterStrikePtr, 1, MEM_RESET);

                        return true;
                    }

                case "Multiple Shot":
                    {
                        if (IsAttackableTarget() == false) return false;

                        Patch(this.Handle, this.MultipleShotPtr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MultipleShotPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MultipleShotPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MultipleShotPtr + 0x140) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MultipleShotPtr + 0x160) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MultipleShotPtr + 0x180) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.MultipleShotPtr + 0x100, "3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000D00");
                        Patch(this.Handle, this.MultipleShotPtr + 0x120, "3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        Patch(this.Handle, this.MultipleShotPtr + 0x140, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        Patch(this.Handle, this.MultipleShotPtr + 0x160, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000002000000000000000000");
                        Patch(this.Handle, this.MultipleShotPtr + 0x180, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000003000000000000000000");

                        IntPtr MultipleShotThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.MultipleShotPtr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (MultipleShotThread != IntPtr.Zero)
                            WaitForSingleObject(MultipleShotThread, uint.MaxValue);

                        CloseHandle(MultipleShotThread);

                        VirtualFreeEx(this.Handle, this.MultipleShotPtr, 1, MEM_RESET);

                        return true;
                    }

                case "Arrow Shower":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (this.ArrowShowerPtr == IntPtr.Zero)
                            this.ArrowShowerPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.ArrowShowerPtr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x140) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x160) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x180) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x1A0) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x1C0) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.ArrowShowerPtr + 0x100, "3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000F00");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x120, "3102" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x140, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x160, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000002000000000000000000");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x180, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000003000000000000000000");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x1A0, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000004000000000000000000");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x1C0, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000005000000000000000000");

                        IntPtr ArrowShowerThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.ArrowShowerPtr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (ArrowShowerThread != IntPtr.Zero)
                            WaitForSingleObject(ArrowShowerThread, uint.MaxValue);

                        CloseHandle(ArrowShowerThread);

                        VirtualFreeEx(this.Handle, this.ArrowShowerPtr, 1, MEM_RESET);

                        return true;
                    }

                case "Süper Archer":
                    {
                        if (IsAttackableTarget() == false) return false;

                        //if (CoordinateDistance(GetX(), GetY(), GetTargetX(), GetTargetY()) > 1 || IsAttackableTarget() == false)
                          //  return false;

                        SendPacket("3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000F00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + AlignDWORD(GetZ()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + "9BFF010000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000002000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + AlignDWORD(GetZ()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + "9BFF020000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + AlignDWORD(GetZ()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + "9BFF010000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000003000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + AlignDWORD(GetTargetX()).Substring(0, 4) + AlignDWORD(GetTargetZ()).Substring(0, 4) + AlignDWORD(GetTargetY()).Substring(0, 4) + "9BFF030000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000004000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + AlignDWORD(GetZ()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + "9BFF040000000000");

                        SendPacket("3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000D00");
                        Thread.Sleep(10);
                        SendPacket("3102" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        Thread.Sleep(10);
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + AlignDWORD(GetZ()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + "9BFF010000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + AlignDWORD(GetZ()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + "9BFF010000000000");
                        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000002000000000000000000");
                        SendPacket("3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + AlignDWORD(GetTargetX()).Substring(0, 4) + AlignDWORD(GetTargetZ()).Substring(0, 4) + AlignDWORD(GetTargetY()).Substring(0, 4) + "9BFF020000000000");

                        /*if (this.ArrowShowerPtr == IntPtr.Zero)
                            this.ArrowShowerPtr = VirtualAllocEx(Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.ArrowShowerPtr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x140) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x160) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x180) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x1A0) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.ArrowShowerPtr + 0x1C0) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");


                        Patch(this.Handle, this.ArrowShowerPtr + 0x100, "3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000F00");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x120, "3102" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x140, "3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x160, "3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000002000000000000000000");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x180, "3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000003000000000000000000");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x1A0, "3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000004000000000000000000");
                        Patch(this.Handle, this.ArrowShowerPtr + 0x1C0, "3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "555")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000005000000000000000000");

                        IntPtr ArrowShowerThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.ArrowShowerPtr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (ArrowShowerThread != IntPtr.Zero)
                            WaitForSingleObject(ArrowShowerThread, uint.MaxValue);

                        CloseHandle(ArrowShowerThread);

                        VirtualFreeEx(this.Handle, this.ArrowShowerPtr, 1, MEM_RESET);

                        if (CoordinateDistance(GetX(), GetY(), GetTargetX(), GetTargetY()) > 1 || IsAttackableTarget() == false)
                            return false;

                        if (this.MultipleShotPtr == IntPtr.Zero)
                            this.MultipleShotPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.MultipleShotPtr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MultipleShotPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MultipleShotPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MultipleShotPtr + 0x140) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MultipleShotPtr + 0x160) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MultipleShotPtr + 0x180) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.MultipleShotPtr + 0x100, "3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000D00");
                        Patch(this.Handle, this.MultipleShotPtr + 0x120, "3102" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000010000000000");
                        Patch(this.Handle, this.MultipleShotPtr + 0x140, "3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000001000000000000000000");
                        Patch(this.Handle, this.MultipleShotPtr + 0x160, "3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000002000000000000000000");
                        Patch(this.Handle, this.MultipleShotPtr + 0x180, "3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + "515")).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000003000000000000000000");

                        IntPtr MultipleShotThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.MultipleShotPtr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (MultipleShotThread != IntPtr.Zero)
                            WaitForSingleObject(MultipleShotThread, uint.MaxValue);

                        CloseHandle(MultipleShotThread);

                        VirtualFreeEx(this.Handle, this.MultipleShotPtr, 1, MEM_RESET);*/

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
                            if (this.PercentageSkillPtr == IntPtr.Zero)
                                this.PercentageSkillPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                            Patch(this.Handle, this.PercentageSkillPtr, "608B0D" +
                                AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.PercentageSkillPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                                AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.PercentageSkillPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                            Patch(this.Handle, this.PercentageSkillPtr + 0x100, "3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000001000");
                            Patch(this.Handle, this.PercentageSkillPtr + 0x120, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000000000000000");

                            IntPtr PercentageSkillThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.PercentageSkillPtr, IntPtr.Zero, 0, IntPtr.Zero);

                            if (PercentageSkillThread != IntPtr.Zero)
                                WaitForSingleObject(PercentageSkillThread, uint.MaxValue);

                            CloseHandle(PercentageSkillThread);

                            VirtualFreeEx(this.Handle, this.PercentageSkillPtr, 1, MEM_RESET);
                        }
                        else
                        {
                            if(Convert.ToBoolean(GetControl("RAttack")) == true)
                            {
                                SendAttackPacket();
                                Thread.Sleep(250);
                            }

                            UseAttackSkill(SkillId, TargetId > 0 ? TargetId : GetTargetId());
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
                        if (this.StealthPtr == IntPtr.Zero)
                            this.StealthPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.StealthPtr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.StealthPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.StealthPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.StealthPtr + 0x100, "3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "00000000000000000000000000001E00");
                        Patch(this.Handle, this.StealthPtr + 0x120, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "000000000000000000000000");

                        IntPtr StealthThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.StealthPtr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (StealthThread != IntPtr.Zero)
                            WaitForSingleObject(StealthThread, uint.MaxValue);

                        CloseHandle(StealthThread);

                        VirtualFreeEx(this.Handle, this.StealthPtr, 1, MEM_RESET);

                        return true;
                    }

                case "Blood of wolf":
                    {
                        if (this.WolfPtr == IntPtr.Zero)
                            this.WolfPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.WolfPtr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.WolfPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.WolfPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.WolfPtr + 0x140) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.WolfPtr + 0x100, "3106" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "000000000000000000000000");
                        Patch(this.Handle, this.WolfPtr + 0x120, "3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(GetX()).Substring(0, 4) + AlignDWORD(GetZ()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + "00000000000000001100");
                        Patch(this.Handle, this.WolfPtr + 0x140, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(GetX()).Substring(0, 4) + AlignDWORD(GetZ()).Substring(0, 4) + AlignDWORD(GetY()).Substring(0, 4) + "000000000000");

                        IntPtr WolfThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.WolfPtr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (WolfThread != IntPtr.Zero)
                            WaitForSingleObject(WolfThread, uint.MaxValue);

                        CloseHandle(WolfThread);

                        VirtualFreeEx(this.Handle, this.WolfPtr, 1, MEM_RESET);

                        Thread.Sleep(1250);

                        return true;
                    }
                case "Swift":
                    {
                        UseTargetSkill(SkillId, TargetId > 0 ? TargetId : GetId());

                        return true;
                    }
                case "Lupine Eyes":
                    {
                        if (this.LupineEyesPtr == IntPtr.Zero)
                            this.LupineEyesPtr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.LupineEyesPtr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.LupineEyesPtr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.LupineEyesPtr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.LupineEyesPtr + 0x100, "3101" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "00000000000000000000000000001400");
                        Patch(this.Handle, this.LupineEyesPtr + 0x120, "3103" + AlignDWORD(SkillId).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetId()).Substring(0, 4) + "000000000000000000000000");

                        IntPtr LupineEyesThread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.LupineEyesPtr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (LupineEyesThread != IntPtr.Zero)
                            WaitForSingleObject(LupineEyesThread, uint.MaxValue);

                        CloseHandle(LupineEyesThread);

                        VirtualFreeEx(this.Handle, this.LupineEyesPtr, 1, MEM_RESET);

                        return true;
                    }
            }

            return false;
        }

        public bool UsePriestSkill(Skill Skill, int TargetId = 0)
        {
            if (IsCharacterAvailable() == false) return false;
            if (Skill.Mana > 0 && GetMp() < Skill.Mana) return false;
            if (Skill.Type == 1 && GetTargetId() <= 0) return false;

            int SkillId = Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"));

            if ((Skill.Type == 2 && TargetId == 0 && IsSkillAffected(SkillId)) || (Skill.Item > 0 && IsInventoryItemExist((int)Skill.Item) == false)) return false;
            if (Skill.ItemCount > 0 && GetInventoryItemCount((int)Skill.Item) < Skill.ItemCount) return false;

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
                            SendAttackPacket();
                            Thread.Sleep(250);
                        }

                        UseAttackSkill(SkillId, TargetId > 0 ? TargetId : GetTargetId());

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

                        UseTargetSkill(SkillId, TargetId > 0 ? TargetId : GetTargetId());

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
            if (Skill.Type == 1 && GetTargetId() <= 0) return false;

            int SkillId = Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"));

            if ((Skill.Type == 2 && TargetId == 0 && IsSkillAffected(SkillId)) || (Skill.Item > 0 && IsInventoryItemExist((int)Skill.Item) == false)) return false;
            if (Skill.ItemCount > 0 && GetInventoryItemCount((int)Skill.Item) < Skill.ItemCount) return false;

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

                        if (this.MageSkillType1Ptr == IntPtr.Zero)
                            this.MageSkillType1Ptr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.MageSkillType1Ptr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MageSkillType1Ptr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MageSkillType1Ptr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.MageSkillType1Ptr + 0x100, "3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"))).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000A00");
                        Patch(this.Handle, this.MageSkillType1Ptr + 0x120, "3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"))).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000000000000000");

                        IntPtr MageSkillType1Thread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.MageSkillType1Ptr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (MageSkillType1Thread != IntPtr.Zero)
                            WaitForSingleObject(MageSkillType1Thread, uint.MaxValue);

                        CloseHandle(MageSkillType1Thread);

                        VirtualFreeEx(this.Handle, this.MageSkillType1Ptr, 1, MEM_RESET);

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

                        UseTargetSkill(SkillId, TargetId > 0 ? TargetId : GetTargetId());

                        return true;
                    }

                case "Fire Spear":
                case "Fire Ball":
                case "Ice Orb":
                case "Static Orb":
                case "Thunder Impact":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (this.MageSkillType3Ptr == IntPtr.Zero)
                            this.MageSkillType3Ptr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.MageSkillType3Ptr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MageSkillType3Ptr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MageSkillType3Ptr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MageSkillType3Ptr + 0x140) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MageSkillType3Ptr + 0x160) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.MageSkillType3Ptr + 0x100, "3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"))).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "00000000000000000000000000000F00");
                        Patch(this.Handle, this.MageSkillType3Ptr + 0x120, "3102" + AlignDWORD(Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"))).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000000000000000");
                        Patch(this.Handle, this.MageSkillType3Ptr + 0x140, "3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"))).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + "000000000000000000000000");
                        Patch(this.Handle, this.MageSkillType3Ptr + 0x160, "3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"))).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + AlignDWORD(GetTargetId()).Substring(0, 4) + AlignDWORD(GetTargetX()).Substring(0, 4) + AlignDWORD(GetTargetZ()).Substring(0, 4) + AlignDWORD(GetTargetY()).Substring(0, 4) + "9BFF0000000000000000");

                        IntPtr MageSkillType3Thread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.MageSkillType3Ptr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (MageSkillType3Thread != IntPtr.Zero)
                            WaitForSingleObject(MageSkillType3Thread, uint.MaxValue);

                        CloseHandle(MageSkillType3Thread);

                        VirtualFreeEx(this.Handle, this.MageSkillType3Ptr, 1, MEM_RESET);

                        return true;
                    }

                case "Flame Blade":
                case "Frozen Blade":
                case "Charged Blade":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (Convert.ToBoolean(GetControl("RAttack")) == true)
                        {
                            SendAttackPacket();
                            Thread.Sleep(250);
                        }

                        UseAttackSkill(SkillId, TargetId > 0 ? TargetId : GetTargetId());

                        return true;
                    }

                case "Ice Burst":
                case "Fire Burst":
                case "Thunder Burst":
                    {
                        if (IsAttackableTarget() == false) return false;

                        if (this.MageSkillType4Ptr == IntPtr.Zero)
                            this.MageSkillType4Ptr = VirtualAllocEx(this.Handle, IntPtr.Zero, 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                        Patch(this.Handle, this.MageSkillType4Ptr, "608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MageSkillType4Ptr + 0x100) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MageSkillType4Ptr + 0x120) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MageSkillType4Ptr + 0x140) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761608B0D" +
                            AlignDWORD(GetAddress("KO_PTR_PKT")) + "6A1B68" + AlignDWORD(this.MageSkillType4Ptr + 0x160) + "BF" + AlignDWORD(GetAddress("KO_PTR_SND")) + "FFD761C3");

                        Patch(this.Handle, this.MageSkillType4Ptr + 0x100, "3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"))).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(GetTargetX()).Substring(0, 4) + AlignDWORD(GetTargetZ()).Substring(0, 4) + AlignDWORD(GetTargetY()).Substring(0, 4) + "00000000000000000F00");
                        Patch(this.Handle, this.MageSkillType4Ptr + 0x120, "3102" + AlignDWORD(Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"))).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(GetTargetX()).Substring(0, 4) + AlignDWORD(GetTargetZ()).Substring(0, 4) + AlignDWORD(GetTargetY()).Substring(0, 4) + "000000000000");
                        Patch(this.Handle, this.MageSkillType4Ptr + 0x140, "3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"))).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(GetTargetX()).Substring(0, 4) + AlignDWORD(GetTargetZ()).Substring(0, 4) + AlignDWORD(GetTargetY()).Substring(0, 4) + "00000000000000000000");
                        Patch(this.Handle, this.MageSkillType4Ptr + 0x160, "3104" + AlignDWORD(Int32.Parse(GetClass().ToString() + Skill.RealId.ToString("D3"))).Substring(0, 6) + "00" + AlignDWORD(GetId()).Substring(0, 4) + "FFFF" + AlignDWORD(GetTargetX()).Substring(0, 4) + AlignDWORD(GetTargetZ()).Substring(0, 4) + AlignDWORD(GetTargetY()).Substring(0, 4) + "9BFF0000000000000000");

                        IntPtr MageSkillType4Thread = CreateRemoteThread(this.Handle, IntPtr.Zero, 0, this.MageSkillType4Ptr, IntPtr.Zero, 0, IntPtr.Zero);

                        if (MageSkillType4Thread != IntPtr.Zero)
                            WaitForSingleObject(MageSkillType4Thread, uint.MaxValue);

                        CloseHandle(MageSkillType4Thread);

                        VirtualFreeEx(this.Handle, this.MageSkillType4Ptr, 1, MEM_RESET);

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

                        UseTargetAreaSkill(SkillId, GetTargetX(), GetTargetY(), GetTargetZ());

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
                .FindAll(x => x.Type == "Sunderies" && x.Zone == GetZone() && (x.Nation == 0 || x.Nation == GetNation()))
                .GroupBy(x => Math.Pow((GetX() - x.X), 2) + Math.Pow((GetY() - x.Y), 2))
                .OrderBy(x => x.Key)
                ?.FirstOrDefault()
                ?.FirstOrDefault();

            if (Sunderies != null)
            {
                Console.WriteLine("Repair -> Start " + Environment.TickCount);

                SendNotice("Repair işlemi başladı.");

                SetAction(EAction.Repairing);

                int iLastX = GetX(); int iLastY = GetY();

                Thread.Sleep(1250);

                if (Sunderies.Town == 1)
                {
                    Console.WriteLine("Repair -> Send /town and wait 7,5 second");

                    SendPacket("4800", 7500);
                }

                Console.WriteLine("Repair -> Moving sunderies and wait 2,5 second");

                SetCoordinate((int)Sunderies.X, (int)Sunderies.Y, 2500);

                while (GetAction() == EAction.Repairing)
                {
                    if (IsCharacterAvailable() == false)
                        return;

                    if (CoordinateDistance(GetX(), GetY(), (int)Sunderies.X, (int)Sunderies.Y) > 5)
                    {
                        Console.WriteLine("Repair -> Sunderies is too far repeat moving");

                        SetCoordinate((int)Sunderies.X, (int)Sunderies.Y);
                    }
                    else
                    {
                        Console.WriteLine("Repair -> Npc action start and wait 2,5 second");

                        RepairAllEquipment((int)Sunderies.RealId, Force, 2500);

                        Console.WriteLine("Repair -> Npc action end. Moving back last location and wait 2,5 second");

                        SetCoordinate((int)iLastX, (int)iLastY, 2500);

                        while (GetAction() == EAction.Repairing)
                        {
                            if (IsCharacterAvailable() == false)
                                return;

                            if (CoordinateDistance(GetX(), GetY(), (int)iLastX, (int)iLastY) > 5)
                            {
                                Console.WriteLine("Repair -> Last location is too far repeat moving");

                                SetCoordinate((int)iLastX, (int)iLastY);
                            }
                            else
                            {
                                Console.WriteLine("Repair -> Setting action None and wait 2,5 second");
                                SetAction(EAction.None);
                            }

                            Thread.Sleep(2500);
                        }

                        this.RepairEventAfterWaitTime = Environment.TickCount;

                        Console.WriteLine("Repair -> Action end " + Environment.TickCount);
                    }

                    Thread.Sleep(2500);
                }

                SendNotice("Repair işlemi sona erdi.");

                Console.WriteLine("Repair -> End " + Environment.TickCount);
            }
            else
            {
                Console.WriteLine("Repair -> Sunderies does not exist (" + GetZone() + ")");
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
            int InventoryItemSlot = GetInventoryItemSlot((int)Item.Id);

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

                int Slot =  0; 
                int Page = 0;

                if (i >= 0 && i <= 23)
                {
                    Page = 0;
                    Slot = i;
                }
                else if(i >= 24 && i <= 47)
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

            Thread.Sleep(125);
            SendPacket("2001" + AlignDWORD(Npc.RealId).Substring(0, 4) + "FFFFFFFF");
            Thread.Sleep(125);
            SendPacket("6A02");
            Thread.Sleep(125);

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public void BuyItem(Item Item, Npc Npc, int Count, int ExecutionAfterWait = 0)
        {
            if (Count == 0) return;

            int InventoryItemSlot = GetInventoryItemSlot((int)Item.Id);

            InventoryItemSlot = InventoryItemSlot != -1 ? InventoryItemSlot : GetInventoryEmptySlot();

            if (InventoryItemSlot == -1) return;

            string ItemCount = "";

            if (Item.BuyPacketCountSize == 0)
                ItemCount = AlignDWORD(Count);
            else
                ItemCount = AlignDWORD(Count).Substring(0, (int)Item.BuyPacketCountSize);

            if (Npc.Type == "Sunderies")
                SendPacket("2101" + "18E40300" + AlignDWORD(Npc.RealId).Substring(0, 4) + "01" + AlignDWORD(Item.Id) + AlignDWORD(InventoryItemSlot - 14).Substring(0, 2) + ItemCount + Item.BuyPacketEnd);
            else
                SendPacket("2101" + "48DC0300" + AlignDWORD(Npc.RealId).Substring(0, 4) + "01" + AlignDWORD(Item.Id) + AlignDWORD(InventoryItemSlot - 14).Substring(0, 2) + ItemCount + Item.BuyPacketEnd);

            Thread.Sleep(125);
            SendPacket("6A02");
            Thread.Sleep(125);

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public void SellSaleableItems(Npc Npc, int ExecutionAfterWait = 0)
        {
            for (int i = 14; i < 42; i++)
            {
                int ItemId = GetInventoryItemId(i);

                if(GetSell(GetNameConst(), ItemId) != null)
                    SellItem(ItemId, Npc, GetInventoryItemCount(ItemId), ExecutionAfterWait);
            }
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

            Thread.Sleep(125);
            SendPacket("6A02");
            Thread.Sleep(125);

            if (ExecutionAfterWait > 0)
                Thread.Sleep(ExecutionAfterWait);
        }

        public void SupplyItemAction(List<Supply> Supply)
        {
            if (GetAction() != EAction.None) return;

            Console.WriteLine("Supply -> Start " + Environment.TickCount);

            SendNotice("Tedarik işlemi başladı.");

            SetAction(EAction.Supplying);

            Thread.Sleep(1250);

            List<Supply> OrderedSupply = Supply.OrderBy(x => x.Npc.Id).ToList();

            bool NeedTown = true;

            int iLastX = GetX(); int iLastY = GetY();

            OrderedSupply.ForEach(x => {

                if(x.Npc.Town == 1 && NeedTown)
                {
                    NeedTown = false;

                    Console.WriteLine("Supply -> Send /town and wait 7,5 second");

                    SendPacket("4800", 7500);
                }

                while (CoordinateDistance(GetX(), GetY(), (int)x.Npc.X, (int)x.Npc.Y) > 5)
                {
                    if (IsCharacterAvailable() == false)
                        return;

                    Console.WriteLine("Supply -> Npc is too far repeat moving");

                    SetCoordinate((int)x.Npc.X, (int)x.Npc.Y, 2500);

                    Thread.Sleep(2500);
                }

                Console.WriteLine("Supply -> Npc action start and wait 2,5 second");

                if (x.Npc.Type == "Inn")
                    WarehouseItemCheckOut(x.Item, x.Npc, Math.Abs(GetInventoryItemCount((int)x.Item.Id) - x.Count), 2500);
                else
                {
                    BuyItem(x.Item, x.Npc, Math.Abs(GetInventoryItemCount((int)x.Item.Id) - x.Count), 2500);

                    Console.WriteLine("Supply -> Sell Saleable items and wait 2,5 second");

                    SellSaleableItems(x.Npc, 2500);
                }

                Console.WriteLine("Supply -> Npc action end.");
            });

            Console.WriteLine("Supply -> Moving back last location and wait 2,5 second");

            SetCoordinate((int)iLastX, (int)iLastY, 2500);

            while (CoordinateDistance(GetX(), GetY(), (int)iLastX, (int)iLastY) > 5)
            {
                if (IsCharacterAvailable() == false)
                    return;

                Console.WriteLine("Supply -> Last location is too far repeat moving");

                SetCoordinate((int)iLastX, (int)iLastY);

                Thread.Sleep(2500);
            }

            Console.WriteLine("Supply -> Setting action None and wait 2,5 second");

            SetAction(EAction.None);

            Console.WriteLine("Supply -> Action end " + Environment.TickCount);

            SendNotice("Tedarik işlemi sona erdi.");

            this.SupplyEventAfterWaitTime = Environment.TickCount;

            Supply.Clear();
        }

        public int GetTargetBase(int TargetId = 0)
        {
            TargetId = TargetId > 0 ? TargetId : GetTargetId();

            if (TargetId > 0)
            {
                if(TargetId > 9999)
                    return GetMobBase(TargetId);
                else
                    return GetPlayerBase(TargetId);
            }

            return 0;
        }

        public int GetMobBase(int MobId)
        {
            int Ebp = Read4Byte(Read4Byte(GetAddress("KO_FLDB")) + 0x34);
            int Fend = Read4Byte(Read4Byte(Ebp + 0x4) + 0x4);
            int Esi = Read4Byte(Ebp);
            int Tick = Environment.TickCount;

            while (Esi != Ebp && Environment.TickCount - Tick < 75)
            {
                int Base = Read4Byte(Esi + 0x10);

                if (Base == 0) break;

                if (Read4Byte(Base + GetAddress("KO_OFF_ID")) == MobId)
                    return Base;

                int Eax = Read4Byte(Esi + 0x8);

                if (Eax != Fend)
                {
                    while (Read4Byte(Eax) != Fend && Environment.TickCount - Tick < 75)
                        Eax = Read4Byte(Eax);

                    Esi = Eax;
                }
                else
                {
                    Eax = Read4Byte(Esi + 0x4);

                    while (Esi == Read4Byte(Eax + 0x8) && Environment.TickCount - Tick < 75)
                    {
                        Esi = Eax;
                        Eax = Read4Byte(Eax + 0x4);
                    }

                    if (Read4Byte(Esi + 0x8) != Eax)
                        Esi = Eax;
                }
            }

            return 0;
        }

        public int SearchMob(ref List<Target> OutTargetList)
        {
            int Ebp = Read4Byte(Read4Byte(GetAddress("KO_FLDB")) + 0x34);
            int Fend = Read4Byte(Read4Byte(Ebp + 0x4) + 0x4);
            int Esi = Read4Byte(Ebp);
            int Tick = Environment.TickCount;

            while (Esi != Ebp && Environment.TickCount - Tick < 75)
            {
                int Base = Read4Byte(Esi + 0x10);

                if (Base == 0) break;

                if (OutTargetList.Any(x => x.Base == Base) == false)
                {
                    Target Target = new Target();

                    int NameLen = Read4Byte(Base + GetAddress("KO_OFF_NAME_LEN"));

                    Target.Base = Base;

                    if (NameLen > 15)
                        Target.Name = ReadString(Read4Byte(Base + GetAddress("KO_OFF_NAME")), NameLen);
                    else
                        Target.Name = ReadString(Base + GetAddress("KO_OFF_NAME"), NameLen);

                    Target.Nation = Read4Byte(Base + GetAddress("KO_OFF_NATION"));
                    Target.State = ReadByte(Base + 0x2A0); //TODO: 0x2A0 KO_OFF_STATE Address Finder
                    Target.X = (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_X")));
                    Target.Y = (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_Y")));

                    OutTargetList.Add(Target);
                }

                int Eax = Read4Byte(Esi + 0x8);

                if (Eax != Fend)
                {
                    while (Read4Byte(Eax) != Fend && Environment.TickCount - Tick < 75)
                        Eax = Read4Byte(Eax);
                       
                    Esi = Eax;
                }
                else
                {
                     Eax = Read4Byte(Esi + 0x4);

                    while (Esi == Read4Byte(Eax + 0x8) && Environment.TickCount - Tick < 75)
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

        public int GetPlayerBase(int PlayerId)
        {
            int Ebp = Read4Byte(Read4Byte(GetAddress("KO_FLDB")) + 0x40);
            int Fend = Read4Byte(Read4Byte(Ebp + 0x4) + 0x4);
            int Esi = Read4Byte(Ebp);
            int Tick = Environment.TickCount;

            while (Esi != Ebp && Environment.TickCount - Tick < 75)
            {
                int Base = Read4Byte(Esi + 0x10);

                if (Base == 0) break;

                if (Read4Byte(Base + GetAddress("KO_OFF_ID")) == PlayerId)
                    return Base;

                int Eax = Read4Byte(Esi + 0x8);

                if (Eax != Fend)
                {
                    while (Read4Byte(Eax) != Fend && Environment.TickCount - Tick < 75)
                        Eax = Read4Byte(Eax);

                    Esi = Eax;
                }
                else
                {
                    Eax = Read4Byte(Esi + 0x4);

                    while (Esi == Read4Byte(Eax + 0x8) && Environment.TickCount - Tick < 75)
                    {
                        Esi = Eax;
                        Eax = Read4Byte(Eax + 0x4);
                    }

                    if (Read4Byte(Esi + 0x8) != Eax)
                        Esi = Eax;
                }
            }

            return 0;
        }

        public int SearchPlayer(ref List<Target> OutTargetList)
        {
            int Ebp = Read4Byte(Read4Byte(GetAddress("KO_FLDB")) + 0x40);
            int Fend = Read4Byte(Read4Byte(Ebp + 0x4) + 0x4);
            int Esi = Read4Byte(Ebp);
            int Tick = Environment.TickCount;

            while (Esi != Ebp && Environment.TickCount - Tick < 75)
            {
                int Base = Read4Byte(Esi + 0x10);

                if (Base == 0) break;

                if (OutTargetList.Any(x => x.Base == Base) == false)
                {
                    Target Target = new Target();

                    int NameLen = Read4Byte(Base + GetAddress("KO_OFF_NAME_LEN"));

                    Target.Base = Base;

                    if (NameLen > 15)
                        Target.Name = ReadString(Read4Byte(Base + GetAddress("KO_OFF_NAME")), NameLen);
                    else
                        Target.Name = ReadString(Base + GetAddress("KO_OFF_NAME"), NameLen);

                    Target.Nation = Read4Byte(Base + GetAddress("KO_OFF_NATION"));
                    Target.State = ReadByte(Base + 0x2A0); //TODO: 0x2A0 KO_OFF_STATE Address Finder
                    Target.X = (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_X")));
                    Target.Y = (int)Math.Round(ReadFloat(Base + GetAddress("KO_OFF_Y")));

                    OutTargetList.Add(Target);
                }

                int Eax = Read4Byte(Esi + 0x8);

                if (Eax != Fend)
                {
                    while (Read4Byte(Eax) != Fend && Environment.TickCount - Tick < 75)
                        Eax = Read4Byte(Eax);

                    Esi = Eax;
                }
                else
                {
                    Eax = Read4Byte(Esi + 0x4);

                    while (Esi == Read4Byte(Eax + 0x8) && Environment.TickCount - Tick < 75)
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

        public int GetPartyList(ref List<Party> PartyList)
        {
            int Base = Read4Byte(Read4Byte(Read4Byte(Read4Byte(GetAddress("KO_PTR_DLG")) + GetAddress("KO_OFF_PtBase")) + GetAddress("KO_OFF_Pt")));

            for (int i = 0; i <= GetPartyCount() - 1; i++)
            {
                Party Party = new Party();

                Party.MemberId = Read4Byte(Base + 0x8);
                Party.MemberClass = Read4Byte(Base + 0x10);
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
               

                if(PartyList.Contains(Party) == false)
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
                int MemberNickLen = Read4Byte(Base + 0x40);

                string MemberName = "";

                if (MemberNickLen > 15)
                    MemberName = ReadString(Read4Byte(Base + 0x30), MemberNickLen);
                else
                    MemberName = ReadString(Base + 0x30, MemberNickLen);

                if(Name == MemberName)
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

                if (MemberHp < MemberMaxHp)
                    return true;

                Base = Read4Byte(Base);
            }

            return false;
        }

        public void SendParty(string Name)
        {
            SendPacket("2F01" + AlignDWORD(Name.Length).Substring(0, 2) + "00" + StringToHex(Name));
            Thread.Sleep(125);
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

        public bool IsTransformationAvailableZone()
        {
            if (GetZone() == 30 || GetZone() == 71 || GetZone() == 75 || GetZone() == 81 || GetZone() == 82 || GetZone() == 83)
                return false;

            return true;
        }

        public void SendNotice(string Text)
        {
            SendPacket("1013" + AlignDWORD(Text.Length).Substring(0, 2) + "00" + StringToHex(Text));
        }



        #endregion
    }
}
