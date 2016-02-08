using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MicrosoftResearch.Infer.Distributions;

namespace Logic
{
    public enum Type1
    {
        A,
        B,
        C
    }
    public enum Type2
    {
        P,
        R,
        S,
        T
    }
    public enum Type3
    {
        U,
        V,
        W,
        X,
        Y,
        Z
    }
    
    public class StbModeling
    {
        public struct User
        {
            // Или другой идентификатор (MAC например)
            public Int32 Id { get; set; }

            public IDictionary<Cluster, Double> Clusters { get; set; } 
        }
        public struct Features
        {
            public Int32 UserId { get; set; }

            public Type1 Type1 { get; set; }
            public Type2 Type2 { get; set; }
            public Type3 Type3 { get; set; }
        }
        public struct Cluster
        {
            public Int32 Id { get; set; }

            public IDictionary<Type1, Double> T1 { get; set; }
            public IDictionary<Type2, Double> T2 { get; set; }
            public IDictionary<Type3, Double> T3 { get; set; }
        }

        public class Profile
        {
            public Dictionary<User, IDictionary<Cluster, Double>> UserDistribution { get; set; }
            public Dictionary<User, Tuple<Dictionary<Type1, Int32>, Dictionary<Type2, Int32>,
                Dictionary<Type3, Int32>>> UserStatistic { get; set; }

            public Dictionary<Cluster, Tuple<Dictionary<Type1, Double>, Dictionary<Type2, Double>,
                Dictionary<Type3, Double>>> ClusterDistribution { get; set; }

            public Double LogLikelihood()
            {
                Func<User, Type1, Double> log1 = (user, type) => Math.Log(ClusterDistribution.Sum(
                    cluster => UserDistribution[user][cluster.Key]*cluster.Value.Item1[type]));

                Func<User, Type2, Double> log2 = (user, type) => Math.Log(ClusterDistribution.Sum(
                    cluster => UserDistribution[user][cluster.Key]*cluster.Value.Item2[type]));

                Func<User, Type3, Double> log3 = (user, type) => Math.Log(ClusterDistribution.Sum(
                    cluster => UserDistribution[user][cluster.Key]*cluster.Value.Item3[type]));

                return UserStatistic.Sum(user =>
                    user.Value.Item1.Sum(t1 => t1.Value*log1(user.Key, t1.Key)) +
                    user.Value.Item2.Sum(t2 => t2.Value*log2(user.Key, t2.Key)) +
                    user.Value.Item3.Sum(t3 => t3.Value*log3(user.Key, t3.Key)));
            }
        }

        public static IList<Cluster> GenerateClusters(Int32 clusterCount)
        {
            var types1 = Enum.GetValues(typeof (Type1)).Cast<Type1>().ToList();
            var types2 = Enum.GetValues(typeof (Type2)).Cast<Type2>().ToList();
            var types3 = Enum.GetValues(typeof (Type3)).Cast<Type3>().ToList();

            var dir1 = new Dirichlet(Enumerable.Repeat(0.1, types1.Count).ToArray());
            var dir2 = new Dirichlet(Enumerable.Repeat(0.1, types2.Count).ToArray());
            var dir3 = new Dirichlet(Enumerable.Repeat(0.1, types3.Count).ToArray());

            return Enumerable.Range(1, clusterCount).Select(i =>
            {
                var sample1 = dir1.Sample();
                var sample2 = dir2.Sample();
                var sample3 = dir3.Sample();
                return new Cluster
                {
                    Id = i,
                    T1 = Enumerable.Range(0, types1.Count).ToDictionary(j => types1[j], j => sample1[j]),
                    T2 = Enumerable.Range(0, types2.Count).ToDictionary(j => types2[j], j => sample2[j]),
                    T3 = Enumerable.Range(0, types3.Count).ToDictionary(j => types3[j], j => sample3[j])
                };
            }).ToList();
        }
        public static IList<User> GenerateUsers(Int32 userCount, IList<Cluster> clusters)
        {
            var prob = new Dirichlet(Enumerable.Repeat(0.3, clusters.Count).ToArray());
            return Enumerable.Range(1, userCount).Select(i => new {Id = i, Sample = prob.Sample()})
                .Select(p => new User
                {
                    Id = p.Id,
                    Clusters = Enumerable.Range(0, clusters.Count).ToDictionary(j => clusters[j], j => p.Sample[j])
                }).ToList();
        }
        public static IList<Features> GenerateFeatures(IList<User> users, IList<Cluster> clusters, Int32 avgCount)
        {
            var norm = new Gaussian(avgCount, avgCount/2.0);
            var features = clusters.ToDictionary(c => c, c => new
            {
                Item1 = new Discrete(c.T1.Values.ToArray()),
                Item2 = new Discrete(c.T2.Values.ToArray()),
                Item3 = new Discrete(c.T3.Values.ToArray())
            });

            return users.Select(u => new {User = u, Prob = new Discrete(u.Clusters.Values.ToArray())})
                .SelectMany(p => Enumerable.Range(0, (Int32) norm.Sample())
                    .Select(i => clusters[p.Prob.Sample()]).Select(cluster =>
                        new Features
                        {
                            UserId = p.User.Id,
                            Type1 = (Type1) features[cluster].Item1.Sample(),
                            Type2 = (Type2) features[cluster].Item2.Sample(),
                            Type3 = (Type3) features[cluster].Item3.Sample(),
                        })).ToList();
        }

        // EM-алгоритм для модели PLSA
        public static Profile PlsaEm(Profile profile, Double perplexityPres = 1e-4, Int32 maxSteps = 1000)
        {
            var index = 0;
            Debug.WriteLine("{0}: LogLikelihood={1}", index, profile.LogLikelihood());

            var clusters = profile.ClusterDistribution.Keys.ToList();
            var users = profile.UserDistribution.Keys.ToList();

            Double epsilon;
            do
            {
                var nwt = profile.WordsDistribution.ToDictionary(p => p.Key, p => p.Value.Keys.ToDictionary(w => w, w => 0.0));
                var ndt = profile.ThemaDistribution.ToDictionary(p => p.Key, p => p.Value.Keys.ToDictionary(t => t, t => 0.0));
                var nt = themas.ToDictionary(t => t, t => 0.0);

                var newProfile = new Profile
                {
                    WordsDistribution = profile.WordsDistribution.ToDictionary(p => p.Key, p => p.Value.ToDictionary(
                        pp => pp.Key, pp => pp.Value)),
                    ThemaDistribution = profile.ThemaDistribution.ToDictionary(p => p.Key, p => p.Value.ToDictionary(
                        pp => pp.Key, pp => pp.Value))
                };

                foreach (var document in documents)
                    foreach (var word in document.Words.GroupBy(w => w))
                    {
                        var z = themas.Sum(thema => newProfile.WordsDistribution[thema][word.Key] * newProfile.ThemaDistribution[document][thema]);
                        foreach (var thema in themas)
                        {
                            var delta = newProfile.WordsDistribution[thema][word.Key] * newProfile.ThemaDistribution[document][thema] *
                                        word.Count() / z;
                            if (delta > 0)
                            {
                                nwt[thema][word.Key] += delta;
                                ndt[document][thema] += delta;
                                nt[thema] += delta;
                            }
                        }
                    }

                newProfile.WordsDistribution.Keys.ToList().ForEach(thema => newProfile.WordsDistribution[thema].Keys
                    .ToList().ForEach(word => newProfile.WordsDistribution[thema][word] = nwt[thema][word] / nt[thema]));
                newProfile.ThemaDistribution.Keys.ToList().ForEach(doc => newProfile.ThemaDistribution[doc].Keys
                    .ToList().ForEach(thema => newProfile.ThemaDistribution[doc][thema] = ndt[doc][thema] / (Double)doc.Words.Count));

                epsilon = 1 - profile.LogLikelihood()/newProfile.LogLikelihood();
                if (epsilon > 0)
                    profile = newProfile;

                Debug.WriteLine("{0}: LogLikelihood={1}", ++index, profile.LogLikelihood());
            } while (epsilon > perplexityPres && index < maxSteps);

            return profile;
        }
    }
}
