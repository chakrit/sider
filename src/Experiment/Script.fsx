﻿// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#load "RingBuffer.fs"


open System
open System.IO

open Sider

let rb = new RingBuffer()