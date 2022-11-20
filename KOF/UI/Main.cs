using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.Linq;
using System.Threading;
using KOF.Common;
using KOF.Common.Win32;
using KOF.Core;
using System.Globalization;


namespace KOF.UI
{
    public partial class Main : Form
    {
        private App _App;
        private AddAccount _AddAccount;

        public Main()
        {
            _App = new App(this);

            _AddAccount = new AddAccount(_App);

            InitializeComponent();

            Platform.SelectedIndex = 0;
        }

        private void InitializeLanguage()
        {
            Control Control = GetNextControl(this, true);

            do
            {
                Control = GetNextControl(Control, true);

                if (Control != null)
                {
                    if (Control.GetType() == typeof(CheckBox))
                    {
                        CheckBox CheckBox = ((CheckBox)Control);
                        CheckBox.Text = _App.GetString(CheckBox.Name);
                    }
                    else if (Control.GetType() == typeof(Button))
                    {
                        Button Button = ((Button)Control);
                        Button.Text = _App.GetString(Button.Name);
                    }
                    else if(Control.GetType() == typeof(Label))
                    {
                        Label Label = ((Label)Control);
                        Label.Text = _App.GetString(Label.Name);
                    }
                    else if(Control.GetType() == typeof(GroupBox))
                    {
                        GroupBox GroupBox = ((GroupBox)Control);
                        GroupBox.Text = _App.GetString(GroupBox.Name);
                    }
                    else if (Control.GetType() == typeof(TabControl))
                    {
                        TabControl TabControl = ((TabControl)Control);
                        TabControl.Text = _App.GetString(TabControl.Name);
                    }
                }

            }
            while (Control != null);

            foreach (ToolStripMenuItem m in MenuStrip1.Items)
                m.Text = _App.GetString(m.Name);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Text = "KOF - v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            _App.Load();

            Control Control = GetNextControl(this, true);

            do
            {
                Control = GetNextControl(Control, true);

                if (Control != null)
                {
                    if (Control.GetType() == typeof(CheckBox))
                    {
                        CheckBox CheckBox = ((CheckBox)Control);
                        bool Value = Convert.ToBoolean(_App.GetControl(CheckBox.Name, CheckBox.Checked.ToString()));

                        if (Value != CheckBox.Checked)
                            CheckBox.Checked = Value;
                    }
                    else if (Control.GetType() == typeof(NumericUpDown))
                    {
                        NumericUpDown NumericUpDown = ((NumericUpDown)Control);
                        int Value = Convert.ToInt32(_App.GetControl(NumericUpDown.Name, NumericUpDown.Value.ToString()));

                        if (Value != NumericUpDown.Value)
                            NumericUpDown.Value = Value;
                    }
                    else if (Control.GetType() == typeof(ComboBox))
                    {
                        ComboBox ComboBox = ((ComboBox)Control);

                        if (ComboBox.SelectedItem != null)
                        {
                            object Value = _App.GetControl(ComboBox.Name, ComboBox.SelectedItem.ToString());

                            if (Value != ComboBox.SelectedItem)
                                ComboBox.SelectedItem = Value;
                        }
                    }
                }

            }
            while (Control != null);

            LoadAccountList(_App.GetControl("Platform"));

            TopMost = AlwaysOnTop.Checked;

            InitializeLanguage();
        }

        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void AccountStart_Click(object sender, EventArgs e)
        {
            var StartAccountList = new List<int>();

            foreach (ListViewItem Item in listAccount.CheckedItems)
                StartAccountList.Add(int.Parse(Item.Text));

            _App.Launcher(StartAccountList);
        }

        private void LoadAccountList(string Platform)
        {
            var AccountCollectionList = _App.Database().GetAccountListByPlatform(Platform);

            if(AccountCollectionList.Count == 0)
            {
                listAccount.Items.Clear();
                return;
            }

            if (listAccount.Items.Count == AccountCollectionList.Count)
                return;

            listAccount.Items.Clear();

            for (int i = 0; i < AccountCollectionList.Count; i++)
            {
                var SelectedAccount = AccountCollectionList[i];

                string[] Row = { "", SelectedAccount.ServerId.ToString(), SelectedAccount.CharacterName.ToString() };

                ListViewItem Item = new ListViewItem(Row);

                Item.Text = SelectedAccount.Id.ToString();

                listAccount.Columns[0].Width = 23;
                listAccount.Items.Add(Item);
            }
        }

        private void AccountDelete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem Item in listAccount.CheckedItems)
            {
                _App.Database().DeleteAccount(int.Parse(Item.Text));
            }

            LoadAccountList(_App.GetControl("Platform"));
        }

        private void AccountAdd_Click(object sender, EventArgs e)
        {
            _AddAccount.Show();
        }

        private void ClientHandle_Click(object sender, EventArgs e)
        {
            _App.HandleProcess();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (_App.Database() == null) return;

            var ClientCollectionList = Storage.ClientCollection;

            var HandledProcessList = new Dictionary<int, string>();

            foreach (Client ClientData in ClientCollectionList)
            {
                if (ClientData == null) continue;
                if (ClientData.IsStarted() && ClientData.IsDisconnected() == false)
                    HandledProcessList.Add(ClientData.GetProcessId(), ClientData.GetName());
            }

            if (HandledProcessList.Count > 0)
            {
                if (HandledProcessList.Count != ClientList.Items.Count)
                {
                    Follow.Enabled = true;
                    FollowComboBox.Enabled = true;

                    ClientList.DataSource = new BindingSource(HandledProcessList, null);
                    ClientList.ValueMember = "Key";
                    ClientList.DisplayMember = "Value";
                }
            }
            else
            {
                Follow.Enabled = false;
                FollowComboBox.Enabled = false;

                ClientList.DataSource = null;
            }

            var FollowableProcessList = new Dictionary<int, string>();

            foreach (Client ClientData in ClientCollectionList)
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                        FollowableProcessList.Add(ClientData.GetProcessId(), ClientData.GetName());
                }
            }

            if (FollowableProcessList.Count > 0)
            {
                if (FollowableProcessList.Count != FollowComboBox.Items.Count || HandledProcessList.Count != ClientList.Items.Count)
                {
                    string FollowedClient = "";

                    if (Storage.FollowedClient != null)
                    {
                        FollowedClient = Storage.FollowedClient.GetName();
                    }

                    FollowComboBox.DataSource = new BindingSource(FollowableProcessList, null);
                    FollowComboBox.ValueMember = "Key";
                    FollowComboBox.DisplayMember = "Value";

                    if (Storage.FollowedClient != null && FollowedClient != "" && FollowComboBox.FindString(FollowedClient) != -1)
                        FollowComboBox.SelectedIndex = FollowComboBox.FindString(FollowedClient);
                }
            }
            else
            {
                FollowComboBox.DataSource = null;
            }

            LoadAccountList(_App.GetControl("Platform"));
        }

        private void ClientClose_Click(object sender, EventArgs e)
        {
            if (ClientList.SelectedItem == null) return;

            var SelectedClient = (KeyValuePair<int, string>)ClientList.SelectedItem;

            Client ClientData = _App.GetProcess(SelectedClient.Key);

            if (ClientData != null)
            {
                ClientData.Destroy();

                if (ClientData.GetProcess().HasExited == false)
                {
                    ClientData.GetProcess().Kill();
                    ClientData.GetProcess().WaitForExit();
                }
            }
        }

        private void CloseAllClient_Click(object sender, EventArgs e)
        {
            _App.CloseAllProcess();
        }

        private void Follow_CheckedChanged(object sender, EventArgs e)
        {
            if (FollowComboBox.SelectedItem == null) return;

            var SelectedClient = (KeyValuePair<int, string>)FollowComboBox.SelectedItem;

            if (Follow.Checked)
                Storage.FollowedClient = _App.GetProcess(SelectedClient.Key);
            else
                Storage.FollowedClient = null;
        }

        private void AutoParty_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(AutoParty.Name, AutoParty.Checked.ToString());
        }

        private void AutoLogin_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(AutoLogin.Name, AutoLogin.Checked.ToString());
        }

        private void ClientList_DoubleClick(object sender, EventArgs e)
        {
            if (ClientList.SelectedItem == null) return;

            var SelectedClient = (KeyValuePair<int, string>)ClientList.SelectedItem;
            Client Client = _App.GetProcess(SelectedClient.Key);

            if (Client != null)
            {
                Client.GetDispatcherInterface().Show();

                foreach (Form f in Application.OpenForms)
                {
                    if (f.Text == Client.GetName())
                    {
                        if (f.WindowState == FormWindowState.Minimized)
                            f.WindowState = FormWindowState.Normal;

                        f.Focus();
                        break;
                    }
                }
            }
        }

        private void FollowComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FollowComboBox.SelectedItem == null || Follow.Checked == false) return;
            var SelectedClient = (KeyValuePair<int, string>)FollowComboBox.SelectedItem;
            Storage.FollowedClient = _App.GetProcess(SelectedClient.Key);      
        }

        private void AutoPartyAccept_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(AutoPartyAccept.Name, AutoPartyAccept.Checked.ToString());

            Storage.AutoPartyAccept = AutoPartyAccept.Checked;
        }

        private void HideAllClient_Click(object sender, EventArgs e)
        {
            foreach (Client ClientData in Storage.ClientCollection)
            {
                if (ClientData == null) continue;
                if (ClientData.HasExited()) continue;

                Win32Api.ShowWindow(ClientData.GetProcess().MainWindowHandle, Win32Api.Windows.HIDE);
            }
        }

        private void ShowAllClient_Click(object sender, EventArgs e)
        {
            foreach (Client ClientData in Storage.ClientCollection)
            {
                if (ClientData == null) continue;
                if (ClientData.HasExited()) continue;


                Win32Api.ShowWindow(ClientData.GetProcess().MainWindowHandle, Win32Api.Windows.SHOW);
            }
        }

        private void Platform_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_App.Database() == null) return;

            string SelectedPlatform = Platform.SelectedItem.ToString();

            _App.SetControl(Platform.Name, SelectedPlatform);

            LoadAccountList(SelectedPlatform);
        }

        private void DiscordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/C9RMpHtccy");
        }

        private void HakkindaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About About = new About();
            About.ShowDialog();
        }

        private void WebSitesiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://kofbot.com");
        }

        private void KonsolToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if !DEBUG
            _App.CreateConsole();
#endif
        }

        private void AlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(AlwaysOnTop.Name, AlwaysOnTop.Checked.ToString());
            TopMost = AlwaysOnTop.Checked;
        }
        

        private void Language_SelectedIndexChanged(object sender, EventArgs e)
        {
            _App.SetControl(Language.Name, Language.SelectedItem.ToString());
            InitializeLanguage();
        }

        private void listAccount_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                e.Cancel = true;
                e.NewWidth = listAccount.Columns[e.ColumnIndex].Width;
            }
        }

        private void ClientList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
