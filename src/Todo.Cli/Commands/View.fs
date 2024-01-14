[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.List

open System
open Spectre.Console
open Argu
open FsToolkit.ErrorHandling
open Todo
open Todo.Cli
open Todo.ItemGroup
open Todo.Cli.Utilities
open Todo.Cli.Commands.Arguments

let private printItemGroups (itemGroups: ItemGroup list) =
    AnsiConsole.Clear()
        
    itemGroups
    |> List.map (_.ToString()) 
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
    let converter = Formatters.Tree.PrettyTree.toLines 
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
        
let private display (filePath: string) (appData: AppData) (parseResults: ParseResults<ViewArguments>) : Result<unit, Exception> =
    match parseResults.TryGetResult ViewArguments.Interactive with
    | Some _ ->
        Ok appData
        |> Result.bind interactiveSession      // result of interactive actions
        |> Result.bind (Files.saveAppData filePath) 
    | None ->
        printItemGroups appData.ItemGroups 
        Ok ()
        
/// <summary>
/// Returns a function that displays a view of the current todos.
/// </summary>
/// <param name="applicationConfiguration">The application configuration.</param>
/// <param name="appData">The application data.</param>
let internal injectView 
    (applicationConfiguration: ApplicationConfiguration)
    (_: ApplicationSettings)
    (appData: AppData)
    : CommandFunction =
    
    let execute (argv: string array) : unit =
        let displayResult = 
            result {
                let! parsedArgs = parseArgv argv
                let saveFilePath = applicationConfiguration.getDataFilePath()
                return! display saveFilePath appData parsedArgs
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