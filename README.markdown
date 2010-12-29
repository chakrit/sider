SIDER : REDIS bindings for C#
====

Inspired by migueldeicaza's first stab at the problem (I used some of his
algorithm) and ServiceStack.Redis (to take it a lot further).

This is a REDIS bindings for C# 4.0 that try to **stick to the metal** as much as
possible which results in:

* Simple API that maps directly to the Redis commands reference.
* Easy to use, no gigantic class hierarchies to setup. No confusing method names.
* As fast as my limited networking skills will allow.
* Supports reading from and writing data to user-supplied streams for GET/SET
  and a few other similar commands to allow Redis to be used to store really
  really large blobs (e.g. user-uploaded files) efficiently memory-wise.
* Upcoming no-frill pipelining support.

As of 24th July 2010, all basic commands have been implemented except for the
following:

* Blocking commands `BLPOP`, `BLPUSH` and the likes. - Needs a few more tests
  with varying Socket configurations.
* Extra options for some commands - e.g. `WITHSCORES` and `AGGREGATE`
* Transaction and pipelining support. - Sider has been designed from the ground
  up to make it easy to pipeline and do transactions, the foundation work is
  already there. I just need a bit more time to finalize and streamline the API.

Other than that, it's solid and somewhat fault-tolerant. I'm using this myself
in production code as well, so expect fast fixes should there be any problems.

# HOWTO

Here's how to use the lib:

    var client = new RedisClient(); // default host:port
    client = new RedisClient("localhost", 6379); // custom

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

Internally, a .NET 40 `ThreadLocal<T>` is used.

Both the client and the clients pool can be plugged into an IoC by using the respective
`IRedisClient` and `IClientsPool` interface respectively.

You can also fine-tune buffer sizes to your liking by passing a
`RedisSettings` instance like so:

    var settings = new RedisSettings(
    host: "192.168.192.111",
    port: 6379,
    autoReconnectOnIdle: false,   // if no idle client disconnection
    readBufferSize: 256,          // optimize for small reads
    writeBufferSize: 65536);       // optimize for heavy writes

    var pool = new ThreadwisePool(settings);
    var client = pool.GetClient();

    // or, pass directly to client
    client = new RedisClient(settings);

...

# Pipeline

Experimental pipelining support is in, simply call the Pipeline method to
perform a pipelined call, with each result of the call returned inside a
Tuple<> with same size as the number of calls or an IEnumerable<object> for a
long pipelined sessions

Example:

    var result = client.Pipeline(c =>
    {
      c.Get("KEY1");
      c.MGet("KEY2", "KEY3", "KEY4", "KEY5", "KEY6", "KEY7", "KEY8", "KEY9");
      c.Keys("MY_VERY_VERY_VERY_VERY_LONG_*_KEY_PTTRNS");
    });
    
    string getResult = result.Item1;
    string[] mGetResults = result.Item2;
    string[] keysResults = result.Item3;

MULTI EXEC is coming right up... hopefully before January ends :)
     
...

# License

Copyright (c) 2010, Chakrit Wichian.
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

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.