using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace AsciiImportExport
{
    /// <summary>
    // Copied from http://stackoverflow.com/questions/4474634/memberexpression-to-memberexpression
    /// </summary>
    internal static class Helpers
    {
        public static MemberExpression GetMemberExpression<TValue, T>(Expression<Func<T, TValue>> expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression.Body is MemberExpression)
            {
                return (MemberExpression) expression.Body;
            }

            if (expression.Body is UnaryExpression)
            {
                Expression operand = ((UnaryExpression) expression.Body).Operand;
                if (operand is MemberExpression)
                {
                    return (MemberExpression) operand;
                }
                if (operand is MethodCallExpression)
                {
                    return ((MethodCallExpression) operand).Object as MemberExpression;
                }
            }

            return null;
        }
    }

    //
    // https://github.com/ServiceStack/ServiceStack.Text
    // ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
    //
    // Authors:
    //   Demis Bellot (demis.bellot@gmail.com)
    //
    // Copyright 2012 ServiceStack Ltd.
    //
    // Licensed under the same terms of ServiceStack: new BSD license.
    //
    public static class StaticAccessors
    {
        public static Func<object, object> GetValueGetter(this PropertyInfo propertyInfo, Type type)
        {
#if NETFX_CORE
			var getMethodInfo = propertyInfo.GetMethod;
			if (getMethodInfo == null) return null;
			return x => getMethodInfo.Invoke(x, new object[0]);
#elif (SILVERLIGHT && !WINDOWS_PHONE) || MONOTOUCH || XBOX
			var getMethodInfo = propertyInfo.GetGetMethod();
			if (getMethodInfo == null) return null;
			return x => getMethodInfo.Invoke(x, new object[0]);
#else

            ParameterExpression instance = Expression.Parameter(typeof (object), "i");
            UnaryExpression convertInstance = Expression.TypeAs(instance, propertyInfo.DeclaringType);
            MemberExpression property = Expression.Property(convertInstance, propertyInfo);
            UnaryExpression convertProperty = Expression.TypeAs(property, typeof (object));
            return Expression.Lambda<Func<object, object>>(convertProperty, instance).Compile();
#endif
        }

        public static Func<T, object> GetValueGetter<T>(this PropertyInfo propertyInfo)
        {
#if NETFX_CORE
			var getMethodInfo = propertyInfo.GetMethod;
            if (getMethodInfo == null) return null;
			return x => getMethodInfo.Invoke(x, new object[0]);
#elif (SILVERLIGHT && !WINDOWS_PHONE) || MONOTOUCH || XBOX
			var getMethodInfo = propertyInfo.GetGetMethod();
			if (getMethodInfo == null) return null;
			return x => getMethodInfo.Invoke(x, new object[0]);
#else
            ParameterExpression instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
            MemberExpression property = Expression.Property(instance, propertyInfo);
            UnaryExpression convert = Expression.TypeAs(property, typeof (object));
            return Expression.Lambda<Func<T, object>>(convert, instance).Compile();
#endif
        }

        public static Func<T, object> GetValueGetter<T>(this FieldInfo fieldInfo)
        {
#if (SILVERLIGHT && !WINDOWS_PHONE) || MONOTOUCH || XBOX
            return x => fieldInfo.GetValue(x);
#else
            ParameterExpression instance = Expression.Parameter(fieldInfo.DeclaringType, "i");
            MemberExpression property = Expression.Field(instance, fieldInfo);
            UnaryExpression convert = Expression.TypeAs(property, typeof (object));
            return Expression.Lambda<Func<T, object>>(convert, instance).Compile();
#endif
        }

#if !XBOX
        public static Action<T, object> GetValueSetter<T>(this PropertyInfo propertyInfo)
        {
            if (typeof (T) != propertyInfo.DeclaringType)
            {
                throw new ArgumentException();
            }

            ParameterExpression instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
            ParameterExpression argument = Expression.Parameter(typeof (object), "a");
#if NETFX_CORE
            var setterCall = Expression.Call(
                instance,
                propertyInfo.SetMethod,
                Expression.Convert(argument, propertyInfo.PropertyType));
#else
            MethodCallExpression setterCall = Expression.Call(
                instance,
                propertyInfo.GetSetMethod(true),
                Expression.Convert(argument, propertyInfo.PropertyType));
#endif

            return Expression.Lambda<Action<T, object>>
                (
                    setterCall, instance, argument
                ).Compile();
        }
#endif
    }

    /// <summary>
    /// Creates parsing and serializing functions for built-in types
    /// A lot of this classes logic was copied and customized from the ServiceStack.Text project
    /// </summary>
    /// <typeparam name="TRet">The type of the columns data</typeparam>
    internal static class ServiceStackTextHelpers
    {
        public delegate object EmptyCtorDelegate();

        public delegate EmptyCtorDelegate EmptyCtorFactoryDelegate(Type type);

        public static EmptyCtorDelegate GetConstructorMethodToCache(Type type)
        {
            ConstructorInfo emptyCtor = type.GetConstructor(Type.EmptyTypes);
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
                var dm = new DynamicMethod("MyCtor", type, Type.EmptyTypes, typeof (ServiceStackTextHelpers).Module, true);
#endif
                ILGenerator ilgen = dm.GetILGenerator();
                ilgen.Emit(OpCodes.Nop);
                ilgen.Emit(OpCodes.Newobj, emptyCtor);
                ilgen.Emit(OpCodes.Ret);

                return (EmptyCtorDelegate) dm.CreateDelegate(typeof (EmptyCtorDelegate));
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

        /// <summary>
        /// Returns a parsing function for built-in types
        /// Copied from ServiceStack.Text and customized to meet demands of this project
        /// </summary>
        /// <param name="type"></param>
        /// <param name="stringFormat">The string format used for DateTime parsing</param>
        /// <param name="booleanTrue">The string that identifies a boolean true value</param>
        /// <param name="provider"></param>
        public static Func<string, object> GetParseFn(Type type, string stringFormat, string booleanTrue, IFormatProvider provider)
        {
            //Note the generic typeof(T) is faster than using var type = typeof(T)
            if (type == typeof (string))
                return value => value;
            if (type == typeof (bool))
                return value => value == booleanTrue;
            if (type == typeof (byte))
                return value => byte.Parse(value);
            if (type == typeof (sbyte))
                return value => sbyte.Parse(value);
            if (type == typeof (short))
                return value => short.Parse(value);
            if (type == typeof (int))
                return value => int.Parse(value);
            if (type == typeof (long))
                return value => long.Parse(value);
            if (type == typeof (float))
                return value => float.Parse(value, provider);
            if (type == typeof (double))
                return value => double.Parse(value, provider);
            if (type == typeof (decimal))
                return value => decimal.Parse(value, provider);

            if (type == typeof (Guid))
                return value => new Guid(value);
            if (type == typeof (DateTime))
            {
                return String.IsNullOrEmpty(stringFormat) ? 
                    (Func<string, object>) (value => DateTime.Parse(value, provider)) : 
                    (value => DateTime.ParseExact(value, stringFormat, provider));
            }

            if (type == typeof (TimeSpan))
                return value => TimeSpan.Parse(value);

            if (type == typeof (char))
            {
                char cValue;
                return value => char.TryParse(value, out cValue) ? cValue : '\0';
            }
            if (type == typeof (ushort))
                return value => ushort.Parse(value);
            if (type == typeof (uint))
                return value => uint.Parse(value);
            if (type == typeof (ulong))
                return value => ulong.Parse(value);

            if (type == typeof (bool?))
                return value => value == null ? (bool?) null : value == booleanTrue;
            if (type == typeof (byte?))
                return value => value == null ? (byte?) null : byte.Parse(value);
            if (type == typeof (sbyte?))
                return value => value == null ? (sbyte?) null : sbyte.Parse(value);
            if (type == typeof (short?))
                return value => value == null ? (short?) null : short.Parse(value);
            if (type == typeof (int?))
                return value => value == null ? (int?) null : int.Parse(value);
            if (type == typeof (long?))
                return value => value == null ? (long?) null : long.Parse(value);
            if (type == typeof (float?))
                return value => value == null ? (float?) null : float.Parse(value, provider);
            if (type == typeof (double?))
                return value => value == null ? (double?) null : double.Parse(value, provider);
            if (type == typeof (decimal?))
                return value => value == null ? (decimal?) null : decimal.Parse(value, provider);

            if (type == typeof (DateTime?))
            {
                return String.IsNullOrEmpty(stringFormat) ?
                    (Func<string, object>)(value => value == null ? (DateTime?) null : DateTime.Parse(value, provider)) :
                    (value => value == null ? (DateTime?) null : DateTime.ParseExact(value, stringFormat, provider));
            }

            if (type == typeof (TimeSpan?))
                return value => value == null ? (TimeSpan?) null : TimeSpan.Parse(value);
            if (type == typeof (Guid?))
                return value => value == null ? (Guid?) null : new Guid(value);
            if (type == typeof (ushort?))
                return value => value == null ? (ushort?) null : ushort.Parse(value);
            if (type == typeof (uint?))
                return value => value == null ? (uint?) null : uint.Parse(value);
            if (type == typeof (ulong?))
                return value => value == null ? (ulong?) null : ulong.Parse(value);

            if (type == typeof (char?))
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
        public static Func<object, string> GetSerializeFunc(Type type, string stringFormat, string booleanTrue, string booleanFalse, IFormatProvider provider)
        {
            if (type == typeof (byte)
                || type == typeof (short)
                || type == typeof (ushort)
                || type == typeof (int)
                || type == typeof (uint)
                || type == typeof (long)
                || type == typeof (ulong)
                )
                return v => v.ToString();

            if (type == typeof (bool))
                return v => ((bool) v) ? booleanTrue : booleanFalse;

            if (type == typeof (DateTime))
                return v => ((DateTime) v).ToString(stringFormat);

            if (type == typeof (Guid))
                return v => ((Guid) v).ToString("N");

            if (type == typeof (float))
                return v => ((float) v).ToString(stringFormat, provider);

            if (type == typeof (double))
                return v => ((double) v).ToString(stringFormat, provider);

            if (type == typeof (decimal))
                return v => ((decimal) v).ToString(stringFormat, provider);

            if (type == typeof (byte?)
                || type == typeof (short?)
                || type == typeof (ushort?)
                || type == typeof (int?)
                || type == typeof (uint?)
                || type == typeof (long?)
                || type == typeof (ulong?)
                )
                return v => v == null ? "" : v.ToString();

            if (type == typeof (bool?))
                return v => v == null ? "" : (((bool) v) ? booleanTrue : booleanFalse);

            if (type == typeof (DateTime?))
                return v => v == null ? "" : ((DateTime) v).ToString(stringFormat);

            if (type == typeof (Guid?))
                return v => v == null ? "" : ((Guid) v).ToString("N");

            if (type == typeof (float?))
                return v => v == null ? "" : ((float) v).ToString(stringFormat, provider);

            if (type == typeof (double?))
                return v => v == null ? "" : ((double) v).ToString(stringFormat, provider);

            if (type == typeof (decimal?))
                return v => v == null ? "" : ((decimal) v).ToString(stringFormat, provider);

            if (type.IsEnum || type.UnderlyingSystemType.IsEnum)
                return type.GetCustomAttributes(typeof (FlagsAttribute), false).Length > 0 ? (Func<object, string>) WriteEnumFlags : v => WriteEnum(v);

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
    }
}