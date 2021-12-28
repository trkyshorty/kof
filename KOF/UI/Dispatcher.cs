using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using KOF.Core;
using KOF.Common;
using KOF.Models;
using System.Diagnostics;
using System.Drawing;

namespace KOF.UI
{
    public partial class Dispatcher : Form
    {
        private Client _Client;
        private LootableItem _LootableItem;
        private SellableItem _SellableItem;
        private PacketLogger _PacketLogger;

        public Dispatcher(App App)
        {
            _Client = new Client(App, this);

            _LootableItem = new LootableItem(_Client);
            _SellableItem = new SellableItem(_Client);
            _PacketLogger = new PacketLogger(_Client);

            InitializeComponent();

            HpPotionItem.SelectedIndex = 0;
            MpPotionItem.SelectedIndex = 0;
            TransformationName.SelectedIndex = 0;
            SupplyHpPotionItem.SelectedIndex = 0;
            SupplyMpPotionItem.SelectedIndex = 0;
            SupplyMasterStoneItem.SelectedIndex = 0;
            SupplyInnHpPotionItem.SelectedIndex = 0;
            SupplyInnMpPotionItem.SelectedIndex = 0;
            SupplyInnMasterStoneItem.SelectedIndex = 0;
            PartyBuffSelect.SelectedIndex = 0;
            PartyAcSelect.SelectedIndex = 0;
            PartyMindSelect.SelectedIndex = 0;
            PartyHealSelect.SelectedIndex = 0;
            UpgradeScroll.SelectedIndex = 0;

#if !DEBUG
            DispatcherTabControl.TabPages.Remove(DeveloperTabPage);
#endif
        }

        public Client GetClient()
        {
            return _Client;
        }

        public void InitializeControl()
        {
            if (Storage.SkillCollection.ContainsKey(Text))
            {
                List<Skill> SkillCollection = Storage.SkillCollection[Text].OrderBy(x => x.RealId).ToList();

                var AttackSkillCollection = new Dictionary<int, string>();
                var TimedSkillCollection = new Dictionary<int, string>();

                foreach (Skill SkillData in SkillCollection)
                {
                    if (SkillData.Type == 1)
                    {
                        if (((SkillData.Tab == -1 && GetClient().GetLevel() >= SkillData.Point) ||
                            GetClient().GetSkillPoint(SkillData.Tab) >= SkillData.Point) && SkillData.Listed == 1)
                            AttackSkillCollection.Add(SkillData.Id, SkillData.Name);
                    }

                    if (SkillData.Type == 2)
                        if (((SkillData.Tab == -1 && GetClient().GetLevel() >= SkillData.Point) ||
                            GetClient().GetSkillPoint(SkillData.Tab) >= SkillData.Point) && SkillData.Listed == 1)
                            TimedSkillCollection.Add(SkillData.Id, SkillData.Name);
                }

                if (AttackSkillCollection.Count > 0)
                {
                    AttackSkillList.DataSource = new BindingSource(AttackSkillCollection, null);
                    AttackSkillList.ValueMember = "Key";
                    AttackSkillList.DisplayMember = "Value";

                    for (int i = 0; i < AttackSkillList.Items.Count; i++)
                    {
                        var SelectedAttackSkill = (KeyValuePair<int, string>)AttackSkillList.Items[i];

                        if (_Client.GetSkillBar(SelectedAttackSkill.Key) != null)
                            AttackSkillList.SetItemChecked(i, true);
                        else
                            AttackSkillList.SetItemChecked(i, false);
                    }
                }

                if (TimedSkillCollection.Count > 0)
                {
                    TimedSkillList.DataSource = new BindingSource(TimedSkillCollection, null);
                    TimedSkillList.ValueMember = "Key";
                    TimedSkillList.DisplayMember = "Value";

                    for (int i = 0; i < TimedSkillList.Items.Count; i++)
                    {
                        var SelectedTimedSkill = (KeyValuePair<int, string>)TimedSkillList.Items[i];

                        if (_Client.GetSkillBar(SelectedTimedSkill.Key) != null)
                            TimedSkillList.SetItemChecked(i, true);
                        else
                            TimedSkillList.SetItemChecked(i, false);
                    }
                }
            }

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

            _LootableItem.InitializeControl();
            _SellableItem.InitializeControl();

            if (GetClient().GetPlatform() == AddressEnum.Platform.JPKO)
            {
                AreaHeal.Enabled = true;
                SpeedHack.Enabled = true;
                DropScroll.Enabled = true;
                StatScroll.Enabled = true;
                AcScroll.Enabled = true;
                AttackScroll.Enabled = true;
                ActionSetCoordinate.Enabled = true;
                Oreads.Enabled = true;
                CoordinateSetButton.Enabled = true;
                FolkGroupBox.Enabled = true;
                Transformation.Enabled = true;
                TransformationName.Enabled = true;
                SupplyInnHostesGroupBox.Enabled = true;
            }

            if (GetClient().GetPlatform() == AddressEnum.Platform.USKO || GetClient().GetPlatform() == AddressEnum.Platform.CNKO)
            {
                CoordinateRouteButton.Enabled = true;
                ActionRoute.Enabled = true;
            }
        }

        private void Dispatcher_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void Dispatcher_Load(object sender, EventArgs e)
        {
            Text = _Client.GetNameConst();
        }

        private void Dispatcher_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
                InitializeControl();
        }

        private void Dispatcher_Closing(object sender, CancelEventArgs e)
        {
            Visible = false;
            e.Cancel = true;

            _Client.Save();
        }

        private void HpPotion_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(HpPotion.Name, HpPotion.Checked.ToString());
        }

        private void MpPotion_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(MpPotion.Name, MpPotion.Checked.ToString());
        }

        private void HpPotionPercent_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(HpPotionPercent.Name, HpPotionPercent.Value.ToString());
        }

        private void MpPotionPercent_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(MpPotionPercent.Name, MpPotionPercent.Value.ToString());
        }

        private void HpPotionItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(HpPotionItem.Name, HpPotionItem.SelectedItem.ToString());
        }

        private void MpPotionItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(MpPotionItem.Name, MpPotionItem.SelectedItem.ToString());
        }

        private void Transformation_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(Transformation.Name, Transformation.Checked.ToString());
        }

        private void TransformationName_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(TransformationName.Name, TransformationName.SelectedItem.ToString());
        }

        private void Wallhack_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(Wallhack.Name, Wallhack.Checked.ToString());
        }

        private void FollowDisable_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(FollowDisable.Name, FollowDisable.Checked.ToString());
        }

        private void Oreads_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(Oreads.Name, Oreads.Checked.ToString());
        }

        private void SpeedHack_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SpeedHack.Name, SpeedHack.Checked.ToString());
        }

        private void AreaHeal_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(AreaHeal.Name, AreaHeal.Checked.ToString());
        }

        private void DeathOnBorn_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(DeathOnBorn.Name, DeathOnBorn.Checked.ToString());
        }

        private void DropScroll_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(DropScroll.Name, DropScroll.Checked.ToString());
        }

        private void StatScroll_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(StatScroll.Name, StatScroll.Checked.ToString());
        }

        private void AcScroll_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(AcScroll.Name, AcScroll.Checked.ToString());
        }

        private void AttackScroll_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(AttackScroll.Name, AttackScroll.Checked.ToString());
        }

        private void RepairSunderies_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(RepairSunderies.Name, RepairSunderies.Checked.ToString());
        }

        private void RepairMagicHammer_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(RepairMagicHammer.Name, RepairMagicHammer.Checked.ToString());
        }

        private void OnlyNoah_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(OnlyNoah.Name, OnlyNoah.Checked.ToString());
        }

        private void SellInventoryFull_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SellInventoryFull.Name, SellInventoryFull.Checked.ToString());
        }

        private void LootOnlyList_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(LootOnlyList.Name, LootOnlyList.Checked.ToString());
        }

        private void LootOnlySell_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(LootOnlySell.Name, LootOnlySell.Checked.ToString());
        }

        private void Minor_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(Minor.Name, Minor.Checked.ToString());
        }

        private void MinorPercent_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(MinorPercent.Name, MinorPercent.Value.ToString());
        }

        private void AttackSkillList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var SelectedAttackSkill = (KeyValuePair<int, string>)AttackSkillList.Items[e.Index];

            if (e.NewValue == CheckState.Checked)
                _Client.SetSkillBar(SelectedAttackSkill.Key, 1);
            else
                _Client.DeleteSkillBar(SelectedAttackSkill.Key);
        }

        private void TimedSkillList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var SelectedTimedSkill = (KeyValuePair<int, string>)TimedSkillList.Items[e.Index];

            if (e.NewValue == CheckState.Checked)
                _Client.SetSkillBar(SelectedTimedSkill.Key, 2);
            else
                _Client.DeleteSkillBar(SelectedTimedSkill.Key);
        }
        private void Attack_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(Attack.Name, Attack.Checked.ToString());

            if (AttackOnSetAreaControl.Checked)
            {
                if (Attack.Checked)
                    AreaControl.Checked = true;
                else
                    AreaControl.Checked = false;
            }
        }

        private void RAttack_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(RAttack.Name, RAttack.Checked.ToString());
        }

        private void AttackSpeed_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(AttackSpeed.Name, AttackSpeed.Value.ToString());
        }

        private void AttackDistance_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(AttackDistance.Name, AttackDistance.Value.ToString());
        }

        private void WaitTime_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(WaitTime.Name, WaitTime.Checked.ToString());
        }

        private void TargetAutoSelect_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(TargetAutoSelect.Name, TargetAutoSelect.Checked.ToString());
        }

        private void ActionMove_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(ActionMove.Name, ActionMove.Checked.ToString());
        }

        private void ActionSetCoordinate_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(ActionSetCoordinate.Name, ActionSetCoordinate.Checked.ToString());
        }

        private void TargetWaitDown_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(TargetWaitDown.Name, TargetWaitDown.Checked.ToString());
        }

        private void AreaControl_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(AreaControl.Name, AreaControl.Checked.ToString());

            if (AttackOnSetAreaControl.Checked && Attack.Checked)
            {
                AreaControlX.Value = _Client.GetX();
                AreaControlY.Value = _Client.GetY();
            }
        }

        private void AreaControlX_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(AreaControlX.Name, AreaControlX.Value.ToString());
        }

        private void AreaControlY_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(AreaControlY.Name, AreaControlY.Value.ToString());
        }

        private void SetAreaControl_Click(object sender, EventArgs e)
        {
            AreaControlX.Value = _Client.GetX();
            AreaControlY.Value = _Client.GetY();
        }

        private void SearchMob_Click(object sender, EventArgs e)
        {
            List<Target> SearchTargetList = new List<Target>();
            if (_Client.SearchMob(ref SearchTargetList) > 0)
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
            List<Target> SearchTargetList = new List<Target>();
            if (_Client.SearchPlayer(ref SearchTargetList) > 0)
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
            TargetList.Items.Clear();
            _Client.ClearTargetAllowed();
        }

        private void TargetOpponentNation_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(TargetOpponentNation.Name, TargetOpponentNation.Checked.ToString());
        }

        private void TargetList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string SelectedTargetList = TargetList.Items[e.Index].ToString();

            if (e.NewValue == CheckState.Checked)
                _Client.AddTargetAllowed(SelectedTargetList);
            else
                _Client.RemoveTargetAllowed(SelectedTargetList);
        }

        private void SupplyHpPotion_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyHpPotion.Name, SupplyHpPotion.Checked.ToString());
        }

        private void SupplyHpPotionCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyHpPotionCount.Name, SupplyHpPotionCount.Value.ToString());
        }

        private void SupplyHpPotionItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyHpPotionItem.Name, SupplyHpPotionItem.SelectedItem.ToString());
        }

        private void SupplyMpPotion_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyMpPotion.Name, SupplyMpPotion.Checked.ToString());
        }

        private void SupplyMpPotionCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyMpPotionCount.Name, SupplyMpPotionCount.Value.ToString());
        }

        private void SupplyMpPotionItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyMpPotionItem.Name, SupplyMpPotionItem.SelectedItem.ToString());
        }

        private void SupplyArrow_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyArrow.Name, SupplyArrow.Checked.ToString());
        }

        private void SupplyArrowCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyArrowCount.Name, SupplyArrowCount.Value.ToString());
        }

        private void SupplyWolf_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyWolf.Name, SupplyWolf.Checked.ToString());
        }

        private void SupplyWolfCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyWolfCount.Name, SupplyWolfCount.Value.ToString());
        }

        private void SupplyTsGem_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyTsGem.Name, SupplyTsGem.Checked.ToString());
        }

        private void SupplyTsGemCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyTsGemCount.Name, SupplyTsGemCount.Value.ToString());
        }

        private void SupplyBook_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyBook.Name, SupplyBook.Checked.ToString());
        }

        private void SupplyBookCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyBookCount.Name, SupplyBookCount.Value.ToString());
        }

        private void SupplyMasterStone_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyMasterStone.Name, SupplyMasterStone.Checked.ToString());
        }

        private void SupplyMasterStoneCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyMasterStoneCount.Name, SupplyMasterStoneCount.Value.ToString());
        }

        private void SupplyInnHpPotion_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnHpPotion.Name, SupplyInnHpPotion.Checked.ToString());
        }

        private void SupplyInnHpPotionCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnHpPotionCount.Name, SupplyInnHpPotionCount.Value.ToString());
        }

        private void SupplyInnHpPotionItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnHpPotionItem.Name, SupplyInnHpPotionItem.SelectedItem.ToString());
        }

        private void SupplyInnMpPotion_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnMpPotion.Name, SupplyInnMpPotion.Checked.ToString());
        }

        private void SupplyInnMpPotionCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnMpPotionCount.Name, SupplyInnMpPotionCount.Value.ToString());
        }

        private void SupplyInnMpPotionItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnMpPotionItem.Name, SupplyInnMpPotionItem.SelectedItem.ToString());
        }

        private void SupplyInnArrow_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnArrow.Name, SupplyInnArrow.Checked.ToString());
        }

        private void SupplyInnArrowCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnArrowCount.Name, SupplyInnArrowCount.Value.ToString());
        }

        private void SupplyInnWolf_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnWolf.Name, SupplyInnWolf.Checked.ToString());
        }

        private void SupplyInnWolfCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnWolfCount.Name, SupplyInnWolfCount.Value.ToString());
        }

        private void SupplyInnTsGem_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnTsGem.Name, SupplyInnTsGem.Checked.ToString());
        }

        private void SupplyInnTsGemCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnTsGemCount.Name, SupplyInnTsGemCount.Value.ToString());
        }

        private void SupplyInnBook_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnBook.Name, SupplyInnBook.Checked.ToString());
        }

        private void SupplyInnBookCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnBookCount.Name, SupplyInnBookCount.Value.ToString());
        }

        private void SupplyInnMasterStone_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnMasterStone.Name, SupplyInnMasterStone.Checked.ToString());
        }

        private void SupplyInnMasterStoneCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnMasterStoneCount.Name, SupplyInnMasterStoneCount.Value.ToString());
        }

        private void SupplyMasterStoneItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyMasterStoneItem.Name, SupplyMasterStoneItem.SelectedItem.ToString());
        }

        private void SupplyInnMasterStoneItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SupplyInnMasterStoneItem.Name, SupplyInnMasterStoneItem.SelectedItem.ToString());
        }

        private void SendPacket_Click(object sender, EventArgs e)
        {
            _Client.SendPacket(Packet.Text);
        }

        private void LootableItem_Click(object sender, EventArgs e)
        {
            _LootableItem.ShowDialog();
        }

        private void SellableItem_Click(object sender, EventArgs e)
        {
            _SellableItem.ShowDialog();
        }

        private void PartyBuff_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyBuff.Name, PartyBuff.Checked.ToString());
        }

        private void PartyAc_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyAc.Name, PartyAc.Checked.ToString());
        }

        private void PartyMind_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyMind.Name, PartyMind.Checked.ToString());
        }

        private void PartyHeal_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyHeal.Name, PartyHeal.Checked.ToString());
        }

        private void PartyCure_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyCure.Name, PartyCure.Checked.ToString());
        }

        private void PartyStr_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyStr.Name, PartyStr.Checked.ToString());
        }

        private void PartyBuffSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyBuffSelect.Name, PartyBuffSelect.SelectedItem.ToString());
        }

        private void PartyAcSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyAcSelect.Name, PartyAcSelect.SelectedItem.ToString());
        }

        private void PartyMindSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyMindSelect.Name, PartyMindSelect.SelectedItem.ToString());
        }

        private void PartyHealValue_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyHealValue.Name, PartyHealValue.Value.ToString());
        }

        private void PartyHealSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyHealSelect.Name, PartyHealSelect.SelectedItem.ToString());
        }

        private void Suicide_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(Suicide.Name, Suicide.Checked.ToString());
        }

        private void SuicidePercent_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(SuicidePercent.Name, SuicidePercent.Value.ToString());
        }

        private void CoordinateSetButton_Click(object sender, EventArgs e)
        {
            if (ToolCoordinateX.Value == 0 || ToolCoordinateY.Value == 0) return;
            _Client.SetCoordinate(Convert.ToInt32(ToolCoordinateX.Value), Convert.ToInt32((ToolCoordinateY.Value)));
        }

        private void CoordinateMoveButton_Click(object sender, EventArgs e)
        {
            _Client.MoveCoordinate(Convert.ToInt32(ToolCoordinateX.Value), Convert.ToInt32((ToolCoordinateY.Value)));
        }

        private void GetCoordinateButton_Click(object sender, EventArgs e)
        {
            ToolCoordinateX.Value = _Client.GetX();
            ToolCoordinateY.Value = _Client.GetY();
        }

        private void MoveCswStone_Click(object sender, EventArgs e)
        {
            _Client.SetCoordinate(505, 863);
        }

        private void PartyMinor_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyMinor.Name, PartyMinor.Checked.ToString());
        }

        private void PartyMinorPercent_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyMinorPercent.Name, PartyMinorPercent.Value.ToString());
        }

        private void PullAway_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PullAway.Name, PullAway.Checked.ToString());
        }

        private void LightningResist_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(LightningResist.Name, LightningResist.Checked.ToString());
        }

        private void FlameResist_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(FlameResist.Name, FlameResist.Checked.ToString());
        }

        private void GlacierResist_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(GlacierResist.Name, GlacierResist.Checked.ToString());
        }

        private void RefreshPartyList_Click(object sender, EventArgs e)
        {
            List<Party> PartyListCollection = new List<Party>();

            if (_Client.GetPartyList(ref PartyListCollection) > 0)
            {
                PartyList.DataSource = new BindingSource(PartyListCollection, null);
                PartyList.ValueMember = "MemberId";
                PartyList.DisplayMember = "MemberName";
            }
            else
                PartyList.DataSource = null;
        }

        private void ClearPartyList_Click(object sender, EventArgs e)
        {
            PartyList.DataSource = null;
            _Client.ClearPartyAllowed();
        }

        private void PartyList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Party SelectedPartyList = (Party)PartyList.Items[e.Index];

            if (e.NewValue == CheckState.Checked)
                _Client.AddPartyAllowed(SelectedPartyList.MemberName);
            else
                _Client.RemovePartyAllowed(SelectedPartyList.MemberName);
        }

        private void PartyCureDisease_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyCureDisease.Name, PartyCureDisease.Checked.ToString());
        }

        private void BlackMarketer_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(BlackMarketer.Name, BlackMarketer.Checked.ToString());
        }

        private void BlackMarketerEventTime_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(BlackMarketerEventTime.Name, BlackMarketerEventTime.Value.ToString());
        }

        private void UpgradeScroll_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Client.SetControl(UpgradeScroll.Name, UpgradeScroll.SelectedItem.ToString());
        }

        private void UpgradeWait_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(UpgradeWait.Name, UpgradeWait.Value.ToString());
        }

        private void StartUpgrade_Click(object sender, EventArgs e)
        {
            _Client.UpgradeEvent();
        }

        private void TakeCswStone_Click(object sender, EventArgs e)
        {
            _Client.SendPacket("8501CC38");
        }

        private void RemoveAllMob_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(RemoveAllMob.Name, RemoveAllMob.Checked.ToString());
        }

        private void MiningEnable_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(MiningEnable.Name, MiningEnable.Checked.ToString());
        }

        private void MiningFullExchange_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(MiningFullExchange.Name, MiningFullExchange.Checked.ToString());
        }
        private void MiningRemoveTrashItem_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(MiningRemoveTrashItem.Name, MiningRemoveTrashItem.Checked.ToString());
        }

        private void GoldenMattock_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(GoldenMattock.Name, GoldenMattock.Checked.ToString());
        }

        private void MonsterStoneEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (MonsterStoneEnable.Checked)
                FollowDisable.Checked = true;

            _Client.SetControl(MonsterStoneEnable.Name, MonsterStoneEnable.Checked.ToString());
        }

        private void PacketLoggerButton_Click(object sender, EventArgs e)
        {
            _PacketLogger.ShowDialog();
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            _Client.Test();
        }

        private void RouteTestStart_Click(object sender, EventArgs e)
        {
            _Client.StartRouteEvent(807, 460);
        }

        private void RouteTestStop_Click(object sender, EventArgs e)
        {
            _Client.StopRouteEvent();
        }

        private void AddSunderies_Click(object sender, EventArgs e)
        {
            _Client.Database().SetNpc(_Client.GetZoneId(),
                "Sunderies",
                _Client.GetTargetId(),
                _Client.GetTargetX(),
                _Client.GetTargetY(),
                NpcNationCommon.Checked ? 0 : _Client.GetNation(),
                _Client.GetPlatform().ToString(),
                NpcInTown.Checked ? 1 : 0);
        }

        private void AddInnHostes_Click(object sender, EventArgs e)
        {
            _Client.Database().SetNpc(_Client.GetZoneId(),
                "Inn",
                _Client.GetTargetId(),
                _Client.GetTargetX(),
                _Client.GetTargetY(),
                NpcNationCommon.Checked ? 0 : _Client.GetNation(),
                _Client.GetPlatform().ToString(),
                NpcInTown.Checked ? 1 : 0);
        }

        private void AddPotion_Click(object sender, EventArgs e)
        {
            _Client.Database().SetNpc(_Client.GetZoneId(),
                "Potion",
                _Client.GetTargetId(),
                _Client.GetTargetX(),
                _Client.GetTargetY(),
                NpcNationCommon.Checked ? 0 : _Client.GetNation(),
                _Client.GetPlatform().ToString(),
                NpcInTown.Checked ? 1 : 0);
        }

        private void AddMiner_Click(object sender, EventArgs e)
        {
            _Client.Database().SetNpc(_Client.GetZoneId(),
                "Miner",
                _Client.GetTargetId(),
                _Client.GetTargetX(),
                _Client.GetTargetY(),
                NpcNationCommon.Checked ? 0 : _Client.GetNation(),
                _Client.GetPlatform().ToString(),
                NpcInTown.Checked ? 1 : 0);
        }

        private void NpcNationCommon_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(NpcNationCommon.Name, NpcNationCommon.Checked.ToString());
        }

        private void NpcInTown_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(NpcInTown.Name, NpcInTown.Checked.ToString());
        }

        private void CoordinateRouteButton_Click(object sender, EventArgs e)
        {
            _Client.StartRouteEvent(Convert.ToInt32(ToolCoordinateX.Value), Convert.ToInt32((ToolCoordinateY.Value)));
        }

        private void AlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(AlwaysOnTop.Name, AlwaysOnTop.Checked.ToString());

            if (AlwaysOnTop.Checked)
                TopMost = true;
            else
                TopMost = false;
        }

        private void AddSelected_Click(object sender, EventArgs e)
        {
            Target Target = new Target();

            Target.Name = _Client.GetTargetName();

            if (Target.Name == "") return;

            Storage.TargetCollection.Add(Target);

            if (TargetList.Items.Contains(Target.Name) == false)
                TargetList.Items.Add(Target.Name);
        }

        private void ActionRoute_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(ActionRoute.Name, ActionRoute.Checked.ToString());
        }

        private void AttackOnSetAreaControl_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(AttackOnSetAreaControl.Name, AttackOnSetAreaControl.Checked.ToString());
        }

        public void RenderMiniMap()
        {
            if (Visible == false) return;
            if (GetClient() == null) return;
            Zone Zone = GetClient().GetZone();
            if (Zone == null || GetClient().GetMiniMapImage() == null) return;

            if (this.InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    ZoneXYInfo.Text = string.Format("{0} ({1}, {2})", Zone.Name, GetClient().GetX(), GetClient().GetY());
                }));
            }
            else
            {
                ZoneXYInfo.Text = string.Format("{0} ({1}, {2})", Zone.Name, GetClient().GetX(), GetClient().GetY());
            }

            Bitmap Image = new Bitmap(GetClient().GetMiniMapImage());

            Graphics Graphic = Graphics.FromImage(Image);

            Brush CharacterColor = Brushes.SpringGreen;
            Brush TargetColor = Brushes.Red;
            Brush NeutralColor = Brushes.DarkBlue;

            List<Target> TargetList = new List<Target>();
            GetClient().SearchMob(ref TargetList);

            if (TargetList.Count > 0)
            {
                GetClient().SearchPlayer(ref TargetList);

                TargetList.ForEach(x =>
                {
                    Position TargetMapPosition = GetClient().GetWorldPositionToMinimap(MiniMap, GetClient().GetZone(), x.X, x.Y);

                    if (GetClient().GetNation() == x.Nation || x.Nation == 0 || x.Nation > 2)
                        Graphic.FillRectangle(NeutralColor, TargetMapPosition.X, TargetMapPosition.Y, 4, 4);
                    else if (GetClient().GetNation() != x.Nation)
                        Graphic.FillRectangle(TargetColor, TargetMapPosition.X, TargetMapPosition.Y, 4, 4);
                });
            }

            Position CharacterMapPosition = GetClient().GetWorldPositionToMinimap(MiniMap, GetClient().GetZone(), GetClient().GetX(), GetClient().GetY());

            int Radius = Convert.ToInt32(GetClient().GetControl("AttackDistance")) / 4;

            if(GetClient().GetGoX() != 0 && GetClient().GetGoY() != 0 && GetClient().GetX() != GetClient().GetGoX() && GetClient().GetY() != GetClient().GetGoY())
            {
                Position CharacterMovePosition = GetClient().GetWorldPositionToMinimap(MiniMap, GetClient().GetZone(), GetClient().GetGoX(), GetClient().GetGoY());

                Graphic.DrawLine(new Pen(CharacterColor), CharacterMapPosition.X, CharacterMapPosition.Y, CharacterMovePosition.X, CharacterMovePosition.Y);

                Graphic.FillEllipse(new SolidBrush(Color.FromArgb(75, 0, 255, 0)), CharacterMovePosition.X - (Radius - 1), CharacterMovePosition.Y - (Radius - 1),
                    Radius * 2, Radius * 2);
            }

            Graphic.FillEllipse(new SolidBrush(Color.FromArgb(75, 0, 0, 255)), CharacterMapPosition.X - (Radius - 1), CharacterMapPosition.Y - (Radius - 1),
                Radius * 2, Radius * 2);

            Graphic.FillRectangle(CharacterColor, CharacterMapPosition.X, CharacterMapPosition.Y, 4, 4);

            MiniMap.Image = Image;
        }

        private void MiniMap_MouseDown(object sender, MouseEventArgs me)
        {
            if (GetClient().GetZone() == null || GetClient().GetMiniMapImage() == null) return;
            Position MiniMapPosition = GetClient().GetMiniMapPositionToWorld(MiniMap, GetClient().GetZone(), me.X, me.Y);

            if(GetClient().GetPlatform() == AddressEnum.Platform.USKO || GetClient().GetPlatform() == AddressEnum.Platform.CNKO)
                GetClient().StartRouteEvent(MiniMapPosition.X, MiniMapPosition.Y);
            else
                GetClient().SetCoordinate(MiniMapPosition.X, MiniMapPosition.Y);
        }

        private void PartyGroupHeal_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyGroupHeal.Name, PartyGroupHeal.Checked.ToString());
        }

        private void PartyGroupHealValue_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyGroupHealValue.Name, PartyGroupHealValue.Value.ToString());
        }

        private void PartyGroupHealMemberCount_ValueChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyGroupHealMemberCount.Name, PartyGroupHealMemberCount.Value.ToString());
        }
    }
}
