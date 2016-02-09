using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MicrosoftResearch.Infer.Distributions;
using EnumerableExtensions = MicrosoftResearch.Infer.Collections.EnumerableExtensions;

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

            public Dictionary<Cluster, Double> Clusters { get; set; } 
        }
        public struct Feature
        {
            public Int32 UserId { get; set; }

            public Type1 Type1 { get; set; }
            public Type2 Type2 { get; set; }
            public Type3 Type3 { get; set; }
        }
        public struct Cluster
        {
            public Int32 Id { get; set; }

            public Dictionary<Type1, Double> T1 { get; set; }
            public Dictionary<Type2, Double> T2 { get; set; }
            public Dictionary<Type3, Double> T3 { get; set; }
        }

        public class Profile
        {
            public Dictionary<User, Dictionary<Cluster, Double>> UserDistribution { get; set; }
            public Dictionary<User, Tuple<Dictionary<Type1, Int32>, Dictionary<Type2, Int32>,
                Dictionary<Type3, Int32>>> UserStatistic { get; set; }

            public Dictionary<Cluster, Tuple<Dictionary<Type1, Double>, Dictionary<Type2, Double>,
                Dictionary<Type3, Double>>> ClusterDistribution { get; set; }

            public Double[] Type1Values { get; set; }
            public Double[] Type2Values { get; set; }
            public Double[] Type3Values { get; set; }
            public Double[] ClusterValues { get; set; }

            public Double LogLikelihood()
            {
                Func<User, Type1, Double> log1 = (user, type) => Math.Log(ClusterDistribution.Sum(
                    cluster => UserDistribution[user][cluster.Key]*cluster.Value.Item1[type]));

                Func<User, Type2, Double> log2 = (user, type) => Math.Log(ClusterDistribution.Sum(
                    cluster => UserDistribution[user][cluster.Key]*cluster.Value.Item2[type]));

                Func<User, Type3, Double> log3 = (user, type) => Math.Log(ClusterDistribution.Sum(
                    cluster => UserDistribution[user][cluster.Key]*cluster.Value.Item3[type]));

                return UserStatistic.Sum(user =>
                    user.Value.Item1.Sum(t1 => t1.Value == 0 ? 0 : t1.Value*log1(user.Key, t1.Key)) +
                    user.Value.Item2.Sum(t2 => t2.Value == 0 ? 0 : t2.Value*log2(user.Key, t2.Key)) +
                    user.Value.Item3.Sum(t3 => t3.Value == 0 ? 0 : t3.Value*log3(user.Key, t3.Key)));
            }

            public static Profile Generate(IList<Feature> features, Int32 clusterCount)
            {
                Func<IEnumerable<Feature>, Dictionary<Type1, Int32>> type1Counter = f => Enum.GetValues(typeof (Type1))
                    .Cast<Type1>().ToDictionary(t => t, t => f.Count(ff => ff.Type1 == t));
                Func<IEnumerable<Feature>, Dictionary<Type2, Int32>> type2Counter = f => Enum.GetValues(typeof (Type2))
                    .Cast<Type2>().ToDictionary(t => t, t => f.Count(ff => ff.Type2 == t));
                Func<IEnumerable<Feature>, Dictionary<Type3, Int32>> type3Counter = f => Enum.GetValues(typeof (Type3))
                    .Cast<Type3>().ToDictionary(t => t, t => f.Count(ff => ff.Type3 == t));

                var result = new Profile();
                var users = features.Select(f => f.UserId).Distinct().ToDictionary(i => i, i => new User {Id = i});
                var clusters = Enumerable.Range(1, clusterCount).ToDictionary(i => i, i => new Cluster {Id = i});

                result.UserStatistic = features.GroupBy(f => f.UserId).ToDictionary(group => users[group.Key], group =>
                    new Tuple<Dictionary<Type1, Int32>, Dictionary<Type2, Int32>,
                        Dictionary<Type3, Int32>>(type1Counter(group), type2Counter(group), type3Counter(group))
                    );

                result.ClusterDistribution = GenerateClusters(clusterCount).ToDictionary(cl => clusters[cl.Id], cl =>
                    new Tuple<Dictionary<Type1, Double>, Dictionary<Type2, Double>,
                        Dictionary<Type3, Double>>(cl.T1, cl.T2, cl.T3));

                result.UserDistribution = GenerateUsers(users.Count, clusters.Values.ToList())
                    .ToDictionary(u => users[u.Id], u => u.Clusters);

                result.Type1Values = Enumerable.Repeat(0.1, Enum.GetNames(typeof (Type1)).Length).ToArray();
                result.Type2Values = Enumerable.Repeat(0.1, Enum.GetNames(typeof(Type2)).Length).ToArray();
                result.Type3Values = Enumerable.Repeat(0.1, Enum.GetNames(typeof(Type3)).Length).ToArray();
                result.ClusterValues = Enumerable.Repeat(0.3, clusters.Count).ToArray();

                return result;
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
        public static IList<Feature> GenerateFeatures(IList<User> users, IList<Cluster> clusters, Int32 avgCount)
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
                        new Feature
                        {
                            UserId = p.User.Id,
                            Type1 = (Type1) features[cluster].Item1.Sample(),
                            Type2 = (Type2) features[cluster].Item2.Sample(),
                            Type3 = (Type3) features[cluster].Item3.Sample(),
                        })).ToList();
        }

        // EM-алгоритм для модели PLSA
        public static Profile PlsaEm(Profile profile, Double ratePres = 1e-4, Int32 maxSteps = 1000)
        {
            var index = 0;
            Debug.WriteLine("{0}: LogLikelihood={1}", index, profile.LogLikelihood());

            var clusters = profile.ClusterDistribution.Keys.ToList();
            var users = profile.UserDistribution.Keys.ToList();

            Double epsilon;
            do
            {
                var ncsj = clusters.ToDictionary(c => c, c =>
                    new
                    {
                        Item1 = Enum.GetValues(typeof (Type1)).Cast<Type1>().ToDictionary(t => t, t => 0.0),
                        Item2 = Enum.GetValues(typeof (Type2)).Cast<Type2>().ToDictionary(t => t, t => 0.0),
                        Item3 = Enum.GetValues(typeof (Type3)).Cast<Type3>().ToDictionary(t => t, t => 0.0),
                    });
                var ncs = clusters.ToDictionary(c => c, c => new[]{0.0,0.0,0.0});
                var ncu = users.ToDictionary(u => u, u => profile.UserDistribution[u].ToDictionary(c => c.Key, c => 0.0));
                var nu = users.ToDictionary(u => u, c => 0.0);

                var newProfile = new Profile
                {
                    UserDistribution = profile.UserDistribution.ToDictionary(p => p.Key,
                        p => p.Value.ToDictionary(pp => pp.Key, pp => pp.Value)),
                    ClusterDistribution = profile.ClusterDistribution.ToDictionary(p => p.Key, p =>
                        new Tuple<Dictionary<Type1, Double>, Dictionary<Type2, Double>, Dictionary<Type3, Double>>(
                            p.Value.Item1.ToDictionary(pp => pp.Key, pp => pp.Value),
                            p.Value.Item2.ToDictionary(pp => pp.Key, pp => pp.Value),
                            p.Value.Item3.ToDictionary(pp => pp.Key, pp => pp.Value))),

                    UserStatistic = profile.UserStatistic,
                };

                foreach (var user in users)
                {
                    // Type1
                    foreach (var type in Enum.GetValues(typeof (Type1)).Cast<Type1>())
                    {
                        var z = clusters.Sum(
                            c => newProfile.UserDistribution[user][c]*newProfile.ClusterDistribution[c].Item1[type]);

                        foreach (var cluster in clusters)
                        {
                            var delta = newProfile.UserDistribution[user][cluster]*
                                        newProfile.ClusterDistribution[cluster].Item1[type]
                                        *newProfile.UserStatistic[user].Item1[type]/z;
                            if (delta > 0)
                            {
                                ncsj[cluster].Item1[type] += delta;
                                ncs[cluster][0] += delta;
                                ncu[user][cluster] += delta;
                                nu[user] += delta;
                            }
                        }
                    }

                    // Type2
                    foreach (var type in Enum.GetValues(typeof(Type2)).Cast<Type2>())
                    {
                        var z = clusters.Sum(
                            c => newProfile.UserDistribution[user][c] * newProfile.ClusterDistribution[c].Item2[type]);

                        foreach (var cluster in clusters)
                        {
                            var delta = newProfile.UserDistribution[user][cluster] *
                                        newProfile.ClusterDistribution[cluster].Item2[type]
                                        * newProfile.UserStatistic[user].Item2[type] / z;
                            if (delta > 0)
                            {
                                ncsj[cluster].Item2[type] += delta;
                                ncs[cluster][1] += delta;
                                ncu[user][cluster] += delta;
                                nu[user] += delta;
                            }
                        }
                    }

                    // Type3
                    foreach (var type in Enum.GetValues(typeof(Type3)).Cast<Type3>())
                    {
                        var z = clusters.Sum(
                            c => newProfile.UserDistribution[user][c] * newProfile.ClusterDistribution[c].Item3[type]);

                        foreach (var cluster in clusters)
                        {
                            var delta = newProfile.UserDistribution[user][cluster] *
                                        newProfile.ClusterDistribution[cluster].Item3[type]
                                        * newProfile.UserStatistic[user].Item3[type] / z;
                            if (delta > 0)
                            {
                                ncsj[cluster].Item3[type] += delta;
                                ncs[cluster][2] += delta;
                                ncu[user][cluster] += delta;
                                nu[user] += delta;
                            }
                        }
                    }
                }

                clusters.ForEach(c => users.ForEach(u => newProfile.UserDistribution[u][c] = ncu[u][c]/nu[u]));

                clusters.ForEach(c =>
                {
                    EnumerableExtensions.ForEach(Enum.GetValues(typeof (Type1)).Cast<Type1>(),
                        type1 => newProfile.ClusterDistribution[c].Item1[type1] = ncsj[c].Item1[type1]/ncs[c][0]);
                    EnumerableExtensions.ForEach(Enum.GetValues(typeof (Type2)).Cast<Type2>(),
                        type2 => newProfile.ClusterDistribution[c].Item2[type2] = ncsj[c].Item2[type2]/ncs[c][1]);
                    EnumerableExtensions.ForEach(Enum.GetValues(typeof (Type3)).Cast<Type3>(),
                        type3 => newProfile.ClusterDistribution[c].Item3[type3] = ncsj[c].Item3[type3]/ncs[c][2]);
                });

                epsilon = (newProfile.LogLikelihood() - profile.LogLikelihood())/Math.Abs(profile.LogLikelihood());
                if (epsilon > 0)
                    profile = newProfile;

                Debug.WriteLine("{0}: LogLikelihood={1}\tepsilon={2}", ++index, profile.LogLikelihood(), epsilon);
            } while (epsilon > ratePres && index < maxSteps);

            return profile;
        }

        // EM-алгоритм для модели LDA
        public static Profile LdaEm(Profile profile, Double ratePres = 1e-4, Int32 maxSteps = 1000)
        {
            var index = 0;
            Debug.WriteLine("{0}: LogLikelihood={1}", index, profile.LogLikelihood());

            var clusters = profile.ClusterDistribution.Keys.ToList();
            var users = profile.UserDistribution.Keys.ToList();

            var a1 = profile.Type1Values;
            var a10 = profile.Type1Values.Sum();
            var a2 = profile.Type2Values;
            var a20 = profile.Type2Values.Sum();
            var a3 = profile.Type3Values;
            var a30 = profile.Type3Values.Sum();
            var b = profile.ClusterValues;
            var b0 = profile.ClusterValues.Sum();

            Double epsilon;
            do
            {
                var ncsj = clusters.ToDictionary(c => c, c =>
                    new
                    {
                        Item1 = Enum.GetValues(typeof(Type1)).Cast<Type1>().ToDictionary(t => t, t => 0.0),
                        Item2 = Enum.GetValues(typeof(Type2)).Cast<Type2>().ToDictionary(t => t, t => 0.0),
                        Item3 = Enum.GetValues(typeof(Type3)).Cast<Type3>().ToDictionary(t => t, t => 0.0),
                    });
                var ncs = clusters.ToDictionary(c => c, c => new[] { 0.0, 0.0, 0.0 });
                var ncu = users.ToDictionary(u => u, u => profile.UserDistribution[u].ToDictionary(c => c.Key, c => 0.0));
                var nu = users.ToDictionary(u => u, c => 0.0);

                var newProfile = new Profile
                {
                    UserDistribution = profile.UserDistribution.ToDictionary(p => p.Key,
                        p => p.Value.ToDictionary(pp => pp.Key, pp => pp.Value)),
                    ClusterDistribution = profile.ClusterDistribution.ToDictionary(p => p.Key, p =>
                        new Tuple<Dictionary<Type1, Double>, Dictionary<Type2, Double>, Dictionary<Type3, Double>>(
                            p.Value.Item1.ToDictionary(pp => pp.Key, pp => pp.Value),
                            p.Value.Item2.ToDictionary(pp => pp.Key, pp => pp.Value),
                            p.Value.Item3.ToDictionary(pp => pp.Key, pp => pp.Value))),

                    UserStatistic = profile.UserStatistic,
                };

                foreach (var user in users)
                {
                    // Type1
                    foreach (var type in Enum.GetValues(typeof(Type1)).Cast<Type1>())
                    {
                        var z = clusters.Sum(
                            c => newProfile.UserDistribution[user][c] * newProfile.ClusterDistribution[c].Item1[type]);

                        foreach (var cluster in clusters)
                        {
                            var delta = newProfile.UserDistribution[user][cluster] *
                                        newProfile.ClusterDistribution[cluster].Item1[type]
                                        * newProfile.UserStatistic[user].Item1[type] / z;
                            if (delta > 0)
                            {
                                ncsj[cluster].Item1[type] += delta;
                                ncs[cluster][0] += delta;
                                ncu[user][cluster] += delta;
                                nu[user] += delta;
                            }
                        }
                    }

                    // Type2
                    foreach (var type in Enum.GetValues(typeof(Type2)).Cast<Type2>())
                    {
                        var z = clusters.Sum(
                            c => newProfile.UserDistribution[user][c] * newProfile.ClusterDistribution[c].Item2[type]);

                        foreach (var cluster in clusters)
                        {
                            var delta = newProfile.UserDistribution[user][cluster] *
                                        newProfile.ClusterDistribution[cluster].Item2[type]
                                        * newProfile.UserStatistic[user].Item2[type] / z;
                            if (delta > 0)
                            {
                                ncsj[cluster].Item2[type] += delta;
                                ncs[cluster][1] += delta;
                                ncu[user][cluster] += delta;
                                nu[user] += delta;
                            }
                        }
                    }

                    // Type3
                    foreach (var type in Enum.GetValues(typeof(Type3)).Cast<Type3>())
                    {
                        var z = clusters.Sum(
                            c => newProfile.UserDistribution[user][c] * newProfile.ClusterDistribution[c].Item3[type]);

                        foreach (var cluster in clusters)
                        {
                            var delta = newProfile.UserDistribution[user][cluster] *
                                        newProfile.ClusterDistribution[cluster].Item3[type]
                                        * newProfile.UserStatistic[user].Item3[type] / z;
                            if (delta > 0)
                            {
                                ncsj[cluster].Item3[type] += delta;
                                ncs[cluster][2] += delta;
                                ncu[user][cluster] += delta;
                                nu[user] += delta;
                            }
                        }
                    }
                }

                clusters.ForEach(c => users.ForEach(u => newProfile.UserDistribution[u][c] =
                    (ncu[u][c] + b[clusters.IndexOf(c)])/(nu[u] + b0)));

                clusters.ForEach(c =>
                {
                    EnumerableExtensions.ForEach(Enum.GetValues(typeof(Type1)).Cast<Type1>(),
                        type1 => newProfile.ClusterDistribution[c].Item1[type1] = 
                            (ncsj[c].Item1[type1] + a1[Enum.GetValues(typeof(Type1)).Cast<Type1>().ToList().IndexOf(type1)]) / (ncs[c][0] + a10));
                    EnumerableExtensions.ForEach(Enum.GetValues(typeof(Type2)).Cast<Type2>(),
                        type2 => newProfile.ClusterDistribution[c].Item2[type2] = 
                            (ncsj[c].Item2[type2] + a2[Enum.GetValues(typeof(Type2)).Cast<Type2>().ToList().IndexOf(type2)]) / (ncs[c][1] + a20));
                    EnumerableExtensions.ForEach(Enum.GetValues(typeof(Type3)).Cast<Type3>(),
                        type3 => newProfile.ClusterDistribution[c].Item3[type3] = 
                            (ncsj[c].Item3[type3]+ a3[Enum.GetValues(typeof(Type3)).Cast<Type3>().ToList().IndexOf(type3)]) / (ncs[c][2] + a30));
                });

                epsilon = (newProfile.LogLikelihood() - profile.LogLikelihood()) / Math.Abs(profile.LogLikelihood());
                if (epsilon > 0)
                    profile = newProfile;

                Debug.WriteLine("{0}: LogLikelihood={1}\tepsilon={2}", ++index, profile.LogLikelihood(), epsilon);
            } while (epsilon > ratePres && index < maxSteps);

            return profile;
        }
    }
}
