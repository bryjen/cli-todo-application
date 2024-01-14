namespace Todo.Cli

open System
open System.IO
open System.Reflection
open System.Text.Json
open FsToolkit.ErrorHandling
open Todo.Cli.Utilities.Files

type ApplicationConfiguration =
    { DataDirectory: string }
    
    (* The "default" value for application configurations. Used for testing. *)
    static member Default =
        { DataDirectory = "" }
    
    /// Returns the string path of the 'data.json' file.
    member this.getDataFilePath () : string =
        Path.Join(executableDirectory, this.DataDirectory, "data.json")
    
module Config =
    
    let private readConfigurationFile () : Result<ApplicationConfiguration, Exception> =
        let configFilePath = Path.Join(executableDirectory, "config.json")
        
        match File.Exists(configFilePath) with
        | true ->
            // File exists, try loading and parsing json into record instance
            try
                let rawJson = File.ReadAllText(configFilePath)
                Ok (JsonSerializer.Deserialize<ApplicationConfiguration>(rawJson))
            with
                | ex -> Error ex
        | false ->
            Error (Exception("Could not find the \"config.json\" file. Please place it in the same directory as the executable."))
            
    let private validateConfiguration applicationConfiguration : Result<ApplicationConfiguration, Exception> =
        // TODO: Whenever you need to do validation.
        Ok applicationConfiguration 
        
    /// Attempts to read and parse the configuration file.
    let getApplicationConfiguration () : Result<ApplicationConfiguration, Exception> =
        result {
            let! configFile = readConfigurationFile () 
            return! validateConfiguration configFile 
        }