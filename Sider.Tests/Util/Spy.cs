using System.Threading.Tasks;
using System;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Sider.Tests {
  public class Spy : Spy<object> {
  }

  public class Spy<T> where T: class {
    public bool Called { get; private set; }
    public T Returns { get; set; }

    public Spy() {
      Called = false;
      Returns = null;
    }

    public Action Action() {
      return () => Called = true;
    }

    public Action<TArg> Action<TArg>() {
      return _ => Called = true;
    }

    public Func<T> Func() {
      return () => {
        Called = true;
        return Returns; 
      };
    }

    public Func<TArg, T> Func<TArg>() {
      return _ => {
        Called = true;
        return Returns;
      };
    }
  }
}

