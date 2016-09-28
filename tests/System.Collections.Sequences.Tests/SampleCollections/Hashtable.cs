﻿using System.Collections.Generic;

namespace System.Collections.Sequences
{
    public sealed class Hashtable<K, V> : ISequence<KeyValuePair<K, V>>
    {
        const double Slack = 0.2;
        const int MinCapacity = 4;

        Entry[] _entries = new Entry[0];
        EqualityComparer<K> _comparer;
        int _count;
        int _capacity;

        struct Entry
        {
            public KeyValuePair<K, V> _pair;
            public int _code;

            public bool IsEmpty { get { return _code == 0; } }
        }

        public Hashtable(EqualityComparer<K> comparer) : this(comparer, 0)
        { }

        public Hashtable(EqualityComparer<K> comparer, int capacity)
        {
            _comparer = comparer;
            if (capacity > 0) {
                int size = GetNextPrime(capacity + (int)(Slack * capacity));
                _entries = new Entry[size];
                _capacity = _entries.Length - (int)(Slack * _entries.Length);
            }
        }

        public bool Add(K key, V value)
        {
            if (_count >= _capacity) {
                EnsureCapacity(_capacity == 0 ? MinCapacity : _capacity * 2);
            }
            _count++;
            var code = _comparer.GetHashCode(key);
            if (code == 0) code = 1;

            int bucket = code;
            while (true) {
                int index = bucket % _entries.Length;
                if (_entries[index].IsEmpty) {
                    _entries[index]._code = code;
                    _entries[index]._pair = new KeyValuePair<K, V>(key, value);
                    return false;
                }
                if (_comparer.Equals(_entries[index]._pair.Key, key)) {
                    throw new InvalidOperationException("key already exists");
                }
                bucket++;
            }
        }

        public void EnsureCapacity(int capacity)
        {
            var newTable = new Hashtable<K, V>(_comparer, capacity);
            foreach (var entry in _entries) {
                if (entry.IsEmpty) continue;
                newTable.Add(entry._pair.Key, entry._pair.Value);
            }
            _entries = newTable._entries;
            _capacity = newTable._capacity;
        }

        static int[] s_primes = new int[] { 5, 11, 19, 37, 83, 157, 311, 613, 1231, 2539, 5009,
            10009, 20011, 40009, 80021, 160001, 320009, 640007, 1280023, 2500009, 5000011, 10000019, 20000003
        };
        private int GetNextPrime(int value)
        {
            foreach (var prime in s_primes) {
                if (prime >= value) return prime;
            }

            // TODO: implement
            throw new NotImplementedException();
        }

        public int Length {
            get {
                return _count;
            }
        }

        public KeyValuePair<K, V> TryGetItem(ref Position position)
        {
            if (!position.IsValid) throw new InvalidOperationException();

            var index = position.IntegerPosition;
            while (index < _entries.Length) {
                var entry = _entries[index];
                if (entry.IsEmpty) { index++; continue; }
                position.IntegerPosition = index + 1;
                return entry._pair;
            }

            position = Position.Invalid;
            return default(KeyValuePair<K, V>);
        }

        public SequenceEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return new SequenceEnumerator<KeyValuePair<K, V>>(this);
        }
    }
}