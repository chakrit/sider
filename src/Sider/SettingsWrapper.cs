
namespace Sider
{
  public abstract class SettingsWrapper
  {
    public RedisSettings Settings { get; private set; }

    protected SettingsWrapper(RedisSettings settings)
    {
      SAssert.ArgumentNotNull(() => settings);

      Settings = settings;
    }
  }
}
