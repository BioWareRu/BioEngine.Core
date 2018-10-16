using System.Collections.Generic;
using System.Globalization;
using Huyn.PluralNet;
using Huyn.PluralNet.Utils;

namespace BioEngine.Core.Helpers
{
    public static class PluralizeHelper
    {
        public static string Pluralize(int number, Dictionary<PluralTypeEnum, string> forms)
        {
            var pluralType = PluralTypeEnum.ZERO;
            if (number > 0)
            {
                var provider = PluralHelper.GetPluralChooser(CultureInfo.CurrentCulture);

                pluralType = provider.ComputePlural(number);
            }

            return forms.ContainsKey(pluralType) ? string.Format(forms[pluralType], number) : number.ToString();
        }
    }
}