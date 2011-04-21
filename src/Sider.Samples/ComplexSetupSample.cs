
using System;
using System.IO;

namespace Sider.Samples
{
  public class ComplexSetupSample : Sample
  {
    public override string Name
    {
      get { return "Complex setup."; }
    }


    protected class PersonnelSerializer : ISerializer<Personnel>
    {
      private MemoryStream _temp;

      public void Init(RedisSettings settings)
      {
        _temp = new MemoryStream(settings.SerializationBufferSize);
      }


      public Personnel Read(Stream src, int length)
      {
        var reader = new BinaryReader(src);
        return new Personnel {
          Name = reader.ReadString(),
          Email = reader.ReadString(),
          Age = reader.ReadInt16(),
          Salary = reader.ReadInt32()
        };
      }


      public int GetBytesNeeded(Personnel obj)
      {
        // serialize to temporary stream to find out the number of bytes needed
        _temp.SetLength(0);

        var bw = new BinaryWriter(_temp);
        bw.Write(obj.Name);
        bw.Write(obj.Email);
        bw.Write(obj.Age);
        bw.Write(obj.Salary);
        bw.Flush();

        return (int)_temp.Length;
      }

      public void Write(Personnel obj, Stream dest, int bytesNeeded)
      {
        // always called *after* GetBytesNeeded so we can reuse the temp stream here
        _temp.Seek(0, SeekOrigin.Begin);
        _temp.CopyTo(dest);
      }
    }


    public override void Run()
    {
      var settings = RedisSettings.New()
        .ReadBufferSize(8192) // large reads
        .WriteBufferSize(256) // small writes
        .StringBufferSize(32) // very short strings
        .SerializationBufferSize(8192) // as large as reads
        .OverrideSerializer(new PersonnelSerializer()) // custom serializer
        .ReconnectOnIdle(true) // auto reconnect on idle
        .ReissueCommandsOnReconnect(false)  // never retry automatically
        .ReissueReadOnReconnect(false);

      // create some test data
      var personnels = Personnel.GetSamplePersonnels();

      // create a client that works with `Personnel` class
      var client = new RedisClient<Personnel>(settings);

      // add personnels data to sorted set
      client.Pipeline(c =>
      {
        client.Del("p:salary", "p:age"); // reset old values
        Array.ForEach(personnels, p => c.ZAdd("p:salary", p.Salary, p));
        Array.ForEach(personnels, p => c.ZAdd("p:age", p.Age, p));
      });

      while (true) {
        WriteLine("0. List personnels sorted by yearly income.");
        WriteLine("1. List personnels sorted by age.");

        var result = ReadLine() == "0" ?
          client.ZRangeByScore("p:salary", 0, 99999) :
          client.ZRangeByScore("p:age", 0, 9999);

        Array.ForEach(result, p => WriteLine(p.ToString()));
      }
    }
  }
}
