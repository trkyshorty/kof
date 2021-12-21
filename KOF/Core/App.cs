using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Management;
using System.Linq;
using KOF.Common;
using KOF.Models;
using KOF.UI;

namespace KOF.Core
{
    public class App : Helper
    {
        private Database _Database { get; set; }
        private Main _MainInterface { get; set; }
        private Thread LauncherThread { get; set; }
        private Thread HandleProcessThread { get; set; }

        public App(Main MainInterface)
        {
            _MainInterface = MainInterface;
        }

        public void Load()
        {
            _Database = new Database();

            Storage.NpcCollection = _Database.GetNpcList();

            Storage.ItemCollection = _Database.GetItemList();

            Storage.ControlCollection.Add("App", _Database.GetControlList("App"));

            InitializeMainThread();
        }

        public Database Database()
        {
            return _Database;
        }

        public string GetControl(string Name, string DefaultValue = "")
        {
            return Database().GetControl("App", "Base", Name, DefaultValue);
        }

        public void SetControl(string Name, string Value)
        {
            Database().SetControl("App", "Base", Name, Value);
        }

        private void InitializeMainThread()
        {
            Thread MainThread = new Thread(() =>
            {
                while (true)
                {
                    foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
                    {
                        if (ClientData == null) continue;

                        if (ClientData.IsDisconnected() || ClientData.GetLevel() == 0)
                        {
                            ClientData.Destroy();

                            if (ClientData.HasExited() == false)
                                ClientData.GetProcess().Kill();

                            Debug.WriteLine("App > PID " + ClientData.GetProcessId() + " Lost");

                            if (Convert.ToBoolean(GetControl("AutoLogin")))
                            {
                                List<int> LoginList = new List<int>();

                                Account Account = Database().GetAccountByName(ClientData.GetAccountName(), ClientData.GetPlatform().ToString());

                                if (Account != null)
                                {
                                    LoginList.Add(Account.Id);

                                    while (LauncherThread != null && LauncherThread.IsAlive)
                                        Thread.Sleep(1);

                                    Launcher(LoginList);

                                    _MainInterface.Notify(System.Windows.Forms.ToolTipIcon.Error, "Bağlantı koptu", ClientData.GetAccountName() + " yeniden oyuna giriş yapıyor.");
                                }
                                else
                                    _MainInterface.Notify(System.Windows.Forms.ToolTipIcon.Error, "Bağlantı koptu", "Yeniden bağlanılamıyor, hesap veritabanında bulunamadı.");
                            }
                        }
                        else
                        {
                            if (Storage.FollowedClient != null && ClientData.GetProcessId() != Storage.FollowedClient.GetProcessId()
                                && ClientData.GetZone() == Storage.FollowedClient.GetZone() && ClientData.IsCharacterAvailable()
                                && ClientData.IsInFallback() == false && ClientData.IsInEnterGame() == false
                                && ClientData.GetAction() == 0
                                && Storage.FollowedClient.GetAction() == 0
                                && Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                            {
                                if (Storage.FollowedClient.GetTargetId() != ClientData.GetTargetId())
                                    ClientData.SelectTarget(Storage.FollowedClient.GetTargetId());

                                if ((ClientData.GetX() != Storage.FollowedClient.GetX() || ClientData.GetY() != Storage.FollowedClient.GetY()))
                                {
                                    if (Convert.ToBoolean(ClientData.GetControl("ActionMove")) == true)
                                        ClientData.MoveCoordinate(Storage.FollowedClient.GetX(), Storage.FollowedClient.GetY());
                                    else if (Convert.ToBoolean(ClientData.GetControl("ActionSetCoordinate")) == true)
                                        ClientData.SetCoordinate(Storage.FollowedClient.GetX(), Storage.FollowedClient.GetY());
                                }

                                if (Convert.ToBoolean(GetControl("AutoParty")) == true)
                                {
                                    if (Storage.FollowedClient.GetPartyCount() < 8 && Storage.FollowedClient.IsPartyMember(ClientData.GetNameConst()) == false)
                                        Storage.FollowedClient.SendParty(ClientData.GetNameConst());
                                }
                            }
                        }
                    }

                    Thread.Sleep(1);
                }
            });

            MainThread.IsBackground = true;
            MainThread.Start();
        }

        public void Launcher(List<int> AccountList)
        {
            if (AccountList.Count == 0)
                return;

            if (LauncherThread != null && LauncherThread.IsAlive)
                return;

            LauncherThread = new Thread(() => LauncherThreadEvent(AccountList));
            LauncherThread.IsBackground = true;
            LauncherThread.Start();
        }

        private void LauncherThreadEvent(List<int> AccountList)
        {
            AccountList.ForEach(AccountId =>
            {
                Account Account = Database().GetAccountById(AccountId);

                if (Account != null)
                {
                    Process Process = new Process();

                    FileInfo FileInfo = new FileInfo(Account.Path);

                    Process.StartInfo = new ProcessStartInfo(FileInfo.Name);
                    Process.StartInfo.WorkingDirectory = FileInfo.Directory.FullName;

                    if (Account.Platform == "JPKO")
                        Process.StartInfo.Arguments = "MGAMEJP " + Account.Name + " " + Account.Hash;
                    else if (Account.Platform == "CNKO")
                        Process.StartInfo.Arguments = Process.GetCurrentProcess().Id.ToString();

                    Process.Start();

                    Debug.WriteLine("Launcher -> " + Account.Name + " starting.");

                    Thread.Sleep(1250);

                    while (!ShowWindow(Process.MainWindowHandle, Windows.NORMAL))
                        Process.Refresh();

                    Dispatcher Dispatcher = new Dispatcher(this);

                    Dispatcher.GetClient().SetAccountName(Account.Name);

                    Dispatcher.GetClient().HandleProcess(Process, null);

                    Thread.Sleep(1250);

                    int Tick = Environment.TickCount;
                    int SplashTry = 0;

                    while (Storage.ClientCollection.ContainsKey(Process.Id) == false)
                    {
                        if (Dispatcher.GetClient().IsDisconnected() || Environment.TickCount - Tick > 60000)
                        {
                            Dispatcher.GetClient().Destroy();

                            if (Dispatcher.GetClient().HasExited() == false)
                                Dispatcher.GetClient().GetProcess().Kill();

                            _MainInterface.Notify(System.Windows.Forms.ToolTipIcon.Error, "Giriş yapılamadı", Account.Name + " yeniden başlatılıyor.");

                            var StartAccountList = new List<int>();
                            StartAccountList.Add(AccountId);
                            LauncherThreadEvent(StartAccountList);
                            return;
                        }

                        switch (Dispatcher.GetClient().GetPhase())
                        {
                            case Processor.EPhase.None:
                            case Processor.EPhase.Authentication:
                            case Processor.EPhase.Loggining:
                            case Processor.EPhase.Selecting:
                                {
                                    if (Convert.ToBoolean(GetControl("AutoLogin")) && Dispatcher.GetClient().HasExited() == false)
                                    {
                                        SetForegroundWindow(Process.MainWindowHandle);

                                        if (SplashTry > 15)
                                        {
                                            if ((Dispatcher.GetClient().GetPhase() == Processor.EPhase.None) && Account.Platform == "CNKO")
                                            {
                                                AutoIt.AutoItX.Send(Account.Name);
                                                Thread.Sleep(50);
                                                AutoIt.AutoItX.Send("{tab}");
                                                Thread.Sleep(50);
                                                AutoIt.AutoItX.Send(Account.Hash);
                                                Thread.Sleep(50);
                                                AutoIt.AutoItX.Send("{enter}");

                                                Thread.Sleep(1250);
                                            }
                                            else
                                            {
                                                AutoIt.AutoItX.Send("{enter 2}");
                                                Thread.Sleep(250);
                                            }
                                        }
                                        else
                                        {
                                            if(Dispatcher.GetClient().GetPhase() == Processor.EPhase.None)
                                            {
                                                AutoIt.AutoItX.MouseClick("LEFT", AutoIt.AutoItX.WinGetPos(Process.MainWindowHandle).X + 25, AutoIt.AutoItX.WinGetPos(Process.MainWindowHandle).Y + 50);
                                                Thread.Sleep(50);
                                            }
                                            
                                            AutoIt.AutoItX.Send("{enter 2}");
                                            Thread.Sleep(50);
                                        }

                                        SplashTry++;
                                    }
                                }
                                break;
                        }


                        if (Dispatcher.GetClient().GetName() != "")
                        {
                            Dispatcher.GetClient().Start();

                            Storage.ClientCollection.Add(Process.Id, Dispatcher.GetClient());
                        }

                        Thread.Sleep(1);
                    };
                }
            });
        }

        public void HandleProcess()
        {
            if (HandleProcessThread != null && HandleProcessThread.IsAlive)
                return;

            HandleProcessThread = new Thread(new ThreadStart(HandleProcessThreadEvent));
            HandleProcessThread.IsBackground = true;
            HandleProcessThread.Start();
        }

        private void HandleProcessThreadEvent()
        {
            ManagementObjectSearcher ManagementObject = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE Name='KnightOnLine.exe'");

            foreach (ManagementObject Management in ManagementObject.Get())
            {
                Int32 ProcessId = Int32.Parse(Management["ProcessId"].ToString());

                try
                {
                    Process Process = Process.GetProcessById(ProcessId);

                    String CommandLine = Management["CommandLine"].ToString();

                    string AccountName = "";

                    if (CommandLine != "" && Storage.ClientCollection.ContainsKey(ProcessId) == false)
                    {
                        var Command = CommandLine.Split(' ');

                        if (Command.Length == 4)
                            AccountName = Command[2];

                        if (Command.Length == 4 && Database().GetAccountByName(Command[2], "JPKO") == null)
                            Database().SetAccount(AccountName, Command[3], Command[0].Replace(@"""", @""), "JPKO");
                    }

                    if (Storage.ClientCollection.ContainsKey(ProcessId) == false)
                    {
                        Dispatcher Dispatcher = new Dispatcher(this);

                        Dispatcher.GetClient().SetAccountName(AccountName);

                        Dispatcher.GetClient().HandleProcess(Process, Management);

                        while (Storage.ClientCollection.ContainsKey(ProcessId) == false)
                        {
                            if (Dispatcher.GetClient().GetName() != "")
                            {
                                Dispatcher.GetClient().Start();

                                Storage.ClientCollection.Add(ProcessId, Dispatcher.GetClient());
                            }

                            Thread.Sleep(125);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                }
            }
        }

        public void CloseAllProcess()
        {
            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;

                ClientData.Destroy();

                if (ClientData.HasExited() == false)
                    ClientData.GetProcess().Kill();
            }
        }

        public Client GetProcess(int ProcessId)
        {
            if (Storage.ClientCollection.ContainsKey(ProcessId))
                return Storage.ClientCollection[ProcessId];

            return null;
        }
    }
}
