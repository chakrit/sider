
using System.Diagnostics.Contracts;
namespace Sider
{
  public class RedisSettings
  {
    public const int DefaultReadBufferSize = 4096;
    public const int DefaultWriterBufferSize = 4096;

    // half of Redis's default connection timeout (3000)
    public const int DefaultSocketPollingInterval = 1500;
    public const int DefaultSocketPollTimeout = 750;

    public const string DefaultHost = "localhost";
    public const int DefaultPort = 6379;


    public string Host { get; private set; }
    public int Port { get; private set; }

    public int SocketPollingInterval { get; private set; }
    public int SocketPollTimeouot { get; private set; }

    public int ReadBufferSize { get; private set; }
    public int WriteBufferSize { get; private set; }


    public RedisSettings(
      string host = DefaultHost,
      int port = DefaultPort,
      int readBufferSize = DefaultReadBufferSize,
      int writeBufferSize = DefaultWriterBufferSize,
      int socketPollingInterval = DefaultSocketPollingInterval,
      int socketPollTimeout = DefaultSocketPollTimeout)
    {
      SAssert.ArgumentSatisfy(() => host, v => !string.IsNullOrEmpty(v),
        "Host cannot be null or empty.");

      SAssert.ArgumentBetween(() => port, 1, 65535);
      SAssert.ArgumentPositive(() => readBufferSize);
      SAssert.ArgumentPositive(() => writeBufferSize);
      SAssert.ArgumentPositive(() => socketPollingInterval);
      SAssert.ArgumentPositive(() => socketPollTimeout);

      Host = host;
      Port = port;
      ReadBufferSize = readBufferSize;
      WriteBufferSize = writeBufferSize;
      SocketPollingInterval = socketPollingInterval;
      SocketPollTimeouot = socketPollTimeout;
    }
  }
}
