
namespace Sider
{
  // clients are rotated in a round-robin fashion,
  // for testing timeouts/connection handling
  public class RoundRobinPool<T> : IClientsPool<T>
  {
    private IRedisClient<T>[] _pool;
    private int _poolIdx;

    public RoundRobinPool(RedisSettings settings, int count)
    {
      _pool = new IRedisClient<T>[count];

      for (var i = 0; i < count; i++)
        _pool[i] = new RedisClient<T>(settings);

      _poolIdx = 0;
    }

    public IRedisClient<T> GetClient()
    {
      _poolIdx++;
      return _pool[_poolIdx %= _pool.Length];
    }
  }
}
