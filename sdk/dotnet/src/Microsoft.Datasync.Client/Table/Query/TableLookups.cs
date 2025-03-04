﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table.Query.Nodes;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Datasync.Client.Table.Query
{
    /// <summary>
    /// The list of supported types for constants.
    /// </summary>
    internal enum ConstantType
    {
        Unknown,
        Null,
        Boolean,
        Byte,
        Character,
        DateTime,
        DateTimeOffset,
        Decimal,
        Double,
        Guid,
        Float,
        Int,
        Long,
        Short,
        SignedByte,
        UnsignedInt,
        UnsignedLong,
        UnsignedShort
    }

    /// <summary>
    /// A set of table lookups, implemented as extension methods.
    /// </summary>
    internal static class TableLookups
    {
        private const string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffZ";

        private static readonly Dictionary<long, ConstantType> ConstantTypeLookupTable = new()
        {
            { (long)typeof(bool).TypeHandle.Value, ConstantType.Boolean },
            { (long)typeof(byte).TypeHandle.Value, ConstantType.Byte },
            { (long)typeof(char).TypeHandle.Value, ConstantType.Character },
            { (long)typeof(DateTime).TypeHandle.Value, ConstantType.DateTime },
            { (long)typeof(DateTimeOffset).TypeHandle.Value, ConstantType.DateTimeOffset },
            { (long)typeof(decimal).TypeHandle.Value, ConstantType.Decimal },
            { (long)typeof(double).TypeHandle.Value, ConstantType.Double },
            { (long)typeof(Guid).TypeHandle.Value, ConstantType.Guid },
            { (long)typeof(float).TypeHandle.Value, ConstantType.Float },
            { (long)typeof(int).TypeHandle.Value, ConstantType.Int },
            { (long)typeof(long).TypeHandle.Value, ConstantType.Long },
            { (long)typeof(short).TypeHandle.Value, ConstantType.Short },
            { (long)typeof(sbyte).TypeHandle.Value, ConstantType.SignedByte },
            { (long)typeof(uint).TypeHandle.Value, ConstantType.UnsignedInt },
            { (long)typeof(ulong).TypeHandle.Value, ConstantType.UnsignedLong },
            { (long)typeof(ushort).TypeHandle.Value, ConstantType.UnsignedShort }
        };

        /// <summary>
        /// Converts the <see cref="BinaryOperatorKind"/> to an OData operator.
        /// </summary>
        internal static string ToODataString(this BinaryOperatorKind kind) => kind switch
        {
            BinaryOperatorKind.Or => "or",
            BinaryOperatorKind.And => "and",
            BinaryOperatorKind.Equal => "eq",
            BinaryOperatorKind.NotEqual => "ne",
            BinaryOperatorKind.GreaterThan => "gt",
            BinaryOperatorKind.GreaterThanOrEqual => "ge",
            BinaryOperatorKind.LessThan => "lt",
            BinaryOperatorKind.LessThanOrEqual => "le",
            BinaryOperatorKind.Add => "add",
            BinaryOperatorKind.Subtract => "sub",
            BinaryOperatorKind.Multiply => "mul",
            BinaryOperatorKind.Divide => "div",
            BinaryOperatorKind.Modulo => "mod",
            _ => throw new NotSupportedException($"'{kind}' is not supported in a 'Where' table query expression.")
        };

        internal static string ToODataString(this ConstantNode node)
        {
            object value = node.Value;
            switch (GetConstantType(value))
            {
                case ConstantType.Null:
                    return "null";
                case ConstantType.Boolean:
                    return ((bool)value).ToString().ToLower();
                case ConstantType.Byte:
                    return $"{value:X2}";
                case ConstantType.Character:
                    string ch = (char)value == '\'' ? "''" : ((char)value).ToString();
                    return $"'{ch}'";
                case ConstantType.DateTime:
                    string dt = new DateTimeOffset(((DateTime)value).ToUniversalTime()).ToString(DateTimeFormat);
                    return $"cast({dt},Edm.DateTimeOffset)";
                case ConstantType.DateTimeOffset:
                    string dto = ((DateTimeOffset)value).ToUniversalTime().ToString(DateTimeFormat);
                    return $"cast({dto},Edm.DateTimeOffset)";
                case ConstantType.Decimal:
                    return $"{value}M";
                case ConstantType.Double:
                    string d = string.Format(CultureInfo.InvariantCulture, "{0}", value);
                    return (d.Contains("E") || d.Contains(".")) ? d : $"{d}.0";
                case ConstantType.Guid:
                    Guid guid = (Guid)value;
                    return $"cast({guid:D},Edm.Guid)";
                case ConstantType.Float:
                    return $"{value}f";
                case ConstantType.Int:
                case ConstantType.Short:
                case ConstantType.UnsignedShort:
                case ConstantType.SignedByte:
                    return $"{value}";
                case ConstantType.Long:
                case ConstantType.UnsignedInt:
                case ConstantType.UnsignedLong:
                    return $"{value}L";
                default:
                    string text = value.ToString().Replace("'", "''");
                    return $"'{text}'";
            }
        }

        /// <summary>
        /// Converts an object into the supported constant type.
        /// </summary>
        /// <param name="value">The reference value</param>
        /// <returns>The <see cref="ConstantType"/></returns>
        internal static ConstantType GetConstantType(object value)
        {
            if (value == null)
            {
                return ConstantType.Null;
            }

            long handle = (long)value.GetType().TypeHandle.Value;
            if (ConstantTypeLookupTable.ContainsKey(handle))
            {
                return ConstantTypeLookupTable[handle];
            }
            else
            {
                return ConstantType.Unknown;
            }
        }
    }
}
