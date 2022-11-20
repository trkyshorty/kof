using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using KOF.Core;
using KOF.Common;
using KOF.Models;

namespace KOF.UI
{
    public partial class LootableItem : Form
    {
        private Client _Client;

        public LootableItem(Client Client)
        {
            _Client = Client;

            InitializeComponent();
        }

        private void LootableItem_Load(object sender, EventArgs e)
        {
            TopMost = true;

            List<Inventory> InventoryList = new List<Inventory>();

            if (_Client.GetAllInventoryItem(ref InventoryList) > 0)
            {
                ListBox1.DataSource = new BindingSource(InventoryList, null);
                ListBox1.ValueMember = "Id";
                ListBox1.DisplayMember = "Name";
            }

            if (_Client.GetLootSize() > 0)
            {
                ListBox2.DataSource = new BindingSource(_Client.GetLootList(), null);
                ListBox2.ValueMember = "ItemId";
                ListBox2.DisplayMember = "ItemName";
            }
        }

        private void LootableItem_Closing(object sender, CancelEventArgs e)
        {
            Visible = false;
            e.Cancel = true;
        }

        public void InitializeControl()
        {
            System.Windows.Forms.Control Control = GetNextControl(this, true);

            do
            {
                Control = GetNextControl(Control, true);

                if (Control != null)
                {
                    if (Control.GetType() == typeof(CheckBox))
                    {
                        CheckBox CheckBox = ((CheckBox)Control);
                        bool Value = Convert.ToBoolean(_Client.GetControl(CheckBox.Name, CheckBox.Checked.ToString()));

                        if (Value != CheckBox.Checked)
                            CheckBox.Checked = Value;
                    }
                    else if (Control.GetType() == typeof(NumericUpDown))
                    {
                        NumericUpDown NumericUpDown = ((NumericUpDown)Control);
                        int Value = Convert.ToInt32(_Client.GetControl(NumericUpDown.Name, NumericUpDown.Value.ToString()));

                        if (Value != NumericUpDown.Value)
                            NumericUpDown.Value = Value;
                    }
                    else if (Control.GetType() == typeof(ComboBox))
                    {
                        ComboBox ComboBox = ((ComboBox)Control);

                        if (ComboBox.SelectedItem != null)
                        {
                            object Value = _Client.GetControl(ComboBox.Name, ComboBox.SelectedItem.ToString());

                            if (Value != ComboBox.SelectedItem)
                                ComboBox.SelectedItem = Value;
                        }
                    }
                }
            }
            while (Control != null);
        }

        private void LoadFromInventory_Click(object sender, EventArgs e)
        {
            List<Inventory> InventoryList = new List<Inventory>();

            if (_Client.GetAllInventoryItem(ref InventoryList) > 0)
            {
                ListBox1.DataSource = new BindingSource(InventoryList, null);
                ListBox1.ValueMember = "Id";
                ListBox1.DisplayMember = "Name";
            }
        }

        private void LoadFromDatabase_Click(object sender, EventArgs e)
        {
            if (Storage.ItemCollection.Count > 0)
            {
                ListBox1.DataSource = new BindingSource(Storage.ItemCollection, null);
                ListBox1.ValueMember = "Id";
                ListBox1.DisplayMember = "Name";
            }
        }

        private void SearchItemList_TextChanged(object sender, EventArgs e)
        {
            int Index = ListBox1.FindString(SearchItemList.Text);

            ListBox1.SelectedIndex = Index;

            if (SearchItemList.Text == "")
                ListBox1.SelectedIndex = -1;
        }

        private void LootConsumable_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(LootConsumable.Name, LootConsumable.Checked.ToString());
        }

        private void LootOther_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(LootOther.Name, LootOther.Checked.ToString());
        }

        private void SearchLootableItemList_TextChanged(object sender, EventArgs e)
        {
            int Index = ListBox2.FindString(SearchLootableItemList.Text);

            ListBox2.SelectedIndex = Index;

            if (SearchLootableItemList.Text == "")
                ListBox2.SelectedIndex = -1;
        }

        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (ListBox1.SelectedItem == null) return;

            if (ListBox1.SelectedItem.GetType() == typeof(Inventory))
            {
                Inventory SelectedItem = ((Inventory)ListBox1.SelectedItem);
                _Client.SetLoot(SelectedItem.Id, SelectedItem.Name);
            }
            else if (ListBox1.SelectedItem.GetType() == typeof(Item))
            {
                Item SelectedItem = ((Item)ListBox1.SelectedItem);
                _Client.SetLoot(SelectedItem.Id, SelectedItem.Name);
            }

            if (_Client.GetLootSize() > 0)
            {
                ListBox2.DataSource = new BindingSource(_Client.GetLootList(), null);
                ListBox2.ValueMember = "ItemId";
                ListBox2.DisplayMember = "ItemName";
            }
        }

        private void ListBox2_DoubleClick(object sender, EventArgs e)
        {
            if (ListBox2.SelectedItem == null) return;

            Loot SelectedLoot = ((Loot)ListBox2.SelectedItem);

            _Client.DeleteLoot(SelectedLoot.ItemId);

            if (_Client.GetLootSize() > 0)
            {
                ListBox2.DataSource = new BindingSource(_Client.GetLootList(), null);
                ListBox2.ValueMember = "ItemId";
                ListBox2.DisplayMember = "ItemName";
            }
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            _Client.ClearLoot();
            ListBox2.DataSource = new BindingSource(null, null);
        }

        private void LootPrice_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(LootPrice.Name, LootPrice.Value.ToString());
        }
    }
}
