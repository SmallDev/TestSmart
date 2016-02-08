using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Logic;

namespace WinClient
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Генерация данных
            var clusters = StbModeling.GenerateClusters(5);
            var users = StbModeling.GenerateUsers(100, clusters);
            var features = StbModeling.GenerateFeatures(users, clusters, 100);
        }
    }
}
