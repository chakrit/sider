
using System;
using System.Windows.Forms;

namespace Sider.Samples.Chat
{
  public partial class MainForm : RedisFormBase
  {
    public MainForm()
    {
      InitializeComponent();
    }

    private void JoinButton_Click(object sender, EventArgs e)
    {
      var room = RoomsCombo.Text;
      var name = NameBox.Text;

      if (string.IsNullOrWhiteSpace(room)) {
        MessageBox.Show("Specify room name!");
        return;
      }

      if (string.IsNullOrWhiteSpace(name)) {
        MessageBox.Show("Specify name!");
        return;
      }

      new ChatForm(name, room).Show();
    }
  }
}
