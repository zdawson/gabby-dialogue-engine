using System;
using System.Collections.Generic;
using UnityEngine;

namespace PotassiumK.GabbyDialogue
{
    [Serializable]
    public class StringDictionary : ISerializableDictionary<string, string> {
        public StringDictionary()
        {
        }

        public StringDictionary(Dictionary<string, string> other) : base(other)
        {
        }
    }

    [Serializable]
    public class ISerializableDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        [Serializable]
        private class Entry
        {
            public K key;
            public V value;
        }

        [SerializeField]
        private List<Entry> _entries = new List<Entry>();

        public ISerializableDictionary()
        {
        }

        public ISerializableDictionary(Dictionary<K, V> other) : base(other)
        {
        }

        public void OnBeforeSerialize()
        {
            // Load the entries for serialization
            _entries.Clear();
            foreach (KeyValuePair<K, V> entry in this)
            {
                _entries.Add(new Entry(){key = entry.Key, value = entry.Value});
            }
        }

        public void OnAfterDeserialize()
        {
            // Clear the backing dictionary and add the data from the lists
            this.Clear();
            foreach (Entry entry in _entries)
            {
                this.Add(entry.key, entry.value);
            }
            _entries.Clear();
        }
    }
}
