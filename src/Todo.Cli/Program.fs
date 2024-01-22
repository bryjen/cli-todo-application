module Todo.Cli.Program

open System
open FsToolkit.ErrorHandling
open Spectre.Console
open Todo
open Todo.Cli.Commands
open Todo.Cli.Utilities
open Todo.Cli.Utilities.Arguments

let commandConfigs = [
    viewCommandTemplate
    createCommandTemplate
    deleteCommandTemplate
    editCommandTemplate
    helpCommandTemplate
]

/// Attempt to read all of the required files. (configuration, user settings, data, etc.)
let onStartup () : Result<ApplicationConfiguration * ApplicationSettings * AppData, Exception> =
    result {
        let! applicationConfiguration = Config.getApplicationConfiguration ()
        let! applicationSettings = Settings.getApplicationSettings () 
        let dataFilePath = applicationConfiguration.getDataFilePath()
        let! appData = Files.initializeAndLoadAppData dataFilePath
        return (applicationConfiguration, applicationSettings, appData)
    }
    
/// Examines the first token in the program arguments, and finds the respective command template record.
let processUserArguments argv : Result<CommandTemplate * string array, Exception> =
    match splitToCommandAndArgs argv with
    | None ->
        Ok (helpCommandTemplate, Array.empty)
        
    | Some (command, arguments) ->
        let commandTemplateOption = List.tryFind (fun commandTemplate -> commandTemplate.CommandName = command) commandConfigs
        match commandTemplateOption with
        | Some commandTemplate -> Ok (commandTemplate, arguments)
        | None -> Error (Exception("Could not find the indicated command, please try again."))
        
/// Executes the command specified in the command template
let processCommand applicationConfiguration applicationSettings appData commandTemplate argv : AppData option =
    let executeFunc = commandTemplate.InjectData applicationConfiguration applicationSettings appData
    
    match executeFunc with
    | ChangesData func ->
        let newAppData = func argv
        Some newAppData
    | NoDataChange func ->
        func argv
        None

[<EntryPoint>]
let main argv =
    let executionResult = 
        result {
            let! (appConfig, appSettings, appData) = onStartup ()
            let! (commandTemplate, commandArguments) = processUserArguments argv
            let newAppDataOption = processCommand appConfig appSettings appData commandTemplate commandArguments 
            
            match newAppDataOption with
            | Some newAppData -> return! Files.saveAppData (appConfig.getDataFilePath()) newAppData 
            | None -> ()
        }
        
    match executionResult with
    | Ok _ ->
        0
    | Error err ->
        AnsiConsole.WriteException(err)
        1