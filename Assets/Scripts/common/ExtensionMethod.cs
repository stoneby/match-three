using System;
using System.Collections.Generic;
using System.Linq;

public static class ExtensionMethod
{
    public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
    {
        if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
            return defaultValue;

        return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
    }

    public static IEnumerable<t> Randomize<t>(this IEnumerable<t> target)
    {
        Random r = new Random();

        return target.OrderBy(x => (r.Next()));
    }
}
