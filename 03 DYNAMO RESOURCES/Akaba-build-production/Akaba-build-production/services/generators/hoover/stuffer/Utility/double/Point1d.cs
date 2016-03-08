using System;

namespace stuffer
{
  // NOTE: This is purely a holder for double extension methods
  internal static class Point1d
  {
    ///////////////////////////////////////////////////////////////////////
    // Epsilon operation methods for double

    private const double DoubleCompare = 0.0001;
    public const double DotCompare = 0.001;

    public static bool IsZero(this double value, double threshold = DoubleCompare)
    {
      return Math.Abs(value) < threshold;
    }

    public static bool Compare(this double value0, double value1, double threshold = DoubleCompare)
    {
      return Math.Abs(value0 - value1) < threshold;
    }

    public static bool CompareLess(this double value0, double value1, double threshold = DoubleCompare)
    {
      return value0 < value1 - threshold;
    }

    public static bool CompareGreater(this double value0, double value1, double threshold = DoubleCompare)
    {
      return value0 > value1 + threshold;
    }
  }
}
