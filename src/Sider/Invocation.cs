
using System;

namespace Sider
{
  public sealed class Invocation
  {
    public static Invocation<TResult> New<TResult>(string command,
      Action<ProtocolWriter> writer,
      Func<ProtocolReader, TResult> reader)
    {
      return new Invocation<TResult>(command, writer, reader);
    }
  }

  public class Invocation<TResult>
  {
    public string Command { get; private set; }

    public Action<ProtocolWriter> WriteAction { get; private set; }
    public Func<ProtocolReader, TResult> ReadAction { get; private set; }

    public Invocation(string command,
      Action<ProtocolWriter> writer,
      Func<ProtocolReader, TResult> reader)
    {
      Command = command;
      WriteAction = writer;
      ReadAction = reader;
    }
  }
}
