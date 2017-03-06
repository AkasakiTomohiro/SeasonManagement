using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeasonManagement
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            dataGridView7.RowHeadersVisible = false;
            dataGridView7.Rows.Add(10);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Height = 590;
            Width = 1000;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            
        }
    }
}
