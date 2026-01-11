using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using System.Runtime.CompilerServices;

namespace Varelen.SmallList.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
//[ShortRunJob]
public class ListSmallListBenchmark<T>
    where T : new()
{
    private SmallList<T> smallList;
    private List<T>? list;
    private T[]? items;
    private T? randomValue;

    [Params(4, 24)]
    public int N;

    [GlobalSetup]
    public void GlobalSetup()
    {
        this.list = [];
        this.smallList = [];
        this.items = new T[N];

        for (int i = 0; i < N; i++)
        {
            T item;

            if (typeof(T).IsValueType)
            {
                item = default!;

                if (typeof(T) == typeof(int))
                {
                    item = (T)Convert.ChangeType(Random.Shared.Next(0, 100), typeof(int));
                }
            }
            else
            {
                item = new T();

                if (item is Data data)
                {
                    data.id = i;
                }
            }

            this.smallList.Add(item);
            this.list.Add(item);
            this.items[i] = item;
        }

        var random = Random.Shared.Next((N / 2) - 1, (N / 2) + 1);
        this.randomValue = this.items[Math.Max(0, random)];
    }

    [BenchmarkCategory("ForEach"), Benchmark(Baseline = true)]
    public int ForEach_List()
    {
        int total = 0;

        foreach (T i in this.list!)
        {
            total += i.GetHashCode();
        }

        return total;
    }

    [BenchmarkCategory("ForEach"), Benchmark]
    public int ForEach_SmallList()
    {
        int total = 0;

        foreach (T i in this.smallList)
        {
            total += i!.GetHashCode();
        }

        return total;
    }

    [BenchmarkCategory("CreationAndForEachIEnumerable"), Benchmark(Baseline = true)]
    public int ForEachIEnumerable_List()
    {
        var list = new List<T>(this.items!);
        return Sum(list);
    }

    [BenchmarkCategory("CreationAndForEachIEnumerable"), Benchmark]
    public int ForEachIEnumerable_SmallList()
    {
        var smallList = new SmallList<T>(this.items!);
        return Sum(smallList);
    }

    [BenchmarkCategory("Indexer"), Benchmark(Baseline = true)]
    public int Indexer_List()
    {
        int total = 0;

        for (int i = 0; i < N; i++)
        {
            total += this.list![i]!.GetHashCode();
        }

        return total;
    }

    [BenchmarkCategory("Indexer"), Benchmark]
    public int Indexer_SmallList()
    {
        int total = 0;

        for (int i = 0; i < N; i++)
        {
            total += this.smallList[i]!.GetHashCode();
        }

        return total;
    }

    [BenchmarkCategory("CreationLoopAdd"), Benchmark(Baseline = true)]
    public List<int> Creation_Loop_List()
    {
        var list = new List<int>();

        for (int i = 0; i < N; i++)
        {
            list.Add(i);
        }

        return list;
    }

    [BenchmarkCategory("CreationLoopAdd"), Benchmark]
    public SmallList<int> Creation_Loop_SmallList()
    {
        var list = new SmallList<int>();

        for (int i = 0; i < N; i++)
        {
            list.Add(i);
        }

        return list;
    }

    [BenchmarkCategory("CreationConstructor"), Benchmark(Baseline = true)]
    public List<T> Creation_Constructor_List()
        => [.. this.items!];

    [BenchmarkCategory("CreationConstructor"), Benchmark]
    public SmallList<T> Creation_Constructor_SmallList()
        => new(this.items!);

    [BenchmarkCategory("Contains"), Benchmark(Baseline = true)]
    public bool Contains_List()
        => this.list!.Contains(this.randomValue!);

    [BenchmarkCategory("Contains"), Benchmark]
    public bool Contains_SmallList()
        => this.smallList.Contains(this.randomValue!);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Sum(IEnumerable<T> list)
    {
        int total = 0;

        foreach (T i in list)
        {
            total += i!.GetHashCode();
        }

        return total;
    }
}
