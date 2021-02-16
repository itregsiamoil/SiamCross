using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace SiamCross.Models.Connection.Protocol
{
    public interface MemItem
    {
        UInt32 Address { get; }
        UInt32 Size { get; }
        string Name { get; }

        void ToArray(byte[] dst, int start = 0);
        byte[] ToArray();
        bool FromArray(byte[] array, UInt32 start = 0);
    }

    public abstract class MemVar : MemItem
    {
        private MemStruct _Parent;
        public MemVar(UInt32 address, MemStruct parent = null, string name = null)
        {
            _Address = address;
            _Parent = parent;
            if (null != _Parent)
                _Parent.Add(this, name);
        }
        protected UInt32 _Address;
        public UInt32 Address => (null == _Parent) ? _Address : _Parent.GetOffset(this) + _Parent.Address;

        public string Name
        {
            get
            {
                if (null == _Parent)
                    return null;
                return _Parent.GetName(this);
            }
        }

        public abstract UInt32 Size { get; }

        public void SetParent(MemStruct parent)
        {
            _Parent = parent;
        }

        public abstract void ToArray(byte[] dst, int start = 0);
        public virtual byte[] ToArray()
        {
            byte[] dst = new byte[Size];
            ToArray(dst, 0);
            return dst;
        }
        public abstract bool FromArray(byte[] array, UInt32 start);
    }


    public class MemVarUInt32 : MemVar
    {
        public override UInt32 Size => sizeof(UInt32);
        public UInt32 Value;
        public MemVarUInt32(UInt32 address = 0, MemStruct parent = null, string name = null)
            : base(address, parent, name)
        { }

        public override void ToArray(byte[] dst, int start = 0)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(dst, Value);
        }
        public override byte[] ToArray()
        {
            return BitConverter.GetBytes(Value);
        }
        public override bool FromArray(byte[] array, UInt32 start)
        {
            Value = BitConverter.ToUInt32(array, (int)start);
            return true;
        }
    }
    public class MemVarUInt16 : MemVar
    {
        public override UInt32 Size => sizeof(UInt16);
        public UInt16 Value;
        public MemVarUInt16(UInt32 address = 0, MemStruct parent = null, string name = null)
            : base(address, parent, name)
        { }
        public override void ToArray(byte[] dst, int start = 0)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(dst, Value);
        }
        public override byte[] ToArray()
        {
            return BitConverter.GetBytes(Value);
        }
        public override bool FromArray(byte[] array, UInt32 start)
        {
            Value = BitConverter.ToUInt16(array, (int)start);
            return true;
        }
    }
    public class MemVarInt16 : MemVar
    {
        public override UInt32 Size => sizeof(Int16);
        public Int16 Value;
        public MemVarInt16(UInt32 address = 0, MemStruct parent = null, string name = null)
            : base(address, parent, name)
        { }
        public override void ToArray(byte[] dst, int start = 0)
        {
            BinaryPrimitives.WriteInt16LittleEndian(dst, Value);
        }
        public override byte[] ToArray()
        {
            return BitConverter.GetBytes(Value);
        }
        public override bool FromArray(byte[] array, UInt32 start)
        {
            Value = BitConverter.ToInt16(array, (int)start);
            return true;
        }
    }
    public class MemVarFloat : MemVar
    {
        public override UInt32 Size => sizeof(float);
        public float Value;
        public MemVarFloat(UInt32 address = 0, MemStruct parent = null, string name = null)
            : base(address, parent, name)
        { }
        public override void ToArray(byte[] dst, int start = 0)
        {
            BitConverter.GetBytes(Value).CopyTo(dst, start);
        }
        public override byte[] ToArray()
        {
            return BitConverter.GetBytes(Value);
        }
        public override bool FromArray(byte[] array, UInt32 start)
        {
            Value = BitConverter.ToSingle(array, (int)start);
            return true;
        }
    }


    public class MemStruct : MemVar
    {
        protected UInt32 _Size = 0;
        public override UInt32 Size => _Size;

        private readonly Dictionary<MemItem, string> _Var = new Dictionary<MemItem, string>();

        public MemStruct(UInt32 address, MemStruct parent = null, string name = null)
            : base(address, parent, name)
        {
        }

        public T Add<T>(T item, string name = null)
        {
            if (!(item is MemVar vv))
                return default(T);
            _Var.Add(vv, name);
            vv.SetParent(this);
            _Size += vv.Size;
            return item;
        }

        public void Reset(UInt32 address)
        {
            _Address = address;
            _Var.Clear();
        }

        public override void ToArray(byte[] dst, int start)
        {
            UInt32 sz = 0;
            foreach (KeyValuePair<MemItem, string> v in _Var)
            {
                v.Key.ToArray().CopyTo(dst, sz + start);
                sz += v.Key.Size;
            }
        }

        public override bool FromArray(byte[] array, UInt32 start = 0)
        {
            UInt32 sz = 0;
            foreach (KeyValuePair<MemItem, string> v in _Var)
            {
                v.Key.FromArray(array, start + sz);
                sz += v.Key.Size;
            }
            return true;
        }

        public string GetName(MemVar item)
        {
            return _Var[item];
        }
        public UInt32 GetOffset(MemVar item)
        {
            UInt32 offset = 0;
            foreach (KeyValuePair<MemItem, string> v in _Var)
            {
                if (v.Key == item)
                    break;
                offset += v.Key.Size;
            }
            return offset;
        }

        public IReadOnlyDictionary<MemItem, string> GetVars()
        {
            return _Var;
        }
    }
}
