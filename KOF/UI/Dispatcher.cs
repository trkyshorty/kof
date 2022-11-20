using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using KOF.Core;
using KOF.Common;
using KOF.Models;
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
#if !DEBUG
            DispatcherTabControl.TabPages.Remove(DeveloperTabPage);
#endif
        }

        public Client GetClient()
        {
            return _Client;
        }

        private void InitializeLanguage()
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
                        CheckBox.Text = _Client._App.GetString(CheckBox.Name);
                    }
                    else if (Control.GetType() == typeof(Button))
                    {
                        Button Button = ((Button)Control);
                        Button.Text = _Client._App.GetString(Button.Name);
                    }
                    else if (Control.GetType() == typeof(Label))
                    {
                        Label Label = ((Label)Control);
                        Label.Text = _Client._App.GetString(Label.Name);
                    }
                    else if (Control.GetType() == typeof(GroupBox))
                    {
                        GroupBox GroupBox = ((GroupBox)Control);
                        GroupBox.Text = _Client._App.GetString(GroupBox.Name);
                    }
                    else if (Control.GetType() == typeof(TabControl))
                    {
                        TabControl TabControl = ((TabControl)Control);
                        TabControl.Text = _Client._App.GetString(TabControl.Name);
                    }
                    else if (Control.GetType() == typeof(TabPage))
                    {
                        TabPage TabPage = ((TabPage)Control);
                        TabPage.Text = _Client._App.GetString(TabPage.Name);
                    }
                }

            }
            while (Control != null);
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
                    else if (Control.GetType() == typeof(TextBox))
                    {
                        TextBox TextBox = ((TextBox)Control);

                        string Value = _Client.GetControl(TextBox.Name, TextBox.Text);

                        if (Value != TextBox.Text)
                            TextBox.Text = Value;
                    }
                }
            }
            while (Control != null);

            List<Skill> skillCollection = _Client.GetSkillList().OrderBy(x => x.RealId).ToList();

            AttackSkillList.Items.Clear();
            AttackSkillList.ValueMember = "Id";
            AttackSkillList.DisplayMember = "Name";

            TimedSkillList.Items.Clear();
            TimedSkillList.ValueMember = "Id";
            TimedSkillList.DisplayMember = "Name";

            foreach (Skill SkillData in skillCollection)
            {
                if (SkillData.Type == 1)
                {
                    if (((SkillData.Tab == -1 && GetClient().GetLevel() >= SkillData.Point) ||
                        GetClient().GetLevel() >= 10 && GetClient().GetSkillPoint(SkillData.Tab) >= SkillData.Point) && SkillData.Listed == 1)
                    {
                        AttackSkillList.Items.Add(SkillData, _Client.GetSkillBar(SkillData.Id) != null ? true : false);
                    }
                }

                if (SkillData.Type == 2)
                {
                    if (((SkillData.Tab == -1 && GetClient().GetLevel() >= SkillData.Point) ||
                        GetClient().GetLevel() >= 10 && GetClient().GetSkillPoint(SkillData.Tab) >= SkillData.Point) && SkillData.Listed == 1)
                    {
                        TimedSkillList.Items.Add(SkillData, _Client.GetSkillBar(SkillData.Id) != null ? true : false);
                    }
                }
            }

            TargetList.Items.Clear();
            TargetList.ValueMember = "Id";
            TargetList.DisplayMember = "Name";

            List<Target> TargetListCollection = _Client.GetTargetList();

            if (TargetListCollection.Count > 0)
            {
                TargetListCollection.ForEach(x =>
                {
                    if (TargetList.Items.Contains(x.Name) == false)
                        TargetList.Items.Add(x.Name, x.Checked == 1 ? true : false);
                });
            }

            _Client.SetControl("Bot", _Client.GetControl("Bot", "false"));

            if (Convert.ToBoolean(_Client.GetControl("Bot")))
                BotStatusButton.ForeColor = Color.LimeGreen;
            else
                BotStatusButton.ForeColor = Color.Red;

            _Client.SetControl("Attack", _Client.GetControl("Attack", "false"));

            if (Convert.ToBoolean(_Client.GetControl("Attack")))
                BotStatusAttackButton.ForeColor = Color.LimeGreen;
            else
                BotStatusAttackButton.ForeColor = Color.Red;

            _Client.SetControl("TimedSkill", _Client.GetControl("TimedSkill", "true"));

            if (Convert.ToBoolean(_Client.GetControl("TimedSkill")))
                BotStatusTimedSkillButton.ForeColor = Color.LimeGreen;
            else
                BotStatusTimedSkillButton.ForeColor = Color.Red;

            _Client.SetControl("RouteSave", _Client.GetControl("RouteSave", "false"));

            if (Convert.ToBoolean(_Client.GetControl("RouteSave")))
                RouteSaveStart.ForeColor = Color.LimeGreen;
            else
                RouteSaveStart.ForeColor = Color.Red;

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
                Transformation.Enabled = true;
                TransformationName.Enabled = true;
                SupplyInnHostesGroupBox.Enabled = true;
                SupplyTsGem.Enabled = true;
                SupplyTsGemCount.Enabled = true;
                SupplyInnTsGem.Enabled = true;
                SupplyInnTsGemCount.Enabled = true;
            }

            if(GetClient().GetJob(GetClient().GetClass()) == "Rogue")
            {
                PartyRogueGroupBox.Enabled = true;
            }

            if (GetClient().GetJob(GetClient().GetClass()) == "Mage")
            {
                PartyMageGroupBox.Enabled = true;
            }

            if (GetClient().GetJob(GetClient().GetClass()) == "Priest")
            {
                PartyPriestGroupBox.Enabled = true;
            }

            if (GetClient().GetPlatform() == AddressEnum.Platform.USKO || GetClient().GetPlatform() == AddressEnum.Platform.CNKO)
            {
                CoordinateRouteButton.Enabled = true;
                Oreads.Enabled = false;
                SpeedHack.Enabled = false;
            }

            List<Route> RouteList = _Client.Database().GetRouteList(_Client.GetZoneId());

            if (RouteList.Count() != RouteListBox.Items.Count)
            {
                if (RouteList.Count() > 0)
                {
                    RouteListBox.DataSource = new BindingSource(RouteList, null);
                    RouteListBox.ValueMember = "Id";
                    RouteListBox.DisplayMember = "Name";

                    SupplyRoute.DataSource = new BindingSource(RouteList, null);
                    SupplyRoute.ValueMember = "Id";
                    SupplyRoute.DisplayMember = "Name";
                }
            }

            InitializeLanguage();

            TopMost = AlwaysOnTop.Checked;
        }

        private void Dispatcher_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void Dispatcher_Load(object sender, EventArgs e)
        {
            Text = _Client.GetName();
        }

        private void Dispatcher_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                Timer1.Enabled = true;
                InitializeControl();
            }
            else
                Timer1.Enabled = false;
        }

        private void Dispatcher_Closing(object sender, CancelEventArgs e)
        {
            Visible = false;
            e.Cancel = true;
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
            Skill selectedAttackSkill = (Skill)AttackSkillList.Items[e.Index];

            if (e.NewValue == CheckState.Checked)
                _Client.SetSkillBar(selectedAttackSkill.Id, 1);
            else
                _Client.DeleteSkillBar(selectedAttackSkill.Id);
        }

        private void TimedSkillList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Skill selectedTimedSkill = (Skill)TimedSkillList.Items[e.Index];

            if (e.NewValue == CheckState.Checked)
                _Client.SetSkillBar(selectedTimedSkill.Id, 2);
            else
                _Client.DeleteSkillBar(selectedTimedSkill.Id);
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

            if (AttackOnSetAreaControl.Checked && Convert.ToBoolean(_Client.GetControl("Attack")))
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
            List<TargetInfo> SearchTargetList = new List<TargetInfo>();

            if (_Client.SearchMob(ref SearchTargetList) > 0)
            {
                SearchTargetList.ForEach(x =>
                {
                    Target Target = _Client.GetTargetList(x.Name);

                    if(Target != null)
                        _Client.SetTargetList(x.Name, Target.Checked);
                    else
                        _Client.SetTargetList(x.Name, 0);

                    if (TargetList.Items.Contains(x.Name) == false)
                        TargetList.Items.Add(x.Name);
                });
            }
        }

        private void SearchPlayer_Click(object sender, EventArgs e)
        {
            List<TargetInfo> SearchTargetList = new List<TargetInfo>();
            if (_Client.SearchPlayer(ref SearchTargetList) > 0)
            {
                SearchTargetList.ForEach(x =>
                {
                    Target Target = _Client.GetTargetList(x.Name);

                    if (Target != null)
                        _Client.SetTargetList(x.Name, Target.Checked);
                    else
                        _Client.SetTargetList(x.Name, 0);

                    if (TargetList.Items.Contains(x.Name) == false)
                        TargetList.Items.Add(x.Name);
                });
            }
        }

        private void ClearTargetList_Click(object sender, EventArgs e)
        {
            TargetList.Items.Clear();
            _Client.ClearTargetList();
        }

        private void TargetOpponentNation_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(TargetOpponentNation.Name, TargetOpponentNation.Checked.ToString());
        }

        private void TargetList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string TargetName = TargetList.Items[e.Index].ToString();

            if (e.NewValue == CheckState.Checked)
                _Client.SetTargetList(TargetName, 1);
            else
                _Client.SetTargetList(TargetName, 0);
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
            _LootableItem.Show();
        }

        private void SellableItem_Click(object sender, EventArgs e)
        {
            _SellableItem.Show();
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

        private void PartyCureDisease_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartyCureDisease.Name, PartyCureDisease.Checked.ToString());
        }

        private void StartUpgrade_Click(object sender, EventArgs e)
        {
            _Client.UpgradeEvent();
        }

        private void TakeCswStone_Click(object sender, EventArgs e)
        {
            _Client.SendPacket("8501CC38");
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

        private void PacketLoggerButton_Click(object sender, EventArgs e)
        {
            _PacketLogger.Show();
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
        }

        private void RouteTestStart_Click(object sender, EventArgs e)
        {
           // _Client.Test();
        }

        private void RouteTestStop_Click(object sender, EventArgs e)
        {
            _Client.StopRouteEvent();
        }

        private void AddInnHostes_Click(object sender, EventArgs e)
        {
            if (_Client.GetTargetId() < 9999) return;

            RouteData routeData = new RouteData();

            routeData.Action = RouteData.Event.INN;
            routeData.Npc = _Client.GetTargetId();
            routeData.X = _Client.GetTargetX();
            routeData.Y = _Client.GetTargetY();
        }

        private void AddMiner_Click(object sender, EventArgs e)
        {
            if (_Client.GetTargetId() < 9999) return;

            RouteData routeData = new RouteData();

            routeData.Action = RouteData.Event.MINER;
            routeData.Npc = _Client.GetTargetId();
            routeData.X = _Client.GetTargetX();
            routeData.Y = _Client.GetTargetY();
        }

        private void CoordinateRouteButton_Click(object sender, EventArgs e)
        {
            _Client.StartRouteEvent(Convert.ToInt32(ToolCoordinateX.Value), Convert.ToInt32((ToolCoordinateY.Value)));
        }

        private void AlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(AlwaysOnTop.Name, AlwaysOnTop.Checked.ToString());
            TopMost = AlwaysOnTop.Checked;
        }

        private void AddSelected_Click(object sender, EventArgs e)
        {
            string TargetName = _Client.GetTargetName();

            if (TargetName == "") return;

            _Client.SetTargetList(TargetName, 0);

            if (TargetList.Items.Contains(TargetName) == false)
                TargetList.Items.Add(TargetName);
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

            List<TargetInfo> TargetList = new List<TargetInfo>();

            GetClient().SearchMob(ref TargetList);

            if (TargetList.Count > 0)
            {
                GetClient().SearchPlayer(ref TargetList);

                TargetList.ForEach(x =>
                {
                    Position TargetMapPosition = GetClient().GetWorldPositionToMinimap(MiniMap, GetClient().GetZone(), x.X, x.Y);

                    if (GetClient().GetNation() == x.Nation || x.Nation > 2)
                        Graphic.FillRectangle(NeutralColor, TargetMapPosition.X, TargetMapPosition.Y, 4, 4);
                    else if (GetClient().GetNation() == 0)
                        Graphic.FillRectangle(TargetColor, TargetMapPosition.X, TargetMapPosition.Y, 4, 4);
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

            if(GetClient().GetTargetId() > 0)
            {
                Position TargetPosition = GetClient().GetWorldPositionToMinimap(MiniMap, GetClient().GetZone(), GetClient().GetTargetX(), GetClient().GetTargetY());

                Graphic.DrawLine(new Pen(TargetColor), CharacterMapPosition.X, CharacterMapPosition.Y, TargetPosition.X, TargetPosition.Y);

                Graphic.FillEllipse(new SolidBrush(Color.FromArgb(75, 255, 0, 0)), TargetPosition.X - (Radius - 1), TargetPosition.Y - (Radius - 1),
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

        private void BotStatusAttackButton_Click(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(_Client.GetControl("Attack")))
            {
                _Client.SetControl("Attack", "false");
                BotStatusAttackButton.ForeColor = Color.Red;
            }
            else
            {
                _Client.SetControl("Attack", "true");
                BotStatusAttackButton.ForeColor = Color.LimeGreen;
            }

            if (AttackOnSetAreaControl.Checked)
            {
                if (Convert.ToBoolean(_Client.GetControl("Attack")))
                    AreaControl.Checked = true;
                else
                    AreaControl.Checked = false;
            }
        }

        private void BotStatusTimedSkillButton_Click(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(_Client.GetControl("TimedSkill")))
            {
                _Client.SetControl("TimedSkill", "false");
                BotStatusTimedSkillButton.ForeColor = Color.Red;
            } 
            else
            {
                _Client.SetControl("TimedSkill", "true");
                BotStatusTimedSkillButton.ForeColor = Color.LimeGreen;
            }
        }

        private void RefreshBotData_Click(object sender, EventArgs e)
        {
            _Client.ReloadCollection();
            InitializeControl();
        }

        private void StartSupplyEvent_Click(object sender, EventArgs e)
        {
            var SelectedRoute = (Route)SupplyRoute.SelectedItem;

            if (SelectedRoute == null)
            {
                MessageBox.Show("Tedarik sayfasından rota seçilmelidir.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _Client.Route(SelectedRoute);
        }

        private void TownButton_Click(object sender, EventArgs e)
        {
            _Client.SendPacket("4800");
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (_Client.HasExited() && Visible)
                Close();
            else
            {
                if (_Client.GetAction() == Processor.EAction.Routing)
                    StartSupplyEvent.ForeColor = Color.LimeGreen;
                else
                    StartSupplyEvent.ForeColor = Color.Black;

                if(_Client.GetPartyCount() > 0)
                {
                    List<Player> PartyListCollection = new List<Player>();

                    if (_Client.GetPartyList(ref PartyListCollection) > 0)
                    {
                        listParty.Items.Clear();

                        for (int i = 0; i < PartyListCollection.Count; i++)
                        {
                            Player Party = PartyListCollection[i];

                            string[] Row = { "", Party.Name, _Client.GetJob(Party.Class), Party.Hp + " / " + Party.MaxHp };

                            ListViewItem Item = new ListViewItem(Row);

                            if(_Client.GetPartyAllowed(Party.Id))
                                Item.Checked = true;
                            else
                                Item.Checked = false;

                            Item.Text = Party.Id.ToString();

                            listParty.Columns[0].Width = 23;
                            listParty.Items.Add(Item);
                        }

                    }
                    else
                        listParty.Items.Clear();
                }

                List<Route> RouteList = _Client.Database().GetRouteList(_Client.GetZoneId());

                if(RouteList.Count() != RouteListBox.Items.Count)
                {
                    if (RouteList.Count() > 0)
                    {
                        RouteListBox.DataSource = new BindingSource(RouteList, null);
                        RouteListBox.ValueMember = "Id";
                        RouteListBox.DisplayMember = "Name";

                        SupplyRoute.DataSource = new BindingSource(RouteList, null);
                        SupplyRoute.ValueMember = "Id";
                        SupplyRoute.DisplayMember = "Name";
                    }
                    else
                        RouteListBox.DataSource = null;
                }

            }
        }

        private void MoveToLoot_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(MoveToLoot.Name, MoveToLoot.Checked.ToString());
        }

        private void BotStatusButton_Click(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(_Client.GetControl("Bot")))
            {
                _Client.SetControl("Bot", "false");
                _Client.SetAction(Processor.EAction.None);
                BotStatusButton.ForeColor = Color.Red;
            }
            else
            {
                _Client.SetControl("Bot", "true");
                BotStatusButton.ForeColor = Color.LimeGreen;
            }
        }

        private void PressOkButton_Click(object sender, EventArgs e)
        {
            if (_Client.GetHp() == 0)
                _Client.SendPacket("1200");
        }

        private void listParty_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var SelectedAttackSkill = listParty.Items[e.Index];

            if (e.NewValue == CheckState.Checked)
                _Client.AddPartyAllowed(int.Parse(SelectedAttackSkill.Text));
            else
                _Client.RemovePartyAllowed(int.Parse(SelectedAttackSkill.Text));
        }

        private void PartySwift_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(PartySwift.Name, PartySwift.Checked.ToString());
        }

        private void RouteSaveStart_Click(object sender, EventArgs e)
        {
            if (RouteName.Text == "")
            {
                MessageBox.Show("Rota adı girilmek zorundadır.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Convert.ToBoolean(_Client.GetControl("RouteSave")))
            {
                _Client.SetControl("RouteSave", "false");
                _Client.SetControl("RouteName", RouteName.Text);
                _Client.RouteSave();
                RouteName.Text = "";
                RouteSaveStart.ForeColor = Color.Red;
            }
            else
            {
                _Client.SetControl("RouteSave", "true");
                RouteSaveStart.ForeColor = Color.LimeGreen;
            }
        }

        private void RouteTestButton_Click(object sender, EventArgs e)
        {
            var SelectedRoute = (Route)RouteListBox.SelectedItem;

            if (SelectedRoute == null)
                return;

            if (RouteTestButton.ForeColor == Color.Red)
                RouteTestButton.ForeColor = Color.LimeGreen;
            else
                RouteTestButton.ForeColor = Color.Red;

            _Client.Route(SelectedRoute);
        }

        private void RouteDeleteButton_Click(object sender, EventArgs e)
        {
            var SelectedRoute = (Route)RouteListBox.SelectedItem;

            if (SelectedRoute == null)
                return;

            _Client.Database().ClearRoute(SelectedRoute);
        }

        private void RouteAddSunderies_Click(object sender, EventArgs e)
        {
            if (_Client.GetTargetId() < 9999) return;

            RouteData routeData = new RouteData();

            routeData.Action = RouteData.Event.SUNDERIES;
            routeData.Npc = _Client.GetTargetId();
            routeData.X = _Client.GetTargetX();
            routeData.Y = _Client.GetTargetY();

            _Client.RouteSetAction(routeData);
        }

        private void RouteAddPotion_Click(object sender, EventArgs e)
        {
            if (_Client.GetTargetId() < 9999) return;

            RouteData routeData = new RouteData();

            routeData.Action = RouteData.Event.POTION;
            routeData.Npc = _Client.GetTargetId();
            routeData.X = _Client.GetTargetX();
            routeData.Y = _Client.GetTargetY();

            _Client.RouteSetAction(routeData);
        }

        private void SupplyRoute_SelectedIndexChanged(object sender, EventArgs e)
        {
            var SelectedSupplyRoute = (Route)SupplyRoute.SelectedItem;

            if (SelectedSupplyRoute == null)
                return;

            _Client.SetControl(SupplyRoute.Name, SelectedSupplyRoute.Id.ToString());
        }

        private void StoreItemButton_Click(object sender, EventArgs e)
        {
        }

        private void CommandPartyRequestValue_TextChanged(object sender, EventArgs e)
        {
            _Client.SetControl(CommandPartyRequestValue.Name, CommandPartyRequestValue.Text.ToString());
        }

        private void CommandBuffValue_TextChanged(object sender, EventArgs e)
        {
            _Client.SetControl(CommandBuffValue.Name, CommandBuffValue.Text.ToString());
        }

        private void CommandSwiftValue_TextChanged(object sender, EventArgs e)
        {
            _Client.SetControl(CommandSwiftValue.Name, CommandSwiftValue.Text.ToString());
        }

        private void CommandTeleportValue_TextChanged(object sender, EventArgs e)
        {
            _Client.SetControl(CommandTeleportValue.Name, CommandTeleportValue.Text.ToString());
        }

        private void CommandPartyRequest_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(CommandPartyRequest.Name, CommandPartyRequest.Checked.ToString());
        }

        private void CommandBuff_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(CommandBuff.Name, CommandBuff.Checked.ToString());
        }

        private void CommandSwift_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(CommandSwift.Name, CommandSwift.Checked.ToString());
        }

        private void CommandTeleport_CheckedChanged(object sender, EventArgs e)
        {
            _Client.SetControl(CommandTeleport.Name, CommandTeleport.Checked.ToString());
        }

    }
}
