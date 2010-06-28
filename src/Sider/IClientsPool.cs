
namespace Sider
{
  public interface IClientsPool
  {
    IRedisClient GetClient();
  }
}
