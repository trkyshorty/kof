
namespace KOF.UI
{
    partial class PacketLogger
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PacketLogger));
            this.TabControl1 = new System.Windows.Forms.TabControl();
            this.TabPage1 = new System.Windows.Forms.TabPage();
            this.SendUnSelectAllPacket = new System.Windows.Forms.Button();
            this.SendSelectAllPacket = new System.Windows.Forms.Button();
            this.SendPacketList = new System.Windows.Forms.CheckedListBox();
            this.SendPacket = new System.Windows.Forms.Button();
            this.Send = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.SendClearOutput = new System.Windows.Forms.Button();
            this.SendEnable = new System.Windows.Forms.CheckBox();
            this.SendPacketOutput = new System.Windows.Forms.TextBox();
            this.TabPage2 = new System.Windows.Forms.TabPage();
            this.RecvUnSelectAllPacket = new System.Windows.Forms.Button();
            this.RecvClearOutput = new System.Windows.Forms.Button();
            this.RecvSelectAllPacket = new System.Windows.Forms.Button();
            this.RecvPacketList = new System.Windows.Forms.CheckedListBox();
            this.RecvEnable = new System.Windows.Forms.CheckBox();
            this.RecvPacketOutput = new System.Windows.Forms.TextBox();
            this.TabControl1.SuspendLayout();
            this.TabPage1.SuspendLayout();
            this.TabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.TabPage1);
            this.TabControl1.Controls.Add(this.TabPage2);
            this.TabControl1.Location = new System.Drawing.Point(1, 1);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(696, 551);
            this.TabControl1.TabIndex = 2;
            // 
            // TabPage1
            // 
            this.TabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.TabPage1.Controls.Add(this.SendUnSelectAllPacket);
            this.TabPage1.Controls.Add(this.SendSelectAllPacket);
            this.TabPage1.Controls.Add(this.SendPacketList);
            this.TabPage1.Controls.Add(this.SendPacket);
            this.TabPage1.Controls.Add(this.Send);
            this.TabPage1.Controls.Add(this.Label2);
            this.TabPage1.Controls.Add(this.SendClearOutput);
            this.TabPage1.Controls.Add(this.SendEnable);
            this.TabPage1.Controls.Add(this.SendPacketOutput);
            this.TabPage1.Location = new System.Drawing.Point(4, 22);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage1.Size = new System.Drawing.Size(688, 525);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "Send";
            // 
            // SendUnSelectAllPacket
            // 
            this.SendUnSelectAllPacket.Location = new System.Drawing.Point(129, 449);
            this.SendUnSelectAllPacket.Name = "SendUnSelectAllPacket";
            this.SendUnSelectAllPacket.Size = new System.Drawing.Size(104, 22);
            this.SendUnSelectAllPacket.TabIndex = 12;
            this.SendUnSelectAllPacket.Text = "Tümünü Kaldır";
            this.SendUnSelectAllPacket.UseVisualStyleBackColor = true;
            // 
            // SendSelectAllPacket
            // 
            this.SendSelectAllPacket.Location = new System.Drawing.Point(3, 449);
            this.SendSelectAllPacket.Name = "SendSelectAllPacket";
            this.SendSelectAllPacket.Size = new System.Drawing.Size(120, 22);
            this.SendSelectAllPacket.TabIndex = 9;
            this.SendSelectAllPacket.Text = "Tümünü Seç";
            this.SendSelectAllPacket.UseVisualStyleBackColor = true;
            // 
            // SendPacketList
            // 
            this.SendPacketList.BackColor = System.Drawing.SystemColors.Control;
            this.SendPacketList.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.SendPacketList.ForeColor = System.Drawing.SystemColors.ControlText;
            this.SendPacketList.FormattingEnabled = true;
            this.SendPacketList.Items.AddRange(new object[] {
            "0x01 WIZ_LOGIN",
            "0x02 WIZ_NEW_CHAR",
            "0x03 WIZ_DEL_CHAR",
            "0x04 WIZ_SEL_CHAR",
            "0x05 WIZ_SEL_NATION",
            "0x06 WIZ_MOVE",
            "0x07 WIZ_USER_INOUT",
            "0x08 WIZ_ATTACK",
            "0x09 WIZ_ROTATE",
            "0x0A WIZ_NPC_INOUT",
            "0x0B WIZ_NPC_MOVE",
            "0x0C WIZ_ALLCHAR_INFO_REQ",
            "0x0D WIZ_GAMESTART",
            "0x0E WIZ_MYINFO",
            "0x0F WIZ_LOGOUT",
            "0x10 WIZ_CHAT",
            "0x11 WIZ_DEAD",
            "0x12 WIZ_REGENE",
            "0x13 WIZ_TIME",
            "0x14 WIZ_WEATHER",
            "0x15 WIZ_REGIONCHANGE",
            "0x16 WIZ_REQ_USERIN",
            "0x17 WIZ_HP_CHANGE",
            "0x18 WIZ_MSP_CHANGE",
            "0x19 WIZ_ITEM_LOG",
            "0x1A WIZ_EXP_CHANGE",
            "0x1B WIZ_LEVEL_CHANGE",
            "0x1C WIZ_NPC_REGION",
            "0x1D WIZ_REQ_NPCIN",
            "0x1E WIZ_WARP",
            "0x1F WIZ_ITEM_MOVE",
            "0x20 WIZ_NPC_EVENT",
            "0x21 WIZ_ITEM_TRADE",
            "0x22 WIZ_TARGET_HP",
            "0x23 WIZ_ITEM_DROP",
            "0x24 WIZ_BUNDLE_OPEN_REQ",
            "0x25 WIZ_TRADE_NPC",
            "0x26 WIZ_ITEM_GET",
            "0x27 WIZ_ZONE_CHANGE",
            "0x28 WIZ_POINT_CHANGE",
            "0x29 WIZ_STATE_CHANGE",
            "0x2A WIZ_LOYALTY_CHANGE",
            "0x2B WIZ_VERSION_CHECK",
            "0x2C WIZ_CRYPTION",
            "0x2D WIZ_USERLOOK_CHANGE",
            "0x2E WIZ_NOTICE",
            "0x2F WIZ_PARTY",
            "0x30 WIZ_EXCHANGE",
            "0x31 WIZ_MAGIC_PROCESS",
            "0x32 WIZ_SKILLPT_CHANGE",
            "0x33 WIZ_OBJECT_EVENT",
            "0x34 WIZ_CLASS_CHANGE",
            "0x35 WIZ_CHAT_TARGET",
            "0x36 WIZ_CONCURRENTUSER",
            "0x37 WIZ_DATASAVE",
            "0x38 WIZ_DURATION",
            "0x39 WIZ_TIMENOTIFY",
            "0x3A WIZ_REPAIR_NPC",
            "0x3B WIZ_ITEM_REPAIR",
            "0x3C WIZ_KNIGHTS_PROCESS",
            "0x3D WIZ_ITEM_COUNT_CHANGE",
            "0x3E WIZ_KNIGHTS_LIST",
            "0x3F WIZ_ITEM_REMOVE",
            "0x40 WIZ_OPERATOR",
            "0x41 WIZ_SPEEDHACK_CHECK",
            "0x42 WIZ_COMPRESS_PACKET",
            "0x43 WIZ_SERVER_CHECK",
            "0x44 WIZ_CONTINOUS_PACKET",
            "0x45 WIZ_WAREHOUSE",
            "0x46 WIZ_SERVER_CHANGE",
            "0x47 WIZ_REPORT_BUG",
            "0x48 WIZ_HOME",
            "0x49 WIZ_FRIEND_REPORT",
            "0x4A WIZ_GOLD_CHANGE",
            "0x4B WIZ_WARP_LIST",
            "0x4C WIZ_VIRTUAL_SERVER",
            "0x4D WIZ_ZONE_CONCURRENT",
            "0x4E WIZ_CLAN_PROCESS",
            "0x4F WIZ_PARTY_BBS",
            "0x50 WIZ_MARKET_BBS",
            "0x51 WIZ_KICKOUT",
            "0x52 WIZ_CLIENT_EVENT",
            "0x53 I_DONT_KNOW",
            "0x54 WIZ_WEIGHT_CHANGE",
            "0x55 WIZ_SELECT_MSG",
            "0x56 WIZ_NPC_SAY",
            "0x57 WIZ_BATTLE_EVENT",
            "0x58 WIZ_AUTHORITY_CHANGE",
            "0x59 WIZ_EDIT_BOX",
            "0x5A WIZ_SANTA",
            "0x5F WIZ_EVENT",
            "0x5B WIZ_ITEM_UPGRADE",
            "0x5E WIZ_ZONEABILITY",
            "0x60 WIZ_STEALTH",
            "0x61 WIZ_ROOM_PACKETPROCESS",
            "0x62 WIZ_ROOM",
            "0x64 WIZ_QUEST",
            "0x66 WIZ_KISS",
            "0x67 WIZ_RECOMMEND_USER",
            "0x68 WIZ_MERCHANT",
            "0x69 WIZ_MERCHANT_INOUT",
            "0x6A WIZ_SHOPPING_MALL",
            "0x6B WIZ_SERVER_INDEX",
            "0x6C WIZ_EFFECT",
            "0x6D WIZ_SIEGE",
            "0x6E WIZ_NAME_CHANGE",
            "0x6F WIZ_WEBPAGE",
            "0x70 WIZ_CAPE",
            "0x71 WIZ_PREMIUM",
            "0x72 WIZ_HACKTOOL",
            "0x73 WIZ_RENTAL",
            "0x75 WIZ_CHALLENGE",
            "0x76 WIZ_PET",
            "0x77 WIZ_CHINA",
            "0x78 WIZ_KING",
            "0x79 WIZ_SKILLDATA",
            "0x7A WIZ_PROGRAMCHECK",
            "0x7B WIZ_BIFROST",
            "0x7C WIZ_REPORT",
            "0x7D WIZ_LOGOSSHOUT",
            "0x80 WIZ_RANK",
            "0x81 WIZ_STORY",
            "0x86 WIZ_MINING",
            "0x87 WIZ_HELMET",
            "0x88 WIZ_PVP",
            "0x89 WIZ_CHANGE_HAIR",
            "0x90 WIZ_DEATH_LIST",
            "0x91 WIZ_CLANPOINTS_BATTLE",
            "0x98 WIZ_TEST",
            "0xA0 XIGNCODE"});
            this.SendPacketList.Location = new System.Drawing.Point(3, 4);
            this.SendPacketList.Name = "SendPacketList";
            this.SendPacketList.Size = new System.Drawing.Size(230, 436);
            this.SendPacketList.TabIndex = 8;
            // 
            // SendPacket
            // 
            this.SendPacket.Location = new System.Drawing.Point(586, 468);
            this.SendPacket.Name = "SendPacket";
            this.SendPacket.Size = new System.Drawing.Size(94, 20);
            this.SendPacket.TabIndex = 7;
            this.SendPacket.Text = "Gönder";
            this.SendPacket.UseVisualStyleBackColor = true;
            // 
            // Send
            // 
            this.Send.BackColor = System.Drawing.SystemColors.Control;
            this.Send.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Send.Location = new System.Drawing.Point(239, 467);
            this.Send.Name = "Send";
            this.Send.Size = new System.Drawing.Size(341, 21);
            this.Send.TabIndex = 6;
            this.Send.Text = "4800";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(421, 470);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(0, 13);
            this.Label2.TabIndex = 5;
            // 
            // SendClearOutput
            // 
            this.SendClearOutput.Location = new System.Drawing.Point(183, 493);
            this.SendClearOutput.Name = "SendClearOutput";
            this.SendClearOutput.Size = new System.Drawing.Size(86, 25);
            this.SendClearOutput.TabIndex = 4;
            this.SendClearOutput.Text = "Temizle";
            this.SendClearOutput.UseVisualStyleBackColor = true;
            // 
            // SendEnable
            // 
            this.SendEnable.AutoSize = true;
            this.SendEnable.ForeColor = System.Drawing.SystemColors.ControlText;
            this.SendEnable.Location = new System.Drawing.Point(10, 500);
            this.SendEnable.Name = "SendEnable";
            this.SendEnable.Size = new System.Drawing.Size(106, 17);
            this.SendEnable.TabIndex = 2;
            this.SendEnable.Text = "Send Paket Dinle";
            this.SendEnable.UseVisualStyleBackColor = true;
            // 
            // SendPacketOutput
            // 
            this.SendPacketOutput.BackColor = System.Drawing.SystemColors.Control;
            this.SendPacketOutput.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.SendPacketOutput.ForeColor = System.Drawing.SystemColors.ControlText;
            this.SendPacketOutput.Location = new System.Drawing.Point(239, 6);
            this.SendPacketOutput.Multiline = true;
            this.SendPacketOutput.Name = "SendPacketOutput";
            this.SendPacketOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.SendPacketOutput.Size = new System.Drawing.Size(443, 455);
            this.SendPacketOutput.TabIndex = 1;
            this.SendPacketOutput.WordWrap = false;
            // 
            // TabPage2
            // 
            this.TabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.TabPage2.Controls.Add(this.RecvUnSelectAllPacket);
            this.TabPage2.Controls.Add(this.RecvClearOutput);
            this.TabPage2.Controls.Add(this.RecvSelectAllPacket);
            this.TabPage2.Controls.Add(this.RecvPacketList);
            this.TabPage2.Controls.Add(this.RecvEnable);
            this.TabPage2.Controls.Add(this.RecvPacketOutput);
            this.TabPage2.Location = new System.Drawing.Point(4, 22);
            this.TabPage2.Name = "TabPage2";
            this.TabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage2.Size = new System.Drawing.Size(688, 525);
            this.TabPage2.TabIndex = 1;
            this.TabPage2.Text = "Recv";
            // 
            // RecvUnSelectAllPacket
            // 
            this.RecvUnSelectAllPacket.Location = new System.Drawing.Point(129, 449);
            this.RecvUnSelectAllPacket.Name = "RecvUnSelectAllPacket";
            this.RecvUnSelectAllPacket.Size = new System.Drawing.Size(104, 22);
            this.RecvUnSelectAllPacket.TabIndex = 16;
            this.RecvUnSelectAllPacket.Text = "Tümünü Kaldır";
            this.RecvUnSelectAllPacket.UseVisualStyleBackColor = true;
            // 
            // RecvClearOutput
            // 
            this.RecvClearOutput.Location = new System.Drawing.Point(183, 493);
            this.RecvClearOutput.Name = "RecvClearOutput";
            this.RecvClearOutput.Size = new System.Drawing.Size(86, 25);
            this.RecvClearOutput.TabIndex = 13;
            this.RecvClearOutput.Text = "Temizle";
            this.RecvClearOutput.UseVisualStyleBackColor = true;
            // 
            // RecvSelectAllPacket
            // 
            this.RecvSelectAllPacket.Location = new System.Drawing.Point(3, 449);
            this.RecvSelectAllPacket.Name = "RecvSelectAllPacket";
            this.RecvSelectAllPacket.Size = new System.Drawing.Size(120, 22);
            this.RecvSelectAllPacket.TabIndex = 10;
            this.RecvSelectAllPacket.Text = "Tümünü Seç";
            this.RecvSelectAllPacket.UseVisualStyleBackColor = true;
            // 
            // RecvPacketList
            // 
            this.RecvPacketList.BackColor = System.Drawing.SystemColors.Control;
            this.RecvPacketList.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.RecvPacketList.ForeColor = System.Drawing.SystemColors.ControlText;
            this.RecvPacketList.FormattingEnabled = true;
            this.RecvPacketList.Items.AddRange(new object[] {
            "0x01 WIZ_LOGIN",
            "0x02 WIZ_NEW_CHAR",
            "0x03 WIZ_DEL_CHAR",
            "0x04 WIZ_SEL_CHAR",
            "0x05 WIZ_SEL_NATION",
            "0x06 WIZ_MOVE",
            "0x07 WIZ_USER_INOUT",
            "0x08 WIZ_ATTACK",
            "0x09 WIZ_ROTATE",
            "0x0A WIZ_NPC_INOUT",
            "0x0B WIZ_NPC_MOVE",
            "0x0C WIZ_ALLCHAR_INFO_REQ",
            "0x0D WIZ_GAMESTART",
            "0x0E WIZ_MYINFO",
            "0x0F WIZ_LOGOUT",
            "0x10 WIZ_CHAT",
            "0x11 WIZ_DEAD",
            "0x12 WIZ_REGENE",
            "0x13 WIZ_TIME",
            "0x14 WIZ_WEATHER",
            "0x15 WIZ_REGIONCHANGE",
            "0x16 WIZ_REQ_USERIN",
            "0x17 WIZ_HP_CHANGE",
            "0x18 WIZ_MSP_CHANGE",
            "0x19 WIZ_ITEM_LOG",
            "0x1A WIZ_EXP_CHANGE",
            "0x1B WIZ_LEVEL_CHANGE",
            "0x1C WIZ_NPC_REGION",
            "0x1D WIZ_REQ_NPCIN",
            "0x1E WIZ_WARP",
            "0x1F WIZ_ITEM_MOVE",
            "0x20 WIZ_NPC_EVENT",
            "0x21 WIZ_ITEM_TRADE",
            "0x22 WIZ_TARGET_HP",
            "0x23 WIZ_ITEM_DROP",
            "0x24 WIZ_BUNDLE_OPEN_REQ",
            "0x25 WIZ_TRADE_NPC",
            "0x26 WIZ_ITEM_GET",
            "0x27 WIZ_ZONE_CHANGE",
            "0x28 WIZ_POINT_CHANGE",
            "0x29 WIZ_STATE_CHANGE",
            "0x2A WIZ_LOYALTY_CHANGE",
            "0x2B WIZ_VERSION_CHECK",
            "0x2C WIZ_CRYPTION",
            "0x2D WIZ_USERLOOK_CHANGE",
            "0x2E WIZ_NOTICE",
            "0x2F WIZ_PARTY",
            "0x30 WIZ_EXCHANGE",
            "0x31 WIZ_MAGIC_PROCESS",
            "0x32 WIZ_SKILLPT_CHANGE",
            "0x33 WIZ_OBJECT_EVENT",
            "0x34 WIZ_CLASS_CHANGE",
            "0x35 WIZ_CHAT_TARGET",
            "0x36 WIZ_CONCURRENTUSER",
            "0x37 WIZ_DATASAVE",
            "0x38 WIZ_DURATION",
            "0x39 WIZ_TIMENOTIFY",
            "0x3A WIZ_REPAIR_NPC",
            "0x3B WIZ_ITEM_REPAIR",
            "0x3C WIZ_KNIGHTS_PROCESS",
            "0x3D WIZ_ITEM_COUNT_CHANGE",
            "0x3E WIZ_KNIGHTS_LIST",
            "0x3F WIZ_ITEM_REMOVE",
            "0x40 WIZ_OPERATOR",
            "0x41 WIZ_SPEEDHACK_CHECK",
            "0x42 WIZ_COMPRESS_PACKET",
            "0x43 WIZ_SERVER_CHECK",
            "0x44 WIZ_CONTINOUS_PACKET",
            "0x45 WIZ_WAREHOUSE",
            "0x46 WIZ_SERVER_CHANGE",
            "0x47 WIZ_REPORT_BUG",
            "0x48 WIZ_HOME",
            "0x49 WIZ_FRIEND_REPORT",
            "0x4A WIZ_GOLD_CHANGE",
            "0x4B WIZ_WARP_LIST",
            "0x4C WIZ_VIRTUAL_SERVER",
            "0x4D WIZ_ZONE_CONCURRENT",
            "0x4E WIZ_CLAN_PROCESS",
            "0x4F WIZ_PARTY_BBS",
            "0x50 WIZ_MARKET_BBS",
            "0x51 WIZ_KICKOUT",
            "0x52 WIZ_CLIENT_EVENT",
            "0x53 I_DONT_KNOW",
            "0x54 WIZ_WEIGHT_CHANGE",
            "0x55 WIZ_SELECT_MSG",
            "0x56 WIZ_NPC_SAY",
            "0x57 WIZ_BATTLE_EVENT",
            "0x58 WIZ_AUTHORITY_CHANGE",
            "0x59 WIZ_EDIT_BOX",
            "0x5A WIZ_SANTA",
            "0x5F WIZ_EVENT",
            "0x5B WIZ_ITEM_UPGRADE",
            "0x5E WIZ_ZONEABILITY",
            "0x60 WIZ_STEALTH",
            "0x61 WIZ_ROOM_PACKETPROCESS",
            "0x62 WIZ_ROOM",
            "0x64 WIZ_QUEST",
            "0x66 WIZ_KISS",
            "0x67 WIZ_RECOMMEND_USER",
            "0x68 WIZ_MERCHANT",
            "0x69 WIZ_MERCHANT_INOUT",
            "0x6A WIZ_SHOPPING_MALL",
            "0x6B WIZ_SERVER_INDEX",
            "0x6C WIZ_EFFECT",
            "0x6D WIZ_SIEGE",
            "0x6E WIZ_NAME_CHANGE",
            "0x6F WIZ_WEBPAGE",
            "0x70 WIZ_CAPE",
            "0x71 WIZ_PREMIUM",
            "0x72 WIZ_HACKTOOL",
            "0x73 WIZ_RENTAL",
            "0x75 WIZ_CHALLENGE",
            "0x76 WIZ_PET",
            "0x77 WIZ_CHINA",
            "0x78 WIZ_KING",
            "0x79 WIZ_SKILLDATA",
            "0x7A WIZ_PROGRAMCHECK",
            "0x7B WIZ_BIFROST",
            "0x7C WIZ_REPORT",
            "0x7D WIZ_LOGOSSHOUT",
            "0x80 WIZ_RANK",
            "0x81 WIZ_STORY",
            "0x86 WIZ_MINING",
            "0x87 WIZ_HELMET",
            "0x88 WIZ_PVP",
            "0x89 WIZ_CHANGE_HAIR",
            "0x90 WIZ_DEATH_LIST",
            "0x91 WIZ_CLANPOINTS_BATTLE",
            "0x98 WIZ_TEST",
            "0xA0 XIGNCODE",
            "DIGER PAKETLER"});
            this.RecvPacketList.Location = new System.Drawing.Point(3, 4);
            this.RecvPacketList.Name = "RecvPacketList";
            this.RecvPacketList.Size = new System.Drawing.Size(230, 436);
            this.RecvPacketList.TabIndex = 9;
            // 
            // RecvEnable
            // 
            this.RecvEnable.AutoSize = true;
            this.RecvEnable.BackColor = System.Drawing.SystemColors.Control;
            this.RecvEnable.ForeColor = System.Drawing.SystemColors.ControlText;
            this.RecvEnable.Location = new System.Drawing.Point(10, 500);
            this.RecvEnable.Name = "RecvEnable";
            this.RecvEnable.Size = new System.Drawing.Size(106, 17);
            this.RecvEnable.TabIndex = 3;
            this.RecvEnable.Text = "Recv Paket Dinle";
            this.RecvEnable.UseVisualStyleBackColor = false;
            // 
            // RecvPacketOutput
            // 
            this.RecvPacketOutput.BackColor = System.Drawing.SystemColors.Control;
            this.RecvPacketOutput.ForeColor = System.Drawing.SystemColors.ControlText;
            this.RecvPacketOutput.Location = new System.Drawing.Point(239, 6);
            this.RecvPacketOutput.Multiline = true;
            this.RecvPacketOutput.Name = "RecvPacketOutput";
            this.RecvPacketOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.RecvPacketOutput.Size = new System.Drawing.Size(443, 455);
            this.RecvPacketOutput.TabIndex = 2;
            this.RecvPacketOutput.WordWrap = false;
            // 
            // PacketLogger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(697, 550);
            this.Controls.Add(this.TabControl1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PacketLogger";
            this.Text = "Packet Logger";
            this.Load += new System.EventHandler(this.PacketLogger_Load);
            this.TabControl1.ResumeLayout(false);
            this.TabPage1.ResumeLayout(false);
            this.TabPage1.PerformLayout();
            this.TabPage2.ResumeLayout(false);
            this.TabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TabControl TabControl1;
        internal System.Windows.Forms.TabPage TabPage2;
        internal System.Windows.Forms.Button RecvUnSelectAllPacket;
        internal System.Windows.Forms.Button RecvClearOutput;
        internal System.Windows.Forms.Button RecvSelectAllPacket;
        internal System.Windows.Forms.CheckedListBox RecvPacketList;
        internal System.Windows.Forms.CheckBox RecvEnable;
        internal System.Windows.Forms.TextBox RecvPacketOutput;
        internal System.Windows.Forms.TabPage TabPage1;
        internal System.Windows.Forms.Button SendUnSelectAllPacket;
        internal System.Windows.Forms.Button SendSelectAllPacket;
        internal System.Windows.Forms.CheckedListBox SendPacketList;
        internal System.Windows.Forms.Button SendPacket;
        internal System.Windows.Forms.TextBox Send;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Button SendClearOutput;
        internal System.Windows.Forms.CheckBox SendEnable;
        internal System.Windows.Forms.TextBox SendPacketOutput;
    }
}