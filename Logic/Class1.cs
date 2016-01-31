using System;
using System.Linq;
using MicrosoftResearch.Infer.Distributions;
using MicrosoftResearch.Infer.Factors;
using MicrosoftResearch.Infer.Models;

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
        public Class1()
        {
            const Int32 count = 5;

        }

        public void Test()
        {
            var dir = new Dirichlet(0.1, 0.1, 0.1);
            var types = Enumerable.Range(1, 100).Select(i =>
            {
                (Type) Discrete.Sample(dir.Sample())
            }).ToList();
        }
    }
}
