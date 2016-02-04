using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MicrosoftResearch.Infer.Collections;
using MicrosoftResearch.Infer.Distributions;

namespace Logic
{
    public static class TextModeling
    {
        public class Word : IEquatable<Word>
        {
            public String Value { get; set; }
            public Boolean Equals(Word other)
            {
                return Value.Equals(other.Value);
            }
        }
        public class Document
        {
            public ICollection<Word> Words { get; set; }
            public IDictionary<Thema, Double> ThemaDistribution { get; set; }
        }
        public class Thema
        {
            public IDictionary<Word, Double> WordsDistribution { get; set; }            
        }

        public static IList<Word> GenerateWords(Int32 wordCount)
        {
            // Можно для каждого id сгенерировать настоящее слово
            return Enumerable.Range(1, wordCount).Select(i => new Word {Value = Convert.ToString(i)}).ToList();
        }
        public static IList<Thema> GenerateThemas(Int32 themaCount, IList<Word> words)
        {
            var probs = new Dirichlet(words.Select(w => 0.4).ToArray());
            var seachWords = Enumerable.Range(0, words.Count).ToDictionary(i => words[i], i => i);
            return Enumerable.Range(1, themaCount).Select(i =>
            {
                var sample = probs.Sample();
                return new Thema
                {
                    WordsDistribution = words.ToDictionary(word => word, word => sample[seachWords[word]])
                };
            }).ToList();
        }
        public static IList<Document> GenerateDocuments(Int32 documentCount, Int32 minLenght, Int32 maxLenght, IList<Thema> themas)
        {
            var random = new Random();
            var probs = new Dirichlet(themas.Select(w => random.NextDouble()/4).ToArray());
            var searchThema = Enumerable.Range(0, themas.Count).ToDictionary(i => themas[i], i => i);

            return Enumerable.Range(1, documentCount).Select(i =>
            {
                var themaDistribution = probs.Sample();
                var document = new Document
                {
                    ThemaDistribution = themas.ToDictionary(t => t, t => themaDistribution[searchThema[t]])
                };

                var discrete = new Discrete(themaDistribution);
                document.Words = Enumerable.Range(1, random.Next(minLenght, maxLenght)).Select(j =>
                {
                    var thema = themas[discrete.Sample()];
                    var index = new Discrete(thema.WordsDistribution.Values.ToArray()).Sample();
                    return thema.WordsDistribution.Keys.ToArray()[index];
                }).ToList();

                return document;
            }).ToList();
        }

        // Возвращает докуенты с распределениями по темам
        public static IList<Document> EmPlsa(IList<Document> documents, Int32 themaNumber)
        {
            var words = documents.SelectMany(d => d.Words).Distinct().ToList();
            var themas = Enumerable.Range(1, themaNumber).Select(i => new Thema
            {
                WordsDistribution = words.ToDictionary(w => w, w => 1.0/words.Count)
            }).ToList();
            documents.ForEach(d => d.ThemaDistribution = themas.ToDictionary(t => t, t => 1.0/themas.Count));

            for (var i = 0; i < 100; ++i)
            {
                var nwt = themas.ToDictionary(t => t, t => t.WordsDistribution.Keys.ToDictionary(w => w, w => 0.0));
                var ndt = documents.ToDictionary(d => d, d => d.ThemaDistribution.Keys.ToDictionary(t => t, t => 0.0));
                var nt = themas.ToDictionary(t => t, t => 0.0);

                foreach (var document in documents)
                    foreach (var word in document.Words.GroupBy(w=>w))
                    {
                        var z = themas.Sum(thema => thema.WordsDistribution[word.Key]*document.ThemaDistribution[thema]);
                        foreach (var thema in themas)
                        {
                            if (word.Count() > 1)
                            {
                                var f = "";
                            }
                            var delta = thema.WordsDistribution[word.Key]*document.ThemaDistribution[thema]*
                                        word.Count()/z;
                            if (delta > 0)
                            {
                                nwt[thema][word.Key] += delta;
                                ndt[document][thema] += delta;
                                nt[thema] += delta;
                            }
                        }
                    }

                themas.ForEach(thema => thema.WordsDistribution.Keys.ToList().ForEach(word =>
                    thema.WordsDistribution[word] = nwt[thema][word]/nt[thema]));
                documents.ForEach(document => document.ThemaDistribution.Keys.ToList().ForEach(thema =>
                    document.ThemaDistribution[thema] = ndt[document][thema]/(Double)document.Words.Count));

                var perplexity = Math.Exp((documents.Sum(document => document.Words.GroupBy(w => w).Sum(
                    w => w.Count()*Math.Log(themas.Sum(t => t.WordsDistribution[w.Key]*document.ThemaDistribution[t])))))/
                                          (-words.Count));
                Debug.WriteLine("{0}: {1}", i, perplexity);
            }

            return documents;
        }
    }
}