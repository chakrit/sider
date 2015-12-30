using System;

namespace Sider.Tests {
  public class Spy : Spy<object> {
  }

  public class Spy<T> where T: class {
    public bool Called { get; private set; }
    public T Returns { get; set; }

    public object[] CalledArgs { get; private set; }

    public Spy() {
      Called = false;
      Returns = null;
      CalledArgs = new object[] { };
    }

    public Action Action() {
      return () => Called = true;
    }

    public Action<TArg> Action<TArg>() {
      return arg => {
        Called = true;
        CalledArgs = new object[] { arg };
      };
    }

    public Func<T> Func() {
      return () => {
        Called = true;
        return Returns; 
      };
    }

    public Func<TArg, T> Func<TArg>() {
      return arg => {
        Called = true;
        CalledArgs = new object[] { arg };
        return Returns;
      };
    }
  }
}

