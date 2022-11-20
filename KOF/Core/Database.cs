using System.Collections.Generic;
using System.Linq;
using System.IO;
using SQLite;
using KOF.Common;
using KOF.Models;
using System.Threading;
using System;
using System.Diagnostics;

namespace KOF.Core
{
    public class Database
    {
        private SQLiteConnection _Connection { get; set; }

        public Database()
        {
            string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string ApplicationFolder = MyDocuments + "\\" + "KOF";

            Directory.CreateDirectory(ApplicationFolder);

            _Connection = new SQLiteConnection(ApplicationFolder + "\\KOF.dat");

            _Connection.CreateTable<Migration>();
            _Connection.CreateTable<Account>();
            _Connection.CreateTable<Control>();
            _Connection.CreateTable<Item>();
            _Connection.CreateTable<Loot>();
            _Connection.CreateTable<Npc>();
            _Connection.CreateTable<Sell>();
            _Connection.CreateTable<Skill>();
            _Connection.CreateTable<SkillBar>();
            _Connection.CreateTable<Zone>();
            _Connection.CreateTable<Target>();

            string MigrationFolder = @".\Migration\";

            if (!Directory.Exists(MigrationFolder))
            {
                System.Windows.Forms.DialogResult Dialog = System.Windows.Forms.MessageBox.Show("Uygulama dosyaları eksik, lütfen uygulamayı güncelleyin.",
                       "KOF", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                if (Dialog == System.Windows.Forms.DialogResult.OK)
                    System.Environment.Exit(1);
            }
            else
            {
                var MigrationThread = new Thread(() =>
                {
                    var Migrations = Directory.GetFiles(MigrationFolder).OrderBy(f => f);

                    _Connection.RunInTransaction(() =>
                    {
                        foreach (string Migration in Migrations)
                        {
                            if (Path.GetExtension(Migration) != ".sql") continue;

                            string MigrationFileName = Path.GetFileNameWithoutExtension(Migration);

                            if (_Connection.Table<Migration>().Where(t => t.File == MigrationFileName).FirstOrDefault() != null) continue;

                            string[] Lines = File.ReadAllLines(Migration);

                            foreach (string Line in Lines)
                                _Connection.ExecuteScalar<string>(Line);

                            Migration MigrationTable = new Migration();
                            MigrationTable.File = MigrationFileName;

                            _Connection.Insert(MigrationTable);
                        }
                    });
                });

                MigrationThread.IsBackground = true;
                MigrationThread.Start();
            }
        }

        public List<Sell> GetSellList(string User, string Platform)
        {
            List<Sell> SellList;

            if (Storage.SellCollection.TryGetValue(User, out SellList))
                return SellList.ToList();
            else
                return _Connection.Table<Sell>().Where(t => t.User == User && t.Platform == Platform).ToList();
        }

        public Sell GetSell(string User, int ItemId, string Platform)
        {
            List<Sell> SellList;

            if (Storage.SellCollection.TryGetValue(User, out SellList))
                return SellList.FirstOrDefault(x => x.ItemId == ItemId && x.Platform == Platform);

            return null;
        }

        public void SetSell(string User, int ItemId, string ItemName, string Platform)
        {
            List<Sell> SellList;

            if (Storage.SellCollection.TryGetValue(User, out SellList))
            {
                Sell SellData = SellList.FirstOrDefault(x => x.ItemId == ItemId && x.Platform == Platform);

                if (SellData == null)
                {
                    SellData = new Sell();

                    SellData.User = User;
                    SellData.ItemId = ItemId;
                    SellData.ItemName = ItemName;
                    SellData.Platform = Platform;

                    SellList.Add(SellData);
                }
            }
        }

        public void DeleteSell(string User, int ItemId, string Platform)
        {
            List<Sell> SellList;

            if (Storage.SellCollection.TryGetValue(User, out SellList))
            {
                Sell SellData = SellList.FirstOrDefault(x => x.ItemId == ItemId && x.Platform == Platform);

                if (SellData != null)
                {
                    SellList.Remove(SellData);

                    if (SellData.Id != 0)
                        _Connection.Delete<Sell>(SellData.Id);
                }
            }
        }

        public void SaveSell(string User)
        {
            List<Sell> SellList;

            if (Storage.SellCollection.TryGetValue(User, out SellList))
            {
                if(SellList.Count > 0)
                {
                    _Connection.RunInTransaction(() =>
                    {
                        SellList.ForEach(Sell =>
                        {
                            if (Sell.Id == 0)
                            {
                                _Connection.Insert(Sell);

                                Sell.Id = (int)SQLite3.LastInsertRowid(_Connection.Handle);
                            }
                            else
                                _Connection.Update(Sell);
                        });
                    });
                }
            }
        }
        public List<Loot> GetLootList(string User, string Platform)
        {
            List<Loot> LootList;

            if (Storage.LootCollection.TryGetValue(User, out LootList))
                return LootList.ToList();
            else
                return _Connection.Table<Loot>().Where(t => t.User == User && t.Platform == Platform).ToList();
        }

        public Loot GetLoot(string User, int ItemId, string Platform)
        {
            List<Loot> LootList;

            if (Storage.LootCollection.TryGetValue(User, out LootList))
                return LootList.FirstOrDefault(x => x.ItemId == ItemId && x.Platform == Platform);

            return null;
        }

        public void SetLoot(string User, int ItemId, string ItemName, string Platform)
        {
            List<Loot> LootList;

            if (Storage.LootCollection.TryGetValue(User, out LootList))
            {
                Loot LootData = LootList.FirstOrDefault(x => x.ItemId == ItemId && x.Platform == Platform);

                if (LootData == null)
                {
                    LootData = new Loot();

                    LootData.User = User;
                    LootData.ItemId = ItemId;
                    LootData.ItemName = ItemName;
                    LootData.Platform = Platform;

                    LootList.Add(LootData);
                }
            }
        }

        public void DeleteLoot(string User, int ItemId, string Platform)
        {
            List<Loot> LootList;

            if (Storage.LootCollection.TryGetValue(User, out LootList))
            {
                Loot LootData = LootList.FirstOrDefault(x => x.ItemId == ItemId && x.Platform == Platform);

                if (LootData != null)
                {
                    LootList.Remove(LootData);

                    if (LootData.Id != 0)
                        _Connection.Delete<Loot>(LootData.Id);
                }
            }
        }

        public void SaveLoot(string User)
        {
            List<Loot> LootList;

            if (Storage.LootCollection.TryGetValue(User, out LootList))
            {
                if (LootList.Count > 0)
                {
                    _Connection.RunInTransaction(() =>
                    {
                        LootList.ForEach(Loot =>
                        {
                            if (Loot.Id == 0)
                            {
                                _Connection.Insert(Loot);

                                Loot.Id = (int)SQLite3.LastInsertRowid(_Connection.Handle);
                            }
                            else
                                _Connection.Update(Loot);
                        });
                    });
                }
            }
        }

        public List<Item> GetItemList()
        {
            return _Connection.Table<Item>().ToList();
        }

        public List<Npc> GetNpcList()
        {
            return _Connection.Table<Npc>().ToList();
        }

        public void SetNpc(int Zone, string Type, int RealId, int X, int Y, int Nation, string Platform, int Town)
        {
            Npc NpcData = _Connection.Table<Npc>().Where(t => t.RealId == RealId && t.Zone == Zone && t.Platform == Platform).FirstOrDefault();

            if (NpcData != null)
            {
                NpcData.Town = Town;
                NpcData.Type = Type;
                NpcData.X = X;
                NpcData.Y = Y;
                NpcData.Nation = Nation;

                _Connection.Update(NpcData);

                Debug.WriteLine(string.Format("{0} updated.", NpcData.Id));
            }
            else
            {
                NpcData = new Npc();

                NpcData.Zone = Zone;
                NpcData.Type = Type;
                NpcData.RealId = RealId;
                NpcData.X = X;
                NpcData.Y = Y;
                NpcData.Town = Town;
                NpcData.Nation = Nation;
                NpcData.Platform = Platform;

                _Connection.Insert(NpcData);

                Debug.WriteLine(string.Format("{0} inserted.", NpcData.Id));
            }

            Storage.NpcCollection = GetNpcList();
        }

        public Skill GetSkillData(string User, int SkillId)
        {
            List<Skill> SkillList;

            if (Storage.SkillCollection.TryGetValue(User, out SkillList))
                return SkillList.FirstOrDefault(x => x.Id == SkillId);

            return null;
        }

        public Skill GetSkillData(string User, string SkillName)
        {
            List<Skill> SkillList;

            if (Storage.SkillCollection.TryGetValue(User, out SkillList))
                return SkillList.FirstOrDefault(x => x.Name == SkillName);

            return null;
        }

        public List<Skill> GetSkillList(string Job)
        {
            List<Skill> SkillList;

            if (Storage.SkillCollection.TryGetValue(Job, out SkillList))
                return SkillList;
            else
                return _Connection.Table<Skill>().Where(t => t.Job == Job).ToList();
        }

        public List<SkillBar> GetSkillBarList(string User, string Platform)
        {
            List<SkillBar> SkillBarList;

            if (Storage.SkillBarCollection.TryGetValue(User, out SkillBarList))
                return SkillBarList.OrderByDescending(p => p.SkillId).ToList();
            else
                return _Connection.Table<SkillBar>().Where(t => t.User == User && t.Platform == Platform).OrderByDescending(t => t.SkillId).ToList();
        }

        public SkillBar GetSkillBar(string User, int SkillId, string Platform)
        {
            List<SkillBar> SkillBarList;

            if (Storage.SkillBarCollection.TryGetValue(User, out SkillBarList))
                return SkillBarList.FirstOrDefault(x => x.SkillId == SkillId && x.Platform == Platform);

            return null;
        }

        public void SetSkillBar(string User, int SkillId, int SkillType, string Platform)
        {
            List<SkillBar> SkillBarList;

            if (Storage.SkillBarCollection.TryGetValue(User, out SkillBarList))
            {
                SkillBar SkillBarData = SkillBarList.FirstOrDefault(x => x.SkillId == SkillId);

                if (SkillBarData == null)
                {
                    SkillBarData = new SkillBar();

                    SkillBarData.User = User;
                    SkillBarData.SkillId = SkillId;
                    SkillBarData.SkillType = SkillType;
                    SkillBarData.Platform = Platform;
                    SkillBarData.UseTime = 0;

                    SkillBarList.Add(SkillBarData);
                }
            }
        }

        public void DeleteSkillBar(string User, int SkillId, string Platform)
        {
            List<SkillBar> SkillBarList;

            if (Storage.SkillBarCollection.TryGetValue(User, out SkillBarList))
            {
                SkillBar SkillBarData = SkillBarList.FirstOrDefault(x => x.SkillId == SkillId && x.Platform == Platform);

                if (SkillBarData != null)
                {
                    SkillBarList.Remove(SkillBarData);

                    if (SkillBarData.Id != 0)
                        _Connection.Delete<SkillBar>(SkillBarData.Id);
                }
            }
        }

        public void SaveSkillBar(string User)
        {
            List<SkillBar> SkillBarList;

            if (Storage.SkillBarCollection.TryGetValue(User, out SkillBarList))
            {
                if (SkillBarList.Count > 0)
                {
                    _Connection.RunInTransaction(() =>
                    {
                        SkillBarList.ForEach(SkillBar =>
                        {
                            if (SkillBar.Id == 0)
                            {
                                _Connection.Insert(SkillBar);

                                SkillBar.Id = (int)SQLite3.LastInsertRowid(_Connection.Handle);
                            }
                            else
                                _Connection.Update(SkillBar);
                        });
                    });
                }
            }
        }

        public List<Control> GetControlList(string Form)
        {
            List<Control> ControlList;

            if (Storage.ControlCollection.TryGetValue(Form, out ControlList))
                return ControlList;
            else
                return _Connection.Table<Control>().Where(t => t.Form == Form).ToList();
        }

        public string GetControl(string Form, string Platform, string Name, string DefaultValue = "")
        {
            List<Control> ControlList;

            if (Storage.ControlCollection.TryGetValue(Form, out ControlList))
            {
                Control ControlData = ControlList.FirstOrDefault(x => x.Name == Name && x.Platform == Platform);

                if (ControlData != null)
                    return ControlData.Value;
                else
                {
                    if (DefaultValue != "")
                        SetControl(Form, Platform, Name, DefaultValue);
                }
            }

            return DefaultValue;
        }

        public void SetControl(string Form, string Platform, string Name, string Value)
        {
            List<Control> ControlList;

            if (Storage.ControlCollection.TryGetValue(Form, out ControlList))
            {
                Control ControlData = ControlList.FirstOrDefault(x => x.Name == Name && x.Platform == Platform);

                if (ControlData != null)
                    ControlData.Value = Value;
                else
                {
                    ControlData = new Control();

                    ControlData.Form = Form;
                    ControlData.Platform = Platform;
                    ControlData.Name = Name;
                    ControlData.Value = Value;

                    ControlList.Add(ControlData);
                }
            }
        }

        public void SaveControl(string Form)
        {
            List<Control> ControlList;

            if (Storage.ControlCollection.TryGetValue(Form, out ControlList))
            {
                if (ControlList.Count > 0)
                {
                    _Connection.RunInTransaction(() =>
                    {
                        ControlList.ForEach(Control =>
                        {
                            if (Control.Id == 0)
                            {
                                _Connection.Insert(Control);

                                Control.Id = (int)SQLite3.LastInsertRowid(_Connection.Handle);
                            }
                            else
                                _Connection.Update(Control);
                        });
                    });
                }
            }
        }

        public void SetAccount(string AccountId, string AccountPassword, string Path, string Platform)
        {
            Account Account = new Account();

            Account.Name = AccountId;
            Account.Hash = AccountPassword;
            Account.Path = Path;
            Account.Platform = Platform;

            _Connection.Insert(Account);

            Account.Id = (int)SQLite3.LastInsertRowid(_Connection.Handle);
        }

        public Account GetAccountById(int Id)
        {
            return _Connection.Table<Account>().Where(t => t.Id == Id).FirstOrDefault();
        }

        public void DeleteAccount(int Id)
        {
            Account Account = GetAccountById(Id);

            if (Account != null)
                _Connection.Delete(Account);
        }

        public Account GetAccountByName(string Name, string Platform)
        {
            return _Connection.Table<Account>().Where(t => t.Name == Name && t.Platform == Platform)?.FirstOrDefault();
        }

        public List<Account> GetAccountListByPlatform(string Platform)
        {
            return _Connection.Table<Account>().Where(t => t.Platform == Platform)?.ToList();
        }

        public Dictionary<int, string> GetAccountList()
        {
            var AccountList = new Dictionary<int, string>();
            var AccountDataList = _Connection.Table<Account>().ToList();
            foreach (Account AccountData in AccountDataList)
                AccountList.Add(AccountData.Id, AccountData.Name);
            return AccountList;
        }

        public Zone GetZoneById(int Id)
        {
            return _Connection.Table<Zone>().Where(t => t.Id == Id).FirstOrDefault();
        }

        public void SetTarget(string User, string Platform, string Name, int Checked)
        {
            List<Target> TargetList;

            if (Storage.TargetCollection.TryGetValue(User, out TargetList))
            {
                Target TargetData = TargetList.FirstOrDefault(x => x.Name == Name);

                if (TargetData != null)
                {
                    TargetData.Name = Name;
                    TargetData.Checked = Checked;
                }
                else
                {
                    TargetData = new Target();

                    TargetData.User = User;
                    TargetData.Platform = Platform;
                    TargetData.Name = Name;
                    TargetData.Checked = Checked;

                    TargetList.Add(TargetData);
                }
            }
        }

        public List<Target> GetTargetList(string User, string Platform)
        {
            List<Target> TargetList;

            if (Storage.TargetCollection.TryGetValue(User, out TargetList))
                return TargetList;
            else
                return _Connection.Table<Target>().Where(t => t.User == User && t.Platform == Platform).ToList();
        }

        public Target GetTarget(string User, string Platform, string Name)
        {
            List<Target> TargetList;

            if (Storage.TargetCollection.TryGetValue(User, out TargetList))
                return TargetList.FirstOrDefault(x => x.Platform == Platform && x.Name == Name);

            return null;
        }

        public void SaveTarget(string User)
        {
            List<Target> TargetList;

            if (Storage.TargetCollection.TryGetValue(User, out TargetList))
            {
                if (TargetList.Count > 0)
                {
                    _Connection.RunInTransaction(() =>
                    {
                        TargetList.ForEach(Target =>
                        {
                            if (Target.Id == 0)
                            {
                                _Connection.Insert(Target);

                                Target.Id = (int)SQLite3.LastInsertRowid(_Connection.Handle);
                            }
                            else
                                _Connection.Update(Target);
                        });
                    });
                }

            }
        }

        public void ClearTarget(string User)
        {
            List<Target> TargetList;

            if (Storage.TargetCollection.TryGetValue(User, out TargetList))
            {
                if (TargetList.Count > 0)
                {
                    _Connection.RunInTransaction(() =>
                    {
                        TargetList.ForEach(Target =>
                        {
                            _Connection.Delete(Target);
                        });
                    });
                }

                Storage.TargetCollection[User].Clear();
            }
        }

    }
}
