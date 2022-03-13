using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue_Tracking_System
{
    public class SuperDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> dictionary;
        private Queue<TKey> keys;
        private int capacity;

        public SuperDictionary(int capacity)
        {
            this.keys = new Queue<TKey>(5);
            this.capacity = capacity;
            this.dictionary = new Dictionary<TKey, TValue>(5);
        }

        public void Add(TKey key, TValue value)
        {
            if (dictionary.Count == capacity)
            {
                var oldestKey = keys.Dequeue();
                dictionary.Remove(oldestKey);
            }

            dictionary.Add(key, value);
            keys.Enqueue(key);
        }

        public TValue this[TKey key]
        {
            get { return dictionary[key]; }
        }
    }
}
