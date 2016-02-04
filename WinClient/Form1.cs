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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Генерация данных
            var words = TextModeling.GenerateWords(50);
            var themas = TextModeling.GenerateThemas(20, words);
            var documents = TextModeling.GenerateDocuments(100, 40, 100, themas);

            // Обучение
            var result = TextModeling.EmPlsa(
                documents.Select(d => new TextModeling.Document {Words = d.Words}).ToList(), 20);
        }
    }
}
