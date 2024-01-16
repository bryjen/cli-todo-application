namespace Todo.Cli

open System
open System.IO
open System.Text.Json
open FsToolkit.ErrorHandling
open Todo.Formatters
open Todo.Cli.Utilities
open Todo.Cli.Utilities.Files

type ApplicationSettings =
    { DisplayFormat: DisplayFormat
      ClearConsoleOnView: bool }
    
    (* The "default" value for application settings. Used for testing. *)
    static member Default =
        { DisplayFormat = PrettyTree
          ClearConsoleOnView = true }

module Settings =
        
    let private readSettingsFile () : Result<ApplicationSettings, Exception> =
        let configFilePath = Path.Join(executableDirectory, "settings.json")
        
        match File.Exists(configFilePath) with
        | true ->
            // File exists, try loading and parsing json into record instance
            try
                let rawJson = File.ReadAllText(configFilePath)
                Ok (JsonSerializer.Deserialize<ApplicationSettings>(rawJson, jsonSerializerOptions))
            with
                | ex -> Error ex
        | false ->
            let msg = "Could not find the \"settings.json\" file. Please place it in the same directory as the executable." 
            Error (FileNotFoundException(msg))
            
    let private validateSettings applicationSettings : Result<ApplicationSettings, Exception> =
        // TODO: Whenever you need to do validation.
        Ok applicationSettings
        
    /// Attempts to read and parse the configuration file.
    let getApplicationSettings () : Result<ApplicationSettings, Exception> =
        result {
            let! appSettings = readSettingsFile () 
            return! validateSettings appSettings 
        }