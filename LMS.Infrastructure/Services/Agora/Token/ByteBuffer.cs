using System;

namespace LMS.Infrastructure.Services.Agora.Token
{
    public class ByteBuffer
    {
        private const int INITIAL_CAPACITY = 1024;
        private byte[] _tempByteArray = new byte[INITIAL_CAPACITY];
        private int _currentLength = 0;
        private int _currentPosition = 0;

        public ByteBuffer()
        {
            Initialize();
        }

        public ByteBuffer(byte[] bytes)
        {
            Initialize();
            PushByteArray(bytes);
        }

        public int Length => _currentLength;

        public int Position
        {
            get => _currentPosition;
            set => _currentPosition = value;
        }

        public byte[] ToByteArray()
        {
            var returnArray = new byte[_currentLength];
            Array.Copy(_tempByteArray, 0, returnArray, 0, _currentLength);
            return returnArray;
        }

        public void Initialize()
        {
            Array.Clear(_tempByteArray, 0, _tempByteArray.Length);
            _currentLength = 0;
            _currentPosition = 0;
        }

        private void EnsureCapacity(int additionalLength)
        {
            int requiredLength = _currentLength + additionalLength;
            if (requiredLength <= _tempByteArray.Length)
            {
                return;
            }

            int capacity = _tempByteArray.Length;
            while (capacity < requiredLength)
            {
                capacity *= 2;
            }

            Array.Resize(ref _tempByteArray, capacity);
        }

        public void PushByte(byte b)
        {
            EnsureCapacity(1);
            _tempByteArray[_currentLength++] = b;
        }

        public void PushByteArray(byte[] byteArray)
        {
            EnsureCapacity(byteArray.Length);
            byteArray.CopyTo(_tempByteArray, _currentLength);
            _currentLength += byteArray.Length;
        }

        public void PushUInt16(ushort num)
        {
            EnsureCapacity(2);
            _tempByteArray[_currentLength++] = (byte)(num & 0xff);
            _tempByteArray[_currentLength++] = (byte)((num >> 8) & 0xff);
        }

        public void PushUInt32(uint num)
        {
            EnsureCapacity(4);
            _tempByteArray[_currentLength++] = (byte)(num & 0xff);
            _tempByteArray[_currentLength++] = (byte)((num >> 8) & 0xff);
            _tempByteArray[_currentLength++] = (byte)((num >> 16) & 0xff);
            _tempByteArray[_currentLength++] = (byte)((num >> 24) & 0xff);
        }

        public byte PopByte()
        {
            return _tempByteArray[_currentPosition++];
        }

        public ushort PopUInt16()
        {
            if (_currentPosition + 1 >= _currentLength) return 0;
            ushort ret = (ushort)(_tempByteArray[_currentPosition] | (_tempByteArray[_currentPosition + 1] << 8));
            _currentPosition += 2;
            return ret;
        }

        public uint PopUInt32()
        {
            if (_currentPosition + 3 >= _currentLength) return 0;
            uint ret = (uint)(_tempByteArray[_currentPosition] |
                             (_tempByteArray[_currentPosition + 1] << 8) |
                             (_tempByteArray[_currentPosition + 2] << 16) |
                             (_tempByteArray[_currentPosition + 3] << 24));
            _currentPosition += 4;
            return ret;
        }

        public byte[] PopByteArray(int length)
        {
            if (_currentPosition + length > _currentLength) return Array.Empty<byte>();
            byte[] ret = new byte[length];
            Array.Copy(_tempByteArray, _currentPosition, ret, 0, length);
            _currentPosition += length;
            return ret;
        }
    }
}

