#region using directives

using System;
using System.Globalization;
using System.Runtime.Serialization;

#endregion

namespace AsciiImportExport
{
    /// <summary>
    /// Creates parsing and serializing functions for built-in types
    /// A lot of this classes logic was copied and customized from the ServiceStack.Text project
    /// </summary>
    /// <typeparam name="TRet">The type of the columns data</typeparam>
    internal static class ServiceStackTextHelpers
    {
        /// <summary>
        /// Returns a parsing function for built-in types
        /// Copied from ServiceStack.Text and customized to meet demands of this project
        /// </summary>
        /// <param name="stringFormat">The string format used for DateTime parsing</param>
        /// <param name="booleanTrue">The string that identifies a boolean true value</param>
        public static Func<string, object> GetParseFn<TRet>(string stringFormat, string booleanTrue, IFormatProvider provider)
        {
            //Note the generic typeof(T) is faster than using var type = typeof(T)
            if (typeof (TRet) == typeof (string))
                return value => value;
            if (typeof (TRet) == typeof (bool))
                return value => value == booleanTrue;
            if (typeof (TRet) == typeof (byte))
                return value => byte.Parse(value);
            if (typeof (TRet) == typeof (sbyte))
                return value => sbyte.Parse(value);
            if (typeof (TRet) == typeof (short))
                return value => short.Parse(value);
            if (typeof (TRet) == typeof (int))
                return value => int.Parse(value);
            if (typeof (TRet) == typeof (long))
                return value => long.Parse(value);
            if (typeof (TRet) == typeof (float))
                return value => float.Parse(value, provider);
            if (typeof (TRet) == typeof (double))
                return value => double.Parse(value, provider);
            if (typeof (TRet) == typeof (decimal))
                return value => decimal.Parse(value, provider);

            if (typeof (TRet) == typeof (Guid))
                return value => new Guid(value);
            if (typeof (TRet) == typeof (DateTime))
                return value => DateTime.ParseExact(value, stringFormat, provider);
            if (typeof (TRet) == typeof (TimeSpan))
                return value => TimeSpan.Parse(value);

            if (typeof (TRet) == typeof (char))
            {
                char cValue;
                return value => char.TryParse(value, out cValue) ? cValue : '\0';
            }
            if (typeof (TRet) == typeof (ushort))
                return value => ushort.Parse(value);
            if (typeof (TRet) == typeof (uint))
                return value => uint.Parse(value);
            if (typeof (TRet) == typeof (ulong))
                return value => ulong.Parse(value);

            if (typeof (TRet) == typeof (bool?))
                return value => value == null ? (bool?) null : value == booleanTrue;
            if (typeof (TRet) == typeof (byte?))
                return value => value == null ? (byte?) null : byte.Parse(value);
            if (typeof (TRet) == typeof (sbyte?))
                return value => value == null ? (sbyte?) null : sbyte.Parse(value);
            if (typeof (TRet) == typeof (short?))
                return value => value == null ? (short?) null : short.Parse(value);
            if (typeof (TRet) == typeof (int?))
                return value => value == null ? (int?) null : int.Parse(value);
            if (typeof (TRet) == typeof (long?))
                return value => value == null ? (long?) null : long.Parse(value);
            if (typeof (TRet) == typeof (float?))
                return value => value == null ? (float?)null : float.Parse(value, provider);
            if (typeof (TRet) == typeof (double?))
                return value => value == null ? (double?)null : double.Parse(value, provider);
            if (typeof (TRet) == typeof (decimal?))
                return value => value == null ? (decimal?)null : decimal.Parse(value, provider);

            if (typeof (TRet) == typeof (DateTime?))
                return value => value == null ? (DateTime?)null : DateTime.ParseExact(value, stringFormat, provider);
            if (typeof (TRet) == typeof (TimeSpan?))
                return value => value == null ? (TimeSpan?) null : TimeSpan.Parse(value);
            if (typeof (TRet) == typeof (Guid?))
                return value => value == null ? (Guid?) null : new Guid(value);
            if (typeof (TRet) == typeof (ushort?))
                return value => value == null ? (ushort?) null : ushort.Parse(value);
            if (typeof (TRet) == typeof (uint?))
                return value => value == null ? (uint?) null : uint.Parse(value);
            if (typeof (TRet) == typeof (ulong?))
                return value => value == null ? (ulong?) null : ulong.Parse(value);

            if (typeof (TRet) == typeof (char?))
            {
                char cValue;
                return value => value == null ? (char?) null : char.TryParse(value, out cValue) ? cValue : '\0';
            }

            return null;
        }

        /// <summary>
        /// Returns a parsing function for built-in types
        /// Copied from ServiceStack.Text and customized to meet demands of this project
        /// </summary>
        /// <param name="stringFormat">The string format used for formatting of DateTime or numeric values</param>
        /// <param name="booleanTrue">The string that identifies a boolean true value</param>
        /// <param name="booleanFalse">The string that identifies a boolean false value</param>
        public static Func<object, string> GetSerializeFunc<TRet>(string stringFormat, string booleanTrue, string booleanFalse, IFormatProvider provider)
        {
            if (typeof (TRet) == typeof (byte)
                || typeof (TRet) == typeof (short)
                || typeof (TRet) == typeof (ushort)
                || typeof (TRet) == typeof (int)
                || typeof (TRet) == typeof (uint)
                || typeof (TRet) == typeof (long)
                || typeof (TRet) == typeof (ulong)
                )
                return v => v.ToString();

            if (typeof (TRet) == typeof (bool))
                return v => ((bool) v) ? booleanTrue : booleanFalse;


            if (typeof (TRet) == typeof (DateTime))
                return v => ((DateTime) v).ToString(stringFormat);


            if (typeof (TRet) == typeof (Guid))
                return v => ((Guid) v).ToString("N");

            if (typeof (TRet) == typeof (float))
                return v => ((float)v).ToString(stringFormat, provider);


            if (typeof (TRet) == typeof (double))
                return v => ((double)v).ToString(stringFormat, provider);


            if (typeof (TRet) == typeof (decimal))
                return v => ((decimal)v).ToString(stringFormat, provider);


            if (typeof (TRet) == typeof (byte?)
                || typeof (TRet) == typeof (short?)
                || typeof (TRet) == typeof (ushort?)
                || typeof (TRet) == typeof (int?)
                || typeof (TRet) == typeof (uint?)
                || typeof (TRet) == typeof (long?)
                || typeof (TRet) == typeof (ulong?)
                )
                return v => v == null ? "" : v.ToString();

            if (typeof (TRet) == typeof (bool?))
                return v => v == null ? "" : (((bool) v) ? booleanTrue : booleanFalse);


            if (typeof (TRet) == typeof (DateTime?))
                return v => v == null ? "" : ((DateTime) v).ToString(stringFormat);

            if (typeof (TRet) == typeof (Guid?))
                return v => v == null ? "" : ((Guid) v).ToString("N");


            if (typeof (TRet) == typeof (float?))
                return v => v == null ? "" : ((float)v).ToString(stringFormat, provider);


            if (typeof (TRet) == typeof (double?))
                return v => v == null ? "" : ((double)v).ToString(stringFormat, provider);


            if (typeof (TRet) == typeof (decimal?))
                return v => v == null ? "" : ((decimal)v).ToString(stringFormat, provider);


            if (typeof (TRet).IsEnum || typeof (TRet).UnderlyingSystemType.IsEnum)
                return typeof (TRet).GetCustomAttributes(typeof (FlagsAttribute), false).Length > 0 ? (Func<object, string>) WriteEnumFlags : v => WriteEnum(v);

            return v => v.ToString();
        }

        private static string WriteEnum(object enumValue)
        {
            if (enumValue == null) return "";
            return enumValue.ToString();
        }

        private static string WriteEnumFlags(object enumFlagValue)
        {
            if (enumFlagValue == null) return "";
            var intVal = (int) enumFlagValue;
            return intVal.ToString();
        }

        public static EmptyCtorDelegate GetConstructorMethodToCache(Type type)
        {
            var emptyCtor = type.GetConstructor(Type.EmptyTypes);
            if (emptyCtor != null)
            {

#if MONOTOUCH || c|| XBOX
				return () => Activator.CreateInstance(type);
                
#elif WINDOWS_PHONE
                return Expression.Lambda<EmptyCtorDelegate>(Expression.New(type)).Compile();
#else
#if SILVERLIGHT
                var dm = new System.Reflection.Emit.DynamicMethod("MyCtor", type, Type.EmptyTypes);
#else
                var dm = new System.Reflection.Emit.DynamicMethod("MyCtor", type, Type.EmptyTypes, typeof(ServiceStackTextHelpers).Module, true);
#endif
                var ilgen = dm.GetILGenerator();
                ilgen.Emit(System.Reflection.Emit.OpCodes.Nop);
                ilgen.Emit(System.Reflection.Emit.OpCodes.Newobj, emptyCtor);
                ilgen.Emit(System.Reflection.Emit.OpCodes.Ret);

                return (EmptyCtorDelegate)dm.CreateDelegate(typeof(EmptyCtorDelegate));
#endif
            }

#if (SILVERLIGHT && !WINDOWS_PHONE) || XBOX
            return () => Activator.CreateInstance(type);
#elif WINDOWS_PHONE
            return Expression.Lambda<EmptyCtorDelegate>(Expression.New(type)).Compile();
#else
            //Anonymous types don't have empty constructors
            return () => FormatterServices.GetUninitializedObject(type);
#endif
        }

        public delegate EmptyCtorDelegate EmptyCtorFactoryDelegate(Type type);
        public delegate object EmptyCtorDelegate();
    }
}