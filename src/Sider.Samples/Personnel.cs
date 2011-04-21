
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace Sider.Samples
{
  public class Personnel
  {
    private static int _instanceCounter = 0;

    public int Id { get; set; }

    public string Name { get; set; }
    public string Email { get; set; }

    public short Age { get; set; }
    public int Salary { get; set; }


    public Personnel() { Id = _instanceCounter++; }

    public static Personnel FromJson(string json)
    {
      return new JavaScriptSerializer().Deserialize<Personnel>(json);
    }

    public string ToJson()
    {
      return new JavaScriptSerializer().Serialize(this);
    }


    public static Personnel[] GetSamplePersonnels()
    {
      return generateSamplePersonnels().ToArray();
    }

    private static IEnumerable<Personnel> generateSamplePersonnels()
    {
      yield return new Personnel {
        Name = "Jane Doe",
        Email = "jane@example.com",
        Age = 20,
        Salary = 2000,
      };

      yield return new Personnel {
        Name = "John Doe",
        Email = "john@example.com",
        Age = 19,
        Salary = 4000,
      };

      yield return new Personnel {
        Name = "Jack Hammer",
        Email = "jack@example.com",
        Age = 18,
        Salary = 3000
      };
    }


    public override string ToString()
    {
      return string.Format("{0,12} ({1}) {2} years old, ${3} yearly income.",
        Name, Email, Age, Salary * 12);
    }
  }
}
