using System;

namespace Sider {
  public partial class RedisSettings {
    static RedisSettings _default = new RedisSettings();

    public static RedisSettings Default {
      get { return _default; }
    }

    public static bool operator ==(RedisSettings left, RedisSettings right) {
      return Object.Equals(left, right);
    }

    public static bool operator !=(RedisSettings left, RedisSettings right) {
      return !Object.Equals(left, right);
    }
  }

  public partial class RedisSettings {
    public Builder Build() {
      return new Builder(this);
    }

    public partial class Builder {
      public Builder()
        : this(new RedisSettings()) {
      }

      internal Builder(RedisSettings existing) {
        _settings = existing.Clone();
      }
    }

  }
}

