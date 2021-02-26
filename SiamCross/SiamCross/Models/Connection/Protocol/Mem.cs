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

    public interface IMemItem
    {
        UInt32 Address { get; }
        string Name { get; }
        IMemValue Data{ get; }
        UInt32 Size { get; }
    }

    public class MemVar: IMemItem , IMemValue
    {
        readonly protected IMemValue _Data;
        readonly private MemStruct _Parent;
        readonly private string _Name;

        public MemStruct Parent => _Parent;
        public IMemValue Data => _Data;
        public string Name => _Name;
        public UInt32 Size => _Data.Size;

        public MemVar(MemStruct parent, IMemValue data, string name = null)
        {
            _Data = data;
            _Name = name;
            _Parent = parent;
        }
        public UInt32 Address => (null == _Parent) ? 0 : _Parent.GetOffset(this) + _Parent.Address;

        
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


    public class MemVarUInt32: MemVar
    {
        public MemVarUInt32(MemStruct parent, MemValueUInt32 data, string name = null)
            : base(parent, data, name)
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
        public MemVarUInt16(MemStruct parent, MemValueUInt16 data, string name = null)
            : base(parent, data, name)
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
        public MemVarInt16(MemStruct parent, MemValueInt16 data, string name = null)
            : base(parent, data, name)
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
        public MemVarFloat(MemStruct parent, MemValueFloat data, string name = null)
            : base(parent, data, name)
        { }
        public float Value
        {
            set
            {
                if (_Data is MemValueFloat dd)
                    dd.Value=value;
            }
            get
            {
                if (_Data is MemValueFloat dd)
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

        protected UInt32 _Address = 0;
        protected UInt32 _Size = 0;
        public UInt32 Size => _Size;
        public UInt32 Address => _Address;

        public MemStruct(UInt32 address)
        {
            _Address = address;
        }


        public MemVarUInt32 Add(MemValueUInt32 item, string name = null)
        {
            var mv = new MemVarUInt32(this, item, name);
            _Store.Add(mv);
            _Size += item.Size;
            return mv;
        }
        public MemVarUInt16 Add(MemValueUInt16 item, string name = null)
        {
            var mv = new MemVarUInt16(this, item, name);
            _Store.Add(mv);
            _Size += item.Size;
            return mv;
        }
        public MemVarInt16 Add(MemValueInt16 item, string name = null)
        {
            var mv = new MemVarInt16(this, item, name);
            _Store.Add(mv);
            _Size += item.Size;
            return mv;
        }
        public MemVarFloat Add(MemValueFloat item, string name = null)
        {
            var mv = new MemVarFloat(this, item, name);
            _Store.Add(mv);
            _Size += item.Size;
            return mv;
        }
        public MemVar Add(MemVar item, string name = null)
        {
            var mv = new MemVar(this, item.Data, name);
            _Store.Add(mv);
            _Size += item.Size;
            return mv;
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
