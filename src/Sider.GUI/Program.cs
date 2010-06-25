
using System;
using System.Windows.Forms;

namespace Sider.GUI
{
  public class Program
  {
    [STAThread]
    internal static void Main() { new Program().Run(); }


    public void Run()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new MainForm());
    }
  }
}
