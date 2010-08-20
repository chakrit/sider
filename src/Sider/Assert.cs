
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Sider
{
  [DebuggerNonUserCode]
  internal static partial class Assert
  {
    [Conditional("DEBUG")]
    public static void ArgumentNotNull(Expression<Func<object>> expr)
    {
      var value = expr.Compile()();
      if (value != null) return;

      throw new ArgumentNullException(extractParamName(expr));
    }

    [Conditional("DEBUG")]
    public static void ArgumentSatisfy<T>(Expression<Func<T>> expr,
      Func<T, bool> condition,
      string rationale)
    {
      var value = expr.Compile()();
      if (condition(value)) return;

      throw new ArgumentException(rationale, extractParamName(expr));
    }

    [Conditional("DEBUG")]
    public static void ArgumentPositive(Expression<Func<int>> expr)
    {
      var value = expr.Compile()();
      if (value > 0) return;

      throw new ArgumentOutOfRangeException(extractParamName(expr));
    }

    [Conditional("DEBUG")]
    public static void ArgumentNonNegative(Expression<Func<int>> expr)
    {
      var value = expr.Compile()();
      if (value >= 0) return;

      throw new ArgumentOutOfRangeException(extractParamName(expr));
    }

    [Conditional("DEBUG")]
    public static void ArgumentBetween(Expression<Func<int>> expr,
      int minInclusive, int maxExclusive)
    {
      var value = expr.Compile()();
      if (value >= minInclusive && value < maxExclusive) return;

      throw new ArgumentOutOfRangeException(extractParamName(expr));
    }

    [Conditional("DEBUG")]
    public static void IsTrue(bool value, Func<Exception> exceptionIfFalse)
    {
      if (!value) throw exceptionIfFalse();
    }


    private static string extractParamName<T>(Expression<Func<T>> expr)
    {
      // ToString() hack to work around the inaccessible FieldExpression type
      var str = expr.ToString();

      // closed-lambda.ToString() returns something like
      // () => ClosureClasshere.variableName so we can snatch the variable name 
      // using simple string manipulation
      return str.Substring(str.LastIndexOf('.') + 1);
    }
  }
}
