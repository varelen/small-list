// SPDX-FileCopyrightText: 2026 varelen
// 
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Varelen.SmallList;

public partial struct SmallList<T> : IList<T>, IReadOnlyList<T>
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FourEnumerator : IEnumerator<T>, IEnumerator
    {
        private const int Size = 4;

        // SAFETY: Do not reorder this because we use Unsafe.Add to index in these
        private T item1;
        private readonly T item2;
        private readonly T item3;
        private readonly T item4;

        private int index;

        public FourEnumerator(in SmallList<T> list)
        {
            Debug.Assert(list.size == Size, "SmallList size mismatch");

            this.item1 = list.item1;
            this.item2 = list.item2;
            this.item3 = list.item3;
            this.item4 = list.item4;
            this.index = -1;
        }

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(this.index >= 0 && this.index < Size, "Current index out of range");

                return Unsafe.Add(ref this.item1, this.index);
            }
        }

        object? IEnumerator.Current => this.Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
            => ++this.index < Size;

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
