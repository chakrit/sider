
using System;
using System.Windows.Forms;

namespace Sider.GUI
{
  public partial class MainForm : Form
  {
    private ClientsController _controller;


    public MainForm()
    {
      InitializeComponent();

      _controller = new ClientsController {
        Host = hostBox.Text,
        Port = (int)portBox.Value
      };
    }


    private void testButton_Click(object sender, EventArgs e)
    {
      withClient(c => { return "OK"; }, "OK", "Connection failed.");
    }

    private void hostBox_TextChanged(object sender, EventArgs e)
    {
      _controller.Host = hostBox.Text;
    }

    private void portBox_ValueChanged(object sender, EventArgs e)
    {
      _controller.Port = (int)portBox.Value;
    }

    private void pingButton_Click(object sender, EventArgs e)
    {
      withClient(c => c.Ping(), true, "PING failed.");
    }

    private void bgSaveButton_Click(object sender, EventArgs e)
    {
      withClient(c => c.BgSave(), true, "BGSAVE failed.");
    }

    private void saveButton_Click(object sender, EventArgs e)
    {
      withClient(c => c.Save(), true, "SAVE failed.");
    }

    private void bgRewriteAOFButton_Click(object sender, EventArgs e)
    {
      withClient(c => c.BgRewriteAOF(), true, "BGREWRITEAOF failed.");
    }

    private void shutdownButton_Click(object sender, EventArgs e)
    {
      withClient(c => c.Shutdown(), "SHUTDOWN failed.");
    }

    private void quitButton_Click(object sender, EventArgs e)
    {
      withClient(c => c.Quit(), "QUIT failed.");
    }


    private void withClient(Action<IRedisClient> redisAct, string errorMessage)
    {
      withClient(c => { redisAct(c); return true; }, true, errorMessage);
    }

    private bool withClient<T>(Func<IRedisClient, T> redisFunc,
      T expectedResult, string errorMessage)
    {
      T result;

      try {
        using (var client = _controller.GetClient())
          result = redisFunc(client);

        if (result.Equals(expectedResult)) {
          MessageBox.Show("OK\n\n" + result.ToString(), "Redis",
            MessageBoxButtons.OK, MessageBoxIcon.Information);

          return true;
        }
        else {
          MessageBox.Show(errorMessage + "\n\n" + result.ToString(), "Redis",
            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

          return false;
        }
      }
      catch (Exception ex) {
        var msgBoxResult = MessageBox.Show("Exception:\n\n" + ex.ToString(), "Redis",
          MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

        return msgBoxResult == System.Windows.Forms.DialogResult.Retry ?
          withClient(redisFunc, expectedResult, errorMessage) :
          false;
      }
    }
  }
}
