using System;
using System.IO;

namespace Sider {
  public abstract class StreamWrapper : IDisposable {
    protected Stream Stream { get; private set; }
    protected RedisSettings Settings { get; private set; }

    protected StreamWrapper(Stream stream, RedisSettings settings) {
      if (stream == null) throw new ArgumentNullException("stream");
      if (settings == null) throw new ArgumentNullException("settings");

      Stream = stream;
      Settings = settings;
    }

    public abstract void Dispose();
  }
}

