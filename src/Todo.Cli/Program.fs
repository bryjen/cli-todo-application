module Todo.Cli.Program

open Todo
open Todo.Cli.Commands
open Todo.Cli.Utilities
open Todo.Cli.Utilities.Arguments

#if Debug
open System
open Spectre.Console

open Todo.ItemGroup
open Todo.ItemGroup.Representations.Tree
open Spectre.Console.Prompts.Extensions
#endif

let commandConfigs = [
    Help.config
    List.config
    Create.config
    Delete.config
    Edit.config
]

[<EntryPoint>]
let rec main argv =
    
#if Debug
    let filePath = Files.filePath
    let appdata = Files.loadAppData filePath  |> Result.toList |> List.head
    let rootitemgroup = { ItemGroup.Default with SubItemGroups = appdata.ItemGroups }
    
    let pairs = Converter.PrettyTree.pairLinesWithObject rootitemgroup
    
    Console.ReadLine() |> ignore 
#endif
   
    //  Get command map 
    let commandMap = Command.CommandMap commandConfigs 
    
    //  Process command using command map
    match splitToCommandAndArgs argv with
    | None ->
        printCommandsHelp Array.empty // functions identically if the user entered '... help'
    | Some (command, arguments) ->
        match commandMap.getConfig command with
        | None -> 
            printfn "Could not find the indicated command, please try again."
        | Some config ->
            processCommand config arguments |> ignore
    0
    
and processCommand (commandConfig: Command.Config) (argv: string array) : AppData option =
    match commandConfig.Function with
    | NoDataChange func ->
        func argv
        None
    | ChangesData func ->
        let newAppData = func argv
        match Files.saveAppData Files.filePath newAppData with
        | Error err -> raise err    //  todo: process error once you get a good idea of app flow
        | Ok _ -> Some newAppData 
