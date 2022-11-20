
namespace KOF.UI
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.AccountAdd = new System.Windows.Forms.Button();
            this.Platform = new System.Windows.Forms.ComboBox();
            this.AccountList = new System.Windows.Forms.CheckedListBox();
            this.AccountDelete = new System.Windows.Forms.Button();
            this.AccountStart = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ClientClose = new System.Windows.Forms.Button();
            this.ClientHandle = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.AutoPartyAccept = new System.Windows.Forms.CheckBox();
            this.FollowComboBox = new System.Windows.Forms.ComboBox();
            this.AutoParty = new System.Windows.Forms.CheckBox();
            this.Follow = new System.Windows.Forms.CheckBox();
            this.ClientList = new System.Windows.Forms.ListBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.TargetWaitDown = new System.Windows.Forms.CheckBox();
            this.AttackOnSetAreaControl = new System.Windows.Forms.CheckBox();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.AreaControl = new System.Windows.Forms.CheckBox();
            this.SetAreaControl = new System.Windows.Forms.Button();
            this.AreaControlY = new System.Windows.Forms.NumericUpDown();
            this.AreaControlX = new System.Windows.Forms.NumericUpDown();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.StartSupplyEvent = new System.Windows.Forms.Button();
            this.RepairAllEquipment = new System.Windows.Forms.Button();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.AddSelected = new System.Windows.Forms.Button();
            this.TargetList = new System.Windows.Forms.CheckedListBox();
            this.ClearTargetList = new System.Windows.Forms.Button();
            this.SearchPlayer = new System.Windows.Forms.Button();
            this.SearchMob = new System.Windows.Forms.Button();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.TargetOpponentNation = new System.Windows.Forms.CheckBox();
            this.TargetAutoSelect = new System.Windows.Forms.CheckBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.ActionRoute = new System.Windows.Forms.CheckBox();
            this.ActionSetCoordinate = new System.Windows.Forms.CheckBox();
            this.ActionMove = new System.Windows.Forms.CheckBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.AttackDistance = new System.Windows.Forms.NumericUpDown();
            this.AttackDistanceLabel = new System.Windows.Forms.Label();
            this.AttackSpeed = new System.Windows.Forms.NumericUpDown();
            this.AttackSpeedLabel = new System.Windows.Forms.Label();
            this.Attack = new System.Windows.Forms.CheckBox();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.HideAllClient = new System.Windows.Forms.Button();
            this.ShowAllClient = new System.Windows.Forms.Button();
            this.ClientCloseAll = new System.Windows.Forms.Button();
            this.AutoLogin = new System.Windows.Forms.CheckBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.AutoAccountSave = new System.Windows.Forms.CheckBox();
            this.MainNotify = new System.Windows.Forms.NotifyIcon(this.components);
            this.MenuStrip1 = new System.Windows.Forms.MenuStrip();
            this.DiscordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GeliştiriciToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.KonsolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.YardımToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HakkindaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.WebSitesiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.groupBox12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AreaControlY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AreaControlX)).BeginInit();
            this.groupBox11.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AttackDistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AttackSpeed)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.MenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.AccountAdd);
            this.groupBox1.Controls.Add(this.Platform);
            this.groupBox1.Controls.Add(this.AccountList);
            this.groupBox1.Controls.Add(this.AccountDelete);
            this.groupBox1.Controls.Add(this.AccountStart);
            this.groupBox1.Location = new System.Drawing.Point(3, 26);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(291, 210);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Hesap";
            // 
            // AccountAdd
            // 
            this.AccountAdd.Location = new System.Drawing.Point(157, 171);
            this.AccountAdd.Name = "AccountAdd";
            this.AccountAdd.Size = new System.Drawing.Size(124, 23);
            this.AccountAdd.TabIndex = 9;
            this.AccountAdd.Text = "Hesap Ekle";
            this.AccountAdd.UseVisualStyleBackColor = true;
            this.AccountAdd.Click += new System.EventHandler(this.AccountAdd_Click);
            // 
            // Platform
            // 
            this.Platform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Platform.FormattingEnabled = true;
            this.Platform.Items.AddRange(new object[] {
            "JPKO",
            "CNKO"});
            this.Platform.Location = new System.Drawing.Point(12, 19);
            this.Platform.Name = "Platform";
            this.Platform.Size = new System.Drawing.Size(131, 21);
            this.Platform.TabIndex = 3;
            this.Platform.SelectedIndexChanged += new System.EventHandler(this.Platform_SelectedIndexChanged);
            // 
            // AccountList
            // 
            this.AccountList.FormattingEnabled = true;
            this.AccountList.Location = new System.Drawing.Point(12, 48);
            this.AccountList.Name = "AccountList";
            this.AccountList.Size = new System.Drawing.Size(131, 148);
            this.AccountList.TabIndex = 0;
            // 
            // AccountDelete
            // 
            this.AccountDelete.Location = new System.Drawing.Point(157, 45);
            this.AccountDelete.Name = "AccountDelete";
            this.AccountDelete.Size = new System.Drawing.Size(124, 23);
            this.AccountDelete.TabIndex = 2;
            this.AccountDelete.Text = "Sil";
            this.AccountDelete.UseVisualStyleBackColor = true;
            this.AccountDelete.Click += new System.EventHandler(this.AccountDelete_Click);
            // 
            // AccountStart
            // 
            this.AccountStart.Location = new System.Drawing.Point(157, 19);
            this.AccountStart.Name = "AccountStart";
            this.AccountStart.Size = new System.Drawing.Size(124, 23);
            this.AccountStart.TabIndex = 1;
            this.AccountStart.Text = "Başlat";
            this.AccountStart.UseVisualStyleBackColor = true;
            this.AccountStart.Click += new System.EventHandler(this.AccountStart_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ClientClose);
            this.groupBox2.Controls.Add(this.ClientHandle);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.ClientList);
            this.groupBox2.Location = new System.Drawing.Point(300, 26);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(314, 210);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Client";
            // 
            // ClientClose
            // 
            this.ClientClose.Location = new System.Drawing.Point(168, 45);
            this.ClientClose.Name = "ClientClose";
            this.ClientClose.Size = new System.Drawing.Size(129, 23);
            this.ClientClose.TabIndex = 3;
            this.ClientClose.Text = "Kapat";
            this.ClientClose.UseVisualStyleBackColor = true;
            this.ClientClose.Click += new System.EventHandler(this.ClientClose_Click);
            // 
            // ClientHandle
            // 
            this.ClientHandle.Location = new System.Drawing.Point(168, 19);
            this.ClientHandle.Name = "ClientHandle";
            this.ClientHandle.Size = new System.Drawing.Size(129, 23);
            this.ClientHandle.TabIndex = 1;
            this.ClientHandle.Text = "Yakala";
            this.ClientHandle.UseVisualStyleBackColor = true;
            this.ClientHandle.Click += new System.EventHandler(this.ClientHandle_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.AutoPartyAccept);
            this.groupBox3.Controls.Add(this.FollowComboBox);
            this.groupBox3.Controls.Add(this.AutoParty);
            this.groupBox3.Controls.Add(this.Follow);
            this.groupBox3.Location = new System.Drawing.Point(168, 77);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(129, 117);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Multi";
            // 
            // AutoPartyAccept
            // 
            this.AutoPartyAccept.AutoSize = true;
            this.AutoPartyAccept.Enabled = false;
            this.AutoPartyAccept.Location = new System.Drawing.Point(9, 91);
            this.AutoPartyAccept.Name = "AutoPartyAccept";
            this.AutoPartyAccept.Size = new System.Drawing.Size(117, 17);
            this.AutoPartyAccept.TabIndex = 4;
            this.AutoPartyAccept.Text = "Oto Party (Accept)";
            this.AutoPartyAccept.UseVisualStyleBackColor = true;
            this.AutoPartyAccept.CheckedChanged += new System.EventHandler(this.AutoPartyAccept_CheckedChanged);
            // 
            // FollowComboBox
            // 
            this.FollowComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FollowComboBox.Enabled = false;
            this.FollowComboBox.FormattingEnabled = true;
            this.FollowComboBox.Location = new System.Drawing.Point(9, 19);
            this.FollowComboBox.Name = "FollowComboBox";
            this.FollowComboBox.Size = new System.Drawing.Size(112, 21);
            this.FollowComboBox.TabIndex = 3;
            this.FollowComboBox.SelectedIndexChanged += new System.EventHandler(this.FollowComboBox_SelectedIndexChanged);
            // 
            // AutoParty
            // 
            this.AutoParty.AutoSize = true;
            this.AutoParty.Enabled = false;
            this.AutoParty.Location = new System.Drawing.Point(9, 71);
            this.AutoParty.Name = "AutoParty";
            this.AutoParty.Size = new System.Drawing.Size(113, 17);
            this.AutoParty.TabIndex = 1;
            this.AutoParty.Text = "Oto Party (Davet)";
            this.AutoParty.UseVisualStyleBackColor = true;
            this.AutoParty.CheckedChanged += new System.EventHandler(this.AutoParty_CheckedChanged);
            // 
            // Follow
            // 
            this.Follow.AutoSize = true;
            this.Follow.Enabled = false;
            this.Follow.Location = new System.Drawing.Point(9, 51);
            this.Follow.Name = "Follow";
            this.Follow.Size = new System.Drawing.Size(72, 17);
            this.Follow.TabIndex = 0;
            this.Follow.Text = "Etkinleştir";
            this.Follow.UseVisualStyleBackColor = true;
            this.Follow.CheckedChanged += new System.EventHandler(this.Follow_CheckedChanged);
            // 
            // ClientList
            // 
            this.ClientList.FormattingEnabled = true;
            this.ClientList.Location = new System.Drawing.Point(10, 21);
            this.ClientList.Name = "ClientList";
            this.ClientList.Size = new System.Drawing.Size(142, 173);
            this.ClientList.TabIndex = 0;
            this.ClientList.DoubleClick += new System.EventHandler(this.ClientList_DoubleClick);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.groupBox13);
            this.groupBox4.Controls.Add(this.groupBox12);
            this.groupBox4.Controls.Add(this.groupBox11);
            this.groupBox4.Controls.Add(this.groupBox10);
            this.groupBox4.Controls.Add(this.groupBox9);
            this.groupBox4.Controls.Add(this.groupBox8);
            this.groupBox4.Controls.Add(this.groupBox7);
            this.groupBox4.Location = new System.Drawing.Point(3, 297);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(611, 233);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Multi Kontrolleri";
            // 
            // groupBox13
            // 
            this.groupBox13.Controls.Add(this.TargetWaitDown);
            this.groupBox13.Controls.Add(this.AttackOnSetAreaControl);
            this.groupBox13.Location = new System.Drawing.Point(149, 127);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(142, 100);
            this.groupBox13.TabIndex = 11;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "Ayarlar";
            // 
            // TargetWaitDown
            // 
            this.TargetWaitDown.AutoSize = true;
            this.TargetWaitDown.Enabled = false;
            this.TargetWaitDown.Location = new System.Drawing.Point(8, 52);
            this.TargetWaitDown.Name = "TargetWaitDown";
            this.TargetWaitDown.Size = new System.Drawing.Size(101, 17);
            this.TargetWaitDown.TabIndex = 2;
            this.TargetWaitDown.Text = "Düşmesini bekle";
            this.TargetWaitDown.UseVisualStyleBackColor = true;
            this.TargetWaitDown.CheckedChanged += new System.EventHandler(this.TargetWaitDown_CheckedChanged);
            // 
            // AttackOnSetAreaControl
            // 
            this.AttackOnSetAreaControl.AutoSize = true;
            this.AttackOnSetAreaControl.Enabled = false;
            this.AttackOnSetAreaControl.Location = new System.Drawing.Point(8, 20);
            this.AttackOnSetAreaControl.Name = "AttackOnSetAreaControl";
            this.AttackOnSetAreaControl.Size = new System.Drawing.Size(119, 30);
            this.AttackOnSetAreaControl.TabIndex = 0;
            this.AttackOnSetAreaControl.Text = "Saldırı başladığında \r\nmerkez al";
            this.AttackOnSetAreaControl.UseVisualStyleBackColor = true;
            this.AttackOnSetAreaControl.CheckedChanged += new System.EventHandler(this.AttackOnSetAreaControl_CheckedChanged);
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.AreaControl);
            this.groupBox12.Controls.Add(this.SetAreaControl);
            this.groupBox12.Controls.Add(this.AreaControlY);
            this.groupBox12.Controls.Add(this.AreaControlX);
            this.groupBox12.Location = new System.Drawing.Point(6, 127);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(137, 100);
            this.groupBox12.TabIndex = 4;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Merkez Yönetimi";
            // 
            // AreaControl
            // 
            this.AreaControl.AutoSize = true;
            this.AreaControl.Enabled = false;
            this.AreaControl.Location = new System.Drawing.Point(6, 20);
            this.AreaControl.Name = "AreaControl";
            this.AreaControl.Size = new System.Drawing.Size(88, 17);
            this.AreaControl.TabIndex = 3;
            this.AreaControl.Text = "Merkeze Dön";
            this.AreaControl.UseVisualStyleBackColor = true;
            this.AreaControl.CheckedChanged += new System.EventHandler(this.AreaControl_CheckedChanged);
            // 
            // SetAreaControl
            // 
            this.SetAreaControl.Enabled = false;
            this.SetAreaControl.Location = new System.Drawing.Point(30, 71);
            this.SetAreaControl.Name = "SetAreaControl";
            this.SetAreaControl.Size = new System.Drawing.Size(75, 23);
            this.SetAreaControl.TabIndex = 2;
            this.SetAreaControl.Text = "Merkez Seç";
            this.SetAreaControl.UseVisualStyleBackColor = true;
            this.SetAreaControl.Click += new System.EventHandler(this.SetAreaControl_Click);
            // 
            // AreaControlY
            // 
            this.AreaControlY.Enabled = false;
            this.AreaControlY.Location = new System.Drawing.Point(73, 43);
            this.AreaControlY.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.AreaControlY.Name = "AreaControlY";
            this.AreaControlY.Size = new System.Drawing.Size(60, 21);
            this.AreaControlY.TabIndex = 1;
            this.AreaControlY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.AreaControlY.ValueChanged += new System.EventHandler(this.AreaControlY_ValueChanged);
            // 
            // AreaControlX
            // 
            this.AreaControlX.Enabled = false;
            this.AreaControlX.Location = new System.Drawing.Point(4, 43);
            this.AreaControlX.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.AreaControlX.Name = "AreaControlX";
            this.AreaControlX.Size = new System.Drawing.Size(60, 21);
            this.AreaControlX.TabIndex = 0;
            this.AreaControlX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.AreaControlX.ValueChanged += new System.EventHandler(this.AreaControlX_ValueChanged);
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.StartSupplyEvent);
            this.groupBox11.Controls.Add(this.RepairAllEquipment);
            this.groupBox11.Location = new System.Drawing.Point(297, 170);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(308, 57);
            this.groupBox11.TabIndex = 10;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Toplu İşlemler";
            // 
            // StartSupplyEvent
            // 
            this.StartSupplyEvent.Enabled = false;
            this.StartSupplyEvent.Location = new System.Drawing.Point(168, 22);
            this.StartSupplyEvent.Name = "StartSupplyEvent";
            this.StartSupplyEvent.Size = new System.Drawing.Size(136, 23);
            this.StartSupplyEvent.TabIndex = 6;
            this.StartSupplyEvent.Text = "Tedariği Hallet";
            this.StartSupplyEvent.UseVisualStyleBackColor = true;
            this.StartSupplyEvent.Click += new System.EventHandler(this.StartSupplyEvent_Click);
            // 
            // RepairAllEquipment
            // 
            this.RepairAllEquipment.Enabled = false;
            this.RepairAllEquipment.Location = new System.Drawing.Point(10, 22);
            this.RepairAllEquipment.Name = "RepairAllEquipment";
            this.RepairAllEquipment.Size = new System.Drawing.Size(136, 23);
            this.RepairAllEquipment.TabIndex = 5;
            this.RepairAllEquipment.Text = "Ekipmanları Tamir Et";
            this.RepairAllEquipment.UseVisualStyleBackColor = true;
            this.RepairAllEquipment.Click += new System.EventHandler(this.RepairAllEquipment_Click);
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.AddSelected);
            this.groupBox10.Controls.Add(this.TargetList);
            this.groupBox10.Controls.Add(this.ClearTargetList);
            this.groupBox10.Controls.Add(this.SearchPlayer);
            this.groupBox10.Controls.Add(this.SearchMob);
            this.groupBox10.Location = new System.Drawing.Point(297, 20);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(306, 144);
            this.groupBox10.TabIndex = 9;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Hedef Listesi";
            // 
            // AddSelected
            // 
            this.AddSelected.Enabled = false;
            this.AddSelected.Location = new System.Drawing.Point(168, 78);
            this.AddSelected.Name = "AddSelected";
            this.AddSelected.Size = new System.Drawing.Size(129, 23);
            this.AddSelected.TabIndex = 6;
            this.AddSelected.Text = "Seçileni Ekle";
            this.AddSelected.UseVisualStyleBackColor = true;
            this.AddSelected.Click += new System.EventHandler(this.AddSelected_Click);
            // 
            // TargetList
            // 
            this.TargetList.FormattingEnabled = true;
            this.TargetList.Location = new System.Drawing.Point(10, 20);
            this.TargetList.Name = "TargetList";
            this.TargetList.Size = new System.Drawing.Size(142, 116);
            this.TargetList.TabIndex = 5;
            this.TargetList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.TargetList_ItemCheck);
            // 
            // ClearTargetList
            // 
            this.ClearTargetList.Enabled = false;
            this.ClearTargetList.Location = new System.Drawing.Point(168, 107);
            this.ClearTargetList.Name = "ClearTargetList";
            this.ClearTargetList.Size = new System.Drawing.Size(129, 23);
            this.ClearTargetList.TabIndex = 4;
            this.ClearTargetList.Text = "Temizle";
            this.ClearTargetList.UseVisualStyleBackColor = true;
            this.ClearTargetList.Click += new System.EventHandler(this.ClearTargetList_Click);
            // 
            // SearchPlayer
            // 
            this.SearchPlayer.Enabled = false;
            this.SearchPlayer.Location = new System.Drawing.Point(168, 49);
            this.SearchPlayer.Name = "SearchPlayer";
            this.SearchPlayer.Size = new System.Drawing.Size(129, 23);
            this.SearchPlayer.TabIndex = 3;
            this.SearchPlayer.Text = "Oyuncu Tara";
            this.SearchPlayer.UseVisualStyleBackColor = true;
            this.SearchPlayer.Click += new System.EventHandler(this.SearchPlayer_Click);
            // 
            // SearchMob
            // 
            this.SearchMob.Enabled = false;
            this.SearchMob.Location = new System.Drawing.Point(168, 20);
            this.SearchMob.Name = "SearchMob";
            this.SearchMob.Size = new System.Drawing.Size(129, 23);
            this.SearchMob.TabIndex = 2;
            this.SearchMob.Text = "Yaratık Tara";
            this.SearchMob.UseVisualStyleBackColor = true;
            this.SearchMob.Click += new System.EventHandler(this.SearchMob_Click);
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.TargetOpponentNation);
            this.groupBox9.Controls.Add(this.TargetAutoSelect);
            this.groupBox9.Location = new System.Drawing.Point(213, 20);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(78, 101);
            this.groupBox9.TabIndex = 8;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Hedef";
            // 
            // TargetOpponentNation
            // 
            this.TargetOpponentNation.AutoSize = true;
            this.TargetOpponentNation.Enabled = false;
            this.TargetOpponentNation.Location = new System.Drawing.Point(6, 43);
            this.TargetOpponentNation.Name = "TargetOpponentNation";
            this.TargetOpponentNation.Size = new System.Drawing.Size(59, 17);
            this.TargetOpponentNation.TabIndex = 1;
            this.TargetOpponentNation.Text = "Irk Seç";
            this.TargetOpponentNation.UseVisualStyleBackColor = true;
            this.TargetOpponentNation.CheckedChanged += new System.EventHandler(this.TargetOpponentNation_CheckedChanged);
            // 
            // TargetAutoSelect
            // 
            this.TargetAutoSelect.AutoSize = true;
            this.TargetAutoSelect.Enabled = false;
            this.TargetAutoSelect.Location = new System.Drawing.Point(6, 20);
            this.TargetAutoSelect.Name = "TargetAutoSelect";
            this.TargetAutoSelect.Size = new System.Drawing.Size(69, 17);
            this.TargetAutoSelect.TabIndex = 0;
            this.TargetAutoSelect.Text = "Otomatik";
            this.TargetAutoSelect.UseVisualStyleBackColor = true;
            this.TargetAutoSelect.CheckedChanged += new System.EventHandler(this.TargetAutoSelect_CheckedChanged);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.ActionRoute);
            this.groupBox8.Controls.Add(this.ActionSetCoordinate);
            this.groupBox8.Controls.Add(this.ActionMove);
            this.groupBox8.Location = new System.Drawing.Point(148, 20);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(60, 101);
            this.groupBox8.TabIndex = 7;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Hareket";
            // 
            // ActionRoute
            // 
            this.ActionRoute.AutoSize = true;
            this.ActionRoute.Enabled = false;
            this.ActionRoute.Location = new System.Drawing.Point(9, 66);
            this.ActionRoute.Name = "ActionRoute";
            this.ActionRoute.Size = new System.Drawing.Size(49, 17);
            this.ActionRoute.TabIndex = 8;
            this.ActionRoute.Text = "Rota";
            this.ActionRoute.UseVisualStyleBackColor = true;
            this.ActionRoute.CheckedChanged += new System.EventHandler(this.ActionRoute_CheckedChanged);
            // 
            // ActionSetCoordinate
            // 
            this.ActionSetCoordinate.AutoSize = true;
            this.ActionSetCoordinate.Enabled = false;
            this.ActionSetCoordinate.Location = new System.Drawing.Point(9, 43);
            this.ActionSetCoordinate.Name = "ActionSetCoordinate";
            this.ActionSetCoordinate.Size = new System.Drawing.Size(48, 17);
            this.ActionSetCoordinate.TabIndex = 7;
            this.ActionSetCoordinate.Text = "Zıpla";
            this.ActionSetCoordinate.UseVisualStyleBackColor = true;
            this.ActionSetCoordinate.CheckedChanged += new System.EventHandler(this.ActionSetCoordinate_CheckedChanged);
            // 
            // ActionMove
            // 
            this.ActionMove.AutoSize = true;
            this.ActionMove.Enabled = false;
            this.ActionMove.Location = new System.Drawing.Point(9, 20);
            this.ActionMove.Name = "ActionMove";
            this.ActionMove.Size = new System.Drawing.Size(48, 17);
            this.ActionMove.TabIndex = 6;
            this.ActionMove.Text = "Yürü";
            this.ActionMove.UseVisualStyleBackColor = true;
            this.ActionMove.CheckedChanged += new System.EventHandler(this.ActionMove_CheckedChanged);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.AttackDistance);
            this.groupBox7.Controls.Add(this.AttackDistanceLabel);
            this.groupBox7.Controls.Add(this.AttackSpeed);
            this.groupBox7.Controls.Add(this.AttackSpeedLabel);
            this.groupBox7.Controls.Add(this.Attack);
            this.groupBox7.Location = new System.Drawing.Point(6, 20);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(137, 101);
            this.groupBox7.TabIndex = 6;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Saldırı";
            // 
            // AttackDistance
            // 
            this.AttackDistance.Enabled = false;
            this.AttackDistance.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.AttackDistance.Location = new System.Drawing.Point(80, 64);
            this.AttackDistance.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.AttackDistance.Name = "AttackDistance";
            this.AttackDistance.Size = new System.Drawing.Size(51, 21);
            this.AttackDistance.TabIndex = 9;
            this.AttackDistance.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.AttackDistance.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.AttackDistance.ValueChanged += new System.EventHandler(this.AttackDistance_ValueChanged);
            // 
            // AttackDistanceLabel
            // 
            this.AttackDistanceLabel.AutoSize = true;
            this.AttackDistanceLabel.Enabled = false;
            this.AttackDistanceLabel.Location = new System.Drawing.Point(9, 67);
            this.AttackDistanceLabel.Name = "AttackDistanceLabel";
            this.AttackDistanceLabel.Size = new System.Drawing.Size(68, 13);
            this.AttackDistanceLabel.TabIndex = 8;
            this.AttackDistanceLabel.Text = "Mesafe (M) :";
            // 
            // AttackSpeed
            // 
            this.AttackSpeed.Enabled = false;
            this.AttackSpeed.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.AttackSpeed.Location = new System.Drawing.Point(80, 41);
            this.AttackSpeed.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.AttackSpeed.Name = "AttackSpeed";
            this.AttackSpeed.Size = new System.Drawing.Size(51, 21);
            this.AttackSpeed.TabIndex = 7;
            this.AttackSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.AttackSpeed.Value = new decimal(new int[] {
            1250,
            0,
            0,
            0});
            this.AttackSpeed.ValueChanged += new System.EventHandler(this.AttackSpeed_ValueChanged);
            // 
            // AttackSpeedLabel
            // 
            this.AttackSpeedLabel.AutoSize = true;
            this.AttackSpeedLabel.Enabled = false;
            this.AttackSpeedLabel.Location = new System.Drawing.Point(9, 45);
            this.AttackSpeedLabel.Name = "AttackSpeedLabel";
            this.AttackSpeedLabel.Size = new System.Drawing.Size(52, 13);
            this.AttackSpeedLabel.TabIndex = 6;
            this.AttackSpeedLabel.Text = "Hız (Ms) :";
            // 
            // Attack
            // 
            this.Attack.AutoSize = true;
            this.Attack.Enabled = false;
            this.Attack.Location = new System.Drawing.Point(10, 20);
            this.Attack.Name = "Attack";
            this.Attack.Size = new System.Drawing.Size(55, 17);
            this.Attack.TabIndex = 5;
            this.Attack.Text = "Başlat";
            this.Attack.UseVisualStyleBackColor = true;
            this.Attack.CheckedChanged += new System.EventHandler(this.Attack_CheckedChanged);
            // 
            // Timer1
            // 
            this.Timer1.Enabled = true;
            this.Timer1.Interval = 3000;
            this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.HideAllClient);
            this.groupBox5.Controls.Add(this.ShowAllClient);
            this.groupBox5.Controls.Add(this.ClientCloseAll);
            this.groupBox5.Location = new System.Drawing.Point(300, 237);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(314, 54);
            this.groupBox5.TabIndex = 3;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Client Yönetimi";
            // 
            // HideAllClient
            // 
            this.HideAllClient.Location = new System.Drawing.Point(104, 20);
            this.HideAllClient.Name = "HideAllClient";
            this.HideAllClient.Size = new System.Drawing.Size(94, 23);
            this.HideAllClient.TabIndex = 8;
            this.HideAllClient.Text = "Tümünü Gizle";
            this.HideAllClient.UseVisualStyleBackColor = true;
            this.HideAllClient.Click += new System.EventHandler(this.HideAllClient_Click);
            // 
            // ShowAllClient
            // 
            this.ShowAllClient.Location = new System.Drawing.Point(6, 20);
            this.ShowAllClient.Name = "ShowAllClient";
            this.ShowAllClient.Size = new System.Drawing.Size(94, 23);
            this.ShowAllClient.TabIndex = 7;
            this.ShowAllClient.Text = "Tümünü Göster";
            this.ShowAllClient.UseVisualStyleBackColor = true;
            this.ShowAllClient.Click += new System.EventHandler(this.ShowAllClient_Click);
            // 
            // ClientCloseAll
            // 
            this.ClientCloseAll.Location = new System.Drawing.Point(203, 20);
            this.ClientCloseAll.Name = "ClientCloseAll";
            this.ClientCloseAll.Size = new System.Drawing.Size(94, 23);
            this.ClientCloseAll.TabIndex = 0;
            this.ClientCloseAll.Text = "Tümünü Kapat";
            this.ClientCloseAll.UseVisualStyleBackColor = true;
            this.ClientCloseAll.Click += new System.EventHandler(this.CloseAllClient_Click);
            // 
            // AutoLogin
            // 
            this.AutoLogin.AutoSize = true;
            this.AutoLogin.Checked = true;
            this.AutoLogin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoLogin.Location = new System.Drawing.Point(16, 23);
            this.AutoLogin.Name = "AutoLogin";
            this.AutoLogin.Size = new System.Drawing.Size(72, 17);
            this.AutoLogin.TabIndex = 3;
            this.AutoLogin.Text = "Oto Login";
            this.AutoLogin.UseVisualStyleBackColor = true;
            this.AutoLogin.CheckedChanged += new System.EventHandler(this.AutoLogin_CheckedChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.AutoAccountSave);
            this.groupBox6.Controls.Add(this.AutoLogin);
            this.groupBox6.Location = new System.Drawing.Point(3, 237);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(291, 54);
            this.groupBox6.TabIndex = 4;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Hesap Yönetimi";
            // 
            // AutoAccountSave
            // 
            this.AutoAccountSave.AutoSize = true;
            this.AutoAccountSave.Enabled = false;
            this.AutoAccountSave.Location = new System.Drawing.Point(109, 23);
            this.AutoAccountSave.Name = "AutoAccountSave";
            this.AutoAccountSave.Size = new System.Drawing.Size(107, 17);
            this.AutoAccountSave.TabIndex = 0;
            this.AutoAccountSave.Text = "Oto Kayıt (JPKO)";
            this.AutoAccountSave.UseVisualStyleBackColor = true;
            this.AutoAccountSave.CheckedChanged += new System.EventHandler(this.AutoAccountSave_CheckedChanged);
            // 
            // MainNotify
            // 
            this.MainNotify.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Warning;
            this.MainNotify.BalloonTipText = "Uyarı";
            this.MainNotify.BalloonTipTitle = "1";
            this.MainNotify.Icon = ((System.Drawing.Icon)(resources.GetObject("MainNotify.Icon")));
            this.MainNotify.Text = "KOF";
            this.MainNotify.Visible = true;
            // 
            // MenuStrip1
            // 
            this.MenuStrip1.BackColor = System.Drawing.Color.Transparent;
            this.MenuStrip1.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.MenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DiscordToolStripMenuItem,
            this.GeliştiriciToolStripMenuItem,
            this.YardımToolStripMenuItem});
            this.MenuStrip1.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip1.Name = "MenuStrip1";
            this.MenuStrip1.Size = new System.Drawing.Size(614, 24);
            this.MenuStrip1.TabIndex = 7;
            this.MenuStrip1.Text = "MenuStrip1";
            // 
            // DiscordToolStripMenuItem
            // 
            this.DiscordToolStripMenuItem.Name = "DiscordToolStripMenuItem";
            this.DiscordToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.DiscordToolStripMenuItem.Text = "Discord";
            this.DiscordToolStripMenuItem.Click += new System.EventHandler(this.DiscordToolStripMenuItem_Click);
            // 
            // GeliştiriciToolStripMenuItem
            // 
            this.GeliştiriciToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.KonsolToolStripMenuItem});
            this.GeliştiriciToolStripMenuItem.Name = "GeliştiriciToolStripMenuItem";
            this.GeliştiriciToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.GeliştiriciToolStripMenuItem.Text = "Araçlar";
            // 
            // KonsolToolStripMenuItem
            // 
            this.KonsolToolStripMenuItem.Name = "KonsolToolStripMenuItem";
            this.KonsolToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.KonsolToolStripMenuItem.Text = "Konsol";
            this.KonsolToolStripMenuItem.Click += new System.EventHandler(this.KonsolToolStripMenuItem_Click);
            // 
            // YardımToolStripMenuItem
            // 
            this.YardımToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HakkindaToolStripMenuItem,
            this.WebSitesiToolStripMenuItem});
            this.YardımToolStripMenuItem.Name = "YardımToolStripMenuItem";
            this.YardımToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.YardımToolStripMenuItem.Text = "Yardım";
            // 
            // HakkindaToolStripMenuItem
            // 
            this.HakkindaToolStripMenuItem.Name = "HakkindaToolStripMenuItem";
            this.HakkindaToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.HakkindaToolStripMenuItem.Text = "Hakkında";
            this.HakkindaToolStripMenuItem.Click += new System.EventHandler(this.HakkindaToolStripMenuItem_Click);
            // 
            // WebSitesiToolStripMenuItem
            // 
            this.WebSitesiToolStripMenuItem.Name = "WebSitesiToolStripMenuItem";
            this.WebSitesiToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.WebSitesiToolStripMenuItem.Text = "Web sitesi";
            this.WebSitesiToolStripMenuItem.Click += new System.EventHandler(this.WebSitesiToolStripMenuItem_Click);
            // 
            // AlwaysOnTop
            // 
            this.AlwaysOnTop.AutoSize = true;
            this.AlwaysOnTop.Checked = true;
            this.AlwaysOnTop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AlwaysOnTop.Location = new System.Drawing.Point(533, 8);
            this.AlwaysOnTop.Name = "AlwaysOnTop";
            this.AlwaysOnTop.Size = new System.Drawing.Size(71, 17);
            this.AlwaysOnTop.TabIndex = 4;
            this.AlwaysOnTop.Text = "Üstte Tut";
            this.AlwaysOnTop.UseVisualStyleBackColor = true;
            this.AlwaysOnTop.CheckedChanged += new System.EventHandler(this.AlwaysOnTop_CheckedChanged);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 530);
            this.Controls.Add(this.AlwaysOnTop);
            this.Controls.Add(this.MenuStrip1);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "KOF";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Main_Closing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox13.ResumeLayout(false);
            this.groupBox13.PerformLayout();
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AreaControlY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AreaControlX)).EndInit();
            this.groupBox11.ResumeLayout(false);
            this.groupBox10.ResumeLayout(false);
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AttackDistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AttackSpeed)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.MenuStrip1.ResumeLayout(false);
            this.MenuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button AccountStart;
        private System.Windows.Forms.CheckedListBox AccountList;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button ClientClose;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox FollowComboBox;
        private System.Windows.Forms.CheckBox AutoParty;
        private System.Windows.Forms.CheckBox Follow;
        private System.Windows.Forms.Button ClientHandle;
        private System.Windows.Forms.ListBox ClientList;
        private System.Windows.Forms.Button AccountDelete;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Timer Timer1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button ClientCloseAll;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.CheckBox AutoAccountSave;
        private System.Windows.Forms.CheckBox AutoLogin;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.CheckBox TargetAutoSelect;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.CheckBox ActionSetCoordinate;
        private System.Windows.Forms.CheckBox ActionMove;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.CheckBox Attack;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.Button ClearTargetList;
        private System.Windows.Forms.Button SearchPlayer;
        private System.Windows.Forms.Button SearchMob;
        private System.Windows.Forms.CheckBox TargetOpponentNation;
        private System.Windows.Forms.CheckedListBox TargetList;
        private System.Windows.Forms.GroupBox groupBox11;
        private System.Windows.Forms.Button StartSupplyEvent;
        private System.Windows.Forms.Button HideAllClient;
        private System.Windows.Forms.Button ShowAllClient;
        private System.Windows.Forms.Button AccountAdd;
        private System.Windows.Forms.Button RepairAllEquipment;
        private System.Windows.Forms.CheckBox AutoPartyAccept;
        private System.Windows.Forms.Label AttackSpeedLabel;
        private System.Windows.Forms.NumericUpDown AttackSpeed;
        private System.Windows.Forms.Label AttackDistanceLabel;
        private System.Windows.Forms.NumericUpDown AttackDistance;
        private System.Windows.Forms.NotifyIcon MainNotify;
        private System.Windows.Forms.GroupBox groupBox12;
        private System.Windows.Forms.CheckBox AreaControl;
        private System.Windows.Forms.Button SetAreaControl;
        private System.Windows.Forms.NumericUpDown AreaControlY;
        private System.Windows.Forms.NumericUpDown AreaControlX;
        private System.Windows.Forms.ComboBox Platform;
        internal System.Windows.Forms.MenuStrip MenuStrip1;
        internal System.Windows.Forms.ToolStripMenuItem GeliştiriciToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem KonsolToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem YardımToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem HakkindaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DiscordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem WebSitesiToolStripMenuItem;
        private System.Windows.Forms.Button AddSelected;
        private System.Windows.Forms.CheckBox AlwaysOnTop;
        private System.Windows.Forms.GroupBox groupBox13;
        private System.Windows.Forms.CheckBox TargetWaitDown;
        private System.Windows.Forms.CheckBox AttackOnSetAreaControl;
        private System.Windows.Forms.CheckBox ActionRoute;
    }
}

