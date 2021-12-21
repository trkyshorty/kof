using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.Linq;
using System.Threading;
using KOF.Common;
using KOF.Common.Win32;
using KOF.Core;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text;

namespace KOF.UI
{
    public partial class Main : Form
    {
        private App _App;
        private AddAccount _AddAccount;

        private int _FollowableProcessListSize = 0;
        private int _HandledProcessListSize = 0;

        public Main()
        {
            _App = new App(this);

            _AddAccount = new AddAccount(_App);

            InitializeComponent();

            Platform.SelectedIndex = 0;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            TopMost = true;

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

                        if (CheckBox.Name == "AreaControl")
                            continue;

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

            _App.HandleProcess();
        }

        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _App.Database().SaveControl("App");
        }

        public void Notify(ToolTipIcon type, string Title, string Message, int TimeOut = 1000)
        {
            MainNotify.BalloonTipIcon = type;
            MainNotify.BalloonTipTitle = Title;
            MainNotify.BalloonTipText = Message;
            MainNotify.ShowBalloonTip(TimeOut);
            MainNotify.Visible = true;
        }

        private void AccountStart_Click(object sender, EventArgs e)
        {
            var StartAccountList = new List<int>();

            foreach (KOF.Models.Account Account in AccountList.CheckedItems)
                StartAccountList.Add(Account.Id);

            while (AccountList.CheckedIndices.Count > 0)
                AccountList.SetItemChecked(AccountList.CheckedIndices[0], false);

            _App.Launcher(StartAccountList);
        }

        private void LoadAccountList(string Platform)
        {
            var AccountCollectionList = _App.Database().GetAccountListByPlatform(Platform);

            if (AccountCollectionList.Count > 0)
            {
                AccountList.DataSource = new BindingSource(AccountCollectionList, null);
                AccountList.ValueMember = "Id";
                AccountList.DisplayMember = "Name";
            }
            else
                AccountList.DataSource = null;
        }

        private void AccountDelete_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < AccountList.Items.Count; i++)
            {
                if (AccountList.GetItemChecked(i))
                {
                    KOF.Models.Account SelectedAccount = (KOF.Models.Account)AccountList.Items[i];

                    _App.Database().DeleteAccount(SelectedAccount.Id);
                }
            }

            LoadAccountList(_App.GetControl("Platform"));
        }

        private void AccountAdd_Click(object sender, EventArgs e)
        {
            _AddAccount.ShowDialog();
        }

        private void ClientHandle_Click(object sender, EventArgs e)
        {
            _App.HandleProcess();

            _HandledProcessListSize = 0;
            _FollowableProcessListSize = 0;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (_App.Database() == null) return;

            var ClientCollectionList = Storage.ClientCollection.Values.ToList();

            var HandledProcessList = new Dictionary<int, string>();

            foreach (Client ClientData in ClientCollectionList)
            {
                if (ClientData == null) continue;
                if (ClientData.IsStarted() && ClientData.IsDisconnected() == false)
                    HandledProcessList.Add(ClientData.GetProcessId(), ClientData.GetNameConst());
            }

            if (HandledProcessList.Count > 0)
            {
                if (HandledProcessList.Count != _HandledProcessListSize)
                {
                    Follow.Enabled = true;
                    FollowComboBox.Enabled = true;

                    ClientList.DataSource = new BindingSource(HandledProcessList, null);
                    ClientList.ValueMember = "Key";
                    ClientList.DisplayMember = "Value";

                    _HandledProcessListSize = HandledProcessList.Count;
                }
            }
            else
            {
                Follow.Enabled = false;
                FollowComboBox.Enabled = false;

                ClientList.DataSource = null;
                _HandledProcessListSize = 0;
            }

            var FollowableProcessList = new Dictionary<int, string>();

            foreach (Client ClientData in ClientCollectionList)
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                        FollowableProcessList.Add(ClientData.GetProcessId(), ClientData.GetNameConst());
                }
            }

            if (FollowableProcessList.Count > 0)
            {
                if (FollowableProcessList.Count != _FollowableProcessListSize || HandledProcessList.Count != _HandledProcessListSize)
                {
                    string FollowedClient = "";

                    if (Storage.FollowedClient != null)
                    {
                        FollowedClient = Storage.FollowedClient.GetNameConst();

                        foreach (Client ClientData in ClientCollectionList)
                        {
                            if (ClientData == null) continue;
                            if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                                && ClientData.IsCharacterAvailable()
                                && (ClientData.GetPhase() == Processor.EPhase.Playing || ClientData.GetPhase() == Processor.EPhase.Warping))
                            {
                                if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                                {
                                    ClientData.SetControl(Attack.Name, Attack.Checked.ToString());
                                    ClientData.SetControl(ActionMove.Name, ActionMove.Checked.ToString());
                                    ClientData.SetControl(ActionSetCoordinate.Name, ActionSetCoordinate.Checked.ToString());
                                    ClientData.SetControl(TargetAutoSelect.Name, TargetAutoSelect.Checked.ToString());
                                    ClientData.SetControl(TargetOpponentNation.Name, TargetOpponentNation.Checked.ToString());
                                    ClientData.SetControl(AttackDistance.Name, AttackDistance.Value.ToString());
                                    ClientData.SetControl(AttackSpeed.Name, AttackSpeed.Value.ToString());
                                    ClientData.SetControl(AreaControlX.Name, AreaControlX.Value.ToString());
                                    ClientData.SetControl(AreaControlY.Name, AreaControlY.Value.ToString());
                                    ClientData.SetControl(AreaControl.Name, AreaControl.Checked.ToString());

                                    for (int i = 0; i < TargetList.Items.Count; i++)
                                    {
                                        if (TargetList.GetItemChecked(i))
                                            ClientData.AddTargetAllowed(TargetList.Items[i].ToString());
                                        else
                                            ClientData.RemoveTargetAllowed(TargetList.Items[i].ToString());
                                    }
                                }
                            }
                        }
                    }

                    FollowComboBox.DataSource = new BindingSource(FollowableProcessList, null);
                    FollowComboBox.ValueMember = "Key";
                    FollowComboBox.DisplayMember = "Value";

                    _FollowableProcessListSize = FollowableProcessList.Count;

                    if (Storage.FollowedClient != null && FollowedClient != "" && FollowComboBox.FindString(FollowedClient) != -1)
                        FollowComboBox.SelectedIndex = FollowComboBox.FindString(FollowedClient);
                }
            }
            else
            {
                FollowComboBox.DataSource = null;
                _FollowableProcessListSize = 0;
            }
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
                    ClientData.GetProcess().Kill();
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
            {
                Storage.FollowedClient = _App.GetProcess(SelectedClient.Key);

                Attack.Enabled = true;
                ActionMove.Enabled = true;
                ActionSetCoordinate.Enabled = true;
                TargetAutoSelect.Enabled = true;
                TargetOpponentNation.Enabled = true;
                AutoParty.Enabled = true;
                AutoPartyAccept.Enabled = true;

                TargetList.Enabled = true;
                SearchMob.Enabled = true;
                SearchPlayer.Enabled = true;
                ClearTargetList.Enabled = true;

                RepairAllEquipment.Enabled = true;
                StartSupplyEvent.Enabled = true;

                AttackSpeed.Enabled = true;
                AttackSpeedLabel.Enabled = true;
                AttackDistance.Enabled = true;
                AttackDistanceLabel.Enabled = true;

                AreaControl.Enabled = true;
                AreaControlX.Enabled = true;
                AreaControlY.Enabled = true;
                SetAreaControl.Enabled = true;
            }
            else
            {
                Storage.FollowedClient = null;

                Attack.Enabled = false;
                ActionMove.Enabled = false;
                ActionSetCoordinate.Enabled = false;
                TargetAutoSelect.Enabled = false;
                TargetOpponentNation.Enabled = false;
                AutoParty.Enabled = false;
                AutoPartyAccept.Enabled = false;

                TargetList.Enabled = false;
                SearchMob.Enabled = false;
                SearchPlayer.Enabled = false;
                ClearTargetList.Enabled = false;

                RepairAllEquipment.Enabled = false;
                StartSupplyEvent.Enabled = false;

                AttackSpeed.Enabled = false;
                AttackSpeedLabel.Enabled = false;
                AttackDistance.Enabled = false;
                AttackDistanceLabel.Enabled = false;

                AreaControl.Enabled = false;
                AreaControlX.Enabled = false;
                AreaControlY.Enabled = false;
                SetAreaControl.Enabled = false;
            }
        }

        private void AutoParty_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(AutoParty.Name, AutoParty.Checked.ToString());
        }

        private void AutoAccountSave_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(AutoAccountSave.Name, AutoAccountSave.Checked.ToString());
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
            if (Client != null) Client.GetDispatcherInterface().ShowDialog();
        }

        private void FollowComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FollowComboBox.SelectedItem == null || Follow.Checked == false) return;
            var SelectedClient = (KeyValuePair<int, string>)FollowComboBox.SelectedItem;
            Storage.FollowedClient = _App.GetProcess(SelectedClient.Key);
        }

        private void SearchMob_Click(object sender, EventArgs e)
        {
            if (Storage.FollowedClient == null) return;

            List<Target> SearchTargetList = new List<Target>();
            if (Storage.FollowedClient.SearchMob(ref SearchTargetList) > 0)
            {
                Storage.TargetCollection = Storage.TargetCollection
                    .Union(SearchTargetList
                            .GroupBy(x => x.Name)
                            .Select(x => x.First())
                            .ToList())
                    .GroupBy(x => x.Name)
                    .Select(m => m.First())
                    .ToList();

                Storage.TargetCollection.ForEach(x =>
                {

                    if (TargetList.Items.Contains(x.Name) == false)
                        TargetList.Items.Add(x.Name);
                });
            }
        }

        private void SearchPlayer_Click(object sender, EventArgs e)
        {
            if (Storage.FollowedClient == null) return;

            List<Target> SearchTargetList = new List<Target>();
            if (Storage.FollowedClient.SearchPlayer(ref SearchTargetList) > 0)
            {
                Storage.TargetCollection = Storage.TargetCollection
                    .Union(SearchTargetList
                            .GroupBy(x => x.Name)
                            .Select(x => x.First())
                            .ToList())
                    .GroupBy(x => x.Name)
                    .Select(m => m.First())
                    .ToList();

                Storage.TargetCollection.ForEach(x =>
                {

                    if (TargetList.Items.Contains(x.Name) == false)
                        TargetList.Items.Add(x.Name);
                });
            }
        }

        private void ClearTargetList_Click(object sender, EventArgs e)
        {
            if (Storage.FollowedClient == null) return;

            TargetList.Items.Clear();

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    ClientData.ClearTargetAllowed();
                }
            }
        }

        private void TargetList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (Storage.FollowedClient == null) return;

            string SelectedTargetList = TargetList.Items[e.Index].ToString();

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                    {
                        if (e.NewValue == CheckState.Checked)
                            ClientData.AddTargetAllowed(SelectedTargetList);
                        else
                            ClientData.RemoveTargetAllowed(SelectedTargetList);
                    }
                }
            }
        }

        private void RepairAllEquipment_Click(object sender, EventArgs e)
        {
            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                    {
                        Thread ForceRepairThread = new Thread(() =>
                        {
                            ClientData.RepairEquipmentAction(true);
                        });

                        ForceRepairThread.IsBackground = true;
                        ForceRepairThread.Start();
                    }
                }
            }
        }

        private void StartSupplyEvent_Click(object sender, EventArgs e)
        {
            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable()
                    && ClientData.GetAction() == 0
                    && (ClientData.GetPhase() == Processor.EPhase.Playing || ClientData.GetPhase() == Processor.EPhase.Warping))
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                    {
                        Thread ForceRepairThread = new Thread(() =>
                        {
                            List<Supply> Supply = new List<Supply>();
                            if (ClientData.IsNeedSupply(ref Supply, true))
                                ClientData.SupplyItemAction(Supply);
                        });

                        ForceRepairThread.IsBackground = true;
                        ForceRepairThread.Start();
                    }
                }
            }
        }

        private void ActionMove_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(ActionMove.Name, ActionMove.Checked.ToString());

            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                        ClientData.SetControl(ActionMove.Name, ActionMove.Checked.ToString());
                }
            }
        }

        private void ActionSetCoordinate_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(ActionSetCoordinate.Name, ActionSetCoordinate.Checked.ToString());

            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                        ClientData.SetControl(ActionSetCoordinate.Name, ActionSetCoordinate.Checked.ToString());
                }
            }
        }

        private void TargetAutoSelect_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(TargetAutoSelect.Name, TargetAutoSelect.Checked.ToString());

            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                        ClientData.SetControl(TargetAutoSelect.Name, TargetAutoSelect.Checked.ToString());
                }
            }
        }

        private void TargetOpponentNation_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(TargetOpponentNation.Name, TargetOpponentNation.Checked.ToString());

            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                        ClientData.SetControl(TargetOpponentNation.Name, TargetOpponentNation.Checked.ToString());
                }
            }
        }

        private void Attack_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(Attack.Name, Attack.Checked.ToString());

            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                        ClientData.SetControl(Attack.Name, Attack.Checked.ToString());
                }
            }
        }

        private void AutoPartyAccept_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(AutoPartyAccept.Name, AutoPartyAccept.Checked.ToString());

            Storage.AutoPartyAccept = AutoPartyAccept.Checked;
        }

        private void AttackSpeed_ValueChanged(object sender, EventArgs e)
        {
            _App.SetControl(AttackSpeed.Name, AttackSpeed.Value.ToString());

            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                        ClientData.SetControl(AttackSpeed.Name, AttackSpeed.Value.ToString());
                }
            }
        }

        private void AttackDistance_ValueChanged(object sender, EventArgs e)
        {
            _App.SetControl(AttackDistance.Name, AttackDistance.Value.ToString());

            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                        ClientData.SetControl(AttackDistance.Name, AttackDistance.Value.ToString());
                }
            }
        }

        private void HideAllClient_Click(object sender, EventArgs e)
        {
            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.HasExited()) continue;

                Win32Api.ShowWindow(ClientData.GetProcess().MainWindowHandle, Win32Api.Windows.HIDE);
            }
        }

        private void ShowAllClient_Click(object sender, EventArgs e)
        {
            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.HasExited()) continue;


                Win32Api.ShowWindow(ClientData.GetProcess().MainWindowHandle, Win32Api.Windows.SHOW);
            }
        }

        private void AreaControlX_ValueChanged(object sender, EventArgs e)
        {
            _App.SetControl(AreaControlX.Name, AreaControlX.Value.ToString());

            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                        ClientData.SetControl(AreaControlX.Name, AreaControlX.Value.ToString());
                }
            }
        }

        private void AreaControlY_ValueChanged(object sender, EventArgs e)
        {
            _App.SetControl(AreaControlY.Name, AreaControlY.Value.ToString());

            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                        ClientData.SetControl(AreaControlY.Name, AreaControlY.Value.ToString());
                }
            }
        }

        private void SetAreaControl_Click(object sender, EventArgs e)
        {
            _App.SetControl(AreaControl.Name, AreaControl.Checked.ToString());

            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                    {
                        AreaControlX.Value = Storage.FollowedClient.GetX();
                        AreaControlY.Value = Storage.FollowedClient.GetY();

                        ClientData.SetControl(AreaControlX.Name, AreaControlX.Value.ToString());
                        ClientData.SetControl(AreaControlY.Name, AreaControlY.Value.ToString());
                    }

                }
            }
        }

        private void AreaControl_CheckedChanged(object sender, EventArgs e)
        {
            _App.SetControl(AreaControl.Name, AreaControl.Checked.ToString());

            if (Storage.FollowedClient == null) return;

            foreach (Client ClientData in Storage.ClientCollection.Values.ToList())
            {
                if (ClientData == null) continue;
                if (ClientData.IsDisconnected() == false && ClientData.IsStarted()
                    && ClientData.IsCharacterAvailable())
                {
                    if (Convert.ToBoolean(ClientData.GetControl("FollowDisable")) == false)
                    {
                        ClientData.SetControl(AreaControlX.Name, AreaControlX.Value.ToString());
                        ClientData.SetControl(AreaControlY.Name, AreaControlY.Value.ToString());
                        ClientData.SetControl(AreaControl.Name, AreaControl.Checked.ToString());
                    }

                }
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

        public static void CreateConsole()
        {
            Win32Api.AllocConsole();

            IntPtr StdHandle = Win32Api.CreateFile(
                "CONOUT$",
                Win32Enum.GENERIC_WRITE,
                Win32Enum.FILE_SHARE_WRITE,
                0, Win32Enum.OPEN_EXISTING, 0, 0
            );

            SafeFileHandle SafeFileHandle = new SafeFileHandle(StdHandle, true);
            FileStream FileStream = new FileStream(SafeFileHandle, FileAccess.Write);
            Encoding Encoding = System.Text.Encoding.GetEncoding(Win32Enum.MY_CODE_PAGE);
            StreamWriter StandardOutput = new StreamWriter(FileStream, Encoding);
            StandardOutput.AutoFlush = true;
            Console.SetOut(StandardOutput);
        }

        private void KonsolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateConsole();
        }
    }
}
