
using System;
using System.Windows.Forms;

namespace Sider.Samples.Chat
{
  public class Program
  {
    [STAThread]
    internal static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      (new Program()).Run();
    }

    public void Run()
    {
      Application.Run(new MainForm());
    }
  }
}
