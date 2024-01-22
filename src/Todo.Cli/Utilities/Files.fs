namespace Todo.Cli.Utilities

open System
open System.IO
open System.Reflection
open System.Text.Json
open FsToolkit.ErrorHandling
open Todo

module Files =
    
    /// The directory of the executable file itself
    let internal executableDirectory =
        let assembly = Assembly.GetExecutingAssembly()
        let uri = UriBuilder(assembly.Location)
        Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path))
    
    // Attempts to create a file at the specified path, if it doesn't exist
    let private createFileIfNotExists (filePath: string) =
        if not (File.Exists(filePath)) then
            try
                use fileStream = File.Create(filePath)
                Ok ()
            with ex ->
                Error ex
        else
            Ok ()
    
    // Attempts to asynchronously read the specified file. Returns a 'result' DE. 
    let private readFileWithErrorHandlingAsync (filePath: string) =
        async {
            try
                let! rawJson = File.ReadAllTextAsync(filePath) |> Async.AwaitTask
                return Ok rawJson
            with ex ->
                return Error ex
        }
        
    // Attempts to asynchronously write the specified file. Returns a 'result' DE.
    let private writeFileWithErrorHandlingAsync (filePath: string) (contents: string) =
        async {
            try
                File.WriteAllTextAsync(filePath, contents) |> Async.AwaitTask |> ignore
                return Ok ()
            with ex ->
                return Error ex 
        }
        
    /// Attempts to save the the provided AppData record given a file path.
    let saveAppData (filePath: string) (appData: AppData) : Result<unit, Exception> =
        result {
            do! createFileIfNotExists (filePath)
            let rawJson = JsonSerializer.Serialize(appData, jsonSerializerOptions)
            return! writeFileWithErrorHandlingAsync filePath rawJson |> Async.RunSynchronously 
        }
            
    /// Attempts to load an AppData record from the provided file path. 
    let loadAppData (filePath: string) : Result<AppData, Exception> =
        result {
            let! rawJson = readFileWithErrorHandlingAsync (filePath) |> Async.RunSynchronously 
            return JsonSerializer.Deserialize<AppData>(rawJson, jsonSerializerOptions)
        }
        
    /// Attempts to load an AppData record from the provided file path. If the file does not exist, it is generated.
    let initializeAndLoadAppData (filePath: string) : Result<AppData, Exception> =
        result {
            if not (File.Exists(filePath)) then
                saveAppData filePath AppData.Default |> ignore
                return AppData.Default
            else
                return! loadAppData filePath
        }