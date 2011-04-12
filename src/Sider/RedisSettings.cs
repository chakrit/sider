
using System;

namespace Sider
{
  public sealed class RedisSettings
  {
    public const string DefaultHost = "localhost";
    public const int DefaultPort = 6379;


    private static RedisSettings _default = new RedisSettings();

    public static RedisSettings Default { get { return _default; } }


    public string Host { get; protected set; }
    public int Port { get; protected set; }

    public bool ReconnectOnIdle { get; protected set; }
    public bool ReissueWriteOnReconnect { get; protected set; }

    public int ReadBufferSize { get; protected set; }
    public int WriteBufferSize { get; protected set; }
    public int StringBufferSize { get; protected set; }

    private RedisSettings()
    {
      // set defaults
      Host = "localhost";
      Port = 6379;

      ReconnectOnIdle = true;
      ReissueWriteOnReconnect = true;

      ReadBufferSize = 4096;
      WriteBufferSize = 4096;
      StringBufferSize = 256;
    }

    public RedisSettings(string host = "localhost", int port = 6379) :
      this()
    {
      new Builder(this).Host(host).Port(port);
    }

    public static Builder New() { return new Builder(); }


    public class Builder
    {
      private RedisSettings _settings;

      public Builder() { _settings = new RedisSettings(); }
      internal Builder(RedisSettings instance) { _settings = instance; }

      public static implicit operator RedisSettings(Builder b)
      {
        return b._settings;
      }


      public Builder Host(string host)
      {
        SAssert.ArgumentSatisfy(() => host, v => !string.IsNullOrEmpty(v),
          "Host cannot be null or empty.");

        _settings.Host = host;
        return this;
      }

      public Builder Port(int port)
      {
        SAssert.ArgumentBetween(() => port, 1, 65535);

        _settings.Port = port;
        return this;
      }

      public Builder ReadBufferSize(int bufferSize)
      {
        SAssert.ArgumentPositive(() => bufferSize);

        _settings.ReadBufferSize = bufferSize;
        return this;
      }

      public Builder WriteBufferSize(int bufferSize)
      {
        SAssert.ArgumentPositive(() => bufferSize);

        _settings.WriteBufferSize = bufferSize;
        return this;
      }

      public Builder StringBufferSize(int bufferSize)
      {
        SAssert.ArgumentPositive(() => bufferSize);

        _settings.StringBufferSize = bufferSize;
        return this;
      }

      public Builder BufferSize(int read, int write, int str)
      {
        return this
          .ReadBufferSize(read)
          .WriteBufferSize(write)
          .StringBufferSize(str);
      }

      public Builder ReconnectOnIdle(bool reconnectOnIdle)
      {
        _settings.ReconnectOnIdle = reconnectOnIdle;
        return this;
      }

      public Builder ReissueWriteOnReconnect(bool reissueWriteOnReconnect)
      {
        SAssert.IsTrue(_settings.ReconnectOnIdle, () =>
          new ArgumentException("ReissueWriteOnReconnect requires ReconnectOnIdle"));

        _settings.ReissueWriteOnReconnect = reissueWriteOnReconnect;
        return this;
      }
    }
  }
}
