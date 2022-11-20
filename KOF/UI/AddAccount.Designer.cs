
namespace KOF.UI
{
    partial class AddAccount
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddAccount));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Platform = new System.Windows.Forms.ComboBox();
            this.PlatformLabel = new System.Windows.Forms.Label();
            this.AccountId = new System.Windows.Forms.TextBox();
            this.AccountPassword = new System.Windows.Forms.TextBox();
            this.Save = new System.Windows.Forms.Button();
            this.Clear = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.Path = new System.Windows.Forms.TextBox();
            this.ServerIdLabel = new System.Windows.Forms.Label();
            this.ServerId = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.CharacterId = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.CharacterName = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.ServerId)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Hesap Adı :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Şifre :";
            // 
            // Platform
            // 
            this.Platform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Platform.FormattingEnabled = true;
            this.Platform.Items.AddRange(new object[] {
            "JPKO",
            "CNKO"});
            this.Platform.Location = new System.Drawing.Point(81, 81);
            this.Platform.Name = "Platform";
            this.Platform.Size = new System.Drawing.Size(96, 21);
            this.Platform.TabIndex = 2;
            // 
            // PlatformLabel
            // 
            this.PlatformLabel.AutoSize = true;
            this.PlatformLabel.Location = new System.Drawing.Point(27, 84);
            this.PlatformLabel.Name = "PlatformLabel";
            this.PlatformLabel.Size = new System.Drawing.Size(51, 13);
            this.PlatformLabel.TabIndex = 3;
            this.PlatformLabel.Text = "Platform :";
            // 
            // AccountId
            // 
            this.AccountId.Location = new System.Drawing.Point(81, 6);
            this.AccountId.Name = "AccountId";
            this.AccountId.Size = new System.Drawing.Size(96, 20);
            this.AccountId.TabIndex = 4;
            // 
            // AccountPassword
            // 
            this.AccountPassword.Location = new System.Drawing.Point(81, 31);
            this.AccountPassword.Name = "AccountPassword";
            this.AccountPassword.PasswordChar = '*';
            this.AccountPassword.Size = new System.Drawing.Size(96, 20);
            this.AccountPassword.TabIndex = 5;
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(227, 93);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 6;
            this.Save.Text = "Kaydet";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // Clear
            // 
            this.Clear.Location = new System.Drawing.Point(308, 93);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(75, 23);
            this.Clear.TabIndex = 7;
            this.Clear.Text = "Temizle";
            this.Clear.UseVisualStyleBackColor = true;
            this.Clear.Click += new System.EventHandler(this.Clear_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(46, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Exe :";
            // 
            // Path
            // 
            this.Path.Location = new System.Drawing.Point(81, 55);
            this.Path.Name = "Path";
            this.Path.Size = new System.Drawing.Size(96, 20);
            this.Path.TabIndex = 9;
            this.Path.Click += new System.EventHandler(this.Path_Click);
            // 
            // ServerIdLabel
            // 
            this.ServerIdLabel.AutoSize = true;
            this.ServerIdLabel.Location = new System.Drawing.Point(213, 9);
            this.ServerIdLabel.Name = "ServerIdLabel";
            this.ServerIdLabel.Size = new System.Drawing.Size(68, 13);
            this.ServerIdLabel.TabIndex = 10;
            this.ServerIdLabel.Text = "Server Sıra  :";
            // 
            // ServerId
            // 
            this.ServerId.Location = new System.Drawing.Point(287, 8);
            this.ServerId.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ServerId.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ServerId.Name = "ServerId";
            this.ServerId.Size = new System.Drawing.Size(96, 20);
            this.ServerId.TabIndex = 11;
            this.ServerId.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(204, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Karakter Sıra  :";
            // 
            // CharacterId
            // 
            this.CharacterId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CharacterId.FormattingEnabled = true;
            this.CharacterId.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4"});
            this.CharacterId.Location = new System.Drawing.Point(287, 33);
            this.CharacterId.Name = "CharacterId";
            this.CharacterId.Size = new System.Drawing.Size(96, 21);
            this.CharacterId.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(210, 58);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Karakter Adı :";
            // 
            // CharacterName
            // 
            this.CharacterName.Location = new System.Drawing.Point(287, 58);
            this.CharacterName.Name = "CharacterName";
            this.CharacterName.Size = new System.Drawing.Size(96, 20);
            this.CharacterName.TabIndex = 15;
            // 
            // AddAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(388, 128);
            this.Controls.Add(this.CharacterName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.CharacterId);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ServerId);
            this.Controls.Add(this.ServerIdLabel);
            this.Controls.Add(this.Path);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Clear);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.AccountPassword);
            this.Controls.Add(this.AccountId);
            this.Controls.Add(this.PlatformLabel);
            this.Controls.Add(this.Platform);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddAccount";
            this.Text = "Hesap Ekle";
            this.Load += new System.EventHandler(this.AddAccount_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox Platform;
        private System.Windows.Forms.Label PlatformLabel;
        private System.Windows.Forms.TextBox AccountId;
        private System.Windows.Forms.TextBox AccountPassword;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button Clear;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Path;
        private System.Windows.Forms.Label ServerIdLabel;
        private System.Windows.Forms.NumericUpDown ServerId;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox CharacterId;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox CharacterName;
    }
}