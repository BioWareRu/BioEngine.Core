using System;
using System.Collections.Generic;
using System.Globalization;
using Jeffijoe.MessageFormat;

namespace BioEngine.Core.Helpers
{
    public static class PluralizeHelper
    {
        private static readonly Dictionary<CultureInfo, MessageFormatter> Formatters =
            new Dictionary<CultureInfo, MessageFormatter>();

        private static MessageFormatter GetMessageFormatter()
        {
            if (!Formatters.ContainsKey(CultureInfo.CurrentCulture))
            {
                var mf = new MessageFormatter(true, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                mf.Pluralizers["ru"] = n =>
                {
                    var nTen = n % 10;
                    var nHundred = n % 100;
                    if (Math.Abs(nTen - 1) < double.Epsilon && Math.Abs(nHundred - 11) > double.Epsilon)
                    {
                        return "one";
                    }
                    if (nTen >= 2 && nTen <= 4 && !(nHundred >= 12 && nHundred <= 14))
                    {
                        return "few";
                    }
                    if (Math.Abs(nTen) < double.Epsilon || (nTen >= 5 && nTen <= 9) ||
                        (nHundred >= 11 && nHundred <= 14))
                    {
                        return "many";
                    }

                    return "other";
                };
                Formatters.Add(CultureInfo.CurrentCulture, mf);
            }

            return Formatters[CultureInfo.CurrentCulture];
        }


        public static string Pluralize(this string message, int number)
        {
            return GetMessageFormatter().FormatMessage(message, new Dictionary<string, object>
            {
                {"n", number},
            });
        }

        public static string Pluralize(this string message, double number)
        {
            var formatted = GetMessageFormatter().FormatMessage(message, new Dictionary<string, object>
            {
                {"n", number},
            });
            return formatted;
        }
    }
}
