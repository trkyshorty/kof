
namespace KOF.UI
{
    partial class SellableItem
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SellableItem));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.SearchItemList = new System.Windows.Forms.TextBox();
            this.LoadFromDatabase = new System.Windows.Forms.Button();
            this.LoadFromInventory = new System.Windows.Forms.Button();
            this.ListBox1 = new System.Windows.Forms.ListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.Reset = new System.Windows.Forms.Button();
            this.SearchSellableItemList = new System.Windows.Forms.TextBox();
            this.ListBox2 = new System.Windows.Forms.ListBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(398, 337);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.GroupBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(390, 311);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Eşya Listesi";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.SearchItemList);
            this.GroupBox1.Controls.Add(this.LoadFromDatabase);
            this.GroupBox1.Controls.Add(this.LoadFromInventory);
            this.GroupBox1.Controls.Add(this.ListBox1);
            this.GroupBox1.Location = new System.Drawing.Point(3, 3);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(384, 307);
            this.GroupBox1.TabIndex = 1;
            this.GroupBox1.TabStop = false;
            // 
            // SearchItemList
            // 
            this.SearchItemList.Location = new System.Drawing.Point(7, 17);
            this.SearchItemList.Name = "SearchItemList";
            this.SearchItemList.Size = new System.Drawing.Size(184, 20);
            this.SearchItemList.TabIndex = 3;
            this.SearchItemList.TextChanged += new System.EventHandler(this.SearchItemList_TextChanged);
            // 
            // LoadFromDatabase
            // 
            this.LoadFromDatabase.Location = new System.Drawing.Point(207, 44);
            this.LoadFromDatabase.Name = "LoadFromDatabase";
            this.LoadFromDatabase.Size = new System.Drawing.Size(169, 25);
            this.LoadFromDatabase.TabIndex = 2;
            this.LoadFromDatabase.Text = "Veritabanından Yükle";
            this.LoadFromDatabase.UseVisualStyleBackColor = true;
            this.LoadFromDatabase.Click += new System.EventHandler(this.LoadFromDatabase_Click);
            // 
            // LoadFromInventory
            // 
            this.LoadFromInventory.Location = new System.Drawing.Point(207, 13);
            this.LoadFromInventory.Name = "LoadFromInventory";
            this.LoadFromInventory.Size = new System.Drawing.Size(169, 25);
            this.LoadFromInventory.TabIndex = 1;
            this.LoadFromInventory.Text = "Inventory Yükle";
            this.LoadFromInventory.UseVisualStyleBackColor = true;
            this.LoadFromInventory.Click += new System.EventHandler(this.LoadFromInventory_Click);
            // 
            // ListBox1
            // 
            this.ListBox1.FormattingEnabled = true;
            this.ListBox1.Location = new System.Drawing.Point(7, 45);
            this.ListBox1.Name = "ListBox1";
            this.ListBox1.Size = new System.Drawing.Size(184, 251);
            this.ListBox1.TabIndex = 0;
            this.ListBox1.DoubleClick += new System.EventHandler(this.ListBox1_DoubleClick);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.GroupBox2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(390, 311);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Satılabilecek Eşyalar";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.Reset);
            this.GroupBox2.Controls.Add(this.SearchSellableItemList);
            this.GroupBox2.Controls.Add(this.ListBox2);
            this.GroupBox2.ForeColor = System.Drawing.Color.Black;
            this.GroupBox2.Location = new System.Drawing.Point(3, 3);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(384, 307);
            this.GroupBox2.TabIndex = 1;
            this.GroupBox2.TabStop = false;
            // 
            // Reset
            // 
            this.Reset.Location = new System.Drawing.Point(207, 13);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(169, 25);
            this.Reset.TabIndex = 5;
            this.Reset.Text = "Tümünü Sıfırla";
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.Reset_Click);
            // 
            // SearchSellableItemList
            // 
            this.SearchSellableItemList.Location = new System.Drawing.Point(7, 17);
            this.SearchSellableItemList.Name = "SearchSellableItemList";
            this.SearchSellableItemList.Size = new System.Drawing.Size(184, 20);
            this.SearchSellableItemList.TabIndex = 4;
            this.SearchSellableItemList.TextChanged += new System.EventHandler(this.SearchSellableItemList_TextChanged);
            // 
            // ListBox2
            // 
            this.ListBox2.FormattingEnabled = true;
            this.ListBox2.Location = new System.Drawing.Point(7, 45);
            this.ListBox2.Name = "ListBox2";
            this.ListBox2.Size = new System.Drawing.Size(184, 251);
            this.ListBox2.TabIndex = 0;
            this.ListBox2.DoubleClick += new System.EventHandler(this.ListBox2_DoubleClick);
            // 
            // SellableItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 336);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SellableItem";
            this.Text = "Satılabilecek Eşyalar";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.SellableItem_Closing);
            this.Load += new System.EventHandler(this.SellableItem_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        internal System.Windows.Forms.GroupBox GroupBox1;
        internal System.Windows.Forms.TextBox SearchItemList;
        internal System.Windows.Forms.Button LoadFromDatabase;
        internal System.Windows.Forms.Button LoadFromInventory;
        internal System.Windows.Forms.ListBox ListBox1;
        private System.Windows.Forms.TabPage tabPage2;
        internal System.Windows.Forms.GroupBox GroupBox2;
        internal System.Windows.Forms.Button Reset;
        internal System.Windows.Forms.TextBox SearchSellableItemList;
        internal System.Windows.Forms.ListBox ListBox2;
    }
}