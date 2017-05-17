#r "bin/Debug/Forest.Core.dll"
open Forest
open System

let x = "dsds";;
System.Console.WriteLine x;;

let t = typedefof<IForestContext>;;
System.Console.WriteLine t.FullName;;