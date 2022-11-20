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
using Microsoft.Win32.SafeHandles;
using System.Resources;
using System.Globalization;
using System.Reflection;

namespace KOF.Core
{
    public class App : Helper
    {
        private Database _Database { get; set; }
        private Main _MainInterface { get; set; }
        private Thread LauncherThread { get; set; }
        private Thread HandleProcessThread { get; set; }
        private LogWriter _LogWriter { get; set; } = null;
#if DEBUG
        private LogWriter _DebugWriter { get; set; } = null;
#endif
        private ResourceManager _ResourceManager { get; set; } = null;

        public App(Main MainInterface)
        {
            _MainInterface = MainInterface;

            InitializeLogger();

            //for CNKO login page {tab} problem
            AutoIt.AutoItX.AutoItSetOption("SendKeyDelay", 20);
            AutoIt.AutoItX.AutoItSetOption("SendKeyDownDelay", 20);

#if DEBUG
            CreateConsole();
#endif
        }

        public void CreateConsole()
        {
            AllocConsole();

            IntPtr StdHandle = CreateFile(
                "CONOUT$",
                GENERIC_WRITE,
                FILE_SHARE_WRITE,
                0, OPEN_EXISTING, 0, 0
            );

            SafeFileHandle SafeFileHandle = new SafeFileHandle(StdHandle, true);
            FileStream FileStream = new FileStream(SafeFileHandle, FileAccess.Write);

            LogWriter LogWriter = new LogWriter(FileStream);

            Console.SetOut(LogWriter);
            Console.SetError(LogWriter);

#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(LogWriter));
#endif
        }

        public void InitializeLogger()
        {
            string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string ApplicationFolder = MyDocuments + "\\" + "KOF";

            Directory.CreateDirectory(ApplicationFolder + "\\log");

            string LogFile = ApplicationFolder + "\\log\\log.txt";
            string DebugFile = ApplicationFolder + "\\log\\debug.txt";

            if (GetFileSize(LogFile) > (1024 * 1024 * 100)) // 100MB
                File.Delete(LogFile);

            if (GetFileSize(DebugFile) > (1024 * 1024 * 100)) // 100MB
                File.Delete(DebugFile);

            FileStream LogStream = new FileStream(LogFile, FileMode.Append);

            if(_LogWriter == null)
                _LogWriter = new LogWriter(LogStream);

            Console.SetOut(_LogWriter);
            Console.SetError(_LogWriter);
#if DEBUG
            FileStream DebugStream = new FileStream(DebugFile, FileMode.Append);

            if (_DebugWriter == null)
                _DebugWriter = new LogWriter(DebugStream);

            Debug.Listeners.Add(new TextWriterTraceListener(_DebugWriter));
#endif
        }

        public void Load()
        {
            _Database = new Database();

            _ResourceManager = new ResourceManager("KOF.Localization.Strings", Assembly.GetExecutingAssembly());

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

                        if (ClientData.IsDisconnected())
                        {
                            
                            if (ClientData.HasExited() == false)
                            {
                                ClientData.GetProcess().Kill();
                                ClientData.GetProcess().WaitForExit();
                            }

                            ClientData.Destroy();

                            Debug.WriteLine("App > PID " + ClientData.GetProcessId() + " Lost");

                            if (Convert.ToBoolean(GetControl("AutoLogin")))
                            {
                                List<int> LoginList = new List<int>();

                                Account Account = Database().GetAccountByName(ClientData.GetAccountName(), ClientData.GetPlatform().ToString());

                                if (Account != null)
                                {
                                    LoginList.Add(Account.Id);

                                    while (LauncherThread != null && LauncherThread.IsAlive)
                                        Thread.Sleep(1250);

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
                                && ClientData.GetZoneId() == Storage.FollowedClient.GetZoneId()
                                && ClientData.IsInFallback() == false && ClientData.IsInEnterGame() == false
                                && ClientData.GetAction() == 0
                                && Storage.FollowedClient.GetAction() == 0)
                            {
                                if (ClientData.IsCharacterAvailable() && Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                                {
                                    if (Storage.FollowedClient.GetTargetId() != ClientData.GetTargetId())
                                        ClientData.SelectTarget(Storage.FollowedClient.GetTargetId());

                                    if ((ClientData.GetX() != Storage.FollowedClient.GetX() || ClientData.GetY() != Storage.FollowedClient.GetY()))
                                    {
                                        if (Convert.ToBoolean(ClientData.GetControl("ActionSetCoordinate")) == true)
                                            ClientData.SetCoordinate(Storage.FollowedClient.GetX(), Storage.FollowedClient.GetY());
                                        else if (Convert.ToBoolean(ClientData.GetControl("ActionRoute")) == true)
                                            ClientData.StartRouteEvent(Storage.FollowedClient.GetX(), Storage.FollowedClient.GetY());
                                        else
                                            ClientData.MoveCoordinate(Storage.FollowedClient.GetX(), Storage.FollowedClient.GetY());
                                    }

                                    if (Convert.ToBoolean(GetControl("AutoParty")) == true)
                                    {
                                        if (Storage.FollowedClient.GetPartyCount() < 8 && Storage.FollowedClient.IsPartyMember(ClientData.GetNameConst()) == false)
                                            Storage.FollowedClient.SendParty(ClientData.GetNameConst());
                                    }
                                }
                            }
                        }
                    }

                    Thread.Sleep(125);
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
                    try
                    {
                        Process Process = new Process();

                        FileInfo FileInfo = new FileInfo(Account.Path);

                        Process.StartInfo = new ProcessStartInfo(FileInfo.Name);
                        Process.StartInfo.WorkingDirectory = FileInfo.Directory.FullName;

                        if (Account.Platform == "JPKO")
                            Process.StartInfo.Arguments = "MGAMEJP " + Account.Name + " " + Account.Hash;
                        else if (Account.Platform == "CNKO")
                            Process.StartInfo.Arguments = Process.GetCurrentProcess().Id.ToString() + " CNKO " + Account.Name;

                        Debug.WriteLine(Account.Name + " starting.");

                        Process.Start();

                        if (Account.Platform == "JPKO")
                            Thread.Sleep(15000);
                        else if (Account.Platform == "CNKO")
                            Thread.Sleep(20000);

                        Dispatcher Dispatcher = new Dispatcher(this);

                        Dispatcher.GetClient().HandleProcess(Process, null);

                        int Tick = Environment.TickCount;

                        while (Storage.ClientCollection.ContainsKey(Process.Id) == false)
                        {
                            if (Dispatcher.GetClient().IsDisconnected() || Environment.TickCount - Tick > 60000)
                            {
                                if (Dispatcher.GetClient().HasExited() == false)
                                {
                                    Dispatcher.GetClient().GetProcess().Kill();
                                    Dispatcher.GetClient().GetProcess().WaitForExit();
                                }

                                Dispatcher.GetClient().Destroy();


                                if(Convert.ToBoolean(GetControl("AutoLogin")))
                                {
                                    _MainInterface.Notify(System.Windows.Forms.ToolTipIcon.Error, "Giriş yapılamadı", Account.Name + " yeniden başlatılıyor.");

                                    var StartAccountList = new List<int>();
                                    StartAccountList.Add(AccountId);
                                    LauncherThreadEvent(StartAccountList);
                                }
                                
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
                                            if ((Dispatcher.GetClient().GetPhase() == Processor.EPhase.None) && Account.Platform == "CNKO")
                                            {
                                                if (AutoIt.AutoItX.WinActive(Process.MainWindowHandle) == 0)
                                                    AutoIt.AutoItX.WinActivate(Process.MainWindowHandle);

                                                if (AutoIt.AutoItX.WinActive(Process.MainWindowHandle) != 0)
                                                {
                                                    AutoIt.AutoItX.Send(Account.Name);
                                                    AutoIt.AutoItX.Send("{tab}");
                                                    AutoIt.AutoItX.Send(Account.Hash);
                                                    AutoIt.AutoItX.Send("{enter}");
                                                }
                                            }
                                            else
                                            {
                                                if (AutoIt.AutoItX.WinActive(Process.MainWindowHandle) == 0)
                                                    AutoIt.AutoItX.WinActivate(Process.MainWindowHandle);

                                                if (AutoIt.AutoItX.WinActive(Process.MainWindowHandle) != 0)
                                                    AutoIt.AutoItX.Send("{enter}");
                                            }
                                        }
                                    }
                                    break;
                            }

                            if (Dispatcher.GetClient().GetName() != "")
                            {
                                Dispatcher.GetClient().Start();

                                Storage.ClientCollection.Add(Process.Id, Dispatcher.GetClient());

                                HandleProcess();
                            }

                            Thread.Sleep(1250);
                        };
                    }
                    catch (InvalidOperationException ex)
                    {
                        _MainInterface.Notify(System.Windows.Forms.ToolTipIcon.Error, Account.Name, ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _MainInterface.Notify(System.Windows.Forms.ToolTipIcon.Error, Account.Name, ex.Message);
                    }
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

                    if (Storage.ClientCollection.ContainsKey(ProcessId) == false)
                    {
                        Dispatcher Dispatcher = new Dispatcher(this);

                        String CommandLine = Management["CommandLine"].ToString();

                        if (CommandLine != "")
                        {
                            var Command = CommandLine.Split(' ');
                            if (Command.Length == 4 && Database().GetAccountByName(Command[2], "JPKO") == null)
                                Database().SetAccount(Command[2], Command[3], Command[0].Replace(@"""", @""), "JPKO");
                        }

                        Dispatcher.GetClient().HandleProcess(Process, Management);

                        while (Storage.ClientCollection.ContainsKey(ProcessId) == false)
                        {
                            if (Dispatcher.GetClient().GetName() != "")
                            {
                                Dispatcher.GetClient().Start();

                                Storage.ClientCollection.Add(ProcessId, Dispatcher.GetClient());
                            }
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

                if (ClientData.HasExited() == false)
                {
                    ClientData.GetProcess().Kill();
                    ClientData.GetProcess().WaitForExit();
                }

                ClientData.Destroy();
            }
        }

        public Client GetProcess(int ProcessId)
        {
            if (Storage.ClientCollection.ContainsKey(ProcessId))
                return Storage.ClientCollection[ProcessId];

            return null;
        }

        public String GetString(string Key)
        {
            return _ResourceManager.GetString(Key, new CultureInfo("tr-TR"));
        }
    }
}
