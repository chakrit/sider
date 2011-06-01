
using System;
using System.Globalization;
using System.Text;
using Sider.Serialization;

namespace Sider
{
  public sealed class RedisSettings
  {
    public const string DefaultHost = "localhost";
    public const int DefaultPort = 6379;


    private static RedisSettings _default = new RedisSettings();

    public static RedisSettings Default { get { return _default; } }


    public string Host { get; private set; }
    public int Port { get; private set; }

    public int ConnectionTimeout { get; private set; }
    public bool ReconnectOnIdle { get; private set; }
    public int MaxReconnectRetries { get; private set; }

    public bool ReissueCommandsOnReconnect { get; private set; }
    public bool ReissuePipelinedCallsOnReconnect { get; private set; }

    [Obsolete("Please use ReissueCommandsOnReconnect instead.")]
    public bool ReissueWriteOnReconnect
    {
      get { return ReissueCommandsOnReconnect; }
      set { ReissueCommandsOnReconnect = value; }
    }

    [Obsolete("Please use EncodingBufferSize instead.")]
    public int StringBufferSize
    {
      get { return EncodingBufferSize; }
      set { EncodingBufferSize = value; }
    }

    public int ReadBufferSize { get; private set; }
    public int WriteBufferSize { get; private set; }
    public int EncodingBufferSize { get; private set; }
    public int SerializationBufferSize { get; private set; }

    [Obsolete("Please use EncodingOverride instead.")]
    public Encoding KeyEncoding { get; private set; }

    [Obsolete("Please use EncodingOverride instead.")]
    public Encoding ValueEncoding { get; private set; }

    public ISerializer SerializerOverride { get; private set; }
    public CultureInfo CultureOverride { get; private set; }
    public Encoding EncodingOverride { get; private set; }

    private RedisSettings()
    {
      // set defaults
      Host = "localhost";
      Port = 6379;

      ConnectionTimeout = 0;
      ReconnectOnIdle = true;
      MaxReconnectRetries = 10; // retry forever

      ReissueCommandsOnReconnect = true;
      ReissuePipelinedCallsOnReconnect = true;

      // TODO: Use buffer pools? with growable buffers?
      ReadBufferSize = 4096;
      WriteBufferSize = 4096;
      EncodingBufferSize = 256;
      SerializationBufferSize = 2048;

      SerializerOverride = null;
      CultureOverride = null;
      EncodingOverride = null;
    }

    [Obsolete("Please use .Build() instead.")]
    public static Builder New() { return new Builder(); }

    [Obsolete("Please use .BuildCopy() instead.")]
    public Builder CopyNew() { return new Builder(this); }

    public static Builder Build() { return new Builder(); }
    public Builder BuildCopy() { return new Builder(this); }


    public sealed class Builder
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

      [Obsolete("Please use EncodingBufferSize instead.")]
      public Builder StringBufferSize(int bufferSize)
      {
        SAssert.ArgumentPositive(() => bufferSize);

        _settings.EncodingBufferSize = bufferSize;
        return this;
      }

      public Builder EncodingBufferSize(int bufferSize)
      {
        SAssert.ArgumentPositive(() => bufferSize);

        _settings.EncodingBufferSize = bufferSize;
        return this;
      }

      public Builder SerializationBufferSize(int bufferSize)
      {
        SAssert.ArgumentPositive(() => bufferSize);

        _settings.SerializationBufferSize = bufferSize;
        return this;
      }

      public Builder BufferSize(int read, int write)
      {
        return this
          .ReadBufferSize(read)
          .WriteBufferSize(write);
      }

      public Builder BufferSize(int read, int write, int encoding, int serialization)
      {
        return this
          .ReadBufferSize(read)
          .WriteBufferSize(write)
          .EncodingBufferSize(encoding)
          .SerializationBufferSize(serialization);
      }

      public Builder ConnectionTimeout(int timeout)
      {
        _settings.ConnectionTimeout = timeout;
        return this;
      }

      public Builder ReconnectOnIdle(bool reconnectOnIdle)
      {
        _settings.ReconnectOnIdle = reconnectOnIdle;
        return this;
      }

      public Builder MaxReconnectRetries(int maxRetries)
      {
        SAssert.ArgumentPositive(() => maxRetries);
        SAssert.IsTrue(_settings.ReconnectOnIdle, () =>
          new ArgumentException("MaxReconnectRetries requires ReconnectOnIdle"));

        _settings.MaxReconnectRetries = maxRetries;
        return this;
      }

      [Obsolete("Please ReissueCommandsOnReconnect instead which covers both read and write")]
      public Builder ReissueWriteOnReconnect(bool reissueWriteOnReconnect)
      {
        SAssert.IsTrue(_settings.ReconnectOnIdle, () =>
          new ArgumentException("ReissueWriteOnReconnect requires ReconnectOnIdle"));

        _settings.ReissueCommandsOnReconnect = reissueWriteOnReconnect;
        return this;
      }

      public Builder ReissueCommandsOnReconnect(bool reissueCommandsOnReconnect)
      {
        SAssert.IsTrue(_settings.ReconnectOnIdle, () => new ArgumentException(
          "ReissueCommandsOnReconnect requires ReconnectOnIdle"));

        _settings.ReissueCommandsOnReconnect = reissueCommandsOnReconnect;
        return this;
      }

      public Builder ReissuePipelinedCallsOnReconnect(bool reissuePipeline)
      {
        SAssert.IsTrue(_settings.ReconnectOnIdle, () => new ArgumentException(
          "ReissuePipelinedCallsOnReconnect requires ReconnectOnIdle"));

        _settings.ReissuePipelinedCallsOnReconnect = reissuePipeline;
        return this;
      }

      [Obsolete("Please ReissueCommandsOnReconnect instead which covers both read and write")]
      public Builder ReissueReadOnReconnect(bool reissueReadOnReconnect)
      {
        SAssert.IsTrue(_settings.ReconnectOnIdle, () =>
          new ArgumentException("ReissueReadOnReconnect requires ReconnectOnIdle"));

        _settings.ReissueCommandsOnReconnect = reissueReadOnReconnect;
        return this;
      }

      [Obsolete("Please use EncodingOverride instead.")]
      public Builder ValueEncoding(Encoding encoding)
      {
        SAssert.ArgumentNotNull(() => encoding);

        _settings.ValueEncoding = encoding;
        return this;
      }

      [Obsolete("Please use EncodingOverride instead.")]
      public Builder KeyEncoding(Encoding encoding)
      {
        SAssert.ArgumentNotNull(() => encoding);

        _settings.KeyEncoding = encoding;
        return this;
      }

      public Builder OverrideSerializer(ISerializer serializer)
      {
        // can be null
        _settings.SerializerOverride = serializer;
        return this;
      }

      public Builder OverrideCulture(CultureInfo culture)
      {
        _settings.CultureOverride = culture;
        return this;
      }

      public Builder OverrideEncoding(Encoding encoding)
      {
        _settings.EncodingOverride = encoding;
        return this;
      }
    }
  }
}
