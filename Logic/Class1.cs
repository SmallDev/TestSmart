using System;
using System.Collections.Generic;
using System.Linq;
using MicrosoftResearch.Infer.Distributions;
using MicrosoftResearch.Infer.Maths;

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
    
    public class Class1
    {
        private List<Double[]>[] fi;

        public Class1()
        {
            fi = new List<double[]>[2];
            var fi1 = fi[0] = new List<Double[]>();
            fi1.Add(new[] {0.6, 0.35, 0.05});
            fi1.Add(new[] { 0.25, 0.25, 0.25, 0.25 });
            fi1.Add(new[] {0, 0, 0, 0.9, 0.1});

            var fi2 = fi[1] = new List<Double[]>();
            fi2.Add(new[] { 0.333, 0.333, 0.334 });
            fi2.Add(new[] { 0, 0.8, 0.2, 0 });
            fi2.Add(new[] { 0.95, 0.05, 0, 0, 0 });
        }

        public IEnumerable<Tuple<Type1, Type2, Type3>> Test()
        {
            var dir = new Dirichlet(0.1, 0.1);
            return Enumerable.Range(1, 1000).Select(i => fi[Discrete.Sample(dir.Sample())])
                .Select(prob => new Tuple<Type1, Type2, Type3>(
                    (Type1) Discrete.Sample(Vector.FromArray(prob[0])),
                    (Type2) Discrete.Sample(Vector.FromArray(prob[1])),
                    (Type3) Discrete.Sample(Vector.FromArray(prob[2]))));
        }

        public List<double>[] Plsa(IList<Tuple<Type1, Type2, Type3>> data, List<double>[] fip, List<double>[] tetap)
        {
            var fc = new double[2, 3];
            var xc = new double[2, data.Count];
            var c = new double[2];

            for (var cc = 0; cc < 40; ++cc)
            {
                for (var i = 0; i < data.Count; ++i)
                {
                    for (var f = 0; f < 3; ++f)
                    {
                        var z = fip[0][f]*tetap[0][i] + fip[1][f]*tetap[1][i];
                        for (var j = 0; j < 2; ++j)
                        {
                            var delta = fip[j][f]*tetap[j][i]/z;
                            if (delta > 0)
                            {
                                fc[j, f] += delta;
                                xc[j, i] += delta;
                                c[j] += delta;
                            }
                        }
                    }
                }

                for (var i = 0; i < data.Count; ++i)
                    for (var f = 0; f < 3; ++f)
                        for (var j = 0; j < 2; ++j)
                        {
                            fip[j][f] = fc[j, f]/c[j];
                            tetap[j][i] = xc[j, i]/3;
                        }
            }

            return fip;
        }
    }
}
