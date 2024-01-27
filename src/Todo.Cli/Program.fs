module Todo.Cli.Program

open System
open FsToolkit.ErrorHandling
open Spectre.Console
open Todo
open Todo.Cli.Commands
open Todo.Cli.Utilities
open Todo.Cli.Utilities.Arguments

/// Attempt to read all of the required files. (configuration, user settings, data, etc.)
let onStartup () : Result<ApplicationConfiguration * ApplicationSettings * AppData, Exception> =
    result {
        let! applicationConfiguration = Config.getApplicationConfiguration ()
        let! applicationSettings = Settings.getApplicationSettings () 
        let dataFilePath = applicationConfiguration.getDataFilePath()
        let! appData = Files.initializeAndLoadAppData dataFilePath
        return (applicationConfiguration, applicationSettings, appData)
    }

[<EntryPoint>]
let main argv =
    let executionResult = 
        result {
            let! (appConfig, appSettings, appData) = onStartup ()
            let! argvParsed = ProgramArguments.Parse argv
            do ProgramArguments.Execute appConfig appSettings appData argvParsed
        }
        
    match executionResult with
    | Ok _ ->
        0
    | Error errorValue ->
        AnsiConsole.WriteLine(errorValue.Message)
        -1