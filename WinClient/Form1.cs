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
            var themas = TextModeling.GenerateThemas(5, words);
            var documents = TextModeling.GenerateDocuments(20, 40, 100, themas);

            // Обучение
            var result = TextModeling.EmPlsa(
                documents.Select(d => new TextModeling.Document {Words = d.Words}).ToList(), 20);

            var totalThemas = result.First().ThemaDistribution.Keys.ToList();
            var total = totalThemas.Select(t => t.WordsDistribution.OrderByDescending(pair => pair.Value)
                .Take(5).Select(w => w.Key).ToList()).ToList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Тест на нормальных данных
            var docs = TextModeling.GenerateReadableDocuments();
            var result = TextModeling.EmPlsa(docs, 2);

            var themas = result.First().ThemaDistribution.Keys.ToList();
            var total = themas.Select(t => t.WordsDistribution.OrderByDescending(pair => pair.Value)
                .Take(5).Select(w => w.Key).ToList()).ToList();
        }
    }
}
