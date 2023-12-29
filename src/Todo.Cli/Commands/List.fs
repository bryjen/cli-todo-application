[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.List

open System
open Spectre.Console
open Argu
open FsToolkit.ErrorHandling

open Todo
open Todo.Cli.Utilities

/// <summary>
/// Displays the current configuration of the item groups.
/// </summary>
let private print (appData: AppData) =
    AnsiConsole.Clear()
        
    appData.ItemGroups
    |> List.map (fun itemGroup -> itemGroup.ToString()) 
    |> List.map (fun str -> AnsiConsole.MarkupLine($"%s{str}")) 
    |> ignore
    
/// <summary>
/// Displays the current configuration of the item groups. <b>Is Interactive.</b>
/// </summary>
//
// ** remarks **
// Interactive display <b>CAN</b> change app data. Normally, commands that change data just return updated app data, and
// the saving is delegated elsewhere. The <b>command</b> itself is configured such that it does not change any data,
// however this specific function does. To work around this, we get the updated app data, and then explicitly save the
// new app data here. 
let private interactive (appData: AppData) : Result<AppData, Exception> =
    AnsiConsole.Clear()
    AnsiConsole.MarkupLine("[red]Interactive functionality to be implemented soon.[/]")
    
    appData.ItemGroups
    |> List.map (fun itemGroup -> itemGroup.ToString()) 
    |> List.map (fun str -> AnsiConsole.MarkupLine($"%s{str}")) 
    |> ignore
    
    Ok appData
            
/// <summary>
/// Saves the provided app data.
/// </summary>
/// <param name="newAppData">The app data to save.</param>
let private update (newAppData: AppData) : Result<unit, Exception> =
    match Files.saveAppData Files.filePath newAppData with
    | Ok _ -> Ok () 
    | Error err -> Error err 

/// <summary>
/// Parses the array of CLI arguments.
/// </summary>
let internal parse (argv: string array) : Result<ParseResults<ListArguments>, Exception> =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<ListArguments>(errorHandler = errorHandler)
    
    try
        let parsedArgs = parser.Parse argv
        Ok parsedArgs
    with
        | :? ArguParseException as ex -> Error ex
        | ex -> Error ex
        
let internal display (appData: AppData) (parseResults: ParseResults<ListArguments>) : Result<unit, Exception> =
    match parseResults.TryGetResult ListArguments.Interactive with
    | Some _ ->
        Ok appData
        |> Result.bind interactive // result of interactive actions
        |> Result.bind update // result of saving
    | None ->
        print appData
        Ok ()
        
/// <summary>
/// Implements the logic for the 'list' command.
/// </summary>
/// <param name="argv">List of arguments to be parsed.</param>
let execute (argv: string array) : unit =
    let displayResult = 
        result {
            let appData = match Files.loadAppData Files.filePath with | Ok appData -> appData | Error err -> raise err
            let! parsedArgs = parse argv
            return! display appData parsedArgs
        }
    
    match displayResult with
    | Ok _ ->
        () 
    | Error _ ->
        // maybe print some error message
        ()
        
let config : Command.Config =
    { Command = "list"
      Help = "Displays the current todos." 
      Function = CommandFunction.NoDataChange execute }