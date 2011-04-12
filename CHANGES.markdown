# CHANGES

0.3 - 0.4
---
**Breaking changes:**

* RedisSettings are now built using `RedisSettings.New()` see
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