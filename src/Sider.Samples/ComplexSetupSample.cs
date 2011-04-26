
using System;
using System.Globalization;
using System.IO;
using Sider.Serialization;

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
        .OverrideCulture(CultureInfo.GetCultureInfo("fr-FR")) // non-default culture
        .ReconnectOnIdle(true) // auto reconnect on idle
        .ReissueCommandsOnReconnect(false); // never retry automatically

      // create some test data
      var personnels = Personnel.GetSamplePersonnels();

      // create a client that works with `Personnel` class
      var client = new RedisClient<Personnel>(settings);

      // add personnels data to sorted set
      client.Pipeline(c =>
      {
        client.Del("p:salary", "p:sal_week", "p:age"); // reset old values
        Array.ForEach(personnels, p => c.ZAdd("p:salary", p.Salary, p));
        Array.ForEach(personnels, p => c.ZAdd("p:sal_week", (double)p.Salary / 4.0, p));
        Array.ForEach(personnels, p => c.ZAdd("p:age", p.Age, p));
      });

      while (true) {
        WriteLine("0. List personnels sorted by yearly income.");
        WriteLine("1. List personnels sorted by weekly income.");
        WriteLine("2. List personnels sorted by age.");

        Personnel[] result;

        switch (ReadLine()) {
        case "0": result = client.ZRangeByScore("p:salary", 0, 99999); break;
        case "1": result = client.ZRangeByScore("p:sal_week", 0, 99999); break;
        case "2": result = client.ZRangeByScore("p:age", 0, 99999); break;
        default: continue;
        }

        Array.ForEach(result, p => WriteLine(p.ToString()));
      }
    }
  }
}
