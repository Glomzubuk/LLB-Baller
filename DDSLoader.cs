using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine;

namespace Baller
{
    public static class DDSUnityExtensions
    {
        public static TextureFormat GetTextureFormat(this DDSImage image)
        {
            if (image.HasFourCC)
            {
                FourCC fourCC = image.GetPixelFormatFourCC();
                if (fourCC == ("DXT1")) return TextureFormat.DXT1;
                if (fourCC == ("DXT5")) return TextureFormat.DXT5;
                throw new UnityException("Unsupported PixelFormat! " + fourCC.ToString());
            }
            throw new UnityException("Unsupported Format!");
        }
    }

    public class DDSImage
    {
        private static readonly int HeaderSize = 128;
        DDSHeader _header;
        byte[] rawData;

        public DDSHeader Header { get { return _header; } }
        public HeaderFlags headerFlags { get { return Header.Flags; } }
        public bool IsValid { get { return Header.IsValid && Header.SizeCheck(); } }
        bool CheckFlag(HeaderFlags flag) { return (Header.Flags & flag) == flag; }
        public bool HasHeight { get { return CheckFlag(HeaderFlags.HEIGHT); } }
        public int Height { get { return (int)Header.Height; } }
        public bool HasWidth { get { return CheckFlag(HeaderFlags.WIDTH); } }
        public int Width { get { return (int)Header.Width; } }
        public bool HasDepth { get { return CheckFlag(HeaderFlags.DEPTH); } }
        public int Depth { get { return (int)Header.Depth; } }
        public int MipMapCount { get { return (int)Header.MipMapCount; } }
        public bool HasFourCC { get { return Header.PixelFormat.dwFlags == PixelFormatFlags.FOURCC; } }
        public bool IsUncompressedRGB { get { return Header.PixelFormat.dwFlags == PixelFormatFlags.RGB; } }
        public DDSImage(byte[] rawData)
        {
            this.rawData = rawData;
            _header = ByteArrayToStructure<DDSHeader>(rawData);
        }
        public FourCC GetPixelFormatFourCC()
        {
            return new FourCC(Header.PixelFormat.FourCC);
        }
        public byte[] GetTextureData()
        {
            byte[] texData = new byte[rawData.Length - HeaderSize];
            GetRGBData(texData);
            return texData;
        }

        public void GetRGBData(byte[] rgbData)
        {
            System.Buffer.BlockCopy(rawData, HeaderSize, rgbData, 0, rgbData.Length);
        }

        static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DDSHeader
        {
            public UInt32 magicWord;
            public bool IsValid { get { return magicWord == 0x20534444; } }
            //magicWord is not included in the header size
            public UInt32 size;
            public bool SizeCheck() { return size == 124; }
            public HeaderFlags Flags;
            public UInt32 Height;
            public UInt32 Width;
            public UInt32 PitchOrLinearSize;
            public UInt32 Depth;
            public UInt32 MipMapCount;
            //11
            public UInt32 Reserved1;
            public UInt32 Reserved2;
            public UInt32 Reserved3;
            public UInt32 Reserved4;
            public UInt32 Reserved5;
            public UInt32 Reserved6;
            public UInt32 Reserved7;
            public UInt32 Reserved8;
            public UInt32 Reserved9;
            public UInt32 Reserved10;
            public UInt32 Reserved11;
            public DDSPixelFormat PixelFormat;
            public UInt32 dwCaps;
            public UInt32 dwCaps2;
            public UInt32 dwCaps3;
            public UInt32 dwCaps4;
            public UInt32 dwReserved2;

        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DDSPixelFormat
        {
            public UInt32 size;
            public bool SizeCheck() { return size == 32; }
            public PixelFormatFlags dwFlags;
            //string
            public UInt32 FourCC;
            public UInt32 dwRGBBitCount;
            public UInt32 dwRBitMask;
            public UInt32 dwGBitMask;
            public UInt32 dwBBitMask;
            public UInt32 dwABitMask;

        }
        [Flags]
        public enum HeaderFlags
        {
            CAPS = 0x1,
            HEIGHT = 0x2,
            WIDTH = 0x4,
            PITCH = 0x8,
            PIXELFORMAT = 0x1000,
            MIPMAPCOUNT = 0x20000,
            LINEARSIZE = 0x80000,
            DEPTH = 0x800000,
            TEXTURE = CAPS | HEIGHT | WIDTH | PIXELFORMAT,
        }
        [Flags]
        public enum PixelFormatFlags
        {
            ALPHAPIXELS = 0x1,
            ALPHA = 0x2,
            FOURCC = 0x4,
            RGB = 0x40,
            YUV = 0x200,
            LUMINANCE = 0x20000
        }
    }


    public struct FourCC
    {
        private readonly uint valueDWord;
        private readonly string valueString;

        /// <summary>
        /// Creates a new instance of <see cref="FourCC"/> with an integer value.
        /// </summary>
        /// <param name="value">Integer value of FOURCC.</param>
        public FourCC(uint value)
        {
            valueDWord = value;
            valueString = new string
                              (
                                  new[]
                                  {
                                      (char)(value & 0xFF),
                                      (char)((value & 0xFF00) >> 8),
                                      (char)((value & 0xFF0000) >> 16),
                                      (char)((value & 0xFF000000U) >> 24)
                                  }
                              );
        }

        /// <summary>
        /// Creates a new instance of <see cref="FourCC"/> with a string value.
        /// </summary>
        /// <param name="value">
        /// String value of FOURCC.
        /// Should be not longer than 4 characters, all of them are printable ASCII characters.
        /// </param>
        /// <remarks>
        /// If the value of <paramref name="value"/> is shorter than 4 characters, it is right-padded with spaces.
        /// </remarks>
        public FourCC(string value)
        {
            Contract.Requires(value != null);
            Contract.Requires(value.Length <= 4);
            // Allow only printable ASCII characters
            Contract.Requires(Contract.ForAll(value, c => ' ' <= c && c <= '~'));

            valueString = value.PadRight(4);
            valueDWord = (uint)valueString[0] + ((uint)valueString[1] << 8) + ((uint)valueString[2] << 16) + ((uint)valueString[3] << 24);
        }

        /// <summary>
        /// Returns string representation of this instance.
        /// </summary>
        /// <returns>
        /// String value if all bytes are printable ASCII characters. Otherwise, the hexadecimal representation of integer value.
        /// </returns>
        public override string ToString()
        {
            var isPrintable = valueString.All(c => ' ' <= c && c <= '~');
            return isPrintable ? valueString : valueDWord.ToString("X8");
        }

        /// <summary>
        /// Gets hash code of this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return valueDWord.GetHashCode();
        }

        /// <summary>
        /// Determines whether this instance is equal to other object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is FourCC)
            {
                return (FourCC)obj == this;
            }
            else
            {
                return base.Equals(obj);
            }
        }


        /// <summary>
        /// Converts an integer value to <see cref="FourCC"/>.
        /// </summary>
        public static implicit operator FourCC(uint value)
        {
            return new FourCC(value);
        }

        /// <summary>
        /// Converts a string value to <see cref="FourCC"/>.
        /// </summary>
        public static implicit operator FourCC(string value)
        {
            return new FourCC(value);
        }

        /// <summary>
        /// Gets the integer value of <see cref="FourCC"/> instance.
        /// </summary>
        public static explicit operator uint(FourCC value)
        {
            return value.valueDWord;
        }

        /// <summary>
        /// Gets the string value of <see cref="FourCC"/> instance.
        /// </summary>
        public static explicit operator string(FourCC value)
        {
            return value.valueString;
        }

        /// <summary>
        /// Determines whether two instances of <see cref="FourCC"/> are equal.
        /// </summary>
        public static bool operator ==(FourCC value1, FourCC value2)
        {
            return value1.valueDWord == value2.valueDWord;
        }

        /// <summary>
        /// Determines whether two instances of <see cref="FourCC"/> are not equal.
        /// </summary>
        public static bool operator !=(FourCC value1, FourCC value2)
        {
            return !(value1 == value2);
        }
    }
}