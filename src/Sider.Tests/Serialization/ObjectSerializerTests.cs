
using NUnit.Framework;
using Sider.Serialization;

namespace Sider.Tests.Serialization
{
  public class ObjectSerializerTests : SerializationTestBase<ObjectSerializer, object>
  {
    [Test]
    public void WriteReadRoundtrip_Class_GotEquivalentObject()
    {
      var obj = new TestObj("Hello", 1, "World", 2);
      var result = SerializationRoundtrip(obj);

      // result should be a struct
      Assert.That(result, Is.Not.Null);
      Assert.That(result, Is.InstanceOf(obj.GetType()));

      var toResult = (TestObj)result;
      Assert.That(toResult.Str, Is.EqualTo(obj.Str));
      Assert.That(toResult.Int, Is.EqualTo(obj.Int));
      Assert.That(toResult.Nested, Is.Not.Null);
      Assert.That(toResult.Nested.Str, Is.EqualTo(obj.Nested.Str));
      Assert.That(toResult.Nested.Int, Is.EqualTo(obj.Nested.Int));
    }

    [Test]
    public void WriteReadRoundtrip_Struct_GotEqualObject()
    {
      var obj = new TestStruct("Hello", 42, "World", 1337);
      var result = SerializationRoundtrip(obj);

      Assert.That(result, Is.EqualTo(obj));
    }

    [Test]
    public void WriteReadRoundtrip_Null_GotNullWithoutException()
    {
      var result = SerializationRoundtrip(null);
      Assert.That(result, Is.Null);
    }
  }
}
