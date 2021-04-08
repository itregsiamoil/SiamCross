using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SiamCross.Models.Connection.Protocol
{
    public interface IMemValue
    {
        byte[] ToArray();
        void ToArray(byte[] dst, int start = 0);
        bool FromArray(byte[] array, UInt32 start = 0);
        UInt32 Size { get; }
    }
    public class MemValueUInt32 : IMemValue
    {
        public UInt32 Size => sizeof(UInt32);
        public UInt32 Value;
        public void ToArray(byte[] dst, int start = 0)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(dst, Value);
        }
        public byte[] ToArray()
        {
            return BitConverter.GetBytes(Value);
        }
        public bool FromArray(byte[] array, UInt32 start)
        {
            Value = BitConverter.ToUInt32(array, (int)start);
            return true;
        }
    }
    public class MemValueUInt16 : IMemValue
    {
        public UInt32 Size => sizeof(UInt16);
        public UInt16 Value;
        public void ToArray(byte[] dst, int start = 0)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(dst, Value);
        }
        public byte[] ToArray()
        {
            return BitConverter.GetBytes(Value);
        }
        public bool FromArray(byte[] array, UInt32 start)
        {
            Value = BitConverter.ToUInt16(array, (int)start);
            return true;
        }
    }
    public class MemValueInt16 : IMemValue
    {
        public UInt32 Size => sizeof(Int16);
        public Int16 Value;
        public void ToArray(byte[] dst, int start = 0)
        {
            BinaryPrimitives.WriteInt16LittleEndian(dst, Value);
        }
        public byte[] ToArray()
        {
            return BitConverter.GetBytes(Value);
        }
        public bool FromArray(byte[] array, UInt32 start)
        {
            Value = BitConverter.ToInt16(array, (int)start);
            return true;
        }
    }
    public class MemValueFloat : IMemValue
    {
        public UInt32 Size => sizeof(float);
        public float Value;
        public void ToArray(byte[] dst, int start = 0)
        {
            BitConverter.GetBytes(Value).CopyTo(dst, start);
        }
        public byte[] ToArray()
        {
            return BitConverter.GetBytes(Value);
        }
        public bool FromArray(byte[] array, UInt32 start)
        {
            Value = BitConverter.ToSingle(array, (int)start);
            return true;
        }
    }
    public class MemValueByteArray : IMemValue
    {
        private readonly UInt32 _Size;
        public UInt32 Size => _Size;
        public byte[] Value;

        public MemValueByteArray(UInt32 size = 0)
        {
            Value = new byte[size];
            _Size = size;
        }

        public void ToArray(byte[] dst, int start = 0)
        {
            Value.CopyTo(dst, 0);
        }
        public byte[] ToArray()
        {
            var clone = new byte[_Size];
            Value.CopyTo(clone, 0);
            return clone;
        }
        public bool FromArray(byte[] array, UInt32 start)
        {
            var len = array.Length - start;
            if (len > Value.Length)
                len = Value.Length;
            Array.Copy(array, start, Value, 0, len);
            return true;
        }
    }
    public class MemValueUInt8 : IMemValue
    {
        public UInt32 Size => sizeof(byte);
        public byte Value;
        public void ToArray(byte[] dst, int start = 0)
        {
            dst[start] = Value;
        }
        public byte[] ToArray()
        {
            return new byte[] { Value };
        }
        public bool FromArray(byte[] array, UInt32 start)
        {
            Value = array[start];
            return true;
        }
    }



    public interface IMemItem
    {
        UInt32 Address { get; set; }
        string Name { get; set; }
        IMemValue Data { get; }
        UInt32 Size { get; }
    }

    public class MemVar : IMemItem, IMemValue
    {
        protected readonly IMemValue _Data;
        private UInt32 _Address;
        private string _Name;

        public UInt32 Address { get => _Address; set => _Address = value; }
        public string Name { get => _Name; set => _Name = value; }
        public IMemValue Data => _Data;
        public UInt32 Size => _Data.Size;

        public MemVar(IMemValue data, string name = null, UInt32 addr = 0)
        {
            _Data = data;
            _Name = name;
            _Address = addr;
        }

        public byte[] ToArray()
        {
            return _Data.ToArray();
        }
        public void ToArray(byte[] dst, int start = 0)
        {
            _Data.ToArray(dst, start);
        }
        public bool FromArray(byte[] array, uint start = 0)
        {
            return _Data.FromArray(array, start);
        }
    }
    public class MemVarUInt32 : MemVar
    {
        public MemVarUInt32(string name = null, UInt32 addr = 0, MemValueUInt32 data = null)
            : base(data ?? new MemValueUInt32(), name, addr)
        { }
        public UInt32 Value
        {
            set
            {
                if (_Data is MemValueUInt32 dd)
                    dd.Value = value;
            }
            get
            {
                if (_Data is MemValueUInt32 dd)
                    return dd.Value;
                return default;
            }
        }
    }
    public class MemVarUInt16 : MemVar
    {
        public MemVarUInt16(string name = null, UInt32 addr = 0, MemValueUInt16 data = null)
            : base(data ?? new MemValueUInt16(), name, addr)
        { }
        public UInt16 Value
        {
            set
            {
                if (_Data is MemValueUInt16 dd)
                    dd.Value = value;
            }
            get
            {
                if (_Data is MemValueUInt16 dd)
                    return dd.Value;
                return default;
            }
        }
    }
    public class MemVarInt16 : MemVar
    {
        public MemVarInt16(string name = null, UInt32 addr = 0, MemValueInt16 data = null)
            : base(data ?? new MemValueInt16(), name, addr)
        { }
        public Int16 Value
        {
            set
            {
                if (_Data is MemValueInt16 dd)
                    dd.Value = value;
            }
            get
            {
                if (_Data is MemValueInt16 dd)
                    return dd.Value;
                return default;
            }
        }
    }
    public class MemVarFloat : MemVar
    {
        public MemVarFloat(string name = null, UInt32 addr = 0, MemValueFloat data = null)
            : base(data ?? new MemValueFloat(), name, addr)
        { }
        public float Value
        {
            set
            {
                if (_Data is MemValueFloat dd)
                    dd.Value = value;
            }
            get
            {
                if (_Data is MemValueFloat dd)
                    return dd.Value;
                return default;
            }
        }
    }
    public class MemVarByteArray : MemVar
    {
        public MemVarByteArray(string name = null, UInt32 addr = 0, MemValueByteArray data = null)
            : base(data ?? new MemValueByteArray(0), name, addr)
        { }
        public byte[] Value
        {
            set
            {
                if (_Data is MemValueByteArray dd)
                    dd.Value = value;
            }
            get
            {
                if (_Data is MemValueByteArray dd)
                    return dd.Value;
                return default;
            }
        }
    }
    public class MemVarUInt8 : MemVar
    {
        public MemVarUInt8(string name = null, UInt32 addr = 0, MemValueUInt8 data = null)
            : base(data ?? new MemValueUInt8(), name, addr)
        { }
        public byte Value
        {
            set
            {
                if (_Data is MemValueUInt8 dd)
                    dd.Value = value;
            }
            get
            {
                if (_Data is MemValueUInt8 dd)
                    return dd.Value;
                return default;
            }
        }
    }



    public class MemStruct
    {
        private class MemVarStore : KeyedCollection<string, MemVar>
        {
            protected override string GetKeyForItem(MemVar item)
            {
                // In this example, the key is the part number.
                return item.Name;
            }
        }
        private readonly MemVarStore _Store = new MemVarStore();

        private string _Name;
        protected UInt32 _Address = 0;
        protected UInt32 _Size = 0;

        public string Name { get => _Name; set => _Name = value; }
        public UInt32 Size => _Size;
        public UInt32 Address
        {
            get => _Address;
            set
            {
                _Address = value;
                var offset = _Address;
                foreach (var v in _Store)
                {
                    v.Address = offset;
                    offset += v.Size;
                }
            }
        }


        public MemStruct(UInt32 address)
        {
            _Address = address;
        }


        public T Add<T>(T item)
        {
            if (item is MemVar mv)
            {
                _Store.Add(mv);
                mv.Address = _Address + _Size;
                _Size += mv.Size;
                return item;
            }
            return default;
        }

        public void Reset(UInt32 address)
        {
            _Address = address;
            _Store.Clear();
            _Size = 0;
        }

        public void ToArray(byte[] dst, int start)
        {
            UInt32 sz = 0;
            foreach (var v in _Store)
            {
                v.ToArray().CopyTo(dst, sz + start);
                sz += v.Size;
            }
        }

        public bool FromArray(byte[] array, UInt32 start = 0)
        {
            UInt32 sz = 0;
            foreach (var v in _Store)
            {
                v.FromArray(array, start + sz);
                sz += v.Size;
            }
            return true;
        }

        public UInt32 GetOffset(MemVar item)
        {
            UInt32 offset = 0;
            foreach (var v in _Store)
            {
                if (v == item)
                    break;
                offset += v.Size;
            }
            return offset;
        }

        public IReadOnlyList<MemVar> GetVars()
        {
            return _Store;
        }
    }
}
