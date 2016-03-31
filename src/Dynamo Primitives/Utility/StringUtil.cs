using System;
using System.Collections.Generic;

namespace stuffer
{
  internal static class StringExtensionMethods
  {
    public static IEnumerable<string> GetLines(this string str, bool removeEmptyLines = false)
    {
      if (str == null)
        return null;

      return str.Split(new[] { "\r\n", "\r", "\n" },
          removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
    }

    public static string Repeat(this char ch, int num)
    {
      return new String(ch, num);
    }
  }
}
