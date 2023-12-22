namespace Todo.Cli

open System
open System.IO
open System.Reflection

open System.Text
open System.Text.Json
open Todo

type AppConfig =
    { DataFilesDirectory: string      //  Relative to the actual location of the executable file 
       }

module Files =
    
    /// The directory of the executable file itself.
    let private executableDirectory =
        let assembly = Assembly.GetExecutingAssembly()
        let uri = UriBuilder(assembly.Location)
        Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path))
    
    /// The application configuration.
    /// Loaded from the config.json file.
    let appConfig =
        let rawJson =File.ReadAllText(executableDirectory + "/config.json")
        JsonSerializer.Deserialize<AppConfig>(rawJson)
        
    /// The file path of the file where the app/program data is saved to.
    let filePath = sprintf $"%s{executableDirectory}/%s{appConfig.DataFilesDirectory}/data.json"
    
    /// Attempts to save the the provided AppData record given a file path.
    let saveAppData (filePath: string) (appData: AppData) : Result<unit, Exception> =
        try
            if not (File.Exists(filePath)) then
                use fileStream = File.Create(filePath)
                let jsonData = JsonSerializer.Serialize(appData, JsonSerializerOptions(WriteIndented = true))
                let asBytes = Encoding.ASCII.GetBytes(jsonData)
                Ok (fileStream.Write(asBytes))
            else
                let jsonData = JsonSerializer.Serialize(appData, JsonSerializerOptions(WriteIndented = true))
                Ok (File.WriteAllText(filePath, jsonData))
        with
            | ex -> Error ex
            
    /// Attempts to load an AppData record from the provided file path. 
    let loadAppData (filePath: string) : Result<AppData, Exception> =
        try
            let rawJson = File.ReadAllText(filePath)
            Ok (JsonSerializer.Deserialize<AppData>(rawJson))
        with
            | ex -> Error ex
