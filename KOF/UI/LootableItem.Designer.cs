
namespace KOF.UI
{
    partial class LootableItem
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LootableItem));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.SearchItemList = new System.Windows.Forms.TextBox();
            this.LoadFromDatabase = new System.Windows.Forms.Button();
            this.LoadFromInventory = new System.Windows.Forms.Button();
            this.ListBox1 = new System.Windows.Forms.ListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.GroupBox3 = new System.Windows.Forms.GroupBox();
            this.LootOther = new System.Windows.Forms.CheckBox();
            this.LootConsumable = new System.Windows.Forms.CheckBox();
            this.Reset = new System.Windows.Forms.Button();
            this.SearchLootableItemList = new System.Windows.Forms.TextBox();
            this.ListBox2 = new System.Windows.Forms.ListBox();
            this.Price = new System.Windows.Forms.Label();
            this.LootPrice = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.GroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LootPrice)).BeginInit();
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
            this.tabPage2.Text = "Toplanabilecek Eşyalar";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.GroupBox3);
            this.GroupBox2.Controls.Add(this.Reset);
            this.GroupBox2.Controls.Add(this.SearchLootableItemList);
            this.GroupBox2.Controls.Add(this.ListBox2);
            this.GroupBox2.ForeColor = System.Drawing.Color.Black;
            this.GroupBox2.Location = new System.Drawing.Point(3, 3);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(387, 307);
            this.GroupBox2.TabIndex = 1;
            this.GroupBox2.TabStop = false;
            // 
            // GroupBox3
            // 
            this.GroupBox3.Controls.Add(this.label1);
            this.GroupBox3.Controls.Add(this.LootPrice);
            this.GroupBox3.Controls.Add(this.Price);
            this.GroupBox3.Controls.Add(this.LootOther);
            this.GroupBox3.Controls.Add(this.LootConsumable);
            this.GroupBox3.Location = new System.Drawing.Point(207, 45);
            this.GroupBox3.Name = "GroupBox3";
            this.GroupBox3.Size = new System.Drawing.Size(169, 167);
            this.GroupBox3.TabIndex = 6;
            this.GroupBox3.TabStop = false;
            this.GroupBox3.Text = "Topla";
            // 
            // LootOther
            // 
            this.LootOther.AutoSize = true;
            this.LootOther.Checked = true;
            this.LootOther.CheckState = System.Windows.Forms.CheckState.Checked;
            this.LootOther.Location = new System.Drawing.Point(11, 45);
            this.LootOther.Name = "LootOther";
            this.LootOther.Size = new System.Drawing.Size(51, 17);
            this.LootOther.TabIndex = 2;
            this.LootOther.Text = "Diğer";
            this.LootOther.UseVisualStyleBackColor = true;
            this.LootOther.CheckedChanged += new System.EventHandler(this.LootOther_CheckedChanged);
            // 
            // LootConsumable
            // 
            this.LootConsumable.AutoSize = true;
            this.LootConsumable.Checked = true;
            this.LootConsumable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.LootConsumable.Location = new System.Drawing.Point(11, 21);
            this.LootConsumable.Name = "LootConsumable";
            this.LootConsumable.Size = new System.Drawing.Size(103, 17);
            this.LootConsumable.TabIndex = 1;
            this.LootConsumable.Text = "Sarf Malzemeleri";
            this.LootConsumable.UseVisualStyleBackColor = true;
            this.LootConsumable.CheckedChanged += new System.EventHandler(this.LootConsumable_CheckedChanged);
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
            // SearchLootableItemList
            // 
            this.SearchLootableItemList.Location = new System.Drawing.Point(7, 17);
            this.SearchLootableItemList.Name = "SearchLootableItemList";
            this.SearchLootableItemList.Size = new System.Drawing.Size(184, 20);
            this.SearchLootableItemList.TabIndex = 4;
            this.SearchLootableItemList.TextChanged += new System.EventHandler(this.SearchLootableItemList_TextChanged);
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
            // Price
            // 
            this.Price.AutoSize = true;
            this.Price.Location = new System.Drawing.Point(8, 72);
            this.Price.Name = "Price";
            this.Price.Size = new System.Drawing.Size(49, 13);
            this.Price.TabIndex = 4;
            this.Price.Text = "Topla >=";
            // 
            // LootPrice
            // 
            this.LootPrice.Increment = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.LootPrice.Location = new System.Drawing.Point(63, 70);
            this.LootPrice.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.LootPrice.Name = "LootPrice";
            this.LootPrice.Size = new System.Drawing.Size(100, 20);
            this.LootPrice.TabIndex = 5;
            this.LootPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.LootPrice.ThousandsSeparator = true;
            this.LootPrice.Value = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.LootPrice.ValueChanged += new System.EventHandler(this.LootPrice_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(90, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Noah";
            // 
            // LootableItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 336);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LootableItem";
            this.Text = "Toplanabilecek Eşyalar";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.LootableItem_Closing);
            this.Load += new System.EventHandler(this.LootableItem_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox2.PerformLayout();
            this.GroupBox3.ResumeLayout(false);
            this.GroupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LootPrice)).EndInit();
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
        internal System.Windows.Forms.GroupBox GroupBox3;
        internal System.Windows.Forms.CheckBox LootOther;
        internal System.Windows.Forms.CheckBox LootConsumable;
        internal System.Windows.Forms.Button Reset;
        internal System.Windows.Forms.TextBox SearchLootableItemList;
        internal System.Windows.Forms.ListBox ListBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown LootPrice;
        private System.Windows.Forms.Label Price;
    }
}