using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace NbtSharp
{
    /// <summary>
    ///     Represents a <see cref="BinaryWriter" /> wrapper that takes care of reading primitives from an NBT stream, while
    ///     taking care of endianness, string encoding, and counting bytes written.
    /// </summary>
    public class NbtBinaryWriter : BinaryWriter
    {
        /// <summary>
        ///     The maximum writing chunk size. This value is constant. 
        /// </summary>
        /// <value>4194304, equivalent to 4 MiB.</value>
        public const int MaxWriteChunk = 4 * 1024 * 1024; // Write at most 4 MiB at a time.
        
        private const int BufferSize = 256;
        private const int MaxBufferedStringLength = BufferSize / 4; // UTF-8 chars use at most 4 bytes each
        private static readonly UTF8Encoding Encoding = new(false, true);
        private readonly Encoder _encoder = Encoding.GetEncoder();
        private readonly byte[] _buffer = new byte[BufferSize];
        private readonly Stream _stream;
        private readonly bool _swapNeeded;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="NbtBinaryWriter" /> class.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="bigEndian">A value indicating whether output is in big endian order.</param>
        /// <exception cref="ArgumentException">The stream does not support writing or is already closed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="output" /> is <see langword="null" />.</exception>
        public NbtBinaryWriter(Stream output, bool bigEndian)
            : base(output)
        {
            _stream = output;
            _swapNeeded = (BitConverter.IsLittleEndian == bigEndian);
        }

        /// <summary>
        ///     Gets the underlying stream of the <see cref="NbtBinaryWriter" />.
        /// </summary>
        /// <value>The underlying stream associated with the <see cref="NbtBinaryWriter" />.</value>
        /// <remarks>When accessing this value, the underlying stream is flushed.</remarks>
        public override Stream BaseStream
        {
            get
            {
                _stream.Flush();
                return _stream;
            }
        }

        /// <summary>
        ///     Writes an <see cref="NbtTagType" /> value as represented by an unsigned byte to the current stream and advances
        ///     the stream position by one byte. 
        /// </summary>
        /// <param name="value">The <see cref="NbtTagType" /> to write.</param>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public void Write(NbtTagType value)
        {
            base.Write((byte)value);
        }

        /// <inheritdoc />
        public override void Write(short value)
        {
            unchecked
            {
                if (_swapNeeded)
                {
                    _buffer[0] = (byte)(value >> 8);
                    _buffer[1] = (byte)value;
                }
                else
                {
                    _buffer[0] = (byte)value;
                    _buffer[1] = (byte)(value >> 8);
                }
            }
            _stream.Write(_buffer, 0, 2);
        }


        /// <inheritdoc />
        public override void Write(int value)
        {
            unchecked
            {
                if (_swapNeeded)
                {
                    _buffer[0] = (byte)(value >> 24);
                    _buffer[1] = (byte)(value >> 16);
                    _buffer[2] = (byte)(value >> 8);
                    _buffer[3] = (byte)value;
                }
                else
                {
                    _buffer[0] = (byte)value;
                    _buffer[1] = (byte)(value >> 8);
                    _buffer[2] = (byte)(value >> 16);
                    _buffer[3] = (byte)(value >> 24);
                }
            }
            _stream.Write(_buffer, 0, 4);
        }


        /// <inheritdoc />
        public override void Write(long value)
        {
            unchecked
            {
                if (_swapNeeded)
                {
                    _buffer[0] = (byte)(value >> 56);
                    _buffer[1] = (byte)(value >> 48);
                    _buffer[2] = (byte)(value >> 40);
                    _buffer[3] = (byte)(value >> 32);
                    _buffer[4] = (byte)(value >> 24);
                    _buffer[5] = (byte)(value >> 16);
                    _buffer[6] = (byte)(value >> 8);
                    _buffer[7] = (byte)value;
                }
                else
                {
                    _buffer[0] = (byte)value;
                    _buffer[1] = (byte)(value >> 8);
                    _buffer[2] = (byte)(value >> 16);
                    _buffer[3] = (byte)(value >> 24);
                    _buffer[4] = (byte)(value >> 32);
                    _buffer[5] = (byte)(value >> 40);
                    _buffer[6] = (byte)(value >> 48);
                    _buffer[7] = (byte)(value >> 56);
                }
            }
            _stream.Write(_buffer, 0, 8);
        }

        /// <inheritdoc />
        public override void Write(float value)
        {
            unsafe
            {
                ulong tmpValue = *(uint*)&value;
                unchecked {
                    if (_swapNeeded) {
                        _buffer[0] = (byte)(tmpValue >> 24);
                        _buffer[1] = (byte)(tmpValue >> 16);
                        _buffer[2] = (byte)(tmpValue >> 8);
                        _buffer[3] = (byte)tmpValue;
                    } else {
                        _buffer[0] = (byte)tmpValue;
                        _buffer[1] = (byte)(tmpValue >> 8);
                        _buffer[2] = (byte)(tmpValue >> 16);
                        _buffer[3] = (byte)(tmpValue >> 24);
                    }
                }
            }
            _stream.Write(_buffer, 0, 4);
        }

        /// <inheritdoc />
        public override void Write(double value)
        {
            unsafe
            {
                ulong tmpValue = *(ulong*)&value;
                unchecked
                {
                    if (_swapNeeded)
                    {
                        _buffer[0] = (byte)(tmpValue >> 56);
                        _buffer[1] = (byte)(tmpValue >> 48);
                        _buffer[2] = (byte)(tmpValue >> 40);
                        _buffer[3] = (byte)(tmpValue >> 32);
                        _buffer[4] = (byte)(tmpValue >> 24);
                        _buffer[5] = (byte)(tmpValue >> 16);
                        _buffer[6] = (byte)(tmpValue >> 8);
                        _buffer[7] = (byte)tmpValue;
                    }
                    else
                    {
                        _buffer[0] = (byte)tmpValue;
                        _buffer[1] = (byte)(tmpValue >> 8);
                        _buffer[2] = (byte)(tmpValue >> 16);
                        _buffer[3] = (byte)(tmpValue >> 24);
                        _buffer[4] = (byte)(tmpValue >> 32);
                        _buffer[5] = (byte)(tmpValue >> 40);
                        _buffer[6] = (byte)(tmpValue >> 48);
                        _buffer[7] = (byte)(tmpValue >> 56);
                    }
                }
            }

            _stream.Write(_buffer, 0, 8);
        }

        /// <inheritdoc />
        public override void Write(string value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var byteLength = Encoding.GetByteCount(value);
            Write((short)byteLength);

            if (byteLength <= BufferSize)
            {
                // string can be written at once if it fits within the buffer size
                Encoding.GetBytes(value, 0, value.Length, _buffer, 0);
                _stream.Write(_buffer, 0, byteLength);
                return;
            }

            unsafe
            {
                // aggressively try to not allocate memory in this loop for runtime performance reasons.
                // use an Encoder to write out the string correctly (handling surrogates crossing buffer boundaries properly)  
                var charStart = 0;
                var numLeft = value.Length;
                
                while (numLeft > 0)
                {
                    // figure out how many chars to process this round.
                    var charCount = (numLeft > MaxBufferedStringLength) ? MaxBufferedStringLength : numLeft;
                    int byteLen;
                    
                    fixed (char* pChars = value)
                    {
                        fixed (byte* pBytes = _buffer)
                        {
                            byteLen = _encoder.GetBytes(pChars + charStart, charCount, pBytes, BufferSize, 
                                                        charCount == numLeft);
                        }
                    }

                    _stream.Write(_buffer, 0, byteLen);
                    charStart += charCount;
                    numLeft -= charCount;
                }
            }
        }

        /// <inheritdoc />
        public override void Write(byte[] data, int offset, int count)
        {
            var bytesWritten = 0;
            
            while (bytesWritten < count)
            {
                var bytesToWrite = Math.Min(MaxWriteChunk, count - bytesWritten);
                _stream.Write(data, offset + bytesWritten, bytesToWrite);
                bytesWritten += bytesToWrite;
            }
        }
    }
}
