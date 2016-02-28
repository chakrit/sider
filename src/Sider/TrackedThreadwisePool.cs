using System;
using System.Collections.Generic;
using System.Linq;

namespace Sider
{
  public class TrackedThreadwisePool<T> : ThreadwisePool<T>, IDisposable
  {
    private IList<WeakReference> _clients = new List<WeakReference>();

    public TrackedThreadwisePool(
      Func<RedisSettings.Builder, RedisSettings> settingsFunc) :
    base(settingsFunc) { }

    public TrackedThreadwisePool(string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort,
      int? db = null) :
    base(host, port, db) { }

    public TrackedThreadwisePool(RedisSettings settings, int? db = null) :
    base(settings, db) { }


    public void Prune() {
      _clients = _clients.Where(r => r.IsAlive).ToList();
    }

    public void Dispose() {
      foreach (var weakRef in _clients)
      {
        var client = weakRef.Target as IRedisClient<T>;
        if (client != null) {
          client.Dispose();
        }
      }

      _clients.Clear();
    }


    protected override WeakReference BuildWeakReferenceClient() {
      var weakRef = base.BuildWeakReferenceClient();
      _clients.Add(weakRef);
      return weakRef;
    }
  }
}

