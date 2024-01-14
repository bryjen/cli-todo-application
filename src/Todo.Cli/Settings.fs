namespace Todo.Cli

open System
open System.IO
open System.Reflection
open System.Text.Json
open FsToolkit.ErrorHandling
open Todo.Cli.Utilities.Files

type ApplicationSettings =
    { Placeholder: string }
    
    (* The "default" value for application settings. Used for testing. *)
    static member Default =
        { Placeholder = "" }

module Settings =
        
    let private readSettingsFile (applicationConfiguration: ApplicationConfiguration) : Result<ApplicationSettings, Exception> =
        let configFilePath = Path.Join(executableDirectory, applicationConfiguration.DataDirectory, "data.json")
        
        match File.Exists(configFilePath) with
        | true ->
            // File exists, try loading and parsing json into record instance
            try
                let rawJson = File.ReadAllText(configFilePath)
                Ok (JsonSerializer.Deserialize<ApplicationSettings>(rawJson))
            with
                | ex -> Error ex
        | false ->
            Error (Exception("Could not find the \"config.json\" file. Please place it in the same directory as the executable."))
            
    let private validateSettings applicationSettings : Result<ApplicationSettings, Exception> =
        // TODO: Whenever you need to do validation.
        Ok applicationSettings
        
    /// Attempts to read and parse the configuration file.
    let getApplicationSettings (applicationConfiguration: ApplicationConfiguration) : Result<ApplicationSettings, Exception> =
        (* 
        result {
            let! appSettings = readSettingsFile applicationConfiguration 
            return! validateSettings appSettings 
        }
        *)
        
        Ok ApplicationSettings.Default
