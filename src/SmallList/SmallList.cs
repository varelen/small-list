// SPDX-FileCopyrightText: 2026 varelen
// 
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Varelen.SmallList;

/// <summary>
/// Highly optimized list implementation especially for up to four elements which are inlined with zero allocations.
///
/// It is NOT safe to change this list while iterating, which is taken for a given here. So there is no internal versioning to detect modifications while enumerating.
/// </summary>
/// <typeparam name="T">The generic type.</typeparam>
[DebuggerDisplay("Count = {Count}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct SmallList<T> : IList<T>, IReadOnlyList<T>
{
    /// <summary>
    /// The count of inlinable items of this implementation.
    /// </summary>
    private const int InlinedItemsCount = 4;

    /// <summary>
    /// The initial capacity for this list during first heap allocation.
    /// </summary>
    private const int InitialCapacity = InlinedItemsCount * 2;

    // SAFETY: Do not reorder this because we use Unsafe.Add to index in these
    private T item1;
    private T item2;
    private T item3;
    private T item4;

    private T[]? array;

    private int size;

    /// <summary>
    /// Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    /// <returns>The item to get or set.</returns>
    /// <exception cref="IndexOutOfRangeException">If the index provided is outside of the bounds of this list.</exception>
    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            Debug.Assert(this.size >= 0 && this.size <= InlinedItemsCount || this.array is not null, "Invalid size/array state");

            if ((uint)index > this.size)
            {
                throw new IndexOutOfRangeException();
            }

            ref var start = ref Unsafe.AsRef(in this.item1);

            if (index < InlinedItemsCount)
            {
                return Unsafe.Add(ref Unsafe.AsRef(in this.item1), index);
            }

            Debug.Assert(this.array is not null, "Array should not be null here");

            // PERFORMANCE: Benchmarks have shown that the if and simple array access are faster than 'Unsafe' juggling
            return this.array[index];
        }
#pragma warning disable IDE0251 // Make member 'readonly' - It is directly modified via 'ref Unsafe.Add()'
        set
#pragma warning restore IDE0251 // Make member 'readonly'
        {
            if ((uint)index > this.size)
            {
                throw new IndexOutOfRangeException();
            }

            ref var currentValue = ref Unsafe.NullRef<T>();

            if (index < InlinedItemsCount)
            {
                currentValue = ref Unsafe.Add(ref Unsafe.AsRef<T?>(in this.item1), index);
            }
            else
            {
                Debug.Assert(this.array is not null, "Array should not be null here");

                currentValue = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference<T?>(this.array!), index);
            }

            currentValue = value;
        }
    }

    /// <summary>
    /// Gets the number of items in this list.
    /// </summary>
    public readonly int Count => this.size;

    /// <summary>
    /// Gets the capacity of this list.
    /// </summary>
    public readonly int Capacity => array is null ? this.size : this.array.Length;

    /// <summary>
    /// Returns if this list is read-only (false).
    /// </summary>
    public readonly bool IsReadOnly => false;

    /// <summary>
    /// Creates a new allocation free instance of <see cref="SmallList{T}"/> with size of zero.
    /// </summary>
    public SmallList()
    {
        this.item1 = default!;
        this.item2 = default!;
        this.item3 = default!;
        this.item4 = default!;
        this.array = null;
        this.size = 0;
    }

    /// <summary>
    /// Creates a new allocation free instance of <see cref="SmallList{T}"/> with one item.
    /// </summary>
    public SmallList(T item)
    {
        this.item1 = item;
        this.item2 = default!;
        this.item3 = default!;
        this.item4 = default!;
        this.array = null;
        this.size = 1;
    }

    /// <summary>
    /// Creates a new allocation free instance of <see cref="SmallList{T}"/> with two items.
    /// </summary>
    public SmallList(T item1, T item2)
    {
        this.item1 = item1;
        this.item2 = item2;
        this.item3 = default!;
        this.item4 = default!;
        this.array = null;
        this.size = 2;
    }

    /// <summary>
    /// Creates a new allocation free instance of <see cref="SmallList{T}"/> with three items.
    /// </summary>
    public SmallList(T item1, T item2, T item3)
    {
        this.item1 = item1;
        this.item2 = item2;
        this.item3 = item3;
        this.item4 = default!;
        this.array = null;
        this.size = 3;
    }

    /// <summary>
    /// Creates a new allocation free instance of <see cref="SmallList{T}"/> with four items.
    /// </summary>
    public SmallList(T item1, T item2, T item3, T item4)
    {
        this.item1 = item1;
        this.item2 = item2;
        this.item3 = item3;
        this.item4 = item4;
        this.array = null;
        this.size = 4;
    }

    /// <summary>
    /// Creates a new instance of <see cref="SmallList{T}"/> with the given items.
    ///
    /// This might cause one heap allocation if the length of <paramref name="items"/> is greater than <see cref="InlinedItemsCount"/>.
    /// </summary>
    public SmallList(T[] items)
    {
        this.item1 = default!;
        this.item2 = default!;
        this.item3 = default!;
        this.item4 = default!;
        this.array = null;

        if (items is null || items.Length == 0)
        {
            this.size = 0;
            return;
        }

        int length = items.Length;
        this.size = length;

        if (length > 0)
        {
            this.item1 = items[0];
        }

        if (length > 1)
        {
            this.item2 = items[1];
        }

        if (length > 2)
        {
            this.item3 = items[2];
        }

        if (length > 3)
        {
            this.item4 = items[3];
        }

        if (length > InlinedItemsCount)
        {
            // We already need to allocate, so we can place the inlined items in the array too,
            // which allows for faster access in the enumerator
            this.array = new T[length];
            this.array[0] = this.item1;
            this.array[1] = this.item2;
            this.array[2] = this.item3;
            this.array[3] = this.item4;

            int remaining = length - InlinedItemsCount;

            items.AsSpan(InlinedItemsCount, remaining).CopyTo(this.array.AsSpan(InlinedItemsCount));
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="SmallList{T}"/> with the given items.
    ///
    /// This might cause one heap allocation if there are more items in <paramref name="items"/> than <see cref="InlinedItemsCount"/>.
    /// </summary>
    public SmallList(IEnumerable<T> items)
    {
        this.item1 = default!;
        this.item2 = default!;
        this.item3 = default!;
        this.item4 = default!;
        this.array = null;
        this.size = 0;

        // Fast path for collections, but requires to be more than inline count,
        // otherwise it's not really faster
        if (items is ICollection<T> collection && collection.Count > InlinedItemsCount)
        {
            this.array = new T[BitOperations.RoundUpToPowerOf2((uint)collection.Count)];

            collection.CopyTo(this.array, 0);

            this.item1 = this.array[0];
            this.item2 = this.array[1];
            this.item3 = this.array[2];
            this.item4 = this.array[3];

            this.size = collection.Count;
            return;
        }

        foreach (T item in items)
        {
            this.Add(item);
        }
    }

    /// <summary>
    /// Adds the item to this list.
    ///
    /// This might cause one heap allocation if the size of list is already <see cref="InlinedItemsCount"/> or greater than the current <see cref="Capacity"/>.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        if (this.size < InlinedItemsCount)
        {
            ref var valueRef = ref Unsafe.Add(ref this.item1, this.size);
            valueRef = item;
        }
        else
        {
            if (this.array is null)
            {
                // We already need to allocate, so we can place the inlined items in the array too,
                // which allows for faster access in the enumerator
                this.array = new T[InitialCapacity];
                this.array[0] = this.item1;
                this.array[1] = this.item2;
                this.array[2] = this.item3;
                this.array[3] = this.item4;
            }
            else if (this.size == this.array.Length)
            {
                Debug.Assert(BitOperations.IsPow2(this.array.Length * 2), "Length should be automatically be a power of two");

                var newArray = new T[this.array.Length * 2];

                this.array.AsSpan().CopyTo(newArray.AsSpan());

                this.array = newArray;
            }

            this.array[this.size] = item;
        }

        this.size++;
    }

    /// <summary>
    /// Clears all items from this list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        this.item1 = default!;
        this.item2 = default!;
        this.item3 = default!;
        this.item4 = default!;

        if (this.array is not null && RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(this.array, 0, this.size);
        }

        this.array = null;
        this.size = 0;
    }

    /// <summary>
    /// Returns true if the item was found in the list.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns>true if found, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Contains(T item)
        => this.size != 0 && this.IndexOf(item) >= 0;

    /// <summary>
    /// Copies all items from this list to <paramref name="array"/>.
    /// </summary>
    /// <param name="array">The target array.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array)
        => this.CopyTo(array, 0);

    /// <summary>
    /// Copies all items from this list to <paramref name="array"/> at offset <paramref name="arrayIndex"/>.
    /// </summary>
    /// <param name="array">The target array.</param>
    /// <param name="arrayIndex">The target array offset.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="arrayIndex"/> is out of bounds.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (arrayIndex < 0 || ((uint)(arrayIndex + array.Length) > this.size))
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (this.size < InlinedItemsCount)
        {
            var thisSpan = MemoryMarshal.CreateSpan(ref this.item1, this.size);
            thisSpan.CopyTo(array.AsSpan(arrayIndex));
            return;
        }

        Debug.Assert(this.array is not null, "Array should not be null here");

        Array.Copy(this.array!, 0, array, arrayIndex, this.size);
    }

    /// <summary>
    /// Gets the first found index of the given item.
    /// </summary>
    /// <param name="item">The item to find the first index of.</param>
    /// <returns>The first index of the found item.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int IndexOf(T item)
    {
        int index = -1;

        if (this.size == 0)
        {
            return index;
        }

        if (this.size <= InlinedItemsCount)
        {
            var comparer = EqualityComparer<T>.Default;

            if (comparer.Equals(this.item1, item))
            {
                return 0;
            }

            if (this.size > 1 && comparer.Equals(this.item2, item))
            {
                return 1;
            }

            if (this.size > 2 && comparer.Equals(this.item3, item))
            {
                return 2;
            }

            if (this.size > 3 && comparer.Equals(this.item4, item))
            {
                return 3;
            }
        }
        else
        {
            Debug.Assert(this.array is not null, "Array should not be null here");
            index = Array.IndexOf(this.array, item, 0, this.size);
        }

        return index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, T item)
    {
        throw new NotImplementedException("TODO");
    }

    /// <summary>
    /// Removes the item from this list.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>true if found and removed, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        int index = this.IndexOf(item);

        if (index == -1)
        {
            return false; 
        }

        this.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Removes one item at the given index from this list.
    /// </summary>
    /// <param name="index">The index of the item to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="index"/> is out of bounds.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)this.size);

        this.size--;

        if (this.size > InlinedItemsCount)
        {
            if (index < this.size)
            {
                Debug.Assert(this.array is not null, "Array should not be null here");
                Array.Copy(this.array!, index + 1, this.array!, index, this.size - index);
            }

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                this.array![this.size] = default!;
            }

            return;
        }

        switch (index)
        {
            case 0:
                this.item1 = this.item2;
                this.item2 = this.item3;
                this.item3 = this.item4;
                this.item4 = default!;
                break;
            case 1:
                this.item2 = this.item3;
                this.item3 = this.item4;
                this.item4 = default!;
                break;
            case 2:
                this.item3 = this.item4;
                this.item4 = default!;
                break;
            case 3:
                this.item4 = default!;
                break;
        }
    }

    /// <summary>
    /// Gets the <see cref="RefEnumerator"/> for this list.
    /// </summary>
    /// <returns>The enemerator for this list.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly RefEnumerator GetEnumerator()
        => new(ref Unsafe.AsRef(in this));

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        if (this.size == 0)
        {
            return new EmptyEnumerator();
        }

        // Fast path with highly specialized enumerators
        if (this.size <= InlinedItemsCount)
        {
            return this.size switch
            {
                1 => new OneEnumerator(this),
                2 => new TwoEnumerator(this),
                3 => new ThreeEnumerator(this),
                4 => new FourEnumerator(this),
                _ => throw new InvalidOperationException()
            };
        }

        return new ArrayEnumerator(this);
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<T>)this).GetEnumerator();

    /// <summary>
    /// Specialized enumerator if the list is empty.
    /// </summary>
    public struct EmptyEnumerator : IEnumerator<T>, IEnumerator
    {
        /// <inheritdoc/>
        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                throw new InvalidOperationException("Empty enumerator does not have any items");
            }
        }

        object? IEnumerator.Current => this.Current;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool MoveNext()
            => false;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Reset()
        {
            // Nothing to do
        }

        /// <inheritdoc/>
        public readonly void Dispose()
        {
            // Nothing to do
        }
    }
}
