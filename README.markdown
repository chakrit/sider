# SIDER : REDIS bindings for C#

Inspired by migueldeicaza's first stab at the problem (I used some of his algorithm)
and ServiceStack.Redis (to take it a lot further).

This is a REDIS bindings for C# 4.0 that try to stick to the metal as much as possible which
results in:

* Simple API that maps directly to the Redis commands reference. (no name guessing)
* As fast as practical and can be implemented without too much cryptic code.
* Easy to setup, no gigantic class hierarchies.
* Supports streaming mode to allow Redis to be used to store really really large blobs
  (e.g. user-uploading files) without consuming up too much memory.

At the moment, foundation work is there and works but not all commands are implemented,
yet. The commands I needed are going to be implemented first. But implementing new ones
is realtively easy.

If you'd like a command implemented, look at the `RedisClient.API.cs` file, it should
be pretty easy to add one, or ping me on twitter (@chakrit) I'll happily do it for you :)

For example, here's the code for the `SMembers` command:

    public string[] SMembers(string key)
    {
      writeCmd("SMEMBERS", key);
      return readMultiBulk();
    }

You can figure that out, right?

And here's how to use the lib:

    var client = new RedisClient(); // default host:port
    var client = new RedisClient("localhost", 6379);

    client.Set("HELLO", "World");
    var result = client.Get("HELLO");

    // result == "World";

    client.Dispose() // disconnect

For ASP.NET/Web and/or multi-threaded scenarios, you can use the
`ThreadwisePool` like this:

    var pool = new ThreadwisePool(); // manages clients activations/disposal

    var client = pool.GetClient();
    var result = client.Get("HELLO") == "WORLD";

Internally, a `ThreadLocal<T>` is used.

Both the client and the clients pool can be plugged into an IoC by using the respective
`IRedisClient` and `IClientsPool` interface respectively.

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