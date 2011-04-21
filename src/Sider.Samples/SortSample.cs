
using System;
using System.Linq;

namespace Sider.Samples
{
  public class SortSample : Sample
  {
    public override string Name { get { return "Specifying complex SORT arguments."; } }


    public override void Run()
    {
      var personnels = Personnel.GetSamplePersonnels();
      var client = new RedisClient();

      var listKey = "personnels";

      // setup sample data
      client.Pipeline(c =>
      {
        client.Del(listKey);

        foreach (var person in personnels) {
          var idStr = person.Id.ToString();

          c.LPush(listKey, idStr);
          c.HSet(idStr, "name", person.Name);
          c.HSet(idStr, "email", person.Email);
          c.HIncrBy(idStr, "age", person.Age);
          c.HIncrBy(idStr, "salary", person.Salary);
          c.HSet(idStr, "json", person.ToJson());
        }
      });

      while (true) {
        WriteLine("0. List personnels sorted by yearly income.");
        WriteLine("1. List personnels sorted by age.");
        WriteLine("2. List personnels sorted by id descending.");
        WriteLine("3. List personnels sorted by name.");
        WriteLine("4. List all personnel emails.");

        string[] result;
        var parseResult = true;

        switch (ReadLine()) {
        case "0": {
          result = client.Sort(listKey,
          byPattern: "*->salary",
          getPattern: new[] { "*->json" });
        } break;

        case "1": {
          parseResult = true;
          result = client.Sort(listKey,
          byPattern: "*->age",
          getPattern: new[] { "*->json" });
        } break;

        case "2": {
          parseResult = true;
          result = client.Sort(listKey,
          descending: true,
          getPattern: new[] { "*->json" });
        } break;

        case "3": {
          result = client.Sort(listKey,
          byPattern: "*->name",
          alpha: true,
          getPattern: new[] { "*->json" });
        } break;

        case "4": {
          result = client.Sort(listKey,
          byPattern: "nosort", // non-existent key
          getPattern: new[] { "*->email" });
          parseResult = false;
        } break;

        default: continue;
        }

        if (parseResult)
          result = result
            .Select(json => Personnel.FromJson(json).ToString())
            .ToArray();

        WriteLine("\r\nResults:");
        Array.ForEach(result, WriteLine);
        WriteLine("\r\n");
      }
    }
  }
}
