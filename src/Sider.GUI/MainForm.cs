
using System;
using System.Windows.Forms;

namespace Sider.GUI
{
  public partial class MainForm : Form
  {
    private RedisClient _client = new RedisClient();


    public MainForm() { InitializeComponent(); }


    private void pingButton_Click(object sender, EventArgs e)
    {
      _client.BeginPing(endPing, null);
    }

    private void endPing(IAsyncResult result)
    {
      if (pingLabel.InvokeRequired)
        pingLabel.Invoke(new Action<IAsyncResult>(endPing), result);
      else
        pingLabel.Text = _client.EndPing(result).ToString();
    }
  }
}
