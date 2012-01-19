# CHANGES

v0.9.0 - vNext
---

* MSetNX should call ReadBool, not ReadOk ([@bokononlives](https://github.com/BokononLives) - [issue #3](https://github.com/chakrit/sider/pull/3)) 

**TODO:**

* Proper C# 4 mono build support (http://stackoverflow.com/questions/8181081/error-framework-netframework-4-0-client-profile-not-installed-for-monodevel)
* Redis version detection mechanism (INFO?).
* Untangle connection management from the client so we could do something like IOCP using a connection that's shared between clients.
  (one pipelined connection for normal commands, another for pub/sub etc.)
* Better clients pool interface.
* More Redis protocol conformance tests (correct arguments/correct return values/correct command spelling).

0.8 - v0.9.0
---

* Now versioning semantically (http://semver.org/) (adding the "v" and a
  revision number)
* Backward-compatible variadic write overloads (returning `int` instead of
  `bool`) for `SADD, HDEL, SREM, ZREM, ZADD, LPUSH` and `RPUSH`.
  Available in Redis 2.4+.
* Some methods were missing from the IRedisClient interface. It's now there.
* Experimental `IocpExecutor` support.

0.7 - 0.8
---

* Fix issue #1 reported by @rjlopes, ZScore now returns `double?` -- `null` 
  is returned when an element does not exists in the sorted set.
* `RedisSettings.New()` is now `RedisSettings.Build()`. The old method
  is still there but `[Obsolete]`-ed
* You can now configure clients via the `ThreadwisePool` constructor using
  lambda notation just like `RedisClient`.
* Introduced `BLPOP\BRPOP` overloads that matches with redis docs.
* Introduced `RedisSettings.Builder.OverrideEncoding` (access with
  `RedisSettings.EncodingOverride` to change string encodings used.

Experimental features:

* `RedisSettings.Builder.ConnectionTimeout` for overriding
  internal socket connection timeout to help deal with socket timeout better.
* `AutoActivatingPool` which activates client and disposes
  it immediately when the thread exits. This helps greatly with redis timeout
  issues in high-load environment.

0.6 - 0.7
---

* Complete API support for Redis 2.2 (if I left out any, please let me know)
* Bug fixes
* You can now configure redis settings via the RedisClient constructor directly
  using lambdas.

0.5 - 0.6
---

* MULTI/EXEC support.
* PUBLISH/SUBSCRIBE support via `IObservable`.
* INFO/SLAVEOF/MONITOR support.
* Complex arguments support for `SORT` ... other commands coming soon.
* Refactored in a multi-mode Executor model for cleaner code and better future
  extensibility/optimization support (IOCP?)
* `.Custom()` for introducing your own custom command and read/write action.
  i.e. a command we've not yet to support.

0.4 - 0.5
---

* Improved exception handling. Now defaults to always retry until connects to
  deal with timeouts from Redis side and never throw exceptions. 
* RedisSettings can now be copied with `.CopyNew()` method.
* ThreadwisePool.BuildClient is now `protected virtual` so you can override it
  to roll your own `IRedisClient` implementation.

0.3 - 0.4
---
**Breaking changes:**

* RedisSettings are now built using `RedisSettings.Build()` see
  `/src/Sider.Samples/ComplexSetupSample.cs:78` for an example.
* `RedisClient` now supports custom serialization method so the interface and
  class now has a type parameter: `IRedisClient<T>` and `RedisClient<T>`,
  most of you should simply need to replace `IRedisClient`
  with `IRedisClient<string>` to get everything to compile like in v0.3.
* `SUBSTR` no longer supported, use `GETRANGE` instead as per redis documentation.

**New stuff**

* Binary data and custom serialization support has been added.
  Right now raw `byte[]` buffers and `string` have a dedicated serializer which
  will be used automatically when you create `RedisClient<byte[]>` and 
  `RedisClient<string>`. For all other types, a generic binary serializer
  which uses `BinaryFormatter` will be used. More efficient and specific
  serializers will be added in the upcoming versions.
* New settings `KeyEncoding` and `ValueEncoding` for specifying encoding in
  case you use a non-ASCII characters as key. (v0.3 assumed that keys would be
  ASCII-encodable)
* New settings `SerializationBufferSize` and `SerializerOverride` for use with
  the new custom serialization support.