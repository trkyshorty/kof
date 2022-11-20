
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
            this.AccountGroupBox = new System.Windows.Forms.GroupBox();
            this.listAccount = new System.Windows.Forms.ListView();
            this.ColumnSelect = new System.Windows.Forms.ColumnHeader();
            this.ColumnServer = new System.Windows.Forms.ColumnHeader();
            this.ColumnCharname = new System.Windows.Forms.ColumnHeader();
            this.AccountAdd = new System.Windows.Forms.Button();
            this.Platform = new System.Windows.Forms.ComboBox();
            this.AccountDelete = new System.Windows.Forms.Button();
            this.AccountStart = new System.Windows.Forms.Button();
            this.ClientGroupbox = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.ClientClose = new System.Windows.Forms.Button();
            this.ClientHandle = new System.Windows.Forms.Button();
            this.MultiGroupbox = new System.Windows.Forms.GroupBox();
            this.AutoPartyAccept = new System.Windows.Forms.CheckBox();
            this.FollowComboBox = new System.Windows.Forms.ComboBox();
            this.AutoParty = new System.Windows.Forms.CheckBox();
            this.Follow = new System.Windows.Forms.CheckBox();
            this.ClientList = new System.Windows.Forms.ListBox();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            this.ClientManageGroupbox = new System.Windows.Forms.GroupBox();
            this.HideAllClient = new System.Windows.Forms.Button();
            this.ShowAllClient = new System.Windows.Forms.Button();
            this.ClientCloseAll = new System.Windows.Forms.Button();
            this.AutoLogin = new System.Windows.Forms.CheckBox();
            this.AccountManageGroupbox = new System.Windows.Forms.GroupBox();
            this.Clientless = new System.Windows.Forms.CheckBox();
            this.MenuStrip1 = new System.Windows.Forms.MenuStrip();
            this.DiscordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GeliştiriciToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.KonsolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.YardımToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HakkindaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.WebSitesiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.LanguageLabel = new System.Windows.Forms.Label();
            this.Language = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.consoleRichTextBox = new System.Windows.Forms.RichTextBox();
            this.AccountGroupBox.SuspendLayout();
            this.ClientGroupbox.SuspendLayout();
            this.MultiGroupbox.SuspendLayout();
            this.ClientManageGroupbox.SuspendLayout();
            this.AccountManageGroupbox.SuspendLayout();
            this.MenuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // AccountGroupBox
            // 
            this.AccountGroupBox.Controls.Add(this.listAccount);
            this.AccountGroupBox.Controls.Add(this.AccountAdd);
            this.AccountGroupBox.Controls.Add(this.Platform);
            this.AccountGroupBox.Controls.Add(this.AccountDelete);
            this.AccountGroupBox.Controls.Add(this.AccountStart);
            this.AccountGroupBox.Location = new System.Drawing.Point(3, 26);
            this.AccountGroupBox.Name = "AccountGroupBox";
            this.AccountGroupBox.Size = new System.Drawing.Size(291, 261);
            this.AccountGroupBox.TabIndex = 100;
            this.AccountGroupBox.TabStop = false;
            this.AccountGroupBox.Text = "Hesap";
            this.AccountGroupBox.UseCompatibleTextRendering = true;
            // 
            // listAccount
            // 
            this.listAccount.AutoArrange = false;
            this.listAccount.BackColor = System.Drawing.Color.White;
            this.listAccount.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listAccount.CheckBoxes = true;
            this.listAccount.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnSelect,
            this.ColumnServer,
            this.ColumnCharname});
            this.listAccount.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.listAccount.GridLines = true;
            this.listAccount.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listAccount.LabelWrap = false;
            this.listAccount.Location = new System.Drawing.Point(12, 45);
            this.listAccount.Name = "listAccount";
            this.listAccount.ShowGroups = false;
            this.listAccount.Size = new System.Drawing.Size(201, 201);
            this.listAccount.TabIndex = 10;
            this.listAccount.UseCompatibleStateImageBehavior = false;
            this.listAccount.View = System.Windows.Forms.View.Details;
            this.listAccount.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.listAccount_ColumnWidthChanging);
            // 
            // ColumnSelect
            // 
            this.ColumnSelect.Text = "#";
            this.ColumnSelect.Width = 23;
            // 
            // ColumnServer
            // 
            this.ColumnServer.Text = "Server";
            this.ColumnServer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ColumnServer.Width = 47;
            // 
            // ColumnCharname
            // 
            this.ColumnCharname.Text = "Character";
            this.ColumnCharname.Width = 131;
            // 
            // AccountAdd
            // 
            this.AccountAdd.Location = new System.Drawing.Point(83, 18);
            this.AccountAdd.Name = "AccountAdd";
            this.AccountAdd.Size = new System.Drawing.Size(62, 23);
            this.AccountAdd.TabIndex = 9;
            this.AccountAdd.Text = "Ekle";
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
            this.Platform.Size = new System.Drawing.Size(65, 21);
            this.Platform.TabIndex = 3;
            this.Platform.SelectedIndexChanged += new System.EventHandler(this.Platform_SelectedIndexChanged);
            // 
            // AccountDelete
            // 
            this.AccountDelete.Location = new System.Drawing.Point(219, 74);
            this.AccountDelete.Name = "AccountDelete";
            this.AccountDelete.Size = new System.Drawing.Size(62, 23);
            this.AccountDelete.TabIndex = 2;
            this.AccountDelete.Text = "Sil";
            this.AccountDelete.UseVisualStyleBackColor = true;
            this.AccountDelete.Click += new System.EventHandler(this.AccountDelete_Click);
            // 
            // AccountStart
            // 
            this.AccountStart.Location = new System.Drawing.Point(219, 45);
            this.AccountStart.Name = "AccountStart";
            this.AccountStart.Size = new System.Drawing.Size(62, 23);
            this.AccountStart.TabIndex = 1;
            this.AccountStart.Text = "Başlat";
            this.AccountStart.UseVisualStyleBackColor = true;
            this.AccountStart.Click += new System.EventHandler(this.AccountStart_Click);
            // 
            // ClientGroupbox
            // 
            this.ClientGroupbox.Controls.Add(this.button1);
            this.ClientGroupbox.Controls.Add(this.ClientClose);
            this.ClientGroupbox.Controls.Add(this.ClientHandle);
            this.ClientGroupbox.Controls.Add(this.MultiGroupbox);
            this.ClientGroupbox.Controls.Add(this.ClientList);
            this.ClientGroupbox.Location = new System.Drawing.Point(300, 26);
            this.ClientGroupbox.Name = "ClientGroupbox";
            this.ClientGroupbox.Size = new System.Drawing.Size(314, 261);
            this.ClientGroupbox.TabIndex = 2;
            this.ClientGroupbox.TabStop = false;
            this.ClientGroupbox.Text = "Client";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(189, 210);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
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
            // MultiGroupbox
            // 
            this.MultiGroupbox.Controls.Add(this.AutoPartyAccept);
            this.MultiGroupbox.Controls.Add(this.FollowComboBox);
            this.MultiGroupbox.Controls.Add(this.AutoParty);
            this.MultiGroupbox.Controls.Add(this.Follow);
            this.MultiGroupbox.Location = new System.Drawing.Point(168, 77);
            this.MultiGroupbox.Name = "MultiGroupbox";
            this.MultiGroupbox.Size = new System.Drawing.Size(129, 117);
            this.MultiGroupbox.TabIndex = 2;
            this.MultiGroupbox.TabStop = false;
            this.MultiGroupbox.Text = "Multi";
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
            this.ClientList.Size = new System.Drawing.Size(142, 225);
            this.ClientList.TabIndex = 0;
            this.ClientList.SelectedIndexChanged += new System.EventHandler(this.ClientList_SelectedIndexChanged);
            this.ClientList.DoubleClick += new System.EventHandler(this.ClientList_DoubleClick);
            // 
            // Timer1
            // 
            this.Timer1.Enabled = true;
            this.Timer1.Interval = 500;
            this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // ClientManageGroupbox
            // 
            this.ClientManageGroupbox.Controls.Add(this.HideAllClient);
            this.ClientManageGroupbox.Controls.Add(this.ShowAllClient);
            this.ClientManageGroupbox.Controls.Add(this.ClientCloseAll);
            this.ClientManageGroupbox.Location = new System.Drawing.Point(300, 293);
            this.ClientManageGroupbox.Name = "ClientManageGroupbox";
            this.ClientManageGroupbox.Size = new System.Drawing.Size(314, 54);
            this.ClientManageGroupbox.TabIndex = 3;
            this.ClientManageGroupbox.TabStop = false;
            this.ClientManageGroupbox.Text = "Client Yönetimi";
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
            this.AutoLogin.Location = new System.Drawing.Point(106, 24);
            this.AutoLogin.Name = "AutoLogin";
            this.AutoLogin.Size = new System.Drawing.Size(72, 17);
            this.AutoLogin.TabIndex = 3;
            this.AutoLogin.Text = "Oto Login";
            this.AutoLogin.UseVisualStyleBackColor = true;
            this.AutoLogin.CheckedChanged += new System.EventHandler(this.AutoLogin_CheckedChanged);
            // 
            // AccountManageGroupbox
            // 
            this.AccountManageGroupbox.Controls.Add(this.Clientless);
            this.AccountManageGroupbox.Controls.Add(this.AutoLogin);
            this.AccountManageGroupbox.Location = new System.Drawing.Point(3, 293);
            this.AccountManageGroupbox.Name = "AccountManageGroupbox";
            this.AccountManageGroupbox.Size = new System.Drawing.Size(291, 54);
            this.AccountManageGroupbox.TabIndex = 4;
            this.AccountManageGroupbox.TabStop = false;
            this.AccountManageGroupbox.Text = "Hesap Yönetimi";
            // 
            // Clientless
            // 
            this.Clientless.AutoSize = true;
            this.Clientless.Location = new System.Drawing.Point(12, 24);
            this.Clientless.Name = "Clientless";
            this.Clientless.Size = new System.Drawing.Size(71, 17);
            this.Clientless.TabIndex = 4;
            this.Clientless.Text = "Clientless";
            this.Clientless.UseVisualStyleBackColor = true;
            this.Clientless.CheckedChanged += new System.EventHandler(this.Clientless_CheckedChanged);
            // 
            // MenuStrip1
            // 
            this.MenuStrip1.BackColor = System.Drawing.Color.Transparent;
            this.MenuStrip1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.MenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DiscordToolStripMenuItem,
            this.GeliştiriciToolStripMenuItem,
            this.YardımToolStripMenuItem});
            this.MenuStrip1.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip1.Name = "MenuStrip1";
            this.MenuStrip1.Size = new System.Drawing.Size(618, 24);
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
            this.AlwaysOnTop.Location = new System.Drawing.Point(531, 7);
            this.AlwaysOnTop.Name = "AlwaysOnTop";
            this.AlwaysOnTop.Size = new System.Drawing.Size(71, 17);
            this.AlwaysOnTop.TabIndex = 4;
            this.AlwaysOnTop.Text = "Üstte Tut";
            this.AlwaysOnTop.UseVisualStyleBackColor = true;
            this.AlwaysOnTop.CheckedChanged += new System.EventHandler(this.AlwaysOnTop_CheckedChanged);
            // 
            // LanguageLabel
            // 
            this.LanguageLabel.AutoSize = true;
            this.LanguageLabel.Location = new System.Drawing.Point(427, 8);
            this.LanguageLabel.Name = "LanguageLabel";
            this.LanguageLabel.Size = new System.Drawing.Size(28, 13);
            this.LanguageLabel.TabIndex = 8;
            this.LanguageLabel.Text = "Dil : ";
            // 
            // Language
            // 
            this.Language.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Language.FormattingEnabled = true;
            this.Language.Items.AddRange(new object[] {
            "en-US",
            "tr-TR"});
            this.Language.Location = new System.Drawing.Point(465, 5);
            this.Language.Name = "Language";
            this.Language.Size = new System.Drawing.Size(57, 21);
            this.Language.TabIndex = 9;
            this.Language.SelectedIndexChanged += new System.EventHandler(this.Language_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.consoleRichTextBox);
            this.groupBox1.Location = new System.Drawing.Point(3, 353);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(611, 260);
            this.groupBox1.TabIndex = 101;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Console";
            // 
            // consoleRichTextBox
            // 
            this.consoleRichTextBox.Location = new System.Drawing.Point(3, 17);
            this.consoleRichTextBox.Name = "consoleRichTextBox";
            this.consoleRichTextBox.Size = new System.Drawing.Size(602, 237);
            this.consoleRichTextBox.TabIndex = 0;
            this.consoleRichTextBox.Text = "";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 615);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Language);
            this.Controls.Add(this.LanguageLabel);
            this.Controls.Add(this.AlwaysOnTop);
            this.Controls.Add(this.MenuStrip1);
            this.Controls.Add(this.AccountManageGroupbox);
            this.Controls.Add(this.ClientManageGroupbox);
            this.Controls.Add(this.ClientGroupbox);
            this.Controls.Add(this.AccountGroupBox);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "KOF";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Main_Closing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.AccountGroupBox.ResumeLayout(false);
            this.ClientGroupbox.ResumeLayout(false);
            this.MultiGroupbox.ResumeLayout(false);
            this.MultiGroupbox.PerformLayout();
            this.ClientManageGroupbox.ResumeLayout(false);
            this.AccountManageGroupbox.ResumeLayout(false);
            this.AccountManageGroupbox.PerformLayout();
            this.MenuStrip1.ResumeLayout(false);
            this.MenuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button AccountStart;
        private System.Windows.Forms.GroupBox ClientGroupbox;
        private System.Windows.Forms.Button ClientClose;
        private System.Windows.Forms.GroupBox MultiGroupbox;
        private System.Windows.Forms.ComboBox FollowComboBox;
        private System.Windows.Forms.CheckBox AutoParty;
        private System.Windows.Forms.CheckBox Follow;
        private System.Windows.Forms.Button ClientHandle;
        private System.Windows.Forms.ListBox ClientList;
        private System.Windows.Forms.Button AccountDelete;
        private System.Windows.Forms.Timer Timer1;
        private System.Windows.Forms.GroupBox ClientManageGroupbox;
        private System.Windows.Forms.Button ClientCloseAll;
        private System.Windows.Forms.GroupBox AccountManageGroupbox;
        private System.Windows.Forms.CheckBox AutoLogin;
        private System.Windows.Forms.Button HideAllClient;
        private System.Windows.Forms.Button ShowAllClient;
        private System.Windows.Forms.Button AccountAdd;
        private System.Windows.Forms.CheckBox AutoPartyAccept;
        private System.Windows.Forms.ComboBox Platform;
        internal System.Windows.Forms.MenuStrip MenuStrip1;
        internal System.Windows.Forms.ToolStripMenuItem GeliştiriciToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem KonsolToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem YardımToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem HakkindaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DiscordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem WebSitesiToolStripMenuItem;
        private System.Windows.Forms.CheckBox AlwaysOnTop;
        private System.Windows.Forms.GroupBox groupBox13;
        private System.Windows.Forms.CheckBox TargetWaitDown;
        private System.Windows.Forms.CheckBox AttackOnSetAreaControl;
        private System.Windows.Forms.CheckBox ActionRoute;
    }
}

