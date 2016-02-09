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
            var clusters = StbModeling.GenerateClusters(10);
            var users = StbModeling.GenerateUsers(200, clusters);
            var features = StbModeling.GenerateFeatures(users, clusters, 50);

            var profile = StbModeling.Profile.Generate(features, clusters.Count);
            var result = StbModeling.PlsaEm(profile, maxSteps: 50);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Генерация данных
            var clusters = StbModeling.GenerateClusters(10);
            var users = StbModeling.GenerateUsers(200, clusters);
            var features = StbModeling.GenerateFeatures(users, clusters, 50);

            var profile = StbModeling.Profile.Generate(features, clusters.Count);
            var result = StbModeling.LdaEm(profile, ratePres: 1e-3, maxSteps: 50);
            var result2 = StbModeling.PlsaEm(profile, ratePres: 1e-3, maxSteps: 50);
        }
    }
}
