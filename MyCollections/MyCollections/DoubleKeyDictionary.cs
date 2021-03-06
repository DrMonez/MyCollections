﻿using System;
using System.Collections.Generic;

namespace MyCollections
{
    public class DoubleKeyDictionary<TKeyId, TKeyName, TValue> : IDoubleKeyDictionary<TKeyId, TKeyName, TValue>
    {
        private Keys<TKeyId, TKeyName> _keys = new Keys<TKeyId, TKeyName>();
        private Dictionary<long, TValue> _values = new Dictionary<long, TValue>();
        private IDGenerator<(TKeyId,TKeyName)> _idGenerator = new IDGenerator<(TKeyId, TKeyName)>(); 

        public int Count => _values.Count;

        public ICollection<TKeyId> IdKeys => _keys.IdKeys;

        public ICollection<TKeyName> NameKeys => _keys.NameKeys;

        public ICollection<TValue> Values => _values.Values;

        public TValue this[TKeyId id, TKeyName name]
        {
            get
            {
                if (id == null)
                {
                    throw new ArgumentNullException("id");
                }
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }

                var key = _idGenerator.GetId((id, name), out bool isFirst);
                if (!isFirst)
                {
                    return _values[key];
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }

        public DoubleKeyDictionary() { }

        public DoubleKeyDictionary(TKeyId id, TKeyName name, TValue value)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var key = _idGenerator.GetId((id, name), out bool isFirst);
            _values.Add(key, value);
        }

        public void Add(TKeyId id, TKeyName name, TValue value)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            
            var mainId = _idGenerator.GetId((id, name), out bool isFirst);
            if (!isFirst)
            {
                throw new ArgumentException();
            }
            _keys.Add(id, name);
            _values.Add(mainId, value);
        }

        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
        }

        public void Remove(TKeyId id, TKeyName name)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var key = _idGenerator.GetId((id, name), out bool isFirst);
            if(!isFirst)
            {
                _values.Remove(key);
                _keys.Remove(id, name);
                _idGenerator.Remove((id, name));
            }
        }

        public Dictionary<TKeyName, TValue> GetById(TKeyId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            return GetBy<TKeyName, TKeyId>("id", id);
        }

        public Dictionary<TKeyId, TValue> GetByName(TKeyName name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return GetBy<TKeyId, TKeyName>("name", name);
        }

        private Dictionary<T1, TValue> GetBy<T1,T2>(string type, T2 key)
        {
            var res = new Dictionary<T1, TValue>();

            if (!_keys.TryGetValue(type, key, out List<T1> idList))
            {
                return res;
            }
            foreach (var id in idList)
            {
                var mainKey = type == "id" ? (object)(key, id) : (object)(id, key);
                var currentId = _idGenerator.GetId(((TKeyId,TKeyName))mainKey, out bool isFirst);
                if (!isFirst) res.Add(id, _values[currentId]);
            }
            return res;
        }
    }
}
