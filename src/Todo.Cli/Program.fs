module Todo.Cli.Program

open System
open Todo
open Todo.Cli.Utilities
open Todo.Utilities

let commandMap = 
    match Command.getCommandMap () with
    | Ok commandMap -> commandMap
    | Error ex -> raise ex

/// Splits argv into the command and its arguments
let splitToCommandAndArgs (argv: string array) : (string * string array) option =
    match Array.length argv with
    | 0 -> None
    | 1 -> Some ((Array.head argv), Array.empty<string>)
    | _ -> Some ((Array.head argv), (Array.tail argv))
    
/// Executes the function in a command config using the passed arguments 
let processCommand (commandConfig: Command.Config) (argv: string array) : AppData option =
    match commandConfig.Function with
    | NoDataChange func ->
        func argv
        None
    | ChangesData func ->
        let newAppData = func argv
        match Files.saveAppData Files.filePath newAppData with
        | Error err -> raise err    //  todo: process error once you get a good idea of app flow
        | Ok _ -> Some newAppData 

[<EntryPoint>]
let main argv =
    match splitToCommandAndArgs argv with
    | None ->
        printfn "No arguments passed, functionality will be implemented soon."
    | Some (command, arguments) ->
        match commandMap.getConfig command with
        | None -> 
            printfn "Could not find the indicated command, please try again."
        | Some config ->
            processCommand config arguments |> ignore
    0