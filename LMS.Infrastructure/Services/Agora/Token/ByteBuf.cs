using System;
using System.Collections.Generic;

namespace LMS.Infrastructure.Services.Agora.Token
{
    public class ByteBuf
    {
        private readonly ByteBuffer _buffer;

        public ByteBuf()
        {
            _buffer = new ByteBuffer();
        }

        public ByteBuf(byte[] source)
        {
            _buffer = new ByteBuffer(source);
        }

        public byte[] asBytes()
        {
            return _buffer.ToByteArray();
        }

        public ByteBuf put(ushort v)
        {
            _buffer.PushUInt16(v);
            return this;
        }

        public ByteBuf put(uint v)
        {
            _buffer.PushUInt32(v);
            return this;
        }

        public ByteBuf put(byte[] v)
        {
            put((ushort)v.Length);
            _buffer.PushByteArray(v);
            return this;
        }

        public ByteBuf copy(byte[] v)
        {
            _buffer.PushByteArray(v);
            return this;
        }

        public ByteBuf putIntMap(Dictionary<ushort, uint> extra)
        {
            put((ushort)extra.Count);
            foreach (var item in extra)
            {
                put(item.Key);
                put(item.Value);
            }
            return this;
        }

        public ushort readShort()
        {
            return _buffer.PopUInt16();
        }

        public uint readInt()
        {
            return _buffer.PopUInt32();
        }

        public byte[] readBytes()
        {
            ushort length = readShort();
            return _buffer.PopByteArray(length);
        }

        public Dictionary<ushort, uint> readIntMap()
        {
            var map = new Dictionary<ushort, uint>();
            ushort length = readShort();
            for (short i = 0; i < length; ++i)
            {
                ushort k = readShort();
                uint v = readInt();
                map.Add(k, v);
            }
            return map;
        }
    }
}

