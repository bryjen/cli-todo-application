﻿module Todo.Cli.Program

open Spectre.Console
open Todo
open Todo.Cli.Utilities
open Todo.Cli.Utilities.Arguments


[<EntryPoint>]
let rec main argv =
   
#if Debug
    //  Display
    Todo.Cli.Commands.List.list Array.empty     
      
    let newAppData = Todo.Cli.Commands.Create.create [|"item-group"; "-n"; "COMP 353"; "--path"; "University"|]
    
    //  Display new app data
    newAppData.ItemGroups
    |> List.map (fun itemGroup -> itemGroup.ToString()) 
    |> List.map (fun str -> AnsiConsole.MarkupLine($"%s{str}")) 
    |> ignore
#else
    //  Get command map 
    let commandMap = 
        match Command.getCommandMap () with
        | Ok commandMap -> commandMap
        | Error ex -> raise ex
    
    //  Process command using command map
    match splitToCommandAndArgs argv with
    | None ->
        printfn "No arguments passed, functionality will be implemented soon."
    | Some (command, arguments) ->
        match commandMap.getConfig command with
        | None -> 
            printfn "Could not find the indicated command, please try again."
        | Some config ->
            processCommand config arguments |> ignore
#endif
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
