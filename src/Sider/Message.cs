
namespace Sider
{
  public static class Message
  {
    public static Message<TX> Create<TX>(MessageType type,
      string srcPattern, string srcChannel, TX body, int? channelsCount = null)
    {
      return new Message<TX>(type, srcPattern, srcChannel, body, channelsCount);
    }
  }

  public class Message<T>
  {
    public MessageType Type { get; private set; }

    public string SourceChannel { get; private set; }
    public string SourcePattern { get; private set; }

    public T Body { get; private set; }
    public int? ChannelsCount { get; private set; }


    public Message(MessageType type, string srcPattern, string srcChannel, T body,
      int? channelsCount = null)
    {
      Type = type;
      SourceChannel = srcChannel;
      SourcePattern = srcPattern;
      Body = body;
      ChannelsCount = channelsCount;
    }

  }
}
