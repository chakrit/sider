
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sider
{
  public static class RedisClientPipeliningExtensions
  {
    public static Tuple<T1, T2> Pipeline<T1, T2>(
      this IRedisClient client,
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2) {

      var result = client.Pipeline(c =>
      {
        call1(c);
        call2(c);
      }).ToArray();

      return Tuple.Create(
        (T1) result[0],
        (T2) result[1]);
    }

    public static Tuple<T1, T2, T3> Pipeline<T1, T2, T3>(
      this IRedisClient client,
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3) {

      var result = client.Pipeline(c =>
      {
        call1(c);
        call2(c);
        call3(c);
      }).ToArray();

      return Tuple.Create(
        (T1) result[0],
        (T2) result[1],
        (T3) result[2]);
    }

    public static Tuple<T1, T2, T3, T4> Pipeline<T1, T2, T3, T4>(
      this IRedisClient client,
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4) {

      var result = client.Pipeline(c =>
      {
        call1(c);
        call2(c);
        call3(c);
        call4(c);
      }).ToArray();

      return Tuple.Create(
        (T1) result[0],
        (T2) result[1],
        (T3) result[2],
        (T4) result[3]);
    }

    public static Tuple<T1, T2, T3, T4, T5> Pipeline<T1, T2, T3, T4, T5>(
      this IRedisClient client,
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4,
      Func<IRedisClient, T5> call5) {

      var result = client.Pipeline(c =>
      {
        call1(c);
        call2(c);
        call3(c);
        call4(c);
        call5(c);
      }).ToArray();

      return Tuple.Create(
        (T1) result[0],
        (T2) result[1],
        (T3) result[2],
        (T4) result[3],
        (T5) result[4]);
    }

    public static Tuple<T1, T2, T3, T4, T5, T6> Pipeline<T1, T2, T3, T4, T5, T6>(
      this IRedisClient client,
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4,
      Func<IRedisClient, T5> call5,
      Func<IRedisClient, T6> call6) {

      var result = client.Pipeline(c =>
      {
        call1(c);
        call2(c);
        call3(c);
        call4(c);
        call5(c);
        call6(c);
      }).ToArray();

      return Tuple.Create(
        (T1) result[0],
        (T2) result[1],
        (T3) result[2],
        (T4) result[3],
        (T5) result[4],
        (T6) result[5]);
    }

    public static Tuple<T1, T2, T3, T4, T5, T6, T7> Pipeline<T1, T2, T3, T4, T5, T6, T7>(
      this IRedisClient client,
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4,
      Func<IRedisClient, T5> call5,
      Func<IRedisClient, T6> call6,
      Func<IRedisClient, T7> call7) {

      var result = client.Pipeline(c =>
      {
        call1(c);
        call2(c);
        call3(c);
        call4(c);
        call5(c);
        call6(c);
        call7(c);
      }).ToArray();

      return Tuple.Create(
        (T1) result[0],
        (T2) result[1],
        (T3) result[2],
        (T4) result[3],
        (T5) result[4],
        (T6) result[5],
        (T7) result[6]);
    }

  }
}

