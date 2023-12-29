namespace Todo

open System
open Spectre.Console

[<AutoOpen>]
type Label =
    { Name: string 
      Color: Spectre.Console.Color }
    
    static member Create (name: string) (color: Color) : Result<Label, Exception> =
        Ok { Name = name; Color = color }