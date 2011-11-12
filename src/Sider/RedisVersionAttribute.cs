
using System;

namespace Sider
{
  // attribute for marking redis version number supports in case we need
  // to have compatibility issues in the future
  // (this file was added when 2.4 came out so the lowest supported version is 2.2)
  // TODO: Possibly this should be applied to IRedisClient, not the concrete class.
  public class RedisVersionAttribute : Attribute
  {
    public Version Version { get; private set; }

    public RedisVersionAttribute(int major, int minor, int revision)
    {
      Version = new Version(major, minor, revision);
    }
  }
}
