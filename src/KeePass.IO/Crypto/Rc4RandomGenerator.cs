using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace KeePass.Crypto
{
    public class Rc4RandomGenerator : IRandomGenerator
    {
        private readonly byte[] _state;

        private byte _i;
        private byte _j;

        public Rc4RandomGenerator(IBuffer key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            
            _state = new byte[256];
            var keyLength = key.Length;

            for (uint w = 0; w < 256; ++w)
                _state[w] = (byte)w;

            unchecked
            {
                byte j = 0;
                uint keyIndex = 0;

                for (uint w = 0; w < 256; ++w) // Key setup
                {
                    j += (byte)(_state[w] + key.GetByte(keyIndex));

                    var temp = _state[0];
                    _state[0] = _state[j];
                    _state[j] = temp;

                    ++keyIndex;
                    if (keyIndex >= keyLength)
                        keyIndex = 0;
                }
            }

            GetRandomBytes(512);
        }

        /// <summary>
        /// Get a buffer of random bytes of the specified size.
        /// </summary>
        /// <param name="size">Size of the bytes buffer.</param>
        /// <returns>
        /// Random bytes buffer.
        /// </returns>
        public IBuffer GetRandomBytes(int size)
        {
            var result = new byte[size];

            if (size > 0)
            {
                unchecked
                {
                    for (uint w = 0; w < size; ++w)
                    {
                        ++_i;
                        _j += _state[_i];

                        var temp = _state[_i]; // Swap entries
                        _state[_i] = _state[_j];
                        _state[_j] = temp;

                        temp = (byte)(_state[_i] + _state[_j]);
                        result[w] = _state[temp];
                    }
                }
            }

            return result.AsBuffer();
        }
    }
}