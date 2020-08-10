using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace TaTooIne.Benchmark
{
    [Config(typeof(BenchmarkConfig))]
    public class ForEachVsFor
    {
        private static readonly Random Random = new Random();
        private List<int> _list;

        private static List<int> RandomIntList(int length)
        {
            int Min = 1;
            int Max = 10;
            return Enumerable
                .Repeat(0, length)
                .Select(i => Random.Next(Min, Max))
                .ToList();
        }

        [Params(1, 10, 20)] public int N;

        [GlobalSetup]
        public void Setup()
        {
            _list = RandomIntList(N);
        }

        [Benchmark]
        public void Foreach()
        {
            int total = 0;
            foreach (int i in _list)
            {
                total += i;
            }
        }

        [Benchmark]
        public void For()
        {
            int total = 0;
            for (int i = 0; i < _list.Count; i++)
            {
                total += _list[i];
            }
        }

        [Benchmark]
        public void ForEach()
        {
            int total = 0;
            _list.ForEach(i => total += i);
        }
    }

    internal static class Program
    {
        private static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
