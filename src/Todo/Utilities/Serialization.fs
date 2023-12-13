module Todo.Utilities.Serialization

open System.IO
open System.Text.Json

let jsonSerialize (obj: obj) : string =
    JsonSerializer.Serialize(obj, JsonSerializerOptions(WriteIndented = true))
    
let jsonDeserialize<'T> (rawJson: string) : Option<'T> =
    try
        match JsonSerializer.Deserialize<'T>(rawJson) with
        | value when obj.ReferenceEquals(value, null) -> None
        | value -> Some value
    with
    | _ -> None
    
    
    
let getJsonSerializer (filePath: string) : (obj -> unit) =
    fun object ->
        let rawJson = jsonSerialize object 
        File.WriteAllText(filePath, rawJson)
        ()
        
let getJsonDeserializer<'T> (filePath: string) : (unit -> Option<'T>) =
    fun () ->
        let rawJson = File.ReadAllText(filePath)
        jsonDeserialize<'T> rawJson