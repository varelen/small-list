// SPDX-FileCopyrightText: 2026 varelen
// 
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Varelen.SmallList;

public partial struct SmallList<T> : IList<T>, IReadOnlyList<T>
{
    [StructLayout(LayoutKind.Sequential)]
    public ref struct RefEnumerator
    {
        private readonly ref T first;
        private readonly int count;

        private int index;

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                Debug.Assert(!Unsafe.IsNullRef(ref this.first), "first should not be a null ref");
                Debug.Assert(this.index >= 0 && this.index < this.count, "RefEnumerator Current index out of range");

                return Unsafe.Add(ref this.first, this.index);
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        internal RefEnumerator(ref SmallList<T> list)
        {
            this.count = list.size;
            this.index = -1;
            this.first = ref list.array is not null
                ? ref MemoryMarshal.GetArrayDataReference(list.array)
                : ref list.item1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
            => ++this.index < this.count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            // Nothing to do
        }
    }
}
