
using System;
using System.Diagnostics;

namespace Sider
{
  internal static class Invocation
  {
    [DebuggerStepThrough]
    public static Invocation<TResult> New<TResult>(string command,
      Action<ProtocolWriter> writer,
      Func<ProtocolReader, TResult> reader)
    {
      return new Invocation<TResult>(command, writer, reader);
    }
  }

  internal class Invocation<TResult>
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
