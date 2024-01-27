[<RequireQualifiedAccess>]
module Todo.Cli.Commands.EditItem

open System
open Argu
open Todo
open Todo.Cli.Commands.Arguments

let editItem (appData: AppData) (itemGroupParseResults: ParseResults<ItemArguments>) : Result<AppData, Exception> =
    let propertyToEdit = itemGroupParseResults.GetAllResults() |> List.rev |> List.head
    let path = itemGroupParseResults.TryGetResult ItemArguments.Path
    
    if path.IsSome then
        let (itemName, adjustedPath) = Todo.Utilities.List.splitLast (path.Value) 
        
        match propertyToEdit with
        | ItemArguments.Label innerParseResults ->
            failwith "todo"
        | ItemArguments.Name newName ->
            failwith "todo"
        | ItemArguments.Description newDescription ->
            failwith "todo"
        | _ ->
            Error (Exception(sprintf $"oopsie: %A{itemGroupParseResults.GetAllResults()}")) 
    else
        Error (Exception("You must specify a path!"))
