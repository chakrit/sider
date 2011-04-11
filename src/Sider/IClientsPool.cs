
namespace Sider
{
  public interface IClientsPool<T>
  {
    IRedisClient<T> GetClient();
  }
}
