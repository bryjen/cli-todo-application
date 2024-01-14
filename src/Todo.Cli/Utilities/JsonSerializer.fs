[<AutoOpen>]
module Todo.Cli.Utilities.JsonSerializer

open System.Text.Json
open System.Text.Json.Serialization

/// JSON serializer settings that allow for serialization of F# data types.
/// (Because, for example, DUs aren't serializable using .NET's System.Text.Json serializer by default)
let jsonSerializerOptions =
    let options = JsonSerializerOptions(WriteIndented = true)
    JsonFSharpOptions.Default().AddToJsonSerializerOptions(options)
    options    