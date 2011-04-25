
using System.Windows.Forms;

namespace Sider.Samples.Chat
{
  public class RedisFormBase : Form
  {
    // prevent instatiation while allowing the designer view
    protected RedisFormBase() { }


    protected IRedisClient<string> GetClient()
    {
      return new RedisClient();
    }
  }
}
