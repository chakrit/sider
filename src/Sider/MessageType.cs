
namespace Sider
{
  public enum MessageType
  {
    Unknown,
    Message,
    PMessage,
    Subscribe,
    PSubscribe,
    Unsubscribe,
    PUnsubscribe,
  }

  public static class MessageTypes
  {
    public static MessageType Parse(string value)
    {
    switch (value) {
    case "message": return MessageType.Message;
    case "pmessage": return MessageType.PMessage;
    case "subscribe": return MessageType.Subscribe;
    case "psubscribe": return MessageType.PSubscribe;
    case "unsubscribe": return MessageType.Unsubscribe;
    case "punsubscribe": return MessageType.PUnsubscribe;

    default: return MessageType.Unknown;
      }
    }
  }
}
