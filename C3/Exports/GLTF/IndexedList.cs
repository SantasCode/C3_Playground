using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.Exports.GLTF
{ 
    internal class IndexedList<T> : IList<T> where T : IndexedItem
    {

        //Backing List class
        private List<T> _list;

        public IndexedList()
        {
            _list = new List<T>();
        }

        public T this[int index] { get => _list[index]; set => _list[index] = value; }

        public int Count => _list.Count;

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T item)
        {
            item.Index = _list.Count;
            _list.Add(item);
        }

        public void Clear() => _list.Clear();

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        public int IndexOf(T item) => _list.IndexOf(item);

        public void Insert(int index, T item) => throw new NotSupportedException();

        public bool Remove(T item) => throw new NotSupportedException();

        public void RemoveAt(int index) => throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
