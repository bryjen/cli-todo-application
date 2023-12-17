/// TodoList.Interface.fs
///
/// Contains functions relating to the management and interaction with tοdo groups. Most functions correspond to a
/// single user action. The 'Execute' function receives user input, and handles the corresponding response. 

module Todo.Modules.TodoList_Interface

open System
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open Spectre.Console

open Todo.Modules.Data
open Todo.Core.Utilities.Attributes
open Todo.Core.Console
open Todo.Modules.TodoList

/// <summary>
/// Action method for displaying all tοdo groups.
/// </summary>
[<Obsolete("Old prototype system that uses hard-coding. Use new attribute-based system instead.")>]
[<ActionSignature("list", "printAllTodoGroups", "1. View all todo groups")>]
let printAllTodoGroups (todoGroups: TodoGroup list) (_: string array): TodoGroup list =
    todoGroups
    |> List.map (fun todoGroup -> printfn $"%s{TodoGroup.toString todoGroup}")
    |> ignore
    
    Console.pressEnterToContinue ()
    Console.clearConsole ()
    todoGroups
    
/// <summary>
/// Action method for creating a new tοdo group.
/// </summary>
[<Obsolete("Old prototype system that uses hard-coding. Use new attribute-based system instead.")>]
[<ActionSignature("list", "createTodoGroup", "2. Create todo group")>]
let createTodoGroup (todoGroups: TodoGroup list) (argv: string array) : TodoGroup list =
    
    let information =
        match argv |> Array.toList with
        | first :: second :: _ ->
            (first, second)
        | first :: _ ->
            let description = Console.ask<string>("Please enter a short [green underline]description[/] for the todo group:")
            (first, description)
        | _ ->
            let name = Console.ask<string>("Please enter a [green underline]name[/] for the todo group name:")
            let description = Console.ask<string>("Please enter a short [green underline]description[/] for the todo group:")
            (name, description)
        
    let creationResult = TodoGroup.tryCreateTodoGroup todoGroups (fst information) (snd information) 
    
    match creationResult with
    | None ->
        Console.printError "\n\nA todo group already exists with the same name!\n\n"
        Console.pressEnterToContinue ()
        Console.clearConsole ()
        todoGroups
    | Some value ->
        let newTodoGroup = value :: todoGroups
        Console.printSuccess $"\n\nCourse group \"{(fst information)}\" successfully created!\n\n"
        Console.pressEnterToContinue ()
        Console.clearConsole ()
        newTodoGroup

/// <summary>
/// Action method for deleting a tοdo group.
/// </summary>
[<Obsolete("Old prototype system that uses hard-coding. Use new attribute-based system instead.")>]
[<ActionSignature("list", "deleteTodoGroup", "3. Delete a todo group")>]
let deleteTodoGroup (todoGroups: TodoGroup list) (argv: string array) : TodoGroup list =
    
    let name =
        match argv |> Array.toList with
        | first :: _ ->
            first
        | _ ->
            let name = AnsiConsole.Ask<string>("Please enter the [green underline]name[/] of the todo group to delete:")
            name
        
    let newTodoGroupList = List.filter (fun todoGroup -> todoGroup.Name <> name) todoGroups
    
    match (List.length todoGroups) - (List.length newTodoGroupList) with
    | 0 ->
        Console.printError $"Could not find todo group with the name \'{name}\'"
        Console.pressEnterToContinue ()
        Console.clearConsole ()
        todoGroups
    | _ ->
        Console.printSuccess $"\n\nCourse group \"{name}\" successfully created!\n\n"
        Console.pressEnterToContinue ()
        Console.clearConsole ()
        newTodoGroupList
    



/// <summary>
/// Function that takes in user input and determines what action function to execute.
/// </summary>
[<Obsolete("Old prototype system that uses hard-coding. Use new attribute-based system instead.")>]
let rec execute (argv: string array) (todoGroups: TodoGroup list) : unit =
    
    let actionMap = FunctionMapper.createModuleActionMap "list"
    let quitAction = " . Quit"
    
    let choices = quitAction :: (List.map fst (Map.toList actionMap)) 
    let prompt = "Select what action you would like to perform:"
    let selectedAction = Console.promptUser<string> (PromptOptions.DefaultSelectionPrompt (choices, prompt)) 
    
    match selectedAction with
    | value when value = quitAction ->
        match JsonData.saveTodoGroups JsonData.todoGroupsFilePath todoGroups with
        | Ok _ -> () 
        | Error errorValue -> failwith errorValue
        ()
    | value when (Map.containsKey value actionMap) ->
        let newTodoGroups = actionMap[value].Invoke(null, [| todoGroups; Array.empty<string> |]) :?> TodoGroup list
        execute argv newTodoGroups 
    | _ -> 
        execute argv todoGroups