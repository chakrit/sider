SIDER : REDIS bindings for C#
====

[![Join the chat at https://gitter.im/chakrit/sider](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/chakrit/sider?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

For the latest changes, please see the `CHANGES.markdown` file.

Inspired by migueldeicaza's first stab at the problem (I used some of his
algorithm) and ServiceStack.Redis (to take it a lot further).

**If you have questions/feedbacks, please feel free to shoot it to
[sider-lemonade@googlegroups.com](mailto:sider-lemonade@googlegroups.com),
open a [GitHub issue](https://github.com/chakrit/sider/issues/new) or
ping me on Twitter [@chakrit](http://twitter.com/chakrit)**

**NOTE:** If you are going to run benchmarks against Sider codebase
make sure you have these [assertions](https://github.com/chakrit/sider/blob/master/src/Sider/SAssert.cs)
turned off by running your test projects in RELEASE mode. Otherwise
you might get strange benchmarking results.

# INSTALLATION

The quickest route to getting started with Redis using Sider is via
[NuGet](http://nuget.org). Create a new console application and then open the
Package Manager Console (View -> Other Windows -> Package Manager Console) and
type in:

    install-package sider

Then in your `Program.cs` file, test it out with:

    var client = new RedisClient();
    client.Set("HELLOOO", "WORLD!!!!");
    Console.WriteLine(client.Get("HELLOOO"));

    Console.ReadKey();

If Redis 2.4 is running on the default host/port and everything is working
properly, you should see the string `"WORLD!!!!"` printed to your console.

# ABOUT

This is a REDIS 2.2 bindings for C# 4.0 that try to **stick to the metal**
as much as possible which results in:

* Simple API that maps closely to the Redis commands reference.
* Easy to use, no gigantic class hierarchies to setup. No confusing naming
  convention that obfuscates the real command being sent to Redis.
* Redis publish/subscribe via `IObservable`.
* As fast as my limited networking skills and Redis itself will allow.
  (Which is already lightning-fast thanks to Redis!)
* Supports reading from and writing data to user-supplied streams for GET/SET
  and other similar commands to allow Redis to be used to store really
  really large blobs (e.g. user-uploaded files) without huge buffers.
* Delegate-based pipelining support.

As of **April 26th, 2011**, all commands as
per Redis 2.2 are now implemented with all options. Enjoy! :)

# HOWTO

Here's how to use the lib:

    // connects to redis
    var client = new RedisClient(); // default host:port
    client = new RedisClient("localhost", 6379); // custom host/port

    // redis commands are methods of the RedisClient class
    client.Set("HELLO", "World");
    var result = client.Get("HELLO");
    // result == "World";

    client.Dispose() // disconnect

For ASP.NET/Web and/or multi-threaded concurrent access scenarios, it is
recommended that you use an IoC container to help you with client activations
or you can use the `ThreadwisePool` like this:

    // manages clients activations/disposal
    var pool = new ThreadwisePool();

    var client = pool.GetClient();
    var result = client.Get("HELLO") == "WORLD";

Internally, a .NET 40 `ThreadLocal<T>` is used. Both the client and the clients pool can be plugged into an IoC by using the respective
`IRedisClient` and `IClientsPool` interface respectively.

# PIPELINE

To perform multiple pipelined calls, wrap your commands in a `.Pipeline` call:

    // issue ~ 2k commands in one go
    var result = client.Pipeline(c =>
    {
      for (var i = 0; i < 1000; i++)
        c.Set("HELLO" + i.ToString(), "WORLD" + i.ToString());

      for (var i = 999; i >= 0; i--)
        c.Get("HELLO" + i.ToString()
    });

    // parse results
    var resultArr = result.ToArray();
    for (var i = 0; i < 1000; i++)          // SET results
      Debug.Assert((bool)resultArr[i]); 

    for (var i = 999; i >= 0; i--) {        // GET results
      Debug.Assert(resultArr[i] is string);
      Debug.Assert("WORLD" + i.ToString() == (string)resultArr[i]);
    }

Results are returned as an `IEnumerable<object>` with as many elements as
the number of calls you've made with each object having the same type as the
corresponding pipelined call.

Since its an `IEnumerable<object>`, it also works with LINQ. See the
`LinqPipelineSample.cs` file in the `src/Sider.Samples` folder for a 
sample implementation.

**Strongly-typed extension**

If you only need a fixed number of calls which you can determine at compile-time
then you can use the extension method version of `.Pipeline` to help you with
type-casting.

Example, for a fixed number of calls:

    // fixed number of calls < 8
    // each delegate must call exactly 1 redis command (i.e. IRedisClient method)
    var result = client.Pipeline(
      c => c.Get("KEY1"),
      c => c.MGet("KEY2", "KEY3", "KEY4", "KEY5", "KEY6", "KEY7", "KEY8", "KEY9"),
      c => c.Keys("MY_VERY_VERY_VERY_VERY_LONG_*_KEY_PTTRNS"));
    
    string getResult = result.Item1;
    string[] mGetResults = result.Item2;
    string[] keysResults = result.Item3;

The returned value of these extension methods is a strongly-typed `Tuple<>`.

**NOTE**

Note that pipeline results are not lazy as is usually the case with
`IEnumerable` implementations -- All commands will be executed immediately as
soon as you finish the `.Pipeline` call.

# CUSTOM TYPE / SERIALIZERS

To use `RedisClient` with your own custom type to get automatic serialization,
just add a type parameter like so:

    var client = new RedisClient<MyClass>();
    client.Set("instance", new MyClass());

    var mc = client.Get("instance");
    // mc typed as MyClass

To provide your own serialization mechanism, create a class that implements
`Sider.Serialization.ISerializer<YourClassTypeHere>` and supply it to
Sider like so:

    public class MyClass { }
    public class MySerializer : ISerializer<MyClass> { /* -snip- */ }

    var settings = RedisSettings.Build()
      .OverrideSerializer(new MySerializer());

    var client = new RedisClient<MyClass>(settings);
    
    // instance serialized and deserialized using your custom serializer
    client.Set("instance", new MyClass());
    var value = client.Get("instance")

See the `ComplexSetupSample.cs` file in the `src/Sider.Samples` folder for
a sample implementation.

# BINARY DATA / STREAMING

Right now raw binary data / streaming is supported for commands with a single
value input/output such as `Get\HGet\GetRange` etc. The command which provides
raw/streamed mode will have a `Raw` and `To/From` prefix/suffix such as
`GetRaw\SetRaw` for raw mode and `GetTo\SetFrom` for streamed mode.

I assumed that you will want to work with raw/streamed data only when you really
have large values transferring to and from a simple key (as opposed to a member
of a set or a sorted set). To work exclusively with raw data, I recommend using
`RedisClient<byte[]>` instead which uses an direct buffer read/write
serializer internally.

**Streaming**

Instead of a normal string or the specified type, streamed mode commands
accepts `System.IO.Stream` instead so you can send and receive data to and
from streams such as `FileStream`, `NetworkStream` or ASP.NET `OutputStream`
directly to and from Redis with minimal bufferring:

    var client = new RedisClient();

    // load file content straight into redis
    var filename = @"C:\temp\really_really_large_file.txt";
    using (var fs = File.OpenRead(filename)) {
      client.SetFrom("really_large_file", fs, (int)file.Length);
      fs.Close();
    }

    // writes out the content of a key to a file
    using (var fs = File.OpenWrite(filename)) {
      var bytesWrote = client.GetTo("really_large_file", fs);
      Console.WriteLine("Written {0} bytes of key `{1}`'s content.",
        bytesWrote, filename);
    }

See the `StreamingSample.cs` file in the `src/Sider.Samples` folder for
a sample implementation.

**Raw**

Additionally, there are also raw data mode commands which accepts `byte[]`
buffers directly:

    // create random buffer
    var temp = new byte[4096];
    (new Random()).NextBytes(temp);

    // work with
    client.SetRaw("random", temp);

    var result = client.GetRaw("random");
    for (var i = 0; i < result.Length; i++)
      Trace.Assert(result[i] == temp[i]);

Just note the `-Raw` suffix.

# TRANSACTIONS

Transactions handling are automatic from the user persepective.
Just issue a `MULTI` like you would do via `redis-cli` and the client
will enter transaction mode automatically. While in transaction mode,
please note the following points:

After `MULTI` all non-trasaction commands will result in a `+QUEUED`
response instead of the command's normal response. So return values
from commands after `.Multi()` is meaningless.

    var client = new RedisClient();

    // Enter transaction mode
    client.Multi();

    // x is meaningless since Redis will returns a +QUEUED
    var x = client.Get("X");

    // (continued...)

All command results will be recorded just like when you perform a
`.Pipeline()` call. Recorded results will be read out after you
issue an `EXEC` just like via `redis-cli`

    // (continued...)
    var result = client.Exec().ToArray();

    // result contains 1 element since we've issued
    // only 1 command so far in transaction mode
    var actualXvalue = result[0]; // from .Get("X") above

  That is `.Exec()` will returns an `IEnumerable<object>` with as much
  elements as the number of commands you've issued since `.Multi()`

Likewise, `DISCARD` discards all the commands recorded so far and simply
exits transaction mode.

    // set a sample value
    client.Set("X", "Foobar!");

    // perform an aborted transaction
    client.Multi();
    client.Set("X", "NO");
    client.Discard();

    // check result
    var result = client.Get("X");

    // X == "Foobar!"

Please see the `MultiExecSample.cs` file in the `src\Sider.Samples`
folder for a complete and working example.
 
# CONFIGURATION

You can fine-tune buffer sizes to your liking and provide custom serializers
by passing a `RedisSettings` instance which are built like this:

    var settings = RedisSettings.Build()
      .Host("192.168.192.111")  // custom host
      .Port(9736)               // custom port
      .ReconnectOnIdle(false)   // manage timeouts manually
      .ReadBufferSize(256)      // optimize for small reads
      .WriteBufferSize(65536)   // optimize for large writes

    // pass to pool so all clients use the supplied settings
    var pool = new ThreadwisePool(settings);
    var client = pool.GetClient();

    // or, pass directly to client
    client = new RedisClient(settings);

...

# SUPPORT / CONTRIBUTE

Any improvements to the code is totally welcome! :)

Please post any support request to
[sider-lemonade](http://groups.google.com/group/sider-lemonade) google group.

Or just shoot me an email at `service @ chakrit . net` (without the spaces) or
if you use twitter, feel free to mention [@chakrit](http://twitter.com/chakrit)
for help.

# LICENSE

Copyright (c) 2011, Chakrit Wichian.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

Redistributions of source code must retain the above copyright notice, this list
of conditions and the following disclaimer.

Redistributions in binary form must reproduce the above copyright notice, this
list of conditions and the following disclaimer in the documentation and/or
other materials provided with the distribution.

Neither the name of the Chakrit Wichian nor the names of its contributors may be
used to endorse or promote products derived from this software without specific
prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
