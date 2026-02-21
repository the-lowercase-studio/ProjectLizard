using System;
using System.Collections.Generic;

namespace Assets.CustomEventArgs
{
    public class EnumerableCollectionChangeEventArgs<TItem> : EventArgs
    {
        public IEnumerable<TItem> CollectionAfterChange { get; private set; }

        public EnumerableCollectionChangeEventArgs(IEnumerable<TItem> collectionAfterChange)
        {
            CollectionAfterChange = collectionAfterChange;
        }
    }
}
