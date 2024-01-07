[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.List

open System
open Spectre.Console
open Argu
open FsToolkit.ErrorHandling

open Todo
open Todo.ItemGroup
open Todo.ItemGroup.Representations.Tree
open Todo.Cli.Utilities
open Todo.Cli.Commands.Arguments

let private printItemGroups (itemGroups: ItemGroup list) =
    AnsiConsole.Clear()
        
    itemGroups
    |> List.map (fun itemGroup -> itemGroup.ToString()) 
    |> List.map (fun str -> AnsiConsole.MarkupLine($"%s{str}")) 
    |> ignore
    
// ** remarks **
// Interactive display <b>CAN</b> change app data. Normally, commands that change data just return updated app data, and
// the saving is delegated elsewhere. The <b>command</b> itself is configured such that it does not change any data,
// however this specific function does. To work around this, we get the updated app data, and then explicitly save the
// new app data here. 
let private interactiveSession (appData: AppData) : Result<AppData, Exception> =
    AnsiConsole.Clear()
    
    let rootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    let converter = Converter.PrettyTree.toLines 
    Interactive.treeInteractive rootItemGroup converter |> ignore 
    
    Ok appData

let private parseArgv (argv: string array) : Result<ParseResults<ViewArguments>, Exception> =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<ViewArguments>(errorHandler = errorHandler)
    
    try
        let parsedArgs = parser.Parse argv
        Ok parsedArgs
    with
        | :? ArguParseException as ex -> Error ex
        | ex -> Error ex
        
let private display (appData: AppData) (parseResults: ParseResults<ViewArguments>) : Result<unit, Exception> =
    match parseResults.TryGetResult ViewArguments.Interactive with
    | Some _ ->
        Ok appData
        |> Result.bind interactiveSession      // result of interactive actions
        |> Result.bind (Files.saveAppData Files.filePath) 
    | None ->
        printItemGroups appData.ItemGroups 
        Ok ()
        
/// <summary>
/// Implements the logic for the 'list' command.
/// </summary>
/// <param name="argv">List of arguments to be parsed.</param>
let execute (argv: string array) : unit =
    let displayResult = 
        result {
            let appData = match Files.loadAppData Files.filePath with | Ok appData -> appData | Error err -> raise err
            let! parsedArgs = parseArgv argv
            return! display appData parsedArgs
        }
    
    match displayResult with
    | Ok _ ->
        () 
    | Error _ ->
        // maybe print some error message
        ()
        
let config : Command.Config =
    { Command = "view"
      Help = "Displays the current todos." 
      Function = CommandFunction.NoDataChange execute }