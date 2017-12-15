using System;
using System.Collections;
using System.Collections.Generic;

namespace Portal.Utils
{
    /**
     * https://gist.github.com/ufcpp/2b3e1a5821169f6b21ded175ad05c752
     */
    public static class TupleEnumerable
    {
        public struct IndexedEnumerable<T> : IEnumerable<(T item, int index)>
        {
            private readonly IEnumerable<T> _e;

            public IndexedEnumerable(IEnumerable<T> e)
            {
                _e = e;
            }

            public IndexedEnumerator<T> GetEnumerator() => new IndexedEnumerator<T>(_e.GetEnumerator());

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<(T item, int index)> IEnumerable<(T item, int index)>.GetEnumerator() => GetEnumerator();
        }

        public struct IndexedEnumerator<T> : IEnumerator<(T item, int index)>
        {
            public (T item, int index) Current => (_e.Current, _i);

            private int _i;
            readonly IEnumerator<T> _e;

            internal IndexedEnumerator(IEnumerator<T> e)
            {
                _i = -1;
                _e = e;
            }

            public bool MoveNext()
            {
                _i++;
                return _e.MoveNext();
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        public static IndexedEnumerable<T> Indexed<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return new IndexedEnumerable<T>(source);
        }
    }
}