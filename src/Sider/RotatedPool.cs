
namespace Sider
{
  // clients are rotated in a round-robin fashion,
  // for for testing timeouts/connection handling
  public class RotatedPool : IClientsPool
  {
    private IRedisClient[] _pool;
    private int _poolIdx;

    public RotatedPool(RedisSettings settings, int count)
    {
      _pool = new IRedisClient[count];

      for (var i = 0; i < count; i++)
        _pool[i] = new RedisClient(settings);

      _poolIdx = 0;
    }

    public IRedisClient GetClient()
    {
      _poolIdx++;
      return _pool[_poolIdx %= _pool.Length];
    }
  }
}
