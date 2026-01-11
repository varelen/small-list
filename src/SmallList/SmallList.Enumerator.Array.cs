// SPDX-FileCopyrightText: 2026 varelen
// 
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Varelen.SmallList;

public partial struct SmallList<T> : IList<T>, IReadOnlyList<T>
{
    /// <summary>
    /// Specialized enumerator for <see cref="SmallList{T}"/> if we have heap allocated and all items are in one array.
    /// </summary>
    public struct ArrayEnumerator : IEnumerator<T>, IEnumerator
    {
        private readonly T[] array;
        private readonly int length;

        private int index;

        public ArrayEnumerator(in SmallList<T> list)
        {
            Debug.Assert(list.size > InlinedItemsCount, "Array enumerator should only be used if there are more items than inlined items count");
            Debug.Assert(list.array is not null, "Array should not be null");

            this.array = list.array;
            this.length = list.array.Length;
            this.index = -1;
        }

        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(this.index >= 0 && this.index < this.length, "Current index out of range");
                return this.array[this.index];
            }
        }

        readonly object? IEnumerator.Current => this.Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
            => ++this.index < this.length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
            => this.index = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            // Nothing to do
        }
    }
}
