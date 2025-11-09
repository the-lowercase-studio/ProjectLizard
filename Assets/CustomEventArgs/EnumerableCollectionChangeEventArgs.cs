using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Assets.CustomEventArgs
{
    public class EnumerableCollectionChangeEventArgs<TItem> : EventArgs
    {
        public ImmutableArray<TItem> CollectionAfterChange { get; private set; }

        public EnumerableCollectionChangeEventArgs(IEnumerable<TItem> collectionAfterChange)
        {
            CollectionAfterChange = collectionAfterChange.ToImmutableArray();
        }
    }
}
