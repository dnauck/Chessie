﻿namespace System
open System.Reflection
open System.Runtime.CompilerServices

[<assembly: AssemblyTitleAttribute("Chessie")>]
[<assembly: AssemblyProductAttribute("Chessie")>]
[<assembly: AssemblyDescriptionAttribute("Railway-oriented programming for .NET")>]
[<assembly: AssemblyVersionAttribute("0.6.0")>]
[<assembly: AssemblyFileVersionAttribute("0.6.0")>]
[<assembly: ExtensionAttribute()>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.6.0"
    let [<Literal>] InformationalVersion = "0.6.0"
