
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sider
{
  public interface IPipelinable
  {
    IEnumerable<object> Pipeline(Action<IRedisClient> pipelinedCalls);

    Tuple<T1, T2> Pipeline<T1, T2>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2);

    Tuple<T1, T2, T3> Pipeline<T1, T2, T3>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3);

    Tuple<T1, T2, T3, T4> Pipeline<T1, T2, T3, T4>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4);

    Tuple<T1, T2, T3, T4, T5> Pipeline<T1, T2, T3, T4, T5>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4,
      Func<IRedisClient, T5> call5);

    Tuple<T1, T2, T3, T4, T5, T6> Pipeline<T1, T2, T3, T4, T5, T6>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4,
      Func<IRedisClient, T5> call5,
      Func<IRedisClient, T6> call6);

    Tuple<T1, T2, T3, T4, T5, T6, T7> Pipeline<T1, T2, T3, T4, T5, T6, T7>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4,
      Func<IRedisClient, T5> call5,
      Func<IRedisClient, T6> call6,
      Func<IRedisClient, T7> call7);

  }

  public partial class RedisClient : IPipelinable
  {
    public Tuple<T1, T2> Pipeline<T1, T2>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2)
    {

      var result = Pipeline(client =>
      {
        call1(client);
        call2(client);
      }).ToArray();

      return Tuple.Create(
        (T1)result[0],
        (T2)result[1]);
    }

    public Tuple<T1, T2, T3> Pipeline<T1, T2, T3>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3)
    {

      var result = Pipeline(client =>
      {
        call1(client);
        call2(client);
        call3(client);
      }).ToArray();

      return Tuple.Create(
        (T1)result[0],
        (T2)result[1],
        (T3)result[2]);
    }

    public Tuple<T1, T2, T3, T4> Pipeline<T1, T2, T3, T4>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4)
    {

      var result = Pipeline(client =>
      {
        call1(client);
        call2(client);
        call3(client);
        call4(client);
      }).ToArray();

      return Tuple.Create(
        (T1)result[0],
        (T2)result[1],
        (T3)result[2],
        (T4)result[3]);
    }

    public Tuple<T1, T2, T3, T4, T5> Pipeline<T1, T2, T3, T4, T5>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4,
      Func<IRedisClient, T5> call5)
    {

      var result = Pipeline(client =>
      {
        call1(client);
        call2(client);
        call3(client);
        call4(client);
        call5(client);
      }).ToArray();

      return Tuple.Create(
        (T1)result[0],
        (T2)result[1],
        (T3)result[2],
        (T4)result[3],
        (T5)result[4]);
    }

    public Tuple<T1, T2, T3, T4, T5, T6> Pipeline<T1, T2, T3, T4, T5, T6>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4,
      Func<IRedisClient, T5> call5,
      Func<IRedisClient, T6> call6)
    {

      var result = Pipeline(client =>
      {
        call1(client);
        call2(client);
        call3(client);
        call4(client);
        call5(client);
        call6(client);
      }).ToArray();

      return Tuple.Create(
        (T1)result[0],
        (T2)result[1],
        (T3)result[2],
        (T4)result[3],
        (T5)result[4],
        (T6)result[5]);
    }

    public Tuple<T1, T2, T3, T4, T5, T6, T7> Pipeline<T1, T2, T3, T4, T5, T6, T7>(
      Func<IRedisClient, T1> call1,
      Func<IRedisClient, T2> call2,
      Func<IRedisClient, T3> call3,
      Func<IRedisClient, T4> call4,
      Func<IRedisClient, T5> call5,
      Func<IRedisClient, T6> call6,
      Func<IRedisClient, T7> call7)
    {

      var result = Pipeline(client =>
      {
        call1(client);
        call2(client);
        call3(client);
        call4(client);
        call5(client);
        call6(client);
        call7(client);
      }).ToArray();

      return Tuple.Create(
        (T1)result[0],
        (T2)result[1],
        (T3)result[2],
        (T4)result[3],
        (T5)result[4],
        (T6)result[5],
        (T7)result[6]);
    }

  }
}

