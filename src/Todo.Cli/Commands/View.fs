module Todo.Cli.Commands.View

open Argu
open Spectre.Console
open Todo
open Todo.Formatters
open Todo.ItemGroup
open Todo.Cli
open Todo.Cli.Utilities
open Todo.Cli.Commands.Arguments

// Takes the display format specified in the command, or take the default one, if not provided.
let private getDisplayFormat (appSettings: ApplicationSettings) (viewParseResults: ParseResults<ViewArguments>) =
    let specifiedDisplayFormatOption =
        viewParseResults.TryGetResult ViewArguments.Type
        |> Option.map (_.GetAllResults())
        |> Option.map List.head
    
    match specifiedDisplayFormatOption with
    | Some displayFormatArg -> DisplayFormatArguments.ToDisplayFormatDU displayFormatArg 
    | None -> appSettings.DisplayFormat
    
let private isInteractive (viewParseResults: ParseResults<ViewArguments>) =
    match viewParseResults.TryGetResult ViewArguments.Interactive with
    | Some _ -> true
    | None -> false
    
// Statically displays the app information to the user.
let private staticDisplay (appData: AppData) (displayFormat: DisplayFormat) =
    let rootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    let formatter = DisplayFormat.GetToLinesFunc displayFormat
    let linesToPrint = formatter rootItemGroup
    List.map AnsiConsole.MarkupLine linesToPrint |> ignore
    
// Dynamically and interactively displays the app information to the user.
let private interactiveDisplay (appConfig: ApplicationConfiguration) (appData: AppData) =
    let newAppData = Interactive.interactive appData
    
    let filePath = appConfig.getDataFilePath()
    match Files.saveAppData filePath newAppData with
    | Ok _ ->
        () 
    | Error err ->
        let exceptionMessage = "An error occurred in the interactive view: " + err.Message
        printfn "%s" exceptionMessage

let internal executeViewCommand
    (appConfig: ApplicationConfiguration)
    (appSettings: ApplicationSettings)
    (appData: AppData)
    (viewParseResults: ParseResults<ViewArguments>)
    : unit =
        
    if appSettings.ClearConsoleOnView then AnsiConsole.Clear() else ()
    
    let displayFormat = getDisplayFormat appSettings viewParseResults
    
    if isInteractive viewParseResults then
        interactiveDisplay appConfig appData 
    else
        staticDisplay appData displayFormat