using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace KOF.Core
{
    #region "Enums"
    public class AddressEnum
    {
        public enum Type : byte
        {
            Pointer,
            CallAdd,
            Generate
        }

        public enum Platform : byte
        {
            USKO,
            STEAM,
            JPKO,
            CNKO,
        }
    }
    #endregion

    #region "Storage"
    public class AddressStorage
    {
        public bool Active { get; set; }
        public int Value { get; set; }
        public string Name { get; set; }
        public string Hex { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public AddressEnum.Type Type { get; set; }
        public string Address { get; set; }
        public string Call { get; set; }
        public int CallOffset { get; set; }
        public int AddressOffset { get; set; }
        public string BaseAddress { get; set; }
    }
    #endregion

    #region "Finder"
    public class AddressFinder : Helper
    {
        readonly int DefaultStart = 0x401000;
        readonly int DefaultLength = 0x5B2000;

        public List<AddressStorage> LoadAddressList(IntPtr Handle, AddressEnum.Platform Platform)
        {
            List<AddressStorage> AddressList = new List<AddressStorage>()
            {
                #region [Pointer]
                new AddressStorage(){ Active = true, Name = "KO_PTR_CHR",              Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8D9424QQ52E8QQA1" },
                new AddressStorage(){ Active = true, Name = "KO_PTR_DLG",              Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B1DXXXXXXXX906AXXFFD7A1" },
                new AddressStorage(){ Active = true, Name = "KO_PTR_PKT",              Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "68QQFF15QQ3B2D" },
                new AddressStorage(){ Active = true, Name = "KO_PTR_SND",              Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "68QQFF15QQ3B2D", CallOffset = 0x6D },
                new AddressStorage(){ Active = true, Name = "KO_FMBS",                 Start = 0x4E1000,      Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "CCCCCCCCCC83ECXX5356578D71", CallOffset = 0x8 },
                new AddressStorage(){ Active = true, Name = "KO_FPBS",                 Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "83EC085356578D71XX8D44241850", CallOffset = 0xE },
                new AddressStorage(){ Active = true, Name = "KO_FNC_ISEN",             Start = 0x500000,      Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "8B4424045051E8QQC2", CallOffset = 0xC },
                new AddressStorage(){ Active = true, Name = "KO_PTR_SERVER",           Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8BB424QQ33DB895C24XX8B0D" },
                new AddressStorage(){ Active = true, Name = "KO_PTR_NODC",             Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "C64424XXXXD905" },
                new AddressStorage(){ Active = true, Name = "KO_OTO_LOGIN_PTR",        Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B0DQQ6A00E8QQC3CCCC8B0D" },
                new AddressStorage(){ Active = true, Name = "KO_OTO_LOGIN_01",         Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "75XX837C24XXXX0F85QQQQXXXX85C974", CallOffset = 0x20 },
                new AddressStorage(){ Active = true, Name = "KO_OTO_LOGIN_02",         Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "83ECXX53558BE933DB399D", CallOffset = 0xB },
                new AddressStorage(){ Active = true, Name = "KO_OTO_LOGIN_03",         Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "558BE983BDQQXX0F84", CallOffset = 0xC },
                new AddressStorage(){ Active = true, Name = "KO_OTO_LOGIN_04",         Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "0F8DQQ53555633DB33ED8DB7", CallOffset = 0x1A  },
                new AddressStorage(){ Active = true, Name = "KO_OTO_BTN_PTR",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "6AXX8D5424XX5268QQ894C24XXE8QQA1" },
                new AddressStorage(){ Active = true, Name = "KO_BTN_LEFT",             Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "8B8EQQ89BEQQ8B018B50XX53FFD2", CallOffset = 0x73  },
                new AddressStorage(){ Active = true, Name = "KO_BTN_RIGHT",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "E8QQ83C4XXC3CCCCCCCCCCCCCCCCCCCCCC6AXX68QQ64A1QQ5083EC", CallOffset = 0x10 },
                new AddressStorage(){ Active = true, Name = "KO_BTN_LOGIN",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "566AXX8BF1E8QQA1", CallOffset = 0xB },
                new AddressStorage(){ Active = true, Name = "KO_FLDB",                 Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "5356578BF98B0D" },
                new AddressStorage(){ Active = true, Name = "KO_FNCZ",                 Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "83ECXXA1QQ5657", CallOffset = 0xA },
                new AddressStorage(){ Active = true, Name = "KO_FNCB",                 Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "C3CCCCCCCCCC83ECXXA1QQ5657", CallOffset = 0xA },
                new AddressStorage(){ Active = true, Name = "KO_ITOB",                 Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "0F84QQ8BBC24QQ8B0D" },
                new AddressStorage(){ Active = true, Name = "KO_ITEB",                 Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "0F84QQ8BBC24QQ8B0D", AddressOffset = 0x8 },
                new AddressStorage(){ Active = true, Name = "KO_ITEMFIND",             Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Generate,  Hex = "83EC085356578D711C8D442418508D4C2410518BCEE8QQ8B7C240C8B5E188B3685FF", CallOffset = 0x25 },
                new AddressStorage(){ Active = true, Name = "KO_ITEMDESCALL",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "83ECXXA1QQ33C4894424XX568BF180BEQQXX0F84", CallOffset = 0x1A },
                new AddressStorage(){ Active = true, Name = "KO_ITEMDES",              Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "3DQQ0F85QQ833D" },
                new AddressStorage(){ Active = true, Name = "KO_ITEMDES2",             Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B0DQQE8QQ5FC605" },
                new AddressStorage(){ Active = true, Name = "KO_FAKE_ITEM",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "6AFF68QQ64A1QQ5051555657A1QQ33C4508D4424XX64A3QQ8BF18B6C24XX85ED", CallOffset = 0x2C },
                new AddressStorage(){ Active = true, Name = "KO_SUB_ADDR0",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "5081ECA0020000A1QQ33C489842498020000535657A1QQ33C4508D8424", CallOffset = 0x36 },
                new AddressStorage(){ Active = true, Name = "KO_SUB_ADDR1",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "558BEC83E4XX81EC", CallOffset = 0x8 },
                new AddressStorage(){ Active = true, Name = "KO_PTR_NRML",             Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B0D{0}8B81QQ8B0D", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_SMMB",                 Start = 0x500000,      Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "B9QQ518B0D" },
                new AddressStorage(){ Active = true, Name = "KO_SMMB_FNC",             Start = 0x500000,      Length = DefaultLength, Type = AddressEnum.Type.Generate,  Hex = "83ECXX5356578D71XX8D4424XX508D4C24XX518BCEE8QQ8B7C24XX8B5EXX8B3685FF", CallOffset = 0x25 },
                new AddressStorage(){ Active = true, Name = "KO_ROTA_START",           Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "83ECXX56578BF9E8QQ8B35{0}85F6", BaseAddress = "KO_PTR_DLG", CallOffset = 0x14 },
                new AddressStorage(){ Active = true, Name = "KO_ROTA_STOP",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "83EC08535556578BF98BAFQQ39AFQQ8DB7", CallOffset = 0x17 },
                new AddressStorage(){ Active = true, Name = "KO_DEATH_EFFECT",         Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "7AXX8B138B42XX6AXX6AXX565656", CallOffset = 0xE  },
                new AddressStorage(){ Active = true, Name = "KO_MERC_PACKET",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "83ECXXA1QQ33C4894424XX80B9", CallOffset = 0x10 },
                new AddressStorage(){ Active = true, Name = "KO_MERC_CLOSE",           Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.CallAdd,   Hex = "A1{0}8B80QQ33DB8BE9894424", BaseAddress = "KO_PTR_DLG", CallOffset = 0x44 },
                new AddressStorage(){ Active = true, Name = "KO_MERC_MYBUYITEMS",      Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "BEQQ33D233DBC1E9XXF7F6BE" },

                new AddressStorage(){ Active = true, Name = "KO_RECV_FNC",             Start = 0x521000,  Length = 0x6B2000, Type = AddressEnum.Type.CallAdd,   Hex = "8BE55DC3CCCCCCCCCCCCCCCCCCCCCCCC6A", CallOffset = -0x1 },
                new AddressStorage(){ Active = true, Name = "KO_RECV_PTR",             Start = 0xA01000,  Length = 0xC01000, Type = AddressEnum.Type.CallAdd,   Hex = "5D5B83C4XXC3CCCCCCCCCCCCCCCC568BF1E8", CallOffset = -0x4 },
                #endregion
                #region [Offset]
                new AddressStorage(){ Active = true, Name = "KO_OFF_CLASS",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B15{0}8982", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_NATION",           Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B0D{0}8BB1", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MOVE",             Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8890QQA1{0}8890", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MOVEType",         Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "D998QQ8B0D{0}83B9", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_GoX",              Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "5BA1{0}D986", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_GoZ",              Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "5BA1{0}D986", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x4 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_GoY",              Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "5BA1{0}D986", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x8 },
                //new AddressStorage(){ Active = true, Name = "KO_OFF_X",                Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "83ECXXA1{0}8B90", BaseAddress = "KO_PTR_CHR"},
                //new AddressStorage(){ Active = true, Name = "KO_OFF_Z",                Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "83ECXXA1{0}8B90", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x4  },
                //new AddressStorage(){ Active = true, Name = "KO_OFF_Y",                Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "83ECXXA1{0}8B90", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x8 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_X",                Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "83ECXXA1{0}8B90", BaseAddress = "KO_PTR_CHR", AddressOffset = -0x4},
                new AddressStorage(){ Active = true, Name = "KO_OFF_Z",                Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "83ECXXA1{0}8B90", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_Y",                Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "83ECXXA1{0}8B90", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x4 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_ID",               Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}83C4XX05", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_WH",               Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B0D{0}83B9", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MCOR",             Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "894C2424895424288944242CD94424248B8B" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_PtBase",           Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}3998", BaseAddress = "KO_PTR_DLG" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_PtCount",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "751FA1{0}83B8", BaseAddress = "KO_PTR_DLG" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_Pt",               Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "751BA1{0}83B8", BaseAddress = "KO_PTR_DLG" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MAX_EXP",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}8B90QQ6AXXXX8B90", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_EXP",              Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}8B90QQ6AXXXX8B90", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x8 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MOB",              Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "0F8AQQ8B0D{0}0FB791", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_ZONE",             Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "751EA1{0}8B88", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_NAME_LEN",         Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}85C074XX83B8QQXX8B88", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_NAME",             Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}85C074XX83B8QQXX8B88QQ72XX8B80", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_GOLD",             Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}83B8QQXX7DXX81B8", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MAX_MP",           Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "5250E8QQA1{0}8B88", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MP",               Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "5250E8QQA1{0}8B88QQ8B90", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MAX_HP",           Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "74XXE8QQA1{0}8B88", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_HP",               Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "74XXE8QQA1{0}8B88QQ8B90", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_LEVEL",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "B8QQ8B15{0}8B8A", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = false, Name = "KO_OFF_STATE",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}55558BCEC680", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_POINTStat",        Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "E8QQ0FBFD0A1{0}8990", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_StatSTR",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}55578990", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_StatHP",           Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}55578990", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x8 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_StatDEX",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}55578990", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x10 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_StatINT",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}55578990", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x18 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_StatMP",           Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}55578990", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x20 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_ATTACK",           Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}55578990", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x28 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_DEFENCE",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}55578990", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x30 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_NP",               Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}8B90QQ8B80", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MAXWEIGHT",        Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B0D{0}55578981", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_WEIGHT",           Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B0D{0}55578981", BaseAddress = "KO_PTR_CHR", AddressOffset = 0x8 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_SBARBase",         Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "E8QQA1{0}83B8", BaseAddress = "KO_PTR_DLG" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_BSkPoint",         Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}8B88QQ6AXXE8QQ8B8E", BaseAddress = "KO_PTR_DLG" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_SPoint1",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}8B88QQ6AXXE8QQ8B8E", BaseAddress = "KO_PTR_DLG", AddressOffset = 0x10 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_SPoint2",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}8B88QQ6AXXE8QQ8B8E", BaseAddress = "KO_PTR_DLG", AddressOffset = 0x14 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_SPoint3",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}8B88QQ6AXXE8QQ8B8E", BaseAddress = "KO_PTR_DLG", AddressOffset = 0x18 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_SPoint4",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}8B88QQ6AXXE8QQ8B8E", BaseAddress = "KO_PTR_DLG", AddressOffset = 0x1C },
                new AddressStorage(){ Active = true, Name = "KO_OFF_ITEMB",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B15{0}8B92", BaseAddress = "KO_PTR_DLG" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_ITEMS",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B15{0}8B92QQ8B8482", BaseAddress = "KO_PTR_DLG" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_BANKB",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "EBD9A1{0}8B88", BaseAddress = "KO_PTR_DLG" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_BANKS",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "0F85XXXXXXXX8B13508B82XXXXXXXX8BCBFFD08B8BXXXXXXXX8BB48B" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_BANKCONT",         Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "CCCCCCCCCCCCCCCCCCCCCCCCCCCC568BF180BE" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_SKILLBASE",        Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}0F85QQ83B8QQXX0F84QQ8B88", BaseAddress = "KO_PTR_DLG" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_SKILLID",          Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "83ECXX53558BE98B85QQ8B8D" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_SWIFT",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "8B35{0}8B96QQ2B96", BaseAddress = "KO_PTR_CHR" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MCORX",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "83C4XX89AC24QQ89BC24", AddressOffset = -0x4 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MCORY",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "83C4XX89AC24QQ89BC24", AddressOffset = 0x4 },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MCORZ",            Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "83C4XX89AC24QQ89BC24" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MERC_WINDOW",      Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "6AXX8BCEE8QQ85C075XX8BCFE8QQ8B8F" },
                new AddressStorage(){ Active = true, Name = "KO_OFF_MERC_REMAIN",      Start = DefaultStart,  Length = DefaultLength, Type = AddressEnum.Type.Pointer,   Hex = "A1{0}83B8QQXX745D8B88", BaseAddress = "KO_PTR_DLG" },
                #endregion
            };

            foreach (var Address in AddressList)
            {
                if (Address.Active)
                {
                    if (Platform == AddressEnum.Platform.STEAM)
                    {
                        switch (Address.Name)
                        {
                            case "KO_PTR_PKT":
                                Address.Hex = "75XX894424XXE9QQ3B1D";
                                break;
                            case "KO_PTR_SND":
                                Address.Hex = "75XX894424XXE9QQ3B1D";
                                Address.CallOffset = 0x84;
                                break;
                            case "KO_OTO_LOGIN_04":
                                Address.Hex = "0F8DQQ5355565733DB33FF8DB1";
                                Address.CallOffset = 0x19;
                                break;
                        }
                    }

                    if (Platform == AddressEnum.Platform.JPKO)
                    {
                        switch (Address.Name)
                        {
                            case "KO_PTR_CHR":
                                Address.Hex = "CCCCCCCC8BC18B0D";
                                break;

                            case "KO_OFF_ID":
                                Address.Hex = "0FB6F0A1{0}83C4XX3998";
                                break;

                            case "KO_OFF_X":
                                Address.AddressOffset = 0x0;
                                break;

                            case "KO_OFF_Z":
                                Address.AddressOffset = 0x4;
                                break;

                            case "KO_OFF_Y":
                                Address.AddressOffset = 0x8;
                                break;

                            case "KO_OFF_GOLD":
                                Address.Hex = "A1{0}8B96QQ8B88";
                                break;

                            case "KO_OFF_POINTStat":
                                Address.Hex = "8B55XX0FB74402XX8B0D{0}0FBFC08981";
                                break;

                            case "KO_OFF_SKILLBASE":
                                Address.Hex = "8B0D{0}897424XX895C24XX885C24XX3999";
                                break;

                            case "KO_OFF_SKILLID":
                                Address.Hex = "E8QQ8B87QQ8986QQ8B8FQQ898EQQ8B97QQ5F8996";
                                break;

                            case "KO_PTR_SND":
                                Address.Hex = "68QQFF15QQ3B2D";
                                Address.CallOffset = 0x71;
                                break;

                            case "KO_OFF_MCORX":
                                Address.AddressOffset = -0x14;
                                break;

                            case "KO_OFF_MCORY":
                                Address.AddressOffset = -0xC;
                                break;

                            case "KO_OFF_MCORZ":
                                Address.AddressOffset = -0x10;
                                break;

                            case "KO_FLDB":
                                Address.AddressOffset = -0x4;
                                break;

                            case "KO_OFF_PtBase":
                                Address.Hex = "83C428C20400CCCCCCCCCCCCCC8B81";
                                break;

                            case "KO_OFF_PtCount":
                                Address.Hex = "D95C24508BCFD986";
                                break;

                            case "KO_OFF_Pt":
                                Address.Hex = "CCCCCC83ECXX53558BD98B83";
                                break;

                            case "KO_OFF_BSkPoint":
                            case "KO_OFF_SPoint1":
                                Address.Hex = "740C8B80XXXXXXXX8987XXXXXXXX83BF";
                                Address.AddressOffset = 0x14;
                                break;

                            case "KO_OFF_SPoint2":
                                Address.Hex = "740C8B80XXXXXXXX8987XXXXXXXX83BF";
                                Address.AddressOffset = 0x18;
                                break;

                            case "KO_OFF_SPoint3":
                                Address.Hex = "740C8B80XXXXXXXX8987XXXXXXXX83BF";
                                Address.AddressOffset = 0x1C;
                                break;

                            case "KO_OFF_SPoint4":
                                Address.Hex = "740C8B80XXXXXXXX8987XXXXXXXX83BF";
                                Address.AddressOffset = 0x1E;
                                break;

                            case "KO_OFF_MOVEType":
                                Address.Hex = "8B0D{0}8B81E403000083F804747C83F805747783F80D747283B9";
                                break;

                            case "KO_OFF_ITEMS":
                                Address.Hex = "8B15{0}8B82QQ8B80";
                                Address.AddressOffset = -0x20;
                                break;
                        }
                    }

                    Address.Hex = Address.Hex.ToUpper().Replace("QQ", "XXXXXXXX");

                    switch (Address.Type)
                    {
                        case AddressEnum.Type.Pointer:
                        case AddressEnum.Type.CallAdd:
                            switch (Address.Name)
                            {
                                case "KO_FPBS":
                                    var FmbsResult = AddressList.Where(x => x.Name == "KO_FMBS").SingleOrDefault();
                                    Address.Start = FmbsResult != null ? FmbsResult.Value : 0;
                                    break;
                                case "KO_OFF_MCORX":
                                case "KO_OFF_MCORY":
                                case "KO_OFF_MCORZ":
                                    if (Platform != AddressEnum.Platform.JPKO)
                                    {
                                        var OffMCorResult = AddressList.Where(x => x.Name == "KO_OFF_MCOR").SingleOrDefault();
                                        Address.Start = OffMCorResult != null ? OffMCorResult.Value : 0;
                                    }
                                    break;
                            }

                            var BaseAddress = !string.IsNullOrEmpty(Address.BaseAddress) ? AddressToHex(int.Parse(AddressList.Where(x => x.Name == Address.BaseAddress).SingleOrDefault()?.Address, NumberStyles.HexNumber)) : "";

                            Address.Hex = string.Format(Address.Hex, BaseAddress);
                            Address.Value = FindAddress(Handle, Address.Hex, Address.Start, Address.Length) + Address.Hex.Length / 2;
                            Address.Call = DecimalToHex(Address.Value - Address.CallOffset);
                            Address.Address = Address.Type == AddressEnum.Type.Pointer ? DecimalToHex(Read4Byte(Handle, Address.Value) + Address.AddressOffset) : Address.Call;
                            break;

                        case AddressEnum.Type.Generate:
                            switch (Address.Name)
                            {
                                case "KO_ITEMFIND":
                                    for (int i = 0; i < 9; i++)
                                    {
                                        Address.Value = FindAddress(Handle, Address.Hex, Address.Start, Address.Length) + Address.Hex.Length / 2;
                                        Address.Call = DecimalToHex(Address.Value - Address.CallOffset);
                                        Address.Address = Address.Call;
                                        Address.Start = Address.Value;
                                    }
                                    break;
                                case "KO_SMMB_FNC":
                                    for (int i = 0; i < 7; i++)
                                    {
                                        Address.Value = FindAddress(Handle, Address.Hex, Address.Start, Address.Length) + Address.Hex.Length / 2;
                                        Address.Call = DecimalToHex(Address.Value - Address.CallOffset);
                                        Address.Address = Address.Call;
                                        Address.Start = Address.Value;
                                    }
                                    break;
                                default: break;
                            }
                            break;
                    }
                }
            }
            return AddressList;
        }

    }
}
#endregion
