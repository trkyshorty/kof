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

namespace KOF.Core;

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

    private static List<Control> ControlCollection { get; set; } = new List<Control>();

    public App(Main MainInterface)
    {
        _MainInterface = MainInterface;

        InitializeLogger();

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
        //Debug.Listeners.Add(new TextWriterTraceListener(LogWriter));
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

        //Debug.Listeners.Add(new TextWriterTraceListener(_DebugWriter));
#endif
    }

    public void Load()
    {
        _Database = new Database();

        _ResourceManager = new ResourceManager("KOF.Localization.Strings", Assembly.GetExecutingAssembly());

        Storage.ItemCollection = _Database.GetItemList();

        ControlCollection = _Database.GetControlList("App");

        InitializeMainThread();
    }

    public Database Database()
    {
        return _Database;
    }

    public string GetControl(string name, string defaultValue = "")
    {
        Control control = ControlCollection.Where(x => x.Name == name)?.SingleOrDefault();

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

            control.Form = "App";
            control.Name = name;
            control.Value = value;
            control.Platform = "Base";

            control.Id = Database().SetControl(control);

            ControlCollection.Add(control);
        }
        else
        {
            control.Value = value;
            Database().SetControl(control);
        }
    }

    private void InitializeMainThread()
    {
        Thread MainThread = new Thread(() =>
        {
            while (true)
            {
                if(Storage.ClientCollection.Count == 0)
                {
                    Thread.Sleep(1250);
                    return;
                }

                foreach (Client ClientData in Storage.ClientCollection)
                {
                    if (ClientData == null) continue;

                    if (ClientData.IsDisconnected())
                    {
                        if (Convert.ToBoolean(GetControl("AutoLogin")))
                        {
                            if (ClientData.HasExited() == false)
                            {
                                ClientData.GetProcess().Kill();
                                ClientData.GetProcess().WaitForExit();
                            }

                            ClientData.Destroy();

                            Debug.WriteLine("App > PID " + ClientData.GetProcessId() + " Lost");

                            if (ClientData._AccountData != null)
                            {
                                List<int> LoginList = new List<int>();

                                Account Account = Database().GetAccountById(ClientData._AccountData.Id);

                                if (Account != null)
                                {
                                    LoginList.Add(Account.Id);

                                    Launcher(LoginList);
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(100);
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
                Thread MainThread = new Thread(() =>
                {

                    try
                    {
                        FileInfo fileInfo = new FileInfo(Account.Path);

                        ProcessStartInfo startInfo = new ProcessStartInfo(fileInfo.Name);

                        startInfo.WorkingDirectory = fileInfo.DirectoryName;
                        startInfo.Arguments = Process.GetCurrentProcess().Id.ToString();
                        startInfo.UseShellExecute = true;

                        if (Account.Platform == "JPKO")
                            startInfo.Arguments = "MGAMEJP " + Account.AccountId + " " + Account.Password;
                        else if (Account.Platform == "CNKO")
                            startInfo.Arguments = Process.GetCurrentProcess().Id.ToString() + " CNKO " + Account.CharacterName;

                        Debug.WriteLine(Account.CharacterName + " starting.");

                        Process? process = Process.Start(startInfo);

                        while (!process.WaitForInputIdle())
                            Thread.Sleep(100);

                        Dispatcher Dispatcher = new Dispatcher(this);

                        Dispatcher.GetClient().HandleProcess(process, null, Account);

                        Thread.Sleep(3000);

                        if (Convert.ToBoolean(GetControl("AutoLogin")))
                        {
                            Dispatcher.GetClient().IntroSkip();

                            if (Account.Platform == "CNKO")
                            {
                                while (Dispatcher.GetClient().Read4Byte(Dispatcher.GetClient().GetAddress("KO_PTR_LOGIN_BTN")) == 0)
                                {
                                    if (process.HasExited)
                                        return;

                                    Thread.Sleep(250);
                                }
                            }

                            Thread.Sleep(1000);

                            Dispatcher.GetClient().SetPhase(Processor.EPhase.Loggining);

                            if (!process.HasExited)
                                Dispatcher.GetClient().Login(Account);
                        }

                        while (Dispatcher.GetClient().GetName() == "")
                        {
                            if (process.HasExited)
                                return;

                            Thread.Sleep(250);
                        }

                        if (!process.HasExited)
                        {
                            Dispatcher.GetClient().Start();

                            Storage.ClientCollection.Add(Dispatcher.GetClient());
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                });

                MainThread.IsBackground = true;
                MainThread.Start();
            }

            Thread.Sleep(500);
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

                if (Storage.ClientCollection.Where(x => x.GetProcessId() == ProcessId)?.SingleOrDefault() == null)
                {
                    Dispatcher Dispatcher = new Dispatcher(this);

                    String CommandLine = Management["CommandLine"].ToString();

                    if (CommandLine != "")
                    {
                        /*var Command = CommandLine.Split(' ');
                        if (Command.Length == 4 && Database().GetAccountByName(Command[2], "JPKO") == null)
                            Database().SetAccount(Command[2], Command[3], Command[0].Replace(@"""", @""), "JPKO");*/
                    }

                    Dispatcher.GetClient().HandleProcess(Process, Management, null);

                    Thread.Sleep(3000);

                    while (Dispatcher.GetClient().GetName() == "")
                    {
                        if (Process.HasExited)
                            return;

                        Thread.Sleep(100);
                    }

                    Dispatcher.GetClient().Start();

                    Storage.ClientCollection.Add(Dispatcher.GetClient());
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
        foreach (Client ClientData in Storage.ClientCollection.ToList())
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
        return Storage.ClientCollection.Where(x => x.GetProcessId() == ProcessId)?.SingleOrDefault();
    }

    public String GetString(string Key)
    {
        try
        {
            if(GetControl("Language") == "")
                return _ResourceManager.GetString(Key, new CultureInfo("en-US"));

            return _ResourceManager.GetString(Key, new CultureInfo(GetControl("Language")));
        }
        catch (Exception ex)
        {
            //Debug.WriteLine(ex.StackTrace);
            return Key;
        }  
    }
}
