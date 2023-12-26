[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.List

open System
open Spectre.Console
open Argu

open Todo
open Todo.Cli.Utilities
open Todo.Utilities.Attributes.Command

/// <summary>
/// Arguments for the 'list' command 
/// </summary>
type private ListArguments =
    | [<AltCommandLine("-i")>] Interactive
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Interactive -> "Makes it so that the display is interactive."

/// <summary>
/// Displays the current configuration of the item groups.
/// </summary>
let private display (appData: AppData) =
        
    AnsiConsole.Clear()
        
    appData.ItemGroups
    |> List.map (fun itemGroup -> itemGroup.ToString()) 
    |> List.map (fun str -> AnsiConsole.MarkupLine($"%s{str}")) 
    |> ignore
    
/// <summary>
/// Displays the current configuration of the item groups. <b>Is Interactive.</b>
/// </summary>
let private interactiveDisplay (appData: AppData) =
        
    AnsiConsole.Clear()
    AnsiConsole.MarkupLine("[red]Interactive functionality to be implemented soon.[/]")
    
    appData.ItemGroups
    |> List.map (fun itemGroup -> itemGroup.ToString()) 
    |> List.map (fun str -> AnsiConsole.MarkupLine($"%s{str}")) 
    |> ignore
    
/// <summary>
/// Implements the logic for the 'list' command.
/// </summary>
/// <param name="argv">List of arguments to be parsed.</param>
let list (argv: string array) : unit =
    
    // Loading app data
    let appData = 
        match Files.loadAppData Files.filePath with
        | Ok appData -> appData
        | Error err -> raise err
    
    // Generates the parser 
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<ListArguments>(errorHandler = errorHandler)
    
    // Tries parsing
    try
        let parsedArgs = parser.Parse argv
        
        match parsedArgs.GetAllResults() with
        | parseResults when List.contains Interactive parseResults -> interactiveDisplay appData
        | _ -> display appData
    with
        | :? ArguParseException as ex ->
            printfn $"%s{ex.Message}"
        | ex ->
            printfn "Unexpected exception"
            printfn $"%s{ex.Message}"

[<CommandInformation>]
let ``'list' Command Config`` () : Command.Config =
    { Command = "list"
      Description = "Displays a summarized list of the current todos."
      Help = None
      Function = CommandFunction.NoDataChange list }