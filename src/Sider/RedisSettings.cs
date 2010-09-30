
namespace Sider
{
  public class RedisSettings
  {
    public const int DefaultReadBufferSize = 4096;
    public const int DefaultWriterBufferSize = 4096;

    public const bool DefaultReconnectOnIdle = true;

    public const string DefaultHost = "localhost";
    public const int DefaultPort = 6379;


    public string Host { get; private set; }
    public int Port { get; private set; }

    public bool ReconnectOnIdle { get; private set; }

    public int ReadBufferSize { get; private set; }
    public int WriteBufferSize { get; private set; }


    public RedisSettings(
      string host = DefaultHost,
      int port = DefaultPort,
      int readBufferSize = DefaultReadBufferSize,
      int writeBufferSize = DefaultWriterBufferSize,
      bool reconnectOnIdle = DefaultReconnectOnIdle)
    {
      SAssert.ArgumentSatisfy(() => host, v => !string.IsNullOrEmpty(v),
        "Host cannot be null or empty.");

      SAssert.ArgumentBetween(() => port, 1, 65535);
      SAssert.ArgumentPositive(() => readBufferSize);
      SAssert.ArgumentPositive(() => writeBufferSize);

      Host = host;
      Port = port;
      ReadBufferSize = readBufferSize;
      WriteBufferSize = writeBufferSize;
      ReconnectOnIdle = reconnectOnIdle;
    }
  }
}
