SIDER : REDIS bindings for C#
====

Inspired by migueldeicaza's first stab at the problem (I used some of his
algorithm) and ServiceStack.Redis (to take it a lot further).

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

If Redis 2.2 is running on the default host/port and everything is working
properly, you should see the string `"WORLD!!!!"` printed to your console.

# ABOUT

This is a REDIS 2.2 bindings for C# 4.0 that try to **stick to the metal**
as much as possible which results in:

* Simple API that maps closely to the Redis commands reference.
* Easy to use, no gigantic class hierarchies to setup. No confusing naming
  convention that obfuscates the real command being sent to Redis.
* As fast as my limited networking skills will allow.
* Supports reading from and writing data to user-supplied streams for GET/SET
  and other similar commands to allow Redis to be used to store really
  really large blobs (e.g. user-uploaded files) without huge buffers.
* Delegate-based pipelining support.

As of February 1st, 2011, all basic commands have been implemented except for the
following:

* Extra options for some commands - e.g. `WITHSCORES`, `AGGREGATE` and the likes.
* Better transaction support - Right now you can use `MULTI`, `EXEC` and 
  `DISCARD` inside the `.Pipeline` method which should be enough for most
  cases.

Other than that, it's solid and somewhat fault-tolerant. I'm using this myself
in production code as well, so expect fast fixes should there be any problems.

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

For ASP.NET/Web and/or multi-threaded concurrent access scenarios, you can use
the `ThreadwisePool` like this:

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
    
Example when doing more than 7 calls:
    
    // unlimited number of calls (don't abuse it though)
    var result = client.Pipeline(c => {
      c.Get("KEY1");
      c.Get("KEY2");
      c.Get("KEY3");
      /* -snip- */
      c.Get("KEY8");
      c.Get("KEY9");
      c.Get("KEY10");
    });
    
    var resultArr = result.ToArray();
    Trace.Assert(resultArr[0] == /* value from KEY1 */);
    Trace.Assert(resultArr[1] == /* value from KEY2 */);
    Trace.Assert(resultArr[2] == /* value from KEY3 */);
    // ...

Note that pipeline results are not lazy as is the case with many `IEnumerable`
implementation -- All commands will be executed immediately as soon as you
finish the `.Pipeline` call.

# BINARY DATA / STREAMING

Right now raw binary data / streaming is supported for `Get\Set` and
`HGet\HSet` pairs via `GetTo\SetFrom\GetRaw\SetRaw` and
`HGetTo\HSetFrom\HGetRaw\HSetRaw` commands as I assumed that you will want
to work with raw data only when you really have large values. **Although I *am*
working on a way to provide more types other
than `string`** which you may find in other branches of this repository. The next
version, possibly at 0.5 will support a really wide range of values.

Four commands support streaming mode for working with large binary data such as
image caches with minimal bufferring: `GetTo`, `SetFrom`, `HGetTo` and
`HSetFrom`.

Instead of a normal string, these four commands accepts `System.IO.Stream` so
you can send and receive data to and from `FileStream`, `NetworkStream` or
ASP.NET `OutputStream` with ease:

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

These commands should be used with care, however. As they may allows external
exceptions into the core of Sider which may produce unexpected results.

Additionally, another four commands are there for working with raw `byte[]`
buffers: `GetRaw`, `SetRaw`, `HGetRaw` and `HSetRaw` like so:

    // create random buffer
    var temp = new byte[4096];
    (new Random()).NextBytes(temp);

    // work with
    client.SetRaw("random", temp);

    var result = client.GetRaw("random");
    for (var i = 0; i < result.Length; i++)
      Trace.Assert(result[i] == temp[i]);

...
 
# CONFIGURATION

You can fine-tune buffer sizes to your liking by passing a
`RedisSettings` instance like so:

    var settings = new RedisSettings(
    host: "192.168.192.111",
    port: 6379,
    autoReconnectOnIdle: false,   // if no idle client disconnection
    readBufferSize: 256,          // optimize for small reads
    writeBufferSize: 65536);      // optimize for heavy writes

    var pool = new ThreadwisePool(settings);
    var client = pool.GetClient();

    // or, pass directly to client
    client = new RedisClient(settings);

...

# SUPPORT / CONTRIBUTE

Just shoot me an email at `service @ chakrit . net` (without the spaces) or if
you use twitter, feel free to mention [@chakrit](http://twitter.com/chakrit) for
help.

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
