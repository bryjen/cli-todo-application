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
    
    
    let ta3 = { Name = "Theory Assignment #3"; Description = None; DueDate = DueDate.DateDue DateTime.Now; Labels = List.empty }
    let pa3 = { Name = "Programming Assignment #3"; Description = None; DueDate = DueDate.DateDue DateTime.Now; Labels = List.empty }
    let assignments = { Name = "Assignments"; SubItemGroups = List.empty; Items = [ta3; pa3]; Labels = List.empty }
    
    let material1 = { Name = "Template metaprogramming"; Description = None; DueDate = DueDate.WeekDue 45; Labels = List.empty }
    let material2 = { Name = "Programming Assignment #3"; Description = None; DueDate = DueDate.WeekDue 46; Labels = List.empty }
    let material = { Name = "Material"; SubItemGroups = List.empty; Items = [material1; material2]; Labels = List.empty }
    
    let comp345 = { Name = "COMP 345"; SubItemGroups = [assignments; material]; Items = List.empty; Labels = List.empty }
    let university = { Name = "University"; SubItemGroups = [comp345]; Items = List.empty; Labels = List.empty }
    
    
    let todo1 = { Name = "Refactor Program"; Description = None; DueDate = DueDate.DateDue DateTime.Now; Labels = List.empty }
    let todo2 = { Name = "Add new feature"; Description = None; DueDate = DueDate.DateDue DateTime.Now; Labels = List.empty }
    let todo3 = { Name = "Update documentation"; Description = None; DueDate = DueDate.DateDue DateTime.Now; Labels = List.empty }
    let projectTodos = { Name = "Todo-Console-App"; SubItemGroups = List.empty; Items = [todo1; todo2; todo3]; Labels = List.empty }
    
    let newAppData = { ItemGroups = [university; projectTodos] }
    
    match Files.saveAppData Files.filePath newAppData with
    | Ok _ -> printfn "Success!"
    | Error err -> raise err
    
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