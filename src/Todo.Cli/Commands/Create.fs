[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Create

open System
open Argu

open Todo
open Todo.Cli.Utilities
open Todo.Utilities.Attributes.Command

// TODO: add support for label creation

/// <summary>
/// Arguments for the 'create' command.
/// </summary>
type CreateArguments =
    | [<First; CliPrefix(CliPrefix.None)>] Item_Group of ParseResults<CreateItemGroupArguments>
    | [<First; CliPrefix(CliPrefix.None)>] Item of ParseResults<CreateItemArguments>
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Item_Group _ -> "Creates an item group."
            | Item _ -> "Creates an item."
    
/// Arguments for the 'create item-group' subcommand
and CreateItemGroupArguments =
    | [<Mandatory; AltCommandLine("-n")>] Name of name:string
    | [<AltCommandLine("-d")>] Description of desc:string option
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "The name of the item group."
            | Description _ -> "An accompanying description."
            
/// Arguments for the 'create item' subcommand
and CreateItemArguments =
    | [<Mandatory; AltCommandLine("-n")>] Name of name:string
    | [<AltCommandLine("-d")>] Description of desc:string option
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "The name of the item."
            | Description _ -> "An accompanying description."



/// <summary>
/// Implements the logic for the 'list' command.
/// </summary>
/// <param name="argv">List of arguments to be parsed.</param>
let create (argv: string array) : AppData =
    
    // Loading app data
    let appData = 
        match Files.loadAppData Files.filePath with
        | Ok appData -> appData
        | Error err -> raise err
        
    // Creating the parser
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<CreateArguments>(errorHandler = errorHandler)
    
    // Tries parsing
    try
        let parsedArgs = parser.Parse argv
       
        match parsedArgs.TryGetResult Item_Group with
        | _ when (List.length (parsedArgs.GetAllResults())) = 0 -> printfn "%s" (parser.PrintUsage("You must enter some arguments!")) 
        | None -> printfn $"create item: %A{parsedArgs.GetAllResults()}"     
        | Some _ -> printfn $"create item group: %A{parsedArgs.GetAllResults()}"
    with
        | :? ArguParseException as ex ->
            printfn $"%s{ex.Message}"
        | ex ->
            printfn "Unexpected exception"
            printfn $"%s{ex.Message}"
        
    AppDataFunctions.defaultAppData

[<CommandInformation>]
let ``'create' Command Config`` () : Command.Config =
    { Command = "create"
      Description = "Creates something."
      Help = None
      Function = CommandFunction.ChangesData create}