
using System;
using System.Collections.Generic;

namespace Sider
{
  public partial class RedisClient<T>
  {
    private Queue<Func<RedisReader, object>> _readsQueue;
    private bool _isPipelining;


    private void initReadsQueue()
    {
      _readsQueue = _readsQueue ?? new Queue<Func<RedisReader, object>>();
      _readsQueue.Clear();
    }

    private IEnumerable<object> executeQueuedReads()
    {
      var results = new object[_readsQueue.Count];
      var resultIdx = 0;

      while (_readsQueue.Count > 0)
        results[resultIdx++] = _readsQueue.Dequeue()(_reader);

      return results;
    }


    private IEnumerable<object> executePipeline(Action<IRedisClient<T>> pipelinedCalls,
      int retryCount = 0)
    {
      try {
        initReadsQueue();

        // all writes executed immediately but reads are queued
        _isPipelining = true;
        pipelinedCalls(this);
        _writer.Flush();
        _isPipelining = false;

        // reads out all the return values
        return executeQueuedReads();
      }
      catch (Exception ex) {
        if (!handleException(ex))
          throw;

        // TODO: multiple retries with pipeline maybe too dangerous
        //   so will only retry once in pipeline mode.
        if (_settings.ReissuePipelinedCallsOnReconnect &&
          retryCount < 1)
          return executePipeline(pipelinedCalls, retryCount + 1);

        throw;
      }
    }

  }
}
