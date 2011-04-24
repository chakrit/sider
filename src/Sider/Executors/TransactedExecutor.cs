
using System.Collections.Generic;

namespace Sider.Executors
{
  public class TransactedExecutor : PipelinedExecutor
  {
    public TransactedExecutor(RedisSettings settings,
      ProtocolReader reader, ProtocolWriter writer) :
      base(settings, reader, writer) { }

    /*      if (_inTransaction)
        throw new InvalidOperationException(
          "BLPOP cannot be issued while inside a MULTI/EXEC transaction.");
*/

    public override T Execute<T>(Invocation<T> invocation)
    {
      var result = base.Execute(invocation);
      Reader.ReadQueued();

      return result;
    }

    public override IEnumerable<object> Finish()
    {
      return base.Finish();
    }
  }
}
