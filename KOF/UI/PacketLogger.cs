using System;
using System.Windows.Forms;
using KOF.Core;

namespace KOF.UI
{
    public partial class PacketLogger : Form
    {
        private Client _Client;

        public PacketLogger(Client Client)
        {
            _Client = Client;

            InitializeComponent();
        }

        private void PacketLogger_Load(object sender, EventArgs e)
        {

        }
    }
}
