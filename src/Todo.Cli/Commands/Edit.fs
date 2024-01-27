module Todo.Cli.Commands.Edit

open System
open Argu
open FsToolkit.ErrorHandling
open Todo
open Todo.Cli
open Todo.Cli.Commands.Arguments
open Todo.Cli.Utilities

let private edit editArguments appData =
    match editArguments with
    | EditArguments.Item_Group innerParseResults -> EditItemGroup.editItemGroup appData innerParseResults 
    | EditArguments.Item innerParseResults -> EditItem.editItem appData innerParseResults 

let private handleResults (result: Result<'a, Exception>) =
    match result with
    | Ok _ ->
        ()
    | Error err ->
        let exceptionMessage = "An error occurred while trying to edit the object: " + err.Message
        printfn "%s" exceptionMessage

let internal executeEditCommand
    (appConfig: ApplicationConfiguration)
    (appData: AppData)
    (editParseResults: ParseResults<EditArguments>)
    : unit =

    let filePath = appConfig.getDataFilePath()
    let whatToEdit = List.head (editParseResults.GetAllResults())
    
    result {
        let! creationResult = edit whatToEdit appData 
        return! Files.saveAppData filePath creationResult
    }
    |> handleResults 
