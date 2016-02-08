using System;
using System.Linq;
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

            var profile = TextModeling.Profile.Generate(documents, 20);
            var result = TextModeling.PlsaEm(profile, maxSteps: 100);

            var totalThemas = result.ToModel().First().ThemaDistribution.Keys.ToList();
            var total = totalThemas.Select(t => t.WordsDistribution.OrderByDescending(pair => pair.Value)
                .Take(5).Select(w => w.Key).ToList()).ToList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Тест на нормальных данных
            var docs = TextModeling.GenerateReadableDocuments(); 
            var profile = TextModeling.Profile.Generate(docs, 2);
            var result = TextModeling.PlsaEm(profile, maxSteps: 20);

            var themas = result.ToModel().First().ThemaDistribution.Keys.ToList();
            var total = themas.Select(t => t.WordsDistribution.OrderByDescending(pair => pair.Value)
                .Take(5).Select(w => w.Key).ToList()).ToList();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Генерация данных
            var words = TextModeling.GenerateWords(50);
            var themas = TextModeling.GenerateThemas(5, words);
            var documents = TextModeling.GenerateDocuments(50, 40, 100, themas);

            // Обучение
            var profile = TextModeling.Profile.Generate(documents, 5);
            var result = TextModeling.PlsaGem(profile, maxSteps: 20);

            var totalThemas = result.ToModel().First().ThemaDistribution.Keys.ToList();
            var total = totalThemas.Select(t => t.WordsDistribution.OrderByDescending(pair => pair.Value)
                .Take(5).Select(w => w.Key).ToList()).ToList();
        }


    }
}
