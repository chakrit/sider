
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sider.Tests
{
  [TestClass]
  public class RedisWriterTests
  {
    public struct WriterInfo
    {
      public RedisWriter Writer { get; set; }
      public MemoryStream Stream { get; set; }
      public byte[] Buffer { get; set; }
    }

    private WriterInfo createWriter(int bufferSize = 1024,
      int? writerBufferSize = null)
    {
      var buffer = new byte[bufferSize];
      var stream = new MemoryStream(buffer, true);
      stream.SetLength(0);

      var writer = writerBufferSize.HasValue ?
        new RedisWriter(stream, writerBufferSize.Value) :
        new RedisWriter(stream);

      return new WriterInfo {
        Writer = writer,
        Stream = stream,
        Buffer = buffer
      };
    }

    private byte[] getRandomBuffer(int length = 16)
    {
      var buffer = new byte[length];
      (new Random()).NextBytes(buffer);

      return buffer;
    }


    #region Ctor
    [TestMethod, ExpectedException(typeof(ArgumentNullException)), Conditional("DEBUG")]
    public void Ctor_StreamIsNull_ExceptionThrown()
    {
      new RedisWriter(null);
    }

    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void Ctor_LineBufferSizeIsZero_ExceptionThrown()
    {
      new RedisWriter(new MemoryStream(), 0);
    }

    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void Ctor_LineBufferSizeIsNegative_ExceptionThrown()
    {
      new RedisWriter(new MemoryStream(), -1);
    }

    [TestMethod, ExpectedException(typeof(ArgumentException)), Conditional("DEBUG")]
    public void Ctor_StreamIsNotWritable_ExceptionThrown()
    {
      new RedisWriter(new MemoryStream(new byte[16], false));
    }
    #endregion

    #region WriteLine
    [TestMethod, ExpectedException(typeof(ArgumentNullException)), Conditional("DEBUG")]
    public void WriteLine_NullString_ExceptionThrown()
    {
      createWriter().Writer.WriteLine(null);
    }

    [TestMethod]
    public void WriteLine_EmptyString_CrLfWrittenToStream()
    {
      var pack = createWriter();

      pack.Writer.WriteLine("");

      Assert.AreEqual(0x0D, pack.Buffer[0]);
      Assert.AreEqual(0x0A, pack.Buffer[1]);
    }

    [TestMethod]
    public void WriteLine_SimpleString_StringWithCrLfWrittenToStream()
    {
      var testStr = "test";
      var pack = createWriter();

      pack.Writer.WriteLine(testStr);

      var str = Encoding.Default.GetString(pack.Buffer, 0, 6);
      Assert.AreEqual(testStr + "\r\n", str);
    }

    [TestMethod]
    public void WriteLine_WithNumber_SimpleNumbers_NumberStringWithCrLfWrittenToStream()
    {
      var testNum = 123;
      var pack = createWriter();

      pack.Writer.WriteLine(testNum);

      var resultStr = Encoding.Default.GetString(pack.Buffer, 0, 3);
      Assert.AreEqual(testNum.ToString(), resultStr);
    }
    #endregion

    #region WriteTypeChar
    [TestMethod]
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
        Assert.AreEqual(value, b);
      }
    }

    [TestMethod, ExpectedException(typeof(ArgumentException)), Conditional("DEBUG")]
    public void WriteTypeChar_InvalidChar_ExceptionThrown()
    {
      var pack = createWriter();
      pack.Writer.WriteTypeChar((ResponseType)777);
    }
    #endregion

    #region WriteBulk
    [TestMethod, ExpectedException(typeof(ArgumentNullException)), Conditional("DEBUG")]
    public void WriteBulk_NullBuffer_ExceptionThrown()
    {
      createWriter().Writer.WriteBulk((byte[])null);
    }

    [TestMethod]
    public void WriteBulk_EmptyBuffer_CrLfWrittenToStream()
    {
      var pack = createWriter();
      pack.Writer.WriteBulk(new byte[] { });

      Assert.AreEqual(0x0D, pack.Buffer[0]);
      Assert.AreEqual(0x0A, pack.Buffer[1]);
    }

    [TestMethod]
    public void WriteBulk_SmallBuffer_BufferWithCrLfWrittenToStream()
    {
      var writeBuffer = getRandomBuffer(16);

      writeBulk_writeTestCore(w => w.WriteBulk(writeBuffer), writeBuffer);
    }

    [TestMethod]
    public void WriteBulk_LargeBuffer_BufferWithCrLfWrittenToStream()
    {
      var bufferSize = RedisWriter.DefaultBufferSize * 4;
      var writeBuffer = getRandomBuffer(bufferSize);

      writeBulk_writeTestCore(w => w.WriteBulk(writeBuffer), writeBuffer);
    }


    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void WriteBulk_WithOffset_NegativeOffset_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest(getRandomBuffer(), -1, 16);
    }

    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void WriteBulk_WithOffset_NegativeCount_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest(getRandomBuffer(), 0, -1);
    }

    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void WriteBulk_WithOffset_TooLargeOffset_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest(getRandomBuffer(10), 10, 0);
    }

    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void WriteBulk_WithOffset_TooLargeCount_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest(getRandomBuffer(10), 0, 11);
    }

    [TestMethod, ExpectedException(typeof(ArgumentException)), Conditional("DEBUG")]
    public void WriteBulk_WithOffset_OffsetPlusCountOverflowTheBuffer_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest(getRandomBuffer(10), 7, 7);
    }

    [TestMethod, ExpectedException(typeof(ArgumentNullException)), Conditional("DEBUG")]
    public void WriteBulk_WithOffset_BufferIsNull_ExceptionThrown()
    {
      writeBulk_withOffset_exceptionTest(null, 0, 0);
    }

    [TestMethod]
    public void WriteBulk_WithOffset_ZeroOffsetAndZeroCount_CrLfWrittenToStream()
    {
      var buffer = new byte[0];
      writeBulk_writeTestCore(w => w.WriteBulk(buffer, 0, 0), buffer);
    }

    [TestMethod]
    public void WriteBulk_WithOffset_SmallBuffer_StreamContainsBufferWithCrLf()
    {
      var buffer = getRandomBuffer(16);
      writeBulk_writeTestCore(w => w.WriteBulk(buffer, 0, buffer.Length), buffer);
    }

    [TestMethod]
    public void WriteBulk_WithOffset_NonZeroOffsetWithSmallBuffer_StreamContainsPartialBufferWithCrLf()
    {
      var buffer = getRandomBuffer(16);
      var expected = buffer.Skip(9).ToArray();

      writeBulk_writeTestCore(w => w.WriteBulk(buffer, 9, 16 - 9), expected);
    }

    [TestMethod]
    public void WriteBulk_WithOffset_LargeBuffer_StreamContainsBufferWithCrLf()
    {
      var buffer = getRandomBuffer(RedisWriter.DefaultBufferSize * 4);
      writeBulk_writeTestCore(w => w.WriteBulk(buffer, 0, buffer.Length), buffer);
    }


    private void writeBulk_withOffset_exceptionTest(byte[] buffer, int offset, int count)
    {
      createWriter().Writer.WriteBulk(buffer, offset, count);
    }
    #endregion

    #region WriteBulkFrom
    [TestMethod, ExpectedException(typeof(ArgumentNullException)), Conditional("DEBUG")]
    public void WriteBulkFrom_NullStream_ExceptionThrown()
    {
      writeBulkFrom_exceptionTest(null, 0);
    }

    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void WriteBulkFrom_NegativeCount_ExceptionThrown()
    {
      writeBulkFrom_exceptionTest(new MemoryStream(getRandomBuffer()), -1);
    }

    [TestMethod, ExpectedException(typeof(MyException))]
    public void WriteBulkFrom_InputStreamFailedMidway_ExceptionPreservedAndThrown()
    {
      var buffer = getRandomBuffer(10);
      var stream = new TestExceptionStream(new MemoryStream(buffer),
        5, new MyException());

      writeBulk_writeTestCore(w => w.WriteBulkFrom(stream, buffer.Length), buffer);
    }

    [TestMethod]
    public void WriteBulkFrom_InputStreamFailedMidway_ProtocolStillMaintained()
    {
      var errorIdx = 4; // error at the 5th byte
      var buffer = getRandomBuffer(10);
      var validStream = new MemoryStream(buffer, false);
      var errorStream = new TestExceptionStream(new MemoryStream(buffer, false),
        errorIdx + 1, new MyException());

      // create a writer with only 1 byte buffer size, so all the bytes before
      // the error point is written through to the output stream
      var pack = createWriter(writerBufferSize: 1);

      // write twice
      try {
        pack.Writer.WriteBulkFrom(errorStream, buffer.Length);
        Assert.Fail("Expected MyException to be thrown.");
      }
      catch (MyException) {
        pack.Writer.WriteBulkFrom(validStream, buffer.Length);
      }

      pack.Stream.Flush();
      Assert.AreEqual(pack.Stream.Length, 2 * (10 /*buffer*/ + 2 /*CRLF*/));

      // check the series of bytes in the buffer before the error point
      var bufferIdx = 0;
      for (; bufferIdx < errorIdx; bufferIdx++)
        Assert.AreEqual(buffer[bufferIdx], pack.Buffer[bufferIdx]);

      // skip the rest of the buffer since the result is undefined
      bufferIdx = 10;

      // ensure, though, that the protocol is still maintained
      Assert.AreEqual(pack.Buffer[bufferIdx++], 0x0D);
      Assert.AreEqual(pack.Buffer[bufferIdx++], 0x0A);

      // check the second written buffer, it should also be correct
      for (; bufferIdx < 22; bufferIdx++)
        Assert.AreEqual(pack.Buffer[bufferIdx], buffer[bufferIdx - 12]);

      Assert.AreEqual(pack.Buffer[bufferIdx++], 0x0D);
      Assert.AreEqual(pack.Buffer[bufferIdx++], 0x0A);
    }

    [TestMethod]
    public void WriteBulkFrom_ZeroCount_CrLfWrittenToStream()
    {
      using (var ms = new MemoryStream(getRandomBuffer()))
        writeBulk_writeTestCore(w => w.WriteBulkFrom(ms, 0), new byte[0]);
    }

    [TestMethod]
    public void WriteBulkFrom_SmallData_InputStreamDataWithCrLfWrittenToStream()
    {
      var buffer = getRandomBuffer(16);
      using (var ms = new MemoryStream(buffer))
        writeBulk_writeTestCore(w => w.WriteBulkFrom(ms, buffer.Length), buffer);
    }

    [TestMethod, ExpectedException(typeof(InvalidOperationException)), Conditional("DEBUG")]
    public void WriteBulkFrom_CountLargerThanAvailableInStream_ExceptionThrown()
    {
      var buffer = getRandomBuffer(16);
      using (var ms = new MemoryStream()) {
        ms.Write(buffer, 0, buffer.Length);
        writeBulkFrom_exceptionTest(ms, 99);
      }
    }

    [TestMethod]
    public void WriteBulkFrom_LargeData_AllDataAndCrLfWrittenToStream()
    {
      var buffer = getRandomBuffer(RedisWriter.DefaultBufferSize * 4);
      using (var ms = new MemoryStream(buffer))
        writeBulk_writeTestCore(w => w.WriteBulkFrom(ms, buffer.Length), buffer);
    }


    private void writeBulkFrom_exceptionTest(Stream s, int count)
    {
      createWriter().Writer.WriteBulkFrom(s, count);
    }
    #endregion


    private void writeBulk_writeTestCore(Action<RedisWriter> writeCore, byte[] expectedBuffer)
    {
      var pack = createWriter(expectedBuffer.Length + 2);

      writeCore(pack.Writer);

      var bufferIdx = 0;
      for (; bufferIdx < expectedBuffer.Length; bufferIdx++)
        Assert.AreEqual(expectedBuffer[bufferIdx], pack.Buffer[bufferIdx]);

      Assert.AreEqual(0x0D, pack.Buffer[bufferIdx++]);
      Assert.AreEqual(0x0A, pack.Buffer[bufferIdx++]);
    }
  }
}
