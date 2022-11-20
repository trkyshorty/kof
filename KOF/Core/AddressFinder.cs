using System;
using System.Collections.Generic;
using IniParser;
using IniParser.Model;
using System.Diagnostics;
using System.IO;

namespace KOF.Core
{
    #region "Enums"
    public class AddressEnum
    {
        public enum Type : byte
        {
            Pointer,
            Offset,
        }

        public enum Platform : byte
        {
            USKO,
            STEAM,
            JPKO,
            CNKO,
            PVP,
        }
    }
    #endregion

    #region "Storage"
    public class AddressStorage
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public AddressEnum.Type Type { get; set; }
    }
    #endregion

    #region "Finder"
    public class AddressFinder : Helper
    {
        public List<AddressStorage> LoadAddressList(IntPtr Handle, AddressEnum.Platform Platform)
        {
            List<AddressStorage> AddressList = new List<AddressStorage>()
            {
                new AddressStorage(){ Name = "KO_PTR_CHR", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_DLG", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_PKT", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_SND", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_FLDB", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_FMBS", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_FPBS", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_ROUTE_START", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_ROUTE_STOP", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_STATE", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_INTRO_SKIP_CALL", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_LOGIN", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_LOGIN_BTN", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_LOGIN_BTN_BASE", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_LOGIN_BTN_CALL", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_SERVER_SELECT", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_CHARACTER_SELECT", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_CHARACTER_SELECT_LEFT", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_CHARACTER_SELECT_ENTER", Type = AddressEnum.Type.Pointer },
                new AddressStorage(){ Name = "KO_PTR_CHARACTER_SELECT_RIGHT", Type = AddressEnum.Type.Pointer },

                new AddressStorage(){ Name = "KO_OFF_DEFENCE_POINT", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_ATTACK_POINT", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_STATE", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_MOVE_TYPE", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_MOVE", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_GO_Y", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_GO_Z", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_GO_X", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_Y", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_Z", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_X", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_ZONE", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_ID", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_MOB", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_NAME_LENGTH", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_NAME", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_STAT_INT", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_STAT_MP", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_STAT_DEX", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_STAT_HP", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_STAT_STR", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_STAT_POINT", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_MAX_EXP", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_EXP", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_MAX_MP", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_MP", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_NATION", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_CLASS", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_MAX_HP", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_HP", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_LEVEL", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_GOLD", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_WH", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_ITEM_BASE", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_ITEM_SLOT", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_SKILL_TREE_BASE", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_SKILL_TREE_POINT", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_SKILL_BASE", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_SKILL_SLOT", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_PARTY_BASE", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_PARTY_COUNT", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_PARTY_LIST", Type = AddressEnum.Type.Offset },
                new AddressStorage(){ Name = "KO_OFF_SPEED", Type = AddressEnum.Type.Offset },

            };

            string PointerFile = Directory.GetCurrentDirectory() + "\\Resource\\Pointer\\" + Platform.ToString() + ".ini";

            if (Platform == AddressEnum.Platform.PVP)
            {
                string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string ApplicationFolder = MyDocuments + "\\" + "KOF";
                PointerFile = ApplicationFolder + "\\Pointer.ini";
            }

            var IniParser = new FileIniDataParser();

            if (!File.Exists(PointerFile))
            {
                if(Platform != AddressEnum.Platform.PVP)
                {
                   System.Windows.Forms.DialogResult Dialog = System.Windows.Forms.MessageBox.Show("Uygulama dosyaları eksik, lütfen uygulamayı güncelleyin.",
                        "KOF", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                    if (Dialog == System.Windows.Forms.DialogResult.OK)
                        System.Environment.Exit(1);
                }

                IniData IniData = new IniData();

                foreach (var Address in AddressList)
                    IniData[Address.Type.ToString()][Address.Name] = "";
               
                IniParser.WriteFile(PointerFile, IniData);
            }
            else
            {
                IniData IniData = IniParser.ReadFile(PointerFile);

                foreach (var Address in AddressList)
                    Address.Address = IniData[Address.Type.ToString()][Address.Name];
            }

            return AddressList;
        }

    }
}
#endregion
