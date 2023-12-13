/// JsonData.fs
///
/// Contains functions that serialize/deserialize .json files from a source. 

module Todo.Modules.Data.JsonData

open System
open System.IO
open System.Reflection

open Microsoft.FSharp.Core

open Todo.Utilities.Serialization
open Todo.Modules.TodoList

//  File path of the json file containing serialized tοdo group records
let todoGroupsFilePath = Assembly.GetExecutingAssembly().Location + "/../data/todo_groups.json" 

let saveTodoGroups (filePath: string) (todoGroups: TodoGroup list) : Result<unit, string>  =
    try
        let jsonSerializer = getJsonSerializer filePath
        jsonSerializer todoGroups   //  Write to file
        Ok ()
    with
        | :? FileNotFoundException ->
            Error "File not found."
        | :? UnauthorizedAccessException ->
            Error "Access denied."
        | ex ->
            Error $"An unexpected error occurred: %s{ex.Message}"
    
let loadTodoGroups (filePath: string) : Result<TodoGroup list, string> =
    try
        let jsonDeserializer = getJsonDeserializer<TodoGroup list> filePath
        let deserializedData = jsonDeserializer ()
        
        match deserializedData with
        | Some data -> Ok data
        | None -> Error "Could not deserialize data"
    with
        | :? FileNotFoundException ->
            Error "File not found."
        | :? UnauthorizedAccessException ->
            Error "Access denied."
        | ex ->
            Error $"An unexpected error occurred: %s{ex.Message}"
