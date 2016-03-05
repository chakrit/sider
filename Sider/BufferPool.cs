using System;
using System.ServiceModel.Channels;

namespace Sider {
  public class BufferPool {
    readonly BufferManager manager;
    
    public BufferPool(RedisSettings settings) {
      if (settings == null) throw new ArgumentNullException("settings");

      manager = BufferManager.CreateBufferManager(
        settings.MaxBufferPoolSize,
        settings.MaxBufferSize
      );
    }

    public void Use(int size, Action<byte[]> bufferAction) {
      if (bufferAction == null) throw new ArgumentNullException("bufferAction");

      Use<object>(size, (buffer) => {
        bufferAction(buffer);
        return null;
      });
    }

    public T Use<T>(int size, Func<byte[], T> bufferFunc) {
      if (bufferFunc == null) throw new ArgumentNullException("bufferFunc");
      
      byte[] buffer = null;
      try {
        buffer = manager.TakeBuffer(size);
        return bufferFunc(buffer);

      } finally {
        if (buffer != null) {
          manager.ReturnBuffer(buffer);
        }
      }
    }
  }
}

