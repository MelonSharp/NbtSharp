using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace NbtSharp
{
    /// <summary>
    ///     Represents a <see cref="BinaryReader" /> wrapper that takes care of reading primitives from an NBT stream, while
    ///     taking care of endianness, string encoding, and skipping.
    /// </summary>
    public class NbtBinaryReader : BinaryReader
    {
        private const int SeekBufferSize = 8 * 1024;
        private readonly byte[] _stringConversionBuffer = new byte[64];
        private readonly byte[] _buffer = new byte[sizeof(double)];
        private readonly bool _swapNeeded;
        private byte[]? _seekBuffer;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="NbtBinaryReader" /> class.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="bigEndian">A value indicating whether the expected input is in big endian order.</param>
        public NbtBinaryReader(Stream input, bool bigEndian)
            : base(input)
        {
            _swapNeeded = (BitConverter.IsLittleEndian == bigEndian);
        }
        
        /// <summary>
        ///     Gets or sets the tag selector for this stream.
        /// </summary>
        /// <value>A <see cref="TagSelector" /> representing the tag selector, or <see langword="null" />.</value>
        public TagSelector? Selector { get; set; }

        public NbtTagType ReadTagType()
        {
            int type = ReadByte();
            return type switch
            {
                < 0 => throw new EndOfStreamException(),
                > (int)NbtTagType.IntArray => throw new NbtFormatException("NBT tag type out of range: " + type),
                _ => (NbtTagType)type
            };
        }

        /// <inheritdoc />
        public override short ReadInt16()
        {
            var value = base.ReadInt16();
            return _swapNeeded ? Swap(value) : value;
        }

        /// <inheritdoc />
        public override int ReadInt32()
        {
            var value = base.ReadInt32();
            return _swapNeeded ? Swap(value) : value;
        }

        /// <inheritdoc />
        public override long ReadInt64()
        {
            var value = base.ReadInt64();
            return _swapNeeded ? Swap(value) : value;
        }

        /// <inheritdoc />
        public override float ReadSingle() {
            if (!_swapNeeded)
                return base.ReadSingle();
            
            FillBuffer(sizeof(float));
            Array.Reverse(_buffer, 0, sizeof(float));
            return BitConverter.ToSingle(_buffer, 0);
        }

        /// <inheritdoc />
        public override double ReadDouble() {
            if (!_swapNeeded)
                return base.ReadDouble();
            
            FillBuffer(sizeof(double));
            Array.Reverse(_buffer);
            return BitConverter.ToDouble(_buffer, 0);
        }

        /// <inheritdoc />
        public override string ReadString()
        {
            var length = ReadInt16();
            
            if (length < 0)
                throw new NbtFormatException("Negative string length given!");
            
            if (length < _stringConversionBuffer.Length)
            {
                var stringBytesRead = 0;
                
                while (stringBytesRead < length)
                {
                    var bytesToRead = length - stringBytesRead;
                    var bytesReadThisTime = BaseStream.Read(_stringConversionBuffer, stringBytesRead, bytesToRead);
                    
                    if (bytesReadThisTime == 0)
                        throw new EndOfStreamException();
                    
                    stringBytesRead += bytesReadThisTime;
                }
                
                return Encoding.UTF8.GetString(_stringConversionBuffer, 0, length);
            }
            else
            {
                byte[] stringData = ReadBytes(length);
                
                if (stringData.Length < length)
                    throw new EndOfStreamException();
                
                return Encoding.UTF8.GetString(stringData);
            }
        }

        /// <summary>
        ///     Skips the position of the base stream forward by a specified number of bytes.
        /// </summary>
        /// <param name="bytesToSkip">The number of bytes to skip forward.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bytesToSkip" /> is less than <c>0</c>.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream was reached before the operation completed.</exception>
        public void Skip(int bytesToSkip)
        {
            if (bytesToSkip < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesToSkip));

            if (bytesToSkip == 0)
                return;
            
            if (BaseStream.CanSeek)
            {
                BaseStream.Position += bytesToSkip;
                return;
            }
            
            _seekBuffer ??= new byte[SeekBufferSize];
                
            var bytesSkipped = 0;
            while (bytesSkipped < bytesToSkip)
            {
                var bytesToRead = Math.Min(SeekBufferSize, bytesToSkip - bytesSkipped);
                var bytesRead = BaseStream.Read(_seekBuffer, 0, bytesToRead);
                
                if (bytesRead == 0)
                    throw new EndOfStreamException();
                
                bytesSkipped += bytesRead;
            }
        }

        /// <inheritdoc />
        protected override void FillBuffer(int numBytes)
        {
            var offset = 0;
            
            do
            {
                var bytesRead = BaseStream.Read(_buffer, offset, numBytes - offset);
                
                if (bytesRead == 0)
                    throw new EndOfStreamException();
                
                offset += bytesRead;
            } while (offset < numBytes);
        }

        public void SkipString()
        {
            var length = ReadInt16();
            
            if (length < 0)
                throw new NbtFormatException("Negative string length given!");
            
            Skip(length);
        }

        [DebuggerStepThrough]
        private static short Swap(short v) {
            unchecked
            {
                return (short)((v >> 8) & 0x00FF |
                               (v << 8) & 0xFF00);
            }
        }

        [DebuggerStepThrough]
        private static int Swap(int v)
        {
            unchecked
            {
                var v2 = (uint)v;
                return (int)((v2 >> 24) & 0x000000FF |
                             (v2 >> 8) & 0x0000FF00 |
                             (v2 << 8) & 0x00FF0000 |
                             (v2 << 24) & 0xFF000000);
            }
        }


        [DebuggerStepThrough]
        private static long Swap(long v) {
            unchecked
            {
                return (Swap((int)v) & uint.MaxValue) << 32 |
                       Swap((int)(v >> 32)) & uint.MaxValue;
            }
        }
    }
}
