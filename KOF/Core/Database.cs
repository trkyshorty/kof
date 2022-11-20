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
            _Connection.CreateTable<Route>();
            _Connection.CreateTable<Store>();

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
            return _Connection.Table<Sell>().Where(t => t.User == User && t.Platform == Platform).ToList();
        }

        public int SetSell(Sell sell)
        {
            if (sell.Id == 0)
            {
                _Connection.Insert(sell);
                return (int)SQLite3.LastInsertRowid(_Connection.Handle);
            }
            else
                _Connection.Update(sell);

            return 0;
        }
        public void DeleteSell(Sell sell)
        {
            if (sell.Id != 0)
                _Connection.Delete<Sell>(sell.Id);
        }

        public void ClearSell(List<Sell> sellList)
        {
            if (sellList.Count() == 0)
                return;

            _Connection.RunInTransaction(() =>
            {
                sellList.ForEach(sell =>
                {
                    _Connection.Delete(sell);
                });
            });
        }

        public List<Loot> GetLootList(string User, string Platform)
        {
            return _Connection.Table<Loot>().Where(t => t.User == User && t.Platform == Platform).ToList();
        }

        public int SetLoot(Loot loot)
        {
            if (loot.Id == 0)
            {
                _Connection.Insert(loot);
                return (int)SQLite3.LastInsertRowid(_Connection.Handle);
            }
            else
                _Connection.Update(loot);

            return 0;
        }

        public void DeleteLoot(Loot loot)
        {
            if (loot.Id != 0)
                _Connection.Delete<Loot>(loot.Id);
        }

        public void ClearLoot(List<Loot> lootList)
        {
            if (lootList.Count() == 0)
                return;

            _Connection.RunInTransaction(() =>
            {
                lootList.ForEach(loot =>
                {
                    _Connection.Delete(loot);
                });
            });
        }

        public List<Item> GetItemList()
        {
            return _Connection.Table<Item>().ToList();
        }

        public List<Skill> GetSkillList(string job)
        {
            return _Connection.Table<Skill>().Where(t => t.Job == job).ToList();
        }

        public List<SkillBar> GetSkillBarList(string user, string platform)
        {
            return _Connection.Table<SkillBar>().Where(t => t.User == user && t.Platform == platform).OrderByDescending(t => t.SkillId).ToList();
        }

        public int SetSkillBar(SkillBar skillBar)
        {
            if(skillBar.Id == 0)
            {
                _Connection.Insert(skillBar);
                return (int)SQLite3.LastInsertRowid(_Connection.Handle);
            }
            else
                _Connection.Update(skillBar);

            return 0;
        }

        public void DeleteSkillBar(SkillBar skillBar)
        {
            if (skillBar.Id != 0)
                _Connection.Delete<SkillBar>(skillBar.Id);
        }

        public List<Control> GetControlList(string Form)
        {
            return _Connection.Table<Control>().Where(t => t.Form == Form).ToList();
        }

        public int SetControl(Control control)
        {
            if (control.Id == 0)
            {
                _Connection.Insert(control);
                return (int)SQLite3.LastInsertRowid(_Connection.Handle);
            }
            else
                _Connection.Update(control);

            return 0;
        }

        public void SetAccount(string AccountId, string AccountPassword, string Path, string Platform)
        {
            Account Account = new Account();

            Account.AccountId = AccountId;
            Account.Password = AccountPassword;
            Account.Path = Path;
            Account.Platform = Platform;

            _Connection.Insert(Account);

            Account.Id = (int)SQLite3.LastInsertRowid(_Connection.Handle);
        }

        public void SetAccount(Account Account)
        {
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
            return _Connection.Table<Account>().Where(t => t.CharacterName == Name && t.Platform == Platform)?.FirstOrDefault();
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
                AccountList.Add(AccountData.Id, AccountData.AccountId);
            return AccountList;
        }

        public Zone GetZoneById(int Id)
        {
            return _Connection.Table<Zone>().Where(t => t.Id == Id).FirstOrDefault();
        }

        public int SetTarget(Target target)
        {
            if (target.Id == 0)
            {
                _Connection.Insert(target);
                return (int)SQLite3.LastInsertRowid(_Connection.Handle);
            }
            else
                _Connection.Update(target);

            return 0;
        }

        public List<Target> GetTargetList(string User, string Platform)
        {
            return _Connection.Table<Target>().Where(t => t.User == User && t.Platform == Platform).ToList();
        }

        public void ClearTargetList(List<Target> targetList)
        {
            if (targetList.Count() == 0)
                return;

            _Connection.RunInTransaction(() =>
            {
                targetList.ForEach(target =>
                {
                    _Connection.Delete(target);
                });
            });
        }

        public List<Route> GetRouteList(int zone)
        {
            return _Connection.Table<Route>().Where(t => t.Zone == zone).ToList();
        }

        public Route GetRoute(int id)
        {
            return _Connection.Table<Route>().Where(t => t.Id == id).FirstOrDefault();
        }

        public void SetRoute(string Name, int Zone, string Data)
        {
            KOF.Models.Route route = new KOF.Models.Route();

            route.Name = Name;
            route.Zone = Zone;
            route.Data = Data;

            _Connection.Insert(route);
        }

        public void ClearRoute(KOF.Models.Route route)
        {
            _Connection.Delete(route);
        }


    }
}
