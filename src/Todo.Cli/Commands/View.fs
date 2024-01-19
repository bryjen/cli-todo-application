[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.List

open System
open Spectre.Console
open Argu
open FsToolkit.ErrorHandling
open Todo
open Todo.Cli
open Todo.Formatters
open Todo.ItemGroup
open Todo.Cli.Utilities
open Todo.Cli.Commands.Arguments

let private parseArgv (argv: string array) : Result<ParseResults<ViewArguments>, Exception> =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<ViewArguments>(errorHandler = errorHandler)
    
    try
        let parsedArgs = parser.Parse argv
        Ok parsedArgs
    with
        | :? ArguParseException as ex -> Error ex
        | ex -> Error ex

let private printItemGroups (converter: ItemGroup -> string list) (itemGroups: ItemGroup list) =
    let rootItemGroup = { ItemGroup.Default with SubItemGroups = itemGroups }
    let linesToPrint = converter rootItemGroup
    List.map AnsiConsole.MarkupLine linesToPrint |> ignore
    ()
    
let private interactiveSession (appData: AppData) : Result<AppData, Exception> =
    let newAppData = Interactive.interactive appData  
    Ok newAppData 
        
let private display
    (applicationConfiguration: ApplicationConfiguration)
    (applicationSettings: ApplicationSettings)
    (appData: AppData)
    (parseResults: ParseResults<ViewArguments>)
    : Result<unit, Exception> =
        
    let displayFormat = applicationSettings.DisplayFormat
    let converter = DisplayFormat.GetToLinesFunc displayFormat
    
    if applicationSettings.ClearConsoleOnView then AnsiConsole.Clear() else ()
    
    match parseResults.TryGetResult ViewArguments.Interactive with
    | Some _ ->
        (* Work around since just the 'interactive' session can change data. *)
        result {
            let filePath =  applicationConfiguration.getDataFilePath()
            let! newAppData = interactiveSession appData
            return! Files.saveAppData filePath newAppData
        } 
    | None ->
        printItemGroups converter appData.ItemGroups
        Ok ()
        
        
/// <summary>
/// Returns a function that displays a view of the current todos.
/// </summary>
/// <param name="applicationConfiguration">The application configuration.</param>
/// <param name="applicationSettings">The application settings.</param>
/// <param name="appData">The application data.</param>
let internal injectView 
    (applicationConfiguration: ApplicationConfiguration)
    (applicationSettings: ApplicationSettings)
    (appData: AppData)
    : CommandFunction =
    
    let execute (argv: string array) : unit =
        let displayResult = 
            result {
                let! parsedArgs = parseArgv argv
                return! display applicationConfiguration applicationSettings appData parsedArgs
            }
            
        match displayResult with
        | Ok _ ->
            () 
        | Error ex ->
            AnsiConsole.Write($"Error thrown on display: {ex}")
            
    CommandFunction.NoDataChange execute
   
let viewCommandTemplate : CommandTemplate =
    { CommandName = "view"
      HelpString = "Displays a view of the current todos."
      InjectData = injectView }