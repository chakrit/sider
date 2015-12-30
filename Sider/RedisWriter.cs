using System.IO;

namespace Sider {
  public class RedisWriter {
    readonly Stream stream;

    public RedisWriter(Stream stream) {
      this.stream = stream;
    }
  }
}

