
using System;

namespace Sider
{
  public class RedisSettings
  {
    public const int DefaultReadBufferSize = 4096;
    public const int DefaultWriterBufferSize = 4096;
    public const int DefaultStringBufferSize = 256;

    public const bool DefaultReconnectOnIdle = true;
    public const bool DefaultReissueWriteOnReconnect = true;

    public const string DefaultHost = "localhost";
    public const int DefaultPort = 6379;


    public string Host { get; private set; }
    public int Port { get; private set; }

    public bool ReconnectOnIdle { get; private set; }
    public bool ReissueWriteOnReconnect { get; private set; }

    public int ReadBufferSize { get; private set; }
    public int WriteBufferSize { get; private set; }
    public int StringBufferSize { get; private set; }


    public RedisSettings(
      string host = DefaultHost,
      int port = DefaultPort,
      int readBufferSize = DefaultReadBufferSize,
      int writeBufferSize = DefaultWriterBufferSize,
      int stringBufferSize = DefaultStringBufferSize,
      bool reconnectOnIdle = DefaultReconnectOnIdle,
      bool reissueWriteOnIdle = DefaultReissueWriteOnReconnect)
    {
      SAssert.ArgumentSatisfy(() => host, v => !string.IsNullOrEmpty(v),
        "Host cannot be null or empty.");

      SAssert.ArgumentBetween(() => port, 1, 65535);
      SAssert.ArgumentPositive(() => readBufferSize);
      SAssert.ArgumentPositive(() => writeBufferSize);
      SAssert.ArgumentPositive(() => stringBufferSize);

      // check invalid settings combination
      if (!reconnectOnIdle && reissueWriteOnIdle)
        throw new ArgumentException("Invalid settings combination, " +
          "ReconnectOnIdle is required to turn on ReissueWriteOnReconnect.");

      Host = host;
      Port = port;
      ReadBufferSize = readBufferSize;
      WriteBufferSize = writeBufferSize;
      StringBufferSize = stringBufferSize;
      ReconnectOnIdle = reconnectOnIdle;
      ReissueWriteOnReconnect = reissueWriteOnIdle;
    }
  }
}
