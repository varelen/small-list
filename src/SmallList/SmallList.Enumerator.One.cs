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
    /// <summary>
    /// Specialized enumerator for <see cref="SmallList{T}"/> if there is only one inlined item.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct OneEnumerator : IEnumerator<T>, IEnumerator
    {
        private const int Size = 1;

        private readonly T item1;

        private int index;

        public OneEnumerator(in SmallList<T> list)
        {
            Debug.Assert(list.size == Size, "SmallList size mismatch");

            this.item1 = list.item1;
            this.index = -1;
        }

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                Debug.Assert(this.index >= 0 && this.index < Size, "Current index out of range");

                return this.item1;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        readonly object? IEnumerator.Current => this.Current;

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
