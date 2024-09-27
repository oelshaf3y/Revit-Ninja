using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Revit_Ninja.Reinforcement.FindRFT
{
    public partial class FindRFTForm : Form
    {
        int result = 0;
        List<string> partitions = new List<string>();
        public FindRFTForm(List<string> partitions)
        {
            InitializeComponent();
            this.partitions = partitions;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                MessageBox.Show("You have to enter a rebar number to search for!!");
                return;
            }
            if (int.TryParse(textBox1.Text.Trim(), out result))
            {

                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("You should enter only numbers!!");
                return;
            }
        }

        private void FindRFTForm_Load(object sender, EventArgs e)
        {
            this.comboBox1.Items.AddRange(partitions.ToArray());
        }
    }
}
