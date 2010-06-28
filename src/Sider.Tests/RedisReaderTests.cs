
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sider.Tests
{
  [TestClass]
  public class RedisReaderTests : SiderTestBase
  {
    private RedisReader createReader(string data)
    {
      return createReader(Encoding.Default.GetBytes(data));
    }

    private RedisReader createReader(byte[] data)
    {
      return new RedisReader(new MemoryStream(data));
    }


    [TestMethod, ExpectedException(typeof(ArgumentNullException)), Conditional("DEBUG")]
    public void Ctor_StreamIsNull_ExceptionThrown()
    {
      new RedisReader(null);
    }

    [TestMethod, ExpectedException(typeof(ArgumentException)), Conditional("DEBUG")]
    public void Ctor_StreamUnreadble_ExceptionThrown()
    {
      var tempFile = Path.GetTempFileName();
      var fs = File.OpenWrite(tempFile);

      try {
        new RedisReader(fs);
      }
      finally {
        fs.Close();
        fs.Dispose();

        File.Delete(tempFile);
      }
    }


    #region ReadTypeChar
    [TestMethod]
    public void ReadTypeChar_AllTypeChars_CorrectTypeReturned()
    {
      var typeChars = Enum
        .GetValues(typeof(ResponseType))
        .OfType<ResponseType>()
        .Select(c => (char)c)
        .ToArray();

      var data = typeChars.Aggregate("", (c1, c2) => c1 + c2);
      var reader = createReader(data);

      foreach (var t in typeChars)
        Assert.AreEqual((ResponseType)t, reader.ReadTypeChar());
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadTypeChar_InvalidData_ExceptionThrown()
    {
      var reader = createReader("RANDOM_JUNK");
      reader.ReadTypeChar();
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadTypeChar_EmptyData_ExceptionThrown()
    {
      var reader = createReader("");
      reader.ReadTypeChar();
    }
    #endregion

    #region ReadNumberLine
    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadNumberLine_EmptyData_ExceptionThrown()
    {
      readNumberLine_exceptionTest("");
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadNumberLine_JustCrLf_ExceptionThrown()
    {
      readNumberLine_exceptionTest("\r\n");
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadNumberLine_ZeroWithNoCrLf_ExceptionThrown()
    {
      readNumberLine_exceptionTest("0");
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadNumberLine_ZeroWithCrButNoLf_ExceptionThrown()
    {
      readNumberLine_exceptionTest("0\r");
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadNumberLine_ZeroWithLfButNoCr_ExceptionThrown()
    {
      readNumberLine_exceptionTest("0\n");
    }

    [TestMethod]
    public void ReadNumberLine_LineWithZero_ZeroReturned()
    {
      var reader = createReader("0\r\n");
      var num = reader.ReadNumberLine();

      Assert.AreEqual(0, num);
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadNumberLine_GarbledData_ExceptionThrown()
    {
      readNumberLine_exceptionTest("SDOIJFOEIWJF\r\n");
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadNumberLine_SomeGarbledDigit_ExceptionThrown()
    {
      readNumberLine_exceptionTest("123G456\r\n");
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadNumberLine_NegativeNumWithSomeGarbledDigit_ExceptionThrown()
    {
      readNumberLine_exceptionTest("-123G456\r\n");
    }

    [TestMethod]
    public void ReadNumberLine_LineWithNegativeZero_ZeroReturned()
    {
      var reader = createReader("-0\r\n");
      var num = reader.ReadNumberLine();

      Assert.AreEqual(0, num);
    }

    [TestMethod]
    public void ReadNumberLine_LineWithSingleDigit_DigitReturned()
    {
      readNumberLine_parsingTest(Enumerable.Range(0, 10));
    }

    [TestMethod]
    public void ReadNumberLine_PositiveNumbers_CorrectNumberReturned()
    {
      readNumberLine_parsingTest(Enumerable
        .Range(1, 1000)
        .Where(n => n % 7 == 0));
    }

    [TestMethod]
    public void ReadNumberLine_NegativeNubmers_CorrectNumberReturned()
    {
      readNumberLine_parsingTest(Enumerable
        .Range(1, 1000)
        .Where(n => n % 6 == 0)
        .Select(n => -n));
    }


    private void readNumberLine_exceptionTest(string data)
    {
      var reader = createReader(data);
      reader.ReadNumberLine(); // expected exception
    }

    private void readNumberLine_parsingTest(IEnumerable<int> nums)
    {
      // put each number on its own line with CRLF
      var numString = string.Join("\r\n", nums) + "\r\n";
      var reader = createReader(numString);

      // parse each line as numbers, they should correspond to
      // the input sequence (in exactly the same ordering)
      foreach (var num in nums)
        Assert.AreEqual(num, reader.ReadNumberLine());
    }
    #endregion

    #region ReadStatusLine
    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadStatusLine_EmptyData_ExceptionThrown()
    {
      readStatusLine_exceptionTest("");
    }

    [TestMethod]
    public void ReadStatusLine_JustCrLf_EmptyStringReturned()
    {
      var reader = createReader("\r\n");
      var str = reader.ReadStatusLine();

      Assert.AreEqual("", str);
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadStatusLine_OneCharWithNoCrLf_ExceptionThrown()
    {
      readStatusLine_exceptionTest("X");
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadStatusLine_OneCharWithJustCr_ExceptionThrown()
    {
      readStatusLine_exceptionTest("X\r");
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadStatusLine_OneCharWithJustLf_ExceptionThrown()
    {
      readStatusLine_exceptionTest("X\n");
    }

    [TestMethod]
    public void ReadStatusLine_OneCharWithCrLf_OneCharReturned()
    {
      readStatusLine_parsingTest("X");
    }

    [TestMethod]
    public void ReadStatusLine_IncreasinglyLargerString_CorrectStringReturned()
    {
      var chars = "abcdefghijklmnopqrstuvwxyz" +
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
        "0123456789";

      readStatusLine_parsingTest(Enumerable
        .Range(0, 100)
        .Select(n => new string(chars[n % chars.Length], n))
        .ToArray());
    }


    private void readStatusLine_exceptionTest(string data)
    {
      var reader = createReader(data);
      reader.ReadStatusLine();
    }

    private void readStatusLine_parsingTest(params string[] testStr)
    {
      var data = string.Join("\r\n", testStr) + "\r\n";
      var reader = createReader(data);

      foreach (var line in testStr) {
        Assert.AreEqual(line, reader.ReadStatusLine());
      }
    }
    #endregion

    #region ReadBulk
    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void ReadBulk_LengthIsNegative_ExceptionThrown()
    {
      readBulk_exceptionTest("", -1);
    }

    [TestMethod]
    public void ReadBulk_ZeroLengthData_EmptyBufferReturned()
    {
      var reader = createReader("\r\n");
      var buffer = reader.ReadBulk(0);

      Assert.AreEqual(0, buffer.Length);
    }

    [TestMethod]
    public void ReadBulk_LengthIsPositive_BufferWithSameLengthReturned()
    {
      var reader = createReader("0123456789\r\n");
      var buffer = reader.ReadBulk(10);

      Assert.AreEqual(10, buffer.Length);
    }

    [TestMethod]
    public void ReadBulk_LengthIsPositiveAndDataIsEnough_CorrectDataReturned()
    {
      var testStr = "0123456789abcdefghijklmnopqrstuvwxyz";
      var reader = createReader(testStr + "\r\n");
      var buffer = reader.ReadBulk(testStr.Length);

      Assert.AreEqual(testStr.Length, buffer.Length);
      Assert.AreEqual(testStr, Encoding.Default.GetString(buffer));
    }

    [TestMethod]
    public void ReadBulk_NonStringData_CorrectDataReturned()
    {
      var data = new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
      var crlf = new byte[] { 0x0D, 0x0A };

      var reader = createReader(data.Concat(crlf).ToArray());
      var buffer = reader.ReadBulk(data.Length);

      Assert.AreEqual(data.Length, buffer.Length);
      CollectionAssert.AreEquivalent(data, buffer);
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadBulk_LengthIsPositiveButNotEnoughData_ExceptionThrown()
    {
      readBulk_exceptionTest("012345/r/n", 99);
    }

    [TestMethod]
    public void ReadBulk_ReallyLargeData_CorrectDataReturned()
    {
      var data = new byte[4096];
      var crlf = new byte[] { 0x0D, 0x0A };
      (new Random()).NextBytes(data);

      var reader = createReader(data.Concat(crlf).ToArray());
      var buffer = reader.ReadBulk(data.Length);

      Assert.AreEqual(data.Length, buffer.Length);
      CollectionAssert.AreEquivalent(data, buffer);
    }

    [TestMethod]
    public void ReadBulk_DataWithLotsOfCrLf_CorrectDataReturned()
    {
      var data = new byte[] {
        0x0D, 0x0A, 0xFF, 0x0D, 0x0A, 0xFF,
        0x0D, 0x0A, 0x0D, 0x0A, 0x0D, 0x0A,
      };
      var crlf = new byte[] { 0x0D, 0x0A };

      var buffer = new byte[data.Length];
      var reader = createReader(data.Concat(crlf).ToArray());
      reader.ReadBulk(buffer, 0, data.Length);

      CollectionAssert.AreEquivalent(data, buffer);
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadBulk_DataWithoutCrlf_ExceptionThrown()
    {
      readBulk_exceptionTest("asdf", 4);
    }


    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void ReadBulk_WithBuffer_NegativeOffset_ExceptionThrown()
    {
      readBulk_withBuffer_exceptionTest("asdf", -1, 4);
    }

    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void ReadBulk_WithBuffer_TooLargeOffset_ExceptionThrown()
    {
      readBulk_withBuffer_exceptionTest("asdf", 99, 4);
    }

    [TestMethod, ExpectedException(typeof(ArgumentException)), Conditional("DEBUG")]
    public void ReadBulk_WithBuffer_TooLargeBulkLength_ExceptionThrown()
    {
      readBulk_withBuffer_exceptionTest("0123456\r\n", 0, 999);
    }

    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void ReadBulk_WithBuffer_LengthIsNegative_ExceptionThrown()
    {
      readBulk_withBuffer_exceptionTest("asdf", 0, -1);
    }

    [TestMethod, ExpectedException(typeof(ArgumentNullException)), Conditional("DEBUG")]
    public void ReadBulk_WithBuffer_BufferIsNull_ExceptionThrown()
    {
      byte[] buffer = null;
      var reader = createReader("asdf");
      reader.ReadBulk(buffer, 0, 4);
    }

    [TestMethod]
    public void ReadBulk_WithBuffer_OffsetAndLengthIsZero_EmptyBufferReturned()
    {
      var buffer = new byte[0];
      var reader = createReader("\r\n");

      reader.ReadBulk(buffer, 0, 0);
    }

    [TestMethod]
    public void ReadBulk_WithBuffer_LengthIsPositiveAndDataIsEnough_CorrectDataReturned()
    {
      var testStr = "012345";
      var buffer = new byte[6];
      var reader = createReader(testStr + "\r\n");

      reader.ReadBulk(buffer, 0, 6);

      Assert.AreEqual(testStr,
        Encoding.Default.GetString(buffer));
    }

    [TestMethod]
    public void ReadBulk_WithBuffer_ReallyLargeData_CorrectDataReturned()
    {
      var data = new byte[4096];
      var crlf = new byte[] { 0x0D, 0x0A };
      (new Random()).NextBytes(data);

      var reader = createReader(data.Concat(crlf).ToArray());
      var buffer = reader.ReadBulk(data.Length);

      Assert.AreEqual(data.Length, buffer.Length);
      CollectionAssert.AreEquivalent(data, buffer);
    }


    private void readBulk_exceptionTest(string data, int length)
    {
      var reader = createReader(data);
      reader.ReadBulk(length);
    }

    private void readBulk_withBuffer_exceptionTest(string data, int offset, int length)
    {
      var buffer = new byte[data.Length];
      var reader = createReader(data);
      reader.ReadBulk(buffer, offset, length);
    }
    #endregion

    #region ReadBulkTo
    [TestMethod, ExpectedException(typeof(ArgumentNullException)), Conditional("DEBUG")]
    public void ReadBulkTo_StreamIsNull_ExceptionThrown()
    {
      readBulk_withStream_exceptionTest("0123\r\n", null, 4);
    }

    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void ReadBulkTo_LengthIsNegative_ExceptionThrown()
    {
      readBulk_withStream_exceptionTest("0123\r\n", new MemoryStream(), -1);
    }

    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException)), Conditional("DEBUG")]
    public void ReadBulkTo_NegativeBufferSize_ExceptionThrown()
    {
      readBulk_withStream_exceptionTest("012345\r\n",
        new MemoryStream(), 6, -1);
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadBulkTo_DataWithNoCrLf_ExceptionThrown()
    {
      readBulk_withStream_exceptionTest("012345",
        new MemoryStream(), 6);
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadBulkTo_DataWithJustCr_ExceptionThrown()
    {
      readBulk_withStream_exceptionTest("012345\r",
        new MemoryStream(), 6);
    }

    [TestMethod, ExpectedException(typeof(ResponseException)), Conditional("DEBUG")]
    public void ReadBulkTo_DataWithJustLf_ExceptionThrown()
    {
      readBulk_withStream_exceptionTest("012345\n",
        new MemoryStream(), 6);
    }

    [TestMethod]
    public void ReadBulkTo_LengthIsZero_StreamContainsJustCrLf()
    {
      var reader = createReader("\r\n");
      var ms = new MemoryStream();

      reader.ReadBulkTo(ms, 0);

      Assert.AreEqual(0, ms.Length);
      ms.Dispose();
    }

    [TestMethod]
    public void ReadBulkTo_LengthIsPositive_StreamContainsDataWithSameLength()
    {
      var reader = createReader("012345\r\n");
      var ms = new MemoryStream();

      reader.ReadBulkTo(ms, 6);

      Assert.AreEqual(6, ms.Length);
      ms.Dispose();
    }

    [TestMethod]
    public void ReadBulkTo_ValidArgs_StreamContainsCorrectData()
    {
      var testStr = "012345";
      var reader = createReader(testStr + "\r\n");

      var ms = new MemoryStream();
      reader.ReadBulkTo(ms, testStr.Length);

      Assert.AreEqual(testStr.Length, ms.Length);

      ms.Seek(0, SeekOrigin.Begin);
      var sr = new StreamReader(ms);
      var resultStr = sr.ReadToEnd();

      Assert.AreEqual(testStr, resultStr);
    }


    private void readBulk_withStream_exceptionTest(string data, Stream s, int length,
      int? bufferSize = null)
    {
      var reader = createReader(data);
      if (bufferSize.HasValue)
        reader.ReadBulkTo(s, length, bufferSize.Value);
      else
        reader.ReadBulkTo(s, length);
    }
    #endregion
  }
}
