using System;
using System.Diagnostics;
using System.Globalization;

namespace Keyzoid.Core.Render.Utilities
{
    public static class ArgumentHelper
    {
        [DebuggerHidden]
        public static void AssertNotNull<T>(T arg, string argName)
          where T : class
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        [DebuggerHidden]
        public static void AssertNotEmptyAndNotNull(string arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
            else if (arg.Trim() == "")
            {
                throw new ArgumentException("Argument must be specified.", argName);
            }
        }

        public static void AssertNotEmptyNotNullAndLength(string arg, string argName, int maxLength)
        {
            AssertNotEmptyAndNotNull(arg, argName);
            if (arg.Length > maxLength)
                throw new ArgumentOutOfRangeException(argName, arg, String.Format("Argument must be less than or equal to {0}", maxLength));
        }

        public static void AssertOutOfRange(int arg, string argName, int? minValue, int? maxValue)
        {
            if (minValue.HasValue && (arg < minValue.Value))
                throw new ArgumentOutOfRangeException(argName, arg, String.Format("Argument must me greater than or equal to {0}", minValue.Value));
            if (maxValue.HasValue && (arg > maxValue.Value))
                throw new ArgumentOutOfRangeException(argName, arg, String.Format("Argument must me less than or equal to {0}", maxValue.Value));
        }

        public static void AssertArgTwoAssignableFromArgOne(object arg1, object arg2)
        {
            if (!arg2.GetType().IsAssignableFrom(arg1.GetType()))
                throw new ArgumentException("Argument two is not assignable from arugment one");
        }


        [DebuggerHidden]
        public static void AssertEnumMember<TEnum>(TEnum enumValue)
            where TEnum : struct, IConvertible
        {
            if (Attribute.IsDefined(typeof(TEnum), typeof(FlagsAttribute), false))
            {
                bool throwEx;
                long longValue = enumValue.ToInt64(CultureInfo.InvariantCulture);

                if (longValue == 0)
                {
                    throwEx = !Enum.IsDefined(typeof(TEnum), 0);
                }
                else
                {
                    foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
                    {
                        longValue &= ~value.ToInt64(CultureInfo.InvariantCulture);
                    }

                    throwEx = (longValue != 0);
                }

                if (throwEx)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                      "Enum value '{0}' is not valid for flags enumeration '{1}'.",
                      enumValue, typeof(TEnum).FullName));
                }
            }
            else
            {
                if (!Enum.IsDefined(typeof(TEnum), enumValue))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                        "Enum value '{0}' is not defined for enumeration '{1}'.",
                        enumValue, typeof(TEnum).FullName));
                }
            }
        }

        [DebuggerHidden]
        public static void AssertEnumMember<TEnum>(TEnum enumValue, params TEnum[] validValues)
          where TEnum : struct, IConvertible
        {
            AssertNotNull(validValues, "validValues");

            if (Attribute.IsDefined(typeof(TEnum), typeof(FlagsAttribute), false))
            {
                bool throwEx;
                long longValue = enumValue.ToInt64(CultureInfo.InvariantCulture);

                if (longValue == 0)
                {
                    throwEx = true;

                    foreach (TEnum value in validValues)
                    {
                        if (value.ToInt64(CultureInfo.InvariantCulture) == 0)
                        {
                            throwEx = false;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (TEnum value in validValues)
                    {
                        longValue &= ~value.ToInt64(CultureInfo.InvariantCulture);
                    }

                    throwEx = (longValue != 0);
                }

                if (throwEx)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                      "Enum value '{0}' is not allowed for flags enumeration '{1}'.",
                      enumValue, typeof(TEnum).FullName));
                }
            }
            else
            {
                foreach (TEnum value in validValues)
                {
                    if (enumValue.Equals(value))
                    {
                        return;
                    }
                }

                if (!Enum.IsDefined(typeof(TEnum), enumValue))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                        "Enum value '{0}' is not defined for enumeration '{1}'.",
                        enumValue, typeof(TEnum).FullName));
                }
                else
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                        "Enum value '{0}' is defined for enumeration '{1}' but it is not permitted in this context.",
                        enumValue, typeof(TEnum).FullName));
                }
            }
        }
    }
}
