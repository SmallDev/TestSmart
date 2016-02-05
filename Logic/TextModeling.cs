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
        //public class WordComparer
        public class Word
        {
            public String Value { get; set; }

            public override Boolean Equals(object obj)
            {
                var word = obj as Word;
                return word != null && Equals(word);
            }
            protected Boolean Equals(Word other)
            {
                return String.Equals(Value.ToLower(), other.Value.ToLower());
            }
            public override int GetHashCode()
            {
                return (Value != null ? Value.GetHashCode() : 0);
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
        public static IList<Document> GenerateReadableDocuments()
        {
            //var st1 = new[] { "I", "like", "to", "eat", "broccoli", "and", "banana" };
            //var st2 = new[] { "I", "ate", "a", "banana", "and", "spinach", "smoothie","for", "breakfast" };
            //var st3 = new[] { "Chinchilla", "and", "kitten", "are", "cute"};
            //var st4 = new[] { "My", "sister", "adopted", "a", "kitten", "yesterday"};
            //var st5 = new[] { "Look", "at", "this", "cute", "hamster", "munching", "on", "a", "piece", "of", "broccoli"};

            var st1 = new[] { "I", "like", "broccoli", "banana" };
            var st2 = new[] { "I", "banana", "spinach", "smoothie", "breakfast" };
            var st3 = new[] { "Chinchilla", "kitten", "cute" };
            var st4 = new[] { "Sister", "adopted", "kitten", "yesterday" };
            var st5 = new[] { "Look", "cute", "hamster", "munching", "piece", "broccoli" };

            return new[] {st1, st2, st3, st4, st5}.Select(
                st => new Document {Words = st.Select(s => new Word {Value = s}).ToList()}).ToList();
        }

        // Возвращает докуенты с распределениями по темам
        public static IList<Document> EmPlsa(IList<Document> documents, Int32 themaNumber)
        {
            var words = documents.SelectMany(d => d.Words).Distinct().ToList();
            var themas = Enumerable.Range(1, themaNumber).Select(i =>
            {
                var sample = new Dirichlet(words.Select(w => 0.3).ToArray()).Sample();
                return new Thema
                {
                    WordsDistribution = Enumerable.Range(0, words.Count).ToDictionary(j => words[j], j => sample[j])
                };
            }).ToList();

            documents.ForEach(d =>
            {
                var sample = new Dirichlet(themas.Select(w => 0.1).ToArray()).Sample();
                d.ThemaDistribution = Enumerable.Range(0, themas.Count).ToDictionary(j => themas[j], j => sample[j]);
            });

            var perplexityEpsilon = 2.0;
            var currentPerplexity = 0.0;
            var index = 0;

            while (perplexityEpsilon > 0.01)
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
                if (currentPerplexity > 0)
                    perplexityEpsilon = currentPerplexity - perplexity;

                currentPerplexity = perplexity;
                Debug.WriteLine("{0}: {1}", ++index, perplexity);
            }

            return documents;
        }

        // GEM
        public static IList<Document> EmPlsaGem(IList<Document> documents, Int32 themaNumber)
        {
            var words = documents.SelectMany(d => d.Words).Distinct().ToList();
            var themas = Enumerable.Range(1, themaNumber).Select(i =>
            {
                var sample = new Dirichlet(words.Select(w => 0.3).ToArray()).Sample();
                return new Thema
                {
                    WordsDistribution = Enumerable.Range(0, words.Count).ToDictionary(j => words[j], j => sample[j])
                };
            }).ToList();

            documents.ForEach(d =>
            {
                var sample = new Dirichlet(themas.Select(w => 0.1).ToArray()).Sample();
                d.ThemaDistribution = Enumerable.Range(0, themas.Count).ToDictionary(j => themas[j], j => sample[j]);
            });

            var perplexityEpsilon = 2.0;
            var currentPerplexity = 0.0;
            var index = 0;

            while (perplexityEpsilon > 0.01)
            {
                var nwt = themas.ToDictionary(t => t, t => t.WordsDistribution.Keys.ToDictionary(w => w, w => 0.0));
                var ndt = documents.ToDictionary(d => d, d => d.ThemaDistribution.Keys.ToDictionary(t => t, t => 0.0));
                var nt = themas.ToDictionary(t => t, t => 0.0);
                var nd = documents.ToDictionary(d => d, d => 0.0);
                var ndwt = documents.ToDictionary(d => d, d => d.ThemaDistribution.Keys.ToDictionary(
                    t => t, t => t.WordsDistribution.Keys.ToDictionary(w => w, w => 0.0)));

                foreach (var document in documents)
                {
                    foreach (var word in document.Words.GroupBy(w => w))
                    {
                        var z = themas.Sum(thema => thema.WordsDistribution[word.Key]*document.ThemaDistribution[thema]);
                        foreach (var thema in themas)
                        {
                            var delta = thema.WordsDistribution[word.Key]*document.ThemaDistribution[thema]*
                                        word.Count()/z;

                            var d = delta - ndwt[document][thema][word.Key];
                            if (d > 0)
                            {
                                nwt[thema][word.Key] += d;
                                ndt[document][thema] += d;
                                nt[thema] += d;
                                nd[document] += d;
                                ndwt[document][thema][word.Key] = delta;
                            }
                        }

                        if (index > 0)
                        {
                            themas.ForEach(thema => thema.WordsDistribution[word.Key] = nwt[thema][word.Key] / nt[thema]);
                        }
                    }

                    if (index > 0)
                    {
                        document.ThemaDistribution.Keys.ToList().ForEach(thema =>
                                document.ThemaDistribution[thema] = ndt[document][thema] / nd[document]);
                    }
                }

                var perplexity = Math.Exp((documents.Sum(document => document.Words.GroupBy(w => w).Sum(
                   w => w.Count() * Math.Log(themas.Sum(t => t.WordsDistribution[w.Key] * document.ThemaDistribution[t]))))) /
                                         (-words.Count));

                if (currentPerplexity > 0)
                    perplexityEpsilon = currentPerplexity - perplexity;

                currentPerplexity = perplexity;
                Debug.WriteLine("{0}: {1}", ++index, perplexity);
            }

            return documents;
        }
    }
}