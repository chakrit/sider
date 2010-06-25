
namespace Sider

open System
open System.IO


type RingBuffer(bufferSize:int) =
  let _buffer = Array.create bufferSize 0uy
  

  member x.Size
    with get() = _buffer.Length

  new() = RingBuffer(4096)


  member x.ReadFrom(s:Stream, offset:int, count:int) =
    let mutable result = new Int32()

    x.readCore offset count
      (fun offset_ count_ cont -> cont (s.Read _buffer offset_ count_))
      (fun n -> result <- n)
      
    result

  //member x.BeginReadFrom(s:Stream, offset, count) =  
  
  member private x.readCore(offset:int, count:int, readCore:int -> int -> (int -> unit) -> unit, callback:int -> unit) =

    if count > _buffer.Length then
      raise (new ArgumentException("Read count cannot be larger than RingBuffer.Size.", "count"))
    if offset < 0 then
      raise (new ArgumentOutOfRangeException("Read offset cannot be less than zero.", "offset"))

    let startOffset = offset % _buffer.Length
    let endOffset = startOffset + count

    if endOffset <= _buffer.Length then
      readCore startOffset count callback
    else
      let partialCount = _buffer.Length - startOffset;

      readCore startOffset partialCount (fun n1 ->
        if n1 < partialCount then // we're done
          callback n1
        else
          readCore 0 (count - partialCount) (fun n2 -> callback (n1 + n2)))
    
