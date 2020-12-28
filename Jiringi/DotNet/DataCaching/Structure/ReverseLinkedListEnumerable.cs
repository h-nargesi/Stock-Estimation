using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class ReverseLinkedListEnumerable<T> : IReverseEnumerable<T>,  IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
    {
        public ReverseLinkedListEnumerable(LinkedList<T> linked_list)
        {
            this.linked_list = linked_list ?? throw new ArgumentNullException(nameof(linked_list));
        }

        private readonly LinkedList<T> linked_list;
        public int Count => linked_list.Count;

        public IEnumerator<T> GetEnumerator()
        {
            var point = linked_list.Last;
            while (point != null)
            {
                yield return point.Value;
                point = point.Previous;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
