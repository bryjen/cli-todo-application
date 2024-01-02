[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Edit

open System
open Argu
open Spectre.Console
open FsToolkit.ErrorHandling

open Todo
open Todo.Cli.Utilities
open Todo.Cli.Commands.Arguments
open Todo.Cli.Commands.Edit_ItemGroup

let private parseArgv (argv: string array) : Result<ParseResults<EditArguments>, Exception> =
    let parser = ArgumentParser.Create<EditArguments>(programName = "edit", errorHandler = ExceptionExiter())
    
    try
        let parsedArgs = parser.Parse argv
        match (List.length (parsedArgs.GetAllResults())) = 0 with
        | true -> raise (Exception(parser.PrintUsage()))
        | false ->  Ok parsedArgs
    with
        | :? ArguParseException as ex -> Error ex
        | ex -> Error ex

let private updateAppData (appData: AppData) (parseResults: ParseResults<EditArguments>) : Result<AppData, Exception> =
    result {
        let toEdit = List.head (parseResults.GetAllResults())
        
        match toEdit with
        | Item_Group innerParseResults ->
            let! newItemGroups = editItemGroup appData innerParseResults
            return { appData with ItemGroups = newItemGroups }
    } 

/// <summary>
/// Implements the logic for the 'create' command.
/// </summary>
/// <param name="argv">List of arguments to be parsed.</param>
let execute (argv: string array) : AppData =
    // load the current appdata
    let appData = match Files.loadAppData Files.filePath with | Ok appData -> appData | Error err -> raise err
        
    // perform the modification
    let updateResult = 
        result {
            let! parseResults = parseArgv argv
            return! updateAppData appData parseResults
        }
        
    // process the result
    match updateResult with
    | Ok newAppData ->
        printfn "Successfully created the specified item group." 
        newAppData
    | Error err ->
        printfn $"Creation failed with message: %s{err.Message}"
        appData

let config : Command.Config =
    { Command = "edit"
      Help = "Edit an existing item/item group."
      Function = CommandFunction.ChangesData execute}