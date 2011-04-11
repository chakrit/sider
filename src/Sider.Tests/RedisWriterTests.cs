
using System;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;

namespace Sider.Tests
{
  [TestFixture]
  public class RedisWriterTests
  {
    public struct WriterInfo
    {
      public RedisWriter Writer { get; set; }
      public MemoryStream Stream { get; set; }

      public Mock<ISerializer<string>> Mock { get; set; }
      public byte[] Buffer { get; set; }
    }

    private WriterInfo createWriter(int bufferSize = 1024,
      int? writerBufferSize = null)
    {
      var buffer = new byte[bufferSize];
      var stream = new MemoryStream(buffer, true);
      stream.SetLength(0);

      var writer = writerBufferSize.HasValue ?
        new RedisWriter(stream, new RedisSettings(writeBufferSize: writerBufferSize.Value)) :
        new RedisWriter(stream);

      // for testing purpose, we don't need to keep flushing all the time
      writer.AutoFlush = true;

      return new WriterInfo {
        Writer = writer,
        Stream = stream,
        Buffer = buffer,
        Mock = new Mock<ISerializer<string>>()
      };
    }

    private byte[] getRandomBuffer(int length = 16)
    {
      var buffer = new byte[length];
      (new Random()).NextBytes(buffer);

      return buffer;
    }


    #region Ctor
    [Test]
    public void Ctor_StreamIsNull_ExceptionThrown()
    {
      Assert.Throws<ArgumentNullException>(() => new RedisWriter(null));
    }

    [Test]
    public void Ctor_SettingsIsNull_ExceptionThrown()
    {
      Assert.Throws<ArgumentNullException>(() =>
        new RedisWriter(new MemoryStream(new byte[] { 0xFF }), null));
    }

    [Test]
    public void Ctor_StreamIsNotWritable_ExceptionThrown()
    {
      Assert.Throws<ArgumentException>(() =>
        new RedisWriter(new MemoryStream(new byte[16], false)));
    }
    #endregion

    #region WriteLine
    [Test]
    public void WriteLine_NullString_ExceptionThrown()
    {
      Assert.Throws<ArgumentNullException>(() =>
        createWriter().Writer.WriteLine(null));
    }

    [Test]
    public void WriteLine_EmptyString_CrLfWrittenToStream()
    {
      var pack = createWriter();
      pack.Writer.WriteLine("");

      Assert.That(pack.Buffer[0], Is.EqualTo(0x0D));
      Assert.That(pack.Buffer[1], Is.EqualTo(0X0A));
    }

    [Test]
    public void WriteLine_SimpleString_StringWithCrLfWrittenToStream()
    {
      var testStr = "test";
      var pack = createWriter();

      pack.Writer.WriteLine(testStr);

      var str = Encoding.Default.GetString(pack.Buffer, 0, 6);
      Assert.That(str, Is.EqualTo(testStr + "\r\n"));
    }

    [Test]
    public void WriteLine_WithNumber_SimpleNumbers_NumberStringWithCrLfWrittenToStream()
    {
      var testNum = 123;
      var pack = createWriter();

      pack.Writer.WriteLine(testNum);

      var resultStr = Encoding.Default.GetString(pack.Buffer, 0, 3);
      Assert.That(resultStr, Is.EqualTo(testNum.ToString()));
    }
    #endregion

    #region WriteTypeChar
    [Test]
    public void WriteTypeChar_NormalValues_CorrectTypeCharWrittenToStream()
    {
      var type = typeof(ResponseType);
      var values = Enum
        .GetValues(type)
        .Cast<ResponseType>();

      var pack = createWriter();
      var bufferIdx = 0;

      foreach (var value in values) {
        pack.Writer.WriteTypeChar(value);

        var b = (ResponseType)pack.Buffer[bufferIdx++];
        Assert.That(b, Is.EqualTo(value));
      }
    }

    [Test]
    public void WriteTypeChar_InvalidChar_ExceptionThrown()
    {
      var pack = createWriter();
      Assert.Throws<ArgumentException>(() =>
        pack.Writer.WriteTypeChar((ResponseType)777));
    }
    #endregion

    #region WriteBulk
    [Test]
    public void WriteBulk_NullBuffer_ExceptionThrown()
    {
      Assert.Throws<ArgumentNullException>(() =>
        createWriter().Writer.WriteBulk((byte[])null));
    }

    [Test]
    public void WriteBulk_EmptyBuffer_CrLfWrittenToStream()
    {
      var pack = createWriter();
      pack.Writer.WriteBulk(new byte[] { });

      Assert.That(pack.Buffer[0], Is.EqualTo(0x0D));
      Assert.That(pack.Buffer[1], Is.EqualTo(0x0A));
    }

    [Test]
    public void WriteBulk_SmallBuffer_BufferWithCrLfWrittenToStream()
    {
      var writeBuffer = getRandomBuffer(16);
      writeBulk_writeTestCore(w => w.WriteBulk(writeBuffer), writeBuffer);
    }

    [Test]
    public void WriteBulk_LargeBuffer_BufferWithCrLfWrittenToStream()
    {
      var bufferSize = RedisSettings.DefaultWriterBufferSize * 4;
      var writeBuffer = getRandomBuffer(bufferSize);

      writeBulk_writeTestCore(w => w.WriteBulk(writeBuffer), writeBuffer);
    }


    [Test]
    public void WriteBulk_WithOffset_NegativeOffset_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest<ArgumentOutOfRangeException>(
        getRandomBuffer(), -1, 16);
    }

    [Test]
    public void WriteBulk_WithOffset_NegativeCount_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest<ArgumentOutOfRangeException>(
        getRandomBuffer(), 0, -1);
    }

    [Test]
    public void WriteBulk_WithOffset_TooLargeOffset_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest<ArgumentOutOfRangeException>(
        getRandomBuffer(10), 10, 0);
    }

    [Test]
    public void WriteBulk_WithOffset_TooLargeCount_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest<ArgumentOutOfRangeException>(
        getRandomBuffer(10), 0, 11);
    }

    [Test]
    public void WriteBulk_WithOffset_OffsetPlusCountOverflowTheBuffer_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest<ArgumentException>(
        getRandomBuffer(10), 7, 7);
    }

    [Test]
    public void WriteBulk_WithOffset_BufferIsNull_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest<ArgumentNullException>(null, 0, 0);
    }

    [Test]
    public void WriteBulk_WithOffset_ZeroOffsetAndZeroCount_CrLfWrittenToStream()
    {
      var buffer = new byte[0];
      writeBulk_writeTestCore(w => w.WriteBulk(buffer, 0, 0), buffer);
    }

    [Test]
    public void WriteBulk_WithOffset_SmallBuffer_StreamContainsBufferWithCrLf()
    {
      var buffer = getRandomBuffer(16);
      writeBulk_writeTestCore(w => w.WriteBulk(buffer, 0, buffer.Length), buffer);
    }

    [Test]
    public void WriteBulk_WithOffset_NonZeroOffsetWithSmallBuffer_StreamContainsPartialBufferWithCrLf()
    {
      var buffer = getRandomBuffer(16);
      var expected = buffer.Skip(9).ToArray();

      writeBulk_writeTestCore(w => w.WriteBulk(buffer, 9, 16 - 9), expected);
    }

    [Test]
    public void WriteBulk_WithOffset_LargeBuffer_StreamContainsBufferWithCrLf()
    {
      var buffer = getRandomBuffer(RedisSettings.DefaultWriterBufferSize * 4);
      writeBulk_writeTestCore(w => w.WriteBulk(buffer, 0, buffer.Length), buffer);
    }


    private void writeBulk_withOffset_exceptionTest<TException>(byte[] buffer, int offset,
      int count)
      where TException : Exception
    {
      Assert.Throws<TException>(() =>
        createWriter().Writer.WriteBulk(buffer, offset, count));
    }
    #endregion

    #region WriteBulkFrom
    [Test]
    public void WriteBulkFrom_NullStream_ExceptionThrown()
    {
      writeBulkFrom_exceptionTest<ArgumentNullException>(null, 0);
    }

    [Test]
    public void WriteBulkFrom_NegativeCount_ExceptionThrown()
    {
      writeBulkFrom_exceptionTest<ArgumentOutOfRangeException>(
        new MemoryStream(getRandomBuffer()), -1);
    }

    [Test]
    public void WriteBulkFrom_InputStreamFailedMidway_ExceptionPreservedAndThrown()
    {
      var buffer = getRandomBuffer(10);
      var stream = new TestExceptionStream(new MemoryStream(buffer),
        5, new MyException());

      Assert.Throws<MyException>(() =>
        writeBulk_writeTestCore(w => w.WriteBulkFrom(stream, buffer.Length), buffer));
    }

    [Test]
    public void WriteBulkFrom_ZeroCount_CrLfWrittenToStream()
    {
      using (var ms = new MemoryStream(getRandomBuffer()))
        writeBulk_writeTestCore(w => w.WriteBulkFrom(ms, 0), new byte[0]);
    }

    [Test]
    public void WriteBulkFrom_SmallData_InputStreamDataWithCrLfWrittenToStream()
    {
      var buffer = getRandomBuffer(16);
      using (var ms = new MemoryStream(buffer))
        writeBulk_writeTestCore(w => w.WriteBulkFrom(ms, buffer.Length), buffer);
    }

    [Test]
    public void WriteBulkFrom_CountLargerThanAvailableInStream_ExceptionThrown()
    {
      var buffer = getRandomBuffer(16);
      using (var ms = new MemoryStream()) {
        ms.Write(buffer, 0, buffer.Length);
        writeBulkFrom_exceptionTest<InvalidOperationException>(ms, 99);
      }
    }

    [Test]
    public void WriteBulkFrom_LargeData_AllDataAndCrLfWrittenToStream()
    {
      var buffer = getRandomBuffer(RedisSettings.DefaultWriterBufferSize);
      using (var ms = new MemoryStream(buffer))
        writeBulk_writeTestCore(w => w.WriteBulkFrom(ms, buffer.Length), buffer);
    }


    private void writeBulkFrom_exceptionTest<TException>(Stream s, int count)
      where TException : Exception
    {
      Assert.Throws<TException>(() =>
        createWriter().Writer.WriteBulkFrom(s, count));
    }
    #endregion

    #region WriteSerializedBulk

    [Test]
    public void WriteSerializedBulk_SerializerIsNull_ExceptionThrown()
    {
      Assert.Throws<ArgumentNullException>(() =>
        createWriter().Writer.WriteSerializedBulk<string>(null, string.Empty, 0));
    }

    [Test]
    public void WriteSerializedBulk_NegativeCount_ExceptionThrown()
    {
      var pack = createWriter();
      Assert.Throws<ArgumentOutOfRangeException>(() => pack.Writer
        .WriteSerializedBulk<string>(pack.Mock.Object, "", -1));
    }

    [Test]
    public void WriteSerializedBulk_ValidSerializer_SerializedWriteCalled()
    {
      var pack = createWriter();
      pack.Mock
        .Setup(s => s.Write(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<int>()));

      pack.Writer.WriteSerializedBulk(pack.Mock.Object, "", 3);
      pack.Mock.VerifyAll();
    }

    [Test]
    public void WriteSerailizedBulk_ValidSerializer_WritableStreamPassedToSerializer()
    {
      var pack = createWriter();
      pack.Mock.Setup(s => s.Write(It.IsAny<string>(),
        It.Is<Stream>(stream => stream != null && stream.CanWrite),
        It.IsAny<int>()));

      pack.Writer.WriteSerializedBulk(pack.Mock.Object, "asdf", 4);
      pack.Mock.VerifyAll();
    }

    [Test]
    public void WriteSerializedBulk_ValidObject_ObjectPassedToSerializer()
    {
      var pack = createWriter();
      var str = "YAY";
      pack.Mock.Setup(s => s.Write(str, It.IsAny<Stream>(), It.IsAny<int>()));

      pack.Writer.WriteSerializedBulk(pack.Mock.Object, str, 3);
      pack.Mock.VerifyAll();
    }

    [Test]
    public void WriteSerializedBulk_ValidCount_CountPassedToSerializer()
    {
      var pack = createWriter();
      var str = "YAY";
      pack.Mock.Setup(s => s.Write(It.IsAny<string>(), It.IsAny<Stream>(), 1337));

      pack.Writer.WriteSerializedBulk(pack.Mock.Object, str, 1337);
      pack.Mock.VerifyAll();
    }

    [Test]
    public void WriteSerializedBulk_ValidObject_StreamPassedToSerializer()
    {
      var pack = createWriter();
      var str = "Test";

      var mock = new Mock<ISerializer<string>>();
      mock.Setup(s => s.Write(It.IsAny<string>(),
        It.Is<Stream>(stream => stream != null),
        It.IsAny<int>()));

      pack.Writer.WriteSerializedBulk(mock.Object, str, 4);
      mock.VerifyAll();
    }

    [Test]
    public void WriteSerializedBulk_SerializerDone_CrLfWrittenToStream()
    {
      var pack = createWriter();
      pack.Writer.WriteSerializedBulk(pack.Mock.Object, "test", 1234);

      Assert.That(pack.Buffer[0], Is.EqualTo(0x0D));
      Assert.That(pack.Buffer[1], Is.EqualTo(0x0A));
    }

    #endregion


    private void writeBulk_writeTestCore(Action<RedisWriter> writeCore, byte[] expectedBuffer)
    {
      var pack = createWriter(expectedBuffer.Length + 2);
      writeCore(pack.Writer);

      Assert.That(pack.Buffer.Take(expectedBuffer.Length),
        Is.EquivalentTo(expectedBuffer));

      Assert.That(pack.Buffer[expectedBuffer.Length], Is.EqualTo(0x0D));
      Assert.That(pack.Buffer[expectedBuffer.Length + 1], Is.EqualTo(0x0A));
    }
  }
}
