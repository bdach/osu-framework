// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Framework.Lists
{
    /// <summary>
    /// A <see cref="SortedList{T}"/> with the capability to track items as alive or dead.
    /// Can be used in complex scenarios where a simple index isn't enough to limit the number of items processed
    /// each loop.
    /// </summary>
    public class SparseSortedList<T> : SortedList<T>
    {
        /// <summary>
        /// List tracking whether individual list items are alive.
        /// </summary>
        private readonly List<bool> alive;

        public SparseSortedList()
        {
            alive = new List<bool>();
        }

        public SparseSortedList(Func<T, T, int> comparer)
            : base(comparer)
        {
            alive = new List<bool>();
        }

        public SparseSortedList(IComparer<T> comparer)
            : base(comparer)
        {
            alive = new List<bool>();
        }

        /// <summary>
        /// The index of the first item alive.
        /// </summary>
        public int FirstAlive { get; private set; }

        public override void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
            alive.RemoveRange(index, count);
            if (FirstAlive >= index)
                recomputeFirstAlive(index);
        }

        public override int Add(T item)
        {
            int index = base.Add(item);
            alive.Insert(index, true);
            FirstAlive = Math.Min(index, FirstAlive);
            return index;
        }

        public override void RemoveAt(int index)
        {
            base.RemoveAt(index);
            alive.RemoveAt(index);
            recomputeFirstAlive(index);
        }

        public override void Clear()
        {
            base.Clear();
            alive.Clear();
            FirstAlive = 0;
        }

        private void recomputeFirstAlive(int startingIndex)
        {
            var firstAlive = alive.IndexOf(true, startingIndex);

            FirstAlive = firstAlive < 0 ? Count : firstAlive;
        }

        /// <summary>
        /// Whether the item at <paramref name="index"/> is alive.
        /// </summary>
        public bool IsAlive(int index) => alive[index];

        /// <summary>
        /// Marks item at <paramref name="index"/> as dead.
        /// </summary>
        public void MarkDead(int index) => alive[index] = false;

        /// <summary>
        /// Marks all of the items in the list as alive.
        /// </summary>
        public void MarkAllAlive()
        {
            for (int i = 0; i < alive.Count; ++i)
                alive[i] = true;

            FirstAlive = 0;
        }
    }
}
