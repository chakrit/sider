
namespace Sider
{
  public partial class RedisClient<T>
  {
    private bool _inTransaction;


    private void beginMultiExec()
    {
      _inTransaction = true;
      _isPipelining = true;
      initReadsQueue();
    }

    private void endMultiExec()
    {
      _inTransaction = false;
      _isPipelining = false;
      _writer.Flush();

      // reads out the pending "+QUEUED"
      // TODO: Maybe better to make "+QUEUED" reads immediate
      //   since errors can be thrown instead of "+QUEUED"
      readQueueds(_readsQueue.Count);
    }
  }
}
