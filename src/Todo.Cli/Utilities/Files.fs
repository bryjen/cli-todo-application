namespace Todo.Cli.Utilities

open System
open System.IO
open System.Reflection

open System.Text
open System.Text.Json
open Todo

module Files =
    
    /// The directory of the executable file itself
    let internal executableDirectory =
        let assembly = Assembly.GetExecutingAssembly()
        let uri = UriBuilder(assembly.Location)
        Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path))
        
    /// Attempts to save the the provided AppData record given a file path.
    let saveAppData (filePath: string) (appData: AppData) : Result<unit, Exception> =
        try
            if not (File.Exists(filePath)) then
                use fileStream = File.Create(filePath)
                let jsonData = JsonSerializer.Serialize(appData, jsonSerializerOptions)
                let asBytes = Encoding.ASCII.GetBytes(jsonData)
                Ok (fileStream.Write(asBytes))
            else
                let jsonData = JsonSerializer.Serialize(appData, jsonSerializerOptions)
                Ok (File.WriteAllText(filePath, jsonData))
        with
            | ex -> Error ex
            
    /// Attempts to load an AppData record from the provided file path. 
    let loadAppData (filePath: string) : Result<AppData, Exception> =
        try
            let rawJson = File.ReadAllText(filePath)
            Ok (JsonSerializer.Deserialize<AppData>(rawJson, jsonSerializerOptions))
        with
            | ex -> Error ex
