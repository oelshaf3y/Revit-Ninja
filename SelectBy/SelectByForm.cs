using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Revit_Ninja.SelectBy
{
    public partial class SelectByForm : Form
    {
        public SelectByForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Please Select a Parameter");
                return;
            }
            if (textBox1.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter a value!");
                return;
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }

}
