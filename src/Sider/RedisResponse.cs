
namespace Sider
{
  public struct RedisResponse
  {
    public static RedisResponse<T> Create<T>(ResponseType type, T result)
    {
      return new RedisResponse<T>(type, result);
    }
  }

  public struct RedisResponse<T>
  {
    public ResponseType Type { get; private set; }
    public T Result { get; private set; }

    public RedisResponse(ResponseType type, T result) :
      this()
    {
      Type = type;
      Result = result;
    }
  }
}
