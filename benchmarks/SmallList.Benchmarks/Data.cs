using System.Diagnostics.CodeAnalysis;

namespace Varelen.SmallList.Benchmarks;

public sealed class Data : IEqualityComparer<Data>
{
    public int id;

    public bool Equals(Data? x, Data? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.Equals(y);
    }

    public override bool Equals(object? obj)
        => obj is Data data &&
            id == data.id;

    public int GetHashCode([DisallowNull] Data obj)
        => this.id;

    public override int GetHashCode()
        => HashCode.Combine(id);
}
