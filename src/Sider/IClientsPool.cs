
using System;

namespace Sider
{
  public interface IClientsPool
  {
    IRedisClient GetClient();
  }
}
