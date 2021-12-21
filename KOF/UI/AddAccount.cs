using System;
using System.Windows.Forms;
using KOF.Core;

namespace KOF.UI
{
    public partial class AddAccount : Form
    {
        private App _App;

        public AddAccount(App App)
        {
            _App = App;

            InitializeComponent();

            Platform.SelectedIndex = 0;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (AccountId.Text == "" || AccountPassword.Text == "" || Path.Text == "") return;
            _App.Database().SetAccount(AccountId.Text, AccountPassword.Text, Path.Text, Platform.SelectedItem.ToString());
            Hide();
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            AccountId.Text = "";
            AccountPassword.Text = "";
            Path.Text = "";
            Platform.SelectedIndex = 0;
        }

        private void AddAccount_Load(object sender, EventArgs e)
        {
            TopMost = true;

            AccountId.Text = "";
            AccountPassword.Text = "";
            Path.Text = "";
            Platform.SelectedIndex = 0;
        }

        private void Path_Click(object sender, EventArgs e)
        {
            OpenFileDialog OF = new OpenFileDialog();
            OF.Filter = "KnightOnLine (*.exe) | *.exe";

            if (OF.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OF.FilterIndex = 0;
                OF.RestoreDirectory = true;
                Path.Text = (OF.FileName);
            }
        }
    }
}
