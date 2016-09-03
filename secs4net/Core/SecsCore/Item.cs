﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Security.Permissions;
using System.Text;

namespace Secs4Net
{
    [DebuggerDisplay("<{Format} [{Count}] { (Format==SecsFormat.List) ? string.Empty : ToString() ,nq}>")]
    public struct Item {
       
        public SecsFormat Format { get; }

        /// <summary>
        /// item value/list count
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// item values or list of Item
        /// </summary>
        public IEnumerable Values { get; }

        public IReadOnlyList<Item> Items {
            get
            {
                if (Format != SecsFormat.List)
                    throw new InvalidOperationException("Item is not a list");

                return (IReadOnlyList<Item>)this.Values;
            }
        }

        /// <summary>
        /// get value by specific type
        /// </summary>
        /// <typeparam name="T">return value type</typeparam>
        /// <returns></returns>
        public T GetValue<T>() {
            if (Format == SecsFormat.List)
                throw new InvalidOperationException("Item is not a value item");

            if (Values is T)
                return (T)Values;

            if (Values is T[])
                return ((T[])Values)[0];

            Type valueType = Nullable.GetUnderlyingType(typeof(T));
            if (valueType != null && Values.GetType().GetElementType() == valueType)
                return ((IEnumerable)Values).Cast<T>().First();

            throw new InvalidOperationException("Item value type is incompatible");
        }

        /// <summary>
        /// get value by specific type
        /// </summary>
        /// <typeparam name="T">return value type</typeparam>
        /// <returns></returns>
        public T GetValueOrDefault<T>() {
            if (Format == SecsFormat.List)
                throw new InvalidOperationException("Item is not a value item");

            if (Values is T)
                return (T)Values;

            if (Values is T[])
                return ((T[])Values).FirstOrDefault();

            Type valueType = Nullable.GetUnderlyingType(typeof(T));
            if (valueType != null && Values.GetType().GetElementType() == valueType)
                return ((IEnumerable)Values).Cast<T>().FirstOrDefault();

            throw new InvalidOperationException("Item value type is incompatible");
        }

        public override string ToString() => $"<{Format} [{ Count}] {(Format == SecsFormat.List ? "..." : string.Join(" ", Values.Cast<object>())) } >";

        /// <summary>
        /// if Format is List RawBytes is only header bytes.
        /// otherwise include header and data bytes.
        /// </summary>
        internal readonly ArraySegment<byte> RawData;

        #region Constructor
        /// <summary>
        /// List
        /// </summary>
        Item(IReadOnlyList<Item> items) {
            Format = SecsFormat.List;
            Values = items;
            Count = items.Count;
            int _;
            RawData = new ArraySegment<byte>(Format.EncodeItem(Count, out _));
        }

        /// <summary>
        /// U2,U4,U8
        /// I1,I2,I4,I8
        /// F4,F8
        /// Boolean
        /// </summary>
        Item(SecsFormat format, Array value) {
            Format = format;
            Values = value;
            Count = value.Length;

            Array val = (Array)Values;
            int bytelength = Buffer.ByteLength(val);
            int headerLength;
            byte[] result = Format.EncodeItem(bytelength, out headerLength);
            Buffer.BlockCopy(val, 0, result, headerLength, bytelength);
            result.Reverse(headerLength, headerLength + bytelength, bytelength / val.Length);
            RawData = new ArraySegment<byte>(result);
        }

        /// <summary>
        /// A,J
        /// </summary>
        Item(SecsFormat format, string value, Encoding encoder) {
            Format = format;
            Values = value;
            Count = value.Length;
            string str = (string)Values;
            int headerLength;
            byte[] result = Format.EncodeItem(str.Length, out headerLength);
            encoder.GetBytes(str, 0, str.Length, result, headerLength);
            RawData = new ArraySegment<byte>(result);
            
        }

        /// <summary>
        /// Empty Item(none List)
        /// </summary>
        /// <param name="format"></param>
        /// <param name="value"></param>
        Item(SecsFormat format, IEnumerable value) {
            Format = format;
            Values = value;
            Count = 0;
            RawData = new ArraySegment<byte>(new byte[] { (byte)((byte)Format | 1), 0 });
        }
        #endregion

        #region Type Casting Operator
        public static explicit operator string(Item item) => item.GetValueOrDefault<string>();
        public static explicit operator byte(Item item) => item.GetValue<byte>();
        public static explicit operator sbyte(Item item) => item.GetValue<sbyte>();
        public static explicit operator ushort(Item item) => item.GetValue<ushort>();
        public static explicit operator short(Item item) => item.GetValue<short>();
        public static explicit operator uint(Item item) => item.GetValue<uint>();
        public static explicit operator int(Item item) => item.GetValue<int>();
        public static explicit operator ulong(Item item) => item.GetValue<ulong>();
        public static explicit operator long(Item item) => item.GetValue<long>();
        public static explicit operator float(Item item) => item.GetValue<float>();
        public static explicit operator double(Item item) => item.GetValue<double>();
        public static explicit operator bool(Item item) => item.GetValue<bool>();
        public static explicit operator byte? (Item item) => item.GetValueOrDefault<byte?>();
        public static explicit operator sbyte? (Item item) => item.GetValueOrDefault<sbyte?>();
        public static explicit operator ushort? (Item item) => item.GetValueOrDefault<ushort?>();
        public static explicit operator short? (Item item) => item.GetValueOrDefault<short?>();
        public static explicit operator uint? (Item item) => item.GetValueOrDefault<uint?>();
        public static explicit operator int? (Item item) => item.GetValueOrDefault<int?>();
        public static explicit operator ulong? (Item item) => item.GetValueOrDefault<ulong?>();
        public static explicit operator long? (Item item) => item.GetValueOrDefault<long?>();
        public static explicit operator float? (Item item) => item.GetValueOrDefault<float?>();
        public static explicit operator double? (Item item) => item.GetValueOrDefault<double?>();
        public static explicit operator bool? (Item item) => item.GetValueOrDefault<bool?>();
        public static explicit operator byte[] (Item item) => item.GetValue<byte[]>();
        public static explicit operator sbyte[] (Item item) => item.GetValue<sbyte[]>();
        public static explicit operator ushort[] (Item item) => item.GetValue<ushort[]>();
        public static explicit operator short[] (Item item) => item.GetValue<short[]>();
        public static explicit operator uint[] (Item item) => item.GetValue<uint[]>();
        public static explicit operator int[] (Item item) => item.GetValue<int[]>();
        public static explicit operator ulong[] (Item item) => item.GetValue<ulong[]>();
        public static explicit operator long[] (Item item) => item.GetValue<long[]>();
        public static explicit operator float[] (Item item) => item.GetValue<float[]>();
        public static explicit operator double[] (Item item) => item.GetValue<double[]>();
        public static explicit operator bool[] (Item item) => item.GetValue<bool[]>();
        #endregion

        #region Factory Methods
        internal static Item L(IList<Item> items) => new Item(new ReadOnlyCollection<Item>(items));
        /// <summary>
        /// Create list
        /// </summary>
        /// <param name="items">sub item</param>
        /// <returns></returns>
        public static Item L(IEnumerable<Item> items) => items.Any() ? L(items.ToList()) : L();

        /// <summary>
        /// Create list
        /// </summary>
        /// <param name="items">sub item</param>
        /// <returns></returns>
        public static Item L(params Item[] items) => L((IList<Item>)items);

        /// <summary>
        /// Create binary item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item B(params byte[] value) => new Item(SecsFormat.Binary, value);

        /// <summary>
        /// Create binary item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item B(IEnumerable<byte> value) => value.Any() ? B(value.ToArray()) : B();

        /// <summary>
        /// Create unsigned integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item U1(params byte[] value) => new Item(SecsFormat.U1, value);

        /// <summary>
        /// Create unsigned integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item U1(IEnumerable<byte> value) => value.Any() ? U1(value.ToArray()) : U1();

        /// <summary>
        /// Create unsigned integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item U2(params ushort[] value) => new Item(SecsFormat.U2, value);

        /// <summary>
        /// Create unsigned integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item U2(IEnumerable<ushort> value) => value.Any() ? U2(value.ToArray()) : U2();

        /// <summary>
        /// Create unsigned integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item U4(params uint[] value) => new Item(SecsFormat.U4, value);

        /// <summary>
        /// Create unsigned integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item U4(IEnumerable<uint> value) => value.Any() ? U4(value.ToArray()) : U4();

        /// <summary>
        /// Create unsigned integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item U8(params ulong[] value) => new Item(SecsFormat.U8, value);

        /// <summary>
        /// Create unsigned integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item U8(IEnumerable<ulong> value) => value.Any() ? U8(value.ToArray()) : U8();

        /// <summary>
        /// Create signed integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item I1(params sbyte[] value) => new Item(SecsFormat.I1, value);

        /// <summary>
        /// Create signed integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item I1(IEnumerable<sbyte> value) => value.Any() ? I1(value.ToArray()) : I1();

        /// <summary>
        /// Create signed integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item I2(params short[] value) => new Item(SecsFormat.I2, value);

        /// <summary>
        /// Create signed integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item I2(IEnumerable<short> value) => value.Any() ? I2(value.ToArray()) : I2();

        /// <summary>
        /// Create signed integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item I4(params int[] value) => new Item(SecsFormat.I4, value);

        /// <summary>
        /// Create signed integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item I4(IEnumerable<int> value) => value.Any() ? I4(value.ToArray()) : I4();

        /// <summary>
        /// Create signed integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item I8(params long[] value) => new Item(SecsFormat.I8, value);

        /// <summary>
        /// Create signed integer item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item I8(IEnumerable<long> value) => value.Any() ? I8(value.ToArray()) : I8();

        /// <summary>
        /// Create floating point number item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item F4(params float[] value) => new Item(SecsFormat.F4, value);

        /// <summary>
        /// Create floating point number item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item F4(IEnumerable<float> value) => value.Any() ? F4(value.ToArray()) : F4();

        /// <summary>
        /// Create floating point number item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item F8(params double[] value) => new Item(SecsFormat.F8, value);

        /// <summary>
        /// Create floating point number item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item F8(IEnumerable<double> value) => value.Any() ? F8(value.ToArray()) : F8();

        /// <summary>
        /// Create boolean item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item Boolean(params bool[] value) => new Item(SecsFormat.Boolean, value);

        /// <summary>
        /// Create boolean item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item Boolean(IEnumerable<bool> value) => value.Any() ? Boolean(value.ToArray()) : Boolean();

        /// <summary>
        /// Create string item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Item A(string value) => value != string.Empty ? new Item(SecsFormat.ASCII, value, Encoding.ASCII) : A();

        /// <summary>
        /// Create string item
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("this is special format, make sure you really need it.")]
        public static Item J(string value) => value != string.Empty ? new Item(SecsFormat.JIS8, value, JIS8Encoding) : J();
        #endregion

        #region Share Object

        public static Item L() => EmptyL;
        public static Item B() => EmptyBinary;
        public static Item U1() => EmptyU1;
        public static Item U2() => EmptyU2;
        public static Item U4() => EmptyU4;
        public static Item U8() => EmptyU8;
        public static Item I1() => EmptyI1;
        public static Item I2() => EmptyI2;
        public static Item I4() => EmptyI4;
        public static Item I8() => EmptyI8;
        public static Item F4() => EmptyF4;
        public static Item F8() => EmptyF8;
        public static Item Boolean() => EmptyBoolean;
        public static Item A() => EmptyA;
        public static Item J() => EmptyJ;

        static readonly Item EmptyL = new Item(new ReadOnlyCollection<Item>(new Item[0]));
        static readonly Item EmptyA = new Item(SecsFormat.ASCII, string.Empty);
        static readonly Item EmptyJ = new Item(SecsFormat.JIS8, string.Empty);
        static readonly Item EmptyBoolean = new Item(SecsFormat.Boolean, Enumerable.Empty<bool>());
        static readonly Item EmptyBinary = new Item(SecsFormat.Binary, Enumerable.Empty<byte>());
        static readonly Item EmptyU1 = new Item(SecsFormat.U1, Enumerable.Empty<byte>());
        static readonly Item EmptyU2 = new Item(SecsFormat.U2, Enumerable.Empty<ushort>());
        static readonly Item EmptyU4 = new Item(SecsFormat.U4, Enumerable.Empty<uint>());
        static readonly Item EmptyU8 = new Item(SecsFormat.U8, Enumerable.Empty<ulong>());
        static readonly Item EmptyI1 = new Item(SecsFormat.I1, Enumerable.Empty<sbyte>());
        static readonly Item EmptyI2 = new Item(SecsFormat.I2, Enumerable.Empty<short>());
        static readonly Item EmptyI4 = new Item(SecsFormat.I4, Enumerable.Empty<int>());
        static readonly Item EmptyI8 = new Item(SecsFormat.I8, Enumerable.Empty<long>());
        static readonly Item EmptyF4 = new Item(SecsFormat.F4, Enumerable.Empty<float>());
        static readonly Item EmptyF8 = new Item(SecsFormat.F8, Enumerable.Empty<double>());

        internal static readonly Encoding JIS8Encoding = Encoding.GetEncoding(50222);
        #endregion
    }
}