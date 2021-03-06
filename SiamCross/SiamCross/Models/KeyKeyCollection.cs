﻿using System.Collections.Generic;

namespace SiamCross.Models
{
    public class KeyKeyCollection<T>
    {
        private readonly Dictionary<T, uint> _IdxNameId = new Dictionary<T, uint>();
        private readonly Dictionary<uint, T> _IdxIdName = new Dictionary<uint, T>();
        public KeyKeyCollection() { }
        public bool TryGetId(T name, out uint idx)
        {
            if (null == name)
            {
                idx = uint.MaxValue;
                return false;
            }
            return _IdxNameId.TryGetValue(name, out idx);
        }
        public uint GetId(T name)
        {
            return _IdxNameId[name];
        }
        public bool TryGetName(uint idx, out T name)
        {
            return _IdxIdName.TryGetValue(idx, out name);
        }
        public T GetName(uint id)
        {
            return _IdxIdName[id];
        }
        public bool Add(uint idx, T name)
        {
            if (idx > int.MaxValue)
                return false;
            _IdxNameId.Add(name, idx);
            _IdxIdName.Add(idx, name);
            return true;
        }
        public void Del(uint idx)
        {
            if (TryGetName(idx, out T name))
            {
                _IdxNameId.Remove(name);
                _IdxIdName.Remove(idx);
            }
        }
        public uint Del(T name)
        {
            if (TryGetId(name, out uint idx))
            {
                _IdxNameId.Remove(name);
                _IdxIdName.Remove(idx);
                return idx;
            }
            return uint.MaxValue;
        }
        public void Clear()
        {
            _IdxNameId.Clear();
            _IdxIdName.Clear();
        }

        public IReadOnlyCollection<T> GetNames()
        {
            return _IdxIdName.Values;
        }

    }
}
