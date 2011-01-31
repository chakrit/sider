
using System;
using System.Collections.Generic;
using System.IO;

namespace Sider
{
  public partial class RedisClient
  {
    private void writeCmd(string command)
    {
      writeCore(w => writeCmdStart(w, command, 0));
    }

    private void writeCmd(string command, string arg0)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, 1);
        writeCmdItem(w, arg0);
      });
    }

    private void writeCmd(string command, string arg0, string arg1)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, 2);
        writeCmdItem(w, arg0);
        writeCmdItem(w, arg1);
      });
    }

    private void writeCmd(string command, string arg0, string arg1, string arg2)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, 3);
        writeCmdItem(w, arg0);
        writeCmdItem(w, arg1);
        writeCmdItem(w, arg2);
      });
    }

    private void writeCmd(string command, KeyValuePair<string, string>[] mappings)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, mappings.Length * 2);
        Array.ForEach(mappings, mapping =>
        {
          writeCmdItem(w, mapping.Key);
          writeCmdItem(w, mapping.Value);
        });
      });
    }

    private void writeCmd(string command, string arg0,
      KeyValuePair<string, string>[] mappings)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, (mappings.Length * 2) + 1);
        writeCmdItem(w, arg0);
        Array.ForEach(mappings, mapping =>
        {
          writeCmdItem(w, mapping.Key);
          writeCmdItem(w, mapping.Value);
        });
      });
    }

    private void writeCmd(string command, string[] multiArgs)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, multiArgs.Length);
        Array.ForEach(multiArgs, ma => writeCmdItem(w, ma));
      });
    }

    private void writeCmd(string command, string[] multiArgs, string appendArg)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, multiArgs.Length);
        Array.ForEach(multiArgs, ma => writeCmdItem(w, ma));
        writeCmdItem(w, appendArg);
      });
    }

    private void writeCmd(string command, string arg0, string[] multiArgs)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, multiArgs.Length + 1);
        writeCmdItem(w, arg0);
        Array.ForEach(multiArgs, ma => writeCmdItem(w, ma));
      });
    }

    private void writeCmd(string command, string arg0, string arg1, string[] multiArgs)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, multiArgs.Length + 2);
        writeCmdItem(w, arg0);
        writeCmdItem(w, arg1);
        Array.ForEach(multiArgs, ma => writeCmdItem(w, ma));
      });
    }

    private void writeCmd(string command, string arg0, byte[] arg1)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, 2);
        writeCmdItem(w, arg0);
        writeCmdItem(w, arg1);
      });
    }

    private void writeCmd(string command, string arg0, string arg1, byte[] arg2)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, 3);
        writeCmdItem(w, arg0);
        writeCmdItem(w, arg1);
        writeCmdItem(w, arg2);
      });
    }

    private void writeCmd(string command, string arg0, Stream arg1, int count)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, 2);
        writeCmdItem(w, arg0);
        writeCmdItem(w, arg1, count);
      });
    }

    private void writeCmd(string command, string arg0, string arg1,
      Stream arg2, int count)
    {
      writeCore(w =>
      {
        writeCmdStart(w, command, 3);
        writeCmdItem(w, arg0);
        writeCmdItem(w, arg1);
        writeCmdItem(w, arg2, count);
      });
    }


    private void writeCmdStart(RedisWriter w, string command,
      int numBulks)
    {
      w.WriteTypeChar(ResponseType.MultiBulk);
      w.WriteLine(numBulks + 1);

      w.WriteTypeChar(ResponseType.Bulk);
      w.WriteLine(command.Length);
      w.WriteLine(command);
    }

    private void writeCmdItem(RedisWriter w, string data)
    {
      // TODO: Should really re-use string buffers.
      //  because writeCmditem maybe called in large loops such as 
      //  while using MSET or MGET
      var buffer = encodeStr(data);

      w.WriteTypeChar(ResponseType.Bulk);
      w.WriteLine(buffer.Length);
      w.WriteBulk(buffer);
    }

    private void writeCmdItem(RedisWriter w, Stream source, int count)
    {
      w.WriteTypeChar(ResponseType.Bulk);
      w.WriteLine(count);
      w.WriteBulkFrom(source, count);
    }

    private void writeCmdItem(RedisWriter w, byte[] buffer)
    {
      w.WriteTypeChar(ResponseType.Bulk);
      w.WriteLine(buffer.Length);
      w.WriteBulk(buffer);
    }

    private void writeCmdItem(RedisWriter w, ArraySegment<byte> segment)
    {
      w.WriteTypeChar(ResponseType.Bulk);
      w.WriteLine(segment.Count);
      w.WriteBulk(segment.Array, segment.Offset, segment.Count);
    }
  }
}
