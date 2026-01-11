using BenchmarkDotNet.Running;

using Varelen.SmallList.Benchmarks;

BenchmarkSwitcher.FromTypes(
    [
        typeof(ListSmallListBenchmark<int>),
        typeof(ListSmallListBenchmark<Data>),
    ]).Run(args);
