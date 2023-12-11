using System;
using System.Runtime.InteropServices;

namespace SevenZipSharpSimple.Interop
{
    /// <summary>
    /// Representation of a union that can store different value types.
    /// </summary>
    /// <remarks>
    /// DO NOT change the size of this struct. On release builds, a
    /// size of 16 led to null-reference exceptions.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit, Size = 8 + 8 + 4)]
    internal readonly struct Union
    {
        [FieldOffset(0)]
        private readonly ushort _type;
        
        [FieldOffset(8)]
        private readonly IntPtr _pointer;
        
        [FieldOffset(8)]
        private readonly int _int32;
        
        [FieldOffset(8)]
        private readonly long _int64;
        
        [FieldOffset(8)]
        private readonly uint _uint32;
        
        [FieldOffset(8)]
        private readonly ulong _uint64;
        
        /// <summary>
        /// Gets the union type.
        /// </summary>
        public VarEnum Type => (VarEnum)_type;

        /// <summary>
        /// Gets the underlying value as string. This method must only be called if
        /// the type is one of <see cref="VarEnum.VT_BSTR"/>, <see cref="VarEnum.VT_EMPTY"/>
        /// or <see cref="VarEnum.VT_NULL"/>.
        /// </summary>
        /// <returns>The underlying value as managed string or <see cref="string.Empty"/>,
        /// if the value is not a string.</returns>
        public string AsString()
        {
            switch (Type)
            {
                case VarEnum.VT_BSTR:
                    return Detail.StringMarshal.BinaryStringToManagedString(_pointer);

                default:
                    return string.Empty;
            }
        }
        
        /// <summary>
        /// Gets the underlying value as <see cref="DateTime"/>. This method must only be called if
        /// the type is one of <see cref="VarEnum.VT_FILETIME"/>, <see cref="VarEnum.VT_EMPTY"/>
        /// or <see cref="VarEnum.VT_NULL"/>.
        /// </summary>
        /// <returns>The underlying filetime as <see cref="DateTime"/>. If the <see cref="Type"/>
        /// is not compatible, <see cref="DateTime.Now"/> is being returned.</returns>
        public DateTime AsDateTime()
        {
            switch (Type)
            {
                case VarEnum.VT_FILETIME:
                case VarEnum.VT_I8:
                case VarEnum.VT_UI8:
                    return DateTime.FromFileTime(_int64);

                default:
                    return DateTime.UtcNow;
            }
        }
        
        /// <summary>
        /// Gets the underlying value as 64 bit signed integer. This method must only be called if
        /// the type is one of <see cref="VarEnum.VT_I4"/>, <see cref="VarEnum.VT_I8"/>,
        /// <see cref="VarEnum.VT_UI4"/>, <see cref="VarEnum.VT_UI8"/>, <see cref="VarEnum.VT_EMPTY"/>
        /// or <see cref="VarEnum.VT_NULL"/>.
        /// </summary>
        /// <returns>The underlying value as 64 bit signed integer or 0, if the underlying
        /// value is not a number.</returns>
        public long AsInt64()
        {
            switch (Type)
            {
                case VarEnum.VT_EMPTY:
                case VarEnum.VT_NULL:
                    return 0L;

                case VarEnum.VT_I4:
                    return _int32;

                case VarEnum.VT_I8:
                    return _int64;

                case VarEnum.VT_UI4:
                    return _uint32;

                case VarEnum.VT_UI8:
                    return (long)_uint64;

                default:
                    return 0L;
            }
        }

        /// <summary>
        /// Gets the underlying value as 64 bit unsigned integer. This method must only be called if
        /// the type is one of <see cref="VarEnum.VT_I4"/>, <see cref="VarEnum.VT_I8"/>,
        /// <see cref="VarEnum.VT_UI4"/>, <see cref="VarEnum.VT_UI8"/>, <see cref="VarEnum.VT_EMPTY"/>
        /// or <see cref="VarEnum.VT_NULL"/>.
        /// </summary>
        /// <returns>The underlying value as 64 bit unsigned integer or 0, if the underlying
        /// type is not a number.</returns>
        public ulong AsUInt64()
        {
            switch (Type)
            {
                case VarEnum.VT_I4:
                    return (ulong)_int32;
                
                case VarEnum.VT_I8:
                    return (ulong)_int64;
                
                case VarEnum.VT_UI4:
                    return _uint32;
                
                case VarEnum.VT_UI8:
                    return _uint64;
                
                default:
                    return 0UL;
            }
        }

        /// <summary>
        /// Gets the underlying value as 32 bit unsigned integer. This method must only be called if
        /// the type is one of <see cref="VarEnum.VT_I4"/>, <see cref="VarEnum.VT_I8"/>,
        /// <see cref="VarEnum.VT_UI4"/>, <see cref="VarEnum.VT_UI8"/>, <see cref="VarEnum.VT_EMPTY"/>
        /// or <see cref="VarEnum.VT_NULL"/>.
        /// </summary>
        /// <returns>The underlying value as 32 bit unsigned integer.</returns>
        public uint AsUInt32() => (uint)AsUInt64();

        /// <summary>
        /// Gets the underlying value as 32 bit signed integer. This method must only be called if
        /// the type is one of <see cref="VarEnum.VT_I4"/>, <see cref="VarEnum.VT_I8"/>,
        /// <see cref="VarEnum.VT_UI4"/>, <see cref="VarEnum.VT_UI8"/>, <see cref="VarEnum.VT_EMPTY"/>
        /// or <see cref="VarEnum.VT_NULL"/>.
        /// </summary>
        /// <returns>The underlying value as 32 bit signed integer.</returns>
        public int AsInt32() => (int)AsInt64();
        
        /// <summary>
        /// Gets the underlying value as boolean value. This method must only be called if
        /// the type is one of <see cref="VarEnum.VT_BOOL"/>, <see cref="VarEnum.VT_I4"/>,
        /// <see cref="VarEnum.VT_I8"/>, <see cref="VarEnum.VT_UI4"/>, <see cref="VarEnum.VT_UI8"/>,
        /// <see cref="VarEnum.VT_EMPTY"/> or <see cref="VarEnum.VT_NULL"/>.
        /// </summary>
        /// <returns>The underlying value as boolean value.</returns>
        public bool AsBool()
        {
            return Type == VarEnum.VT_BOOL ? _int32 != 0 : AsUInt64() != 0UL;
        }
    }
}
