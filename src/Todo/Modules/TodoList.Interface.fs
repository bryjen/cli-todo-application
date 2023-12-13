/// TodoList.Interface.fs
///
/// Contains functions relating to the management and interaction with tοdo groups. Most functions correspond to a
/// single user action. The 'Execute' function receives user input, and handles the corresponding response. 

module Todo.Modules.TodoList_Interface

open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open Spectre.Console

open Todo.Modules.Data
open Todo.Utilities.Attributes
open Todo.Utilities.Console
open Todo.Modules.TodoList

/// <summary>
/// Action method for displaying all tοdo groups.
/// </summary>
[<ActionSignature("list", "printAllTodoGroups", "1. View all todo groups")>]
let printAllTodoGroups (todoGroups: TodoGroup list) (_: string array): TodoGroup list =
    todoGroups
    |> List.map (fun todoGroup -> printfn $"%s{TodoGroup.toString todoGroup}")
    |> ignore
    
    pressEnterToContinue ()
    clearConsole ()
    todoGroups
    
/// <summary>
/// Action method for creating a new tοdo group.
/// </summary>
[<ActionSignature("list", "createTodoGroup", "2. Create todo group")>]
let createTodoGroup (todoGroups: TodoGroup list) (argv: string array) : TodoGroup list =
    
    let information =
        match argv |> Array.toList with
        | first :: second :: _ ->
            (first, second)
        | first :: _ ->
            let description = AnsiConsole.Ask<string>("Please enter a short [green underline]description[/] for the todo group:")
            (first, description)
        | _ ->
            let name = AnsiConsole.Ask<string>("Please enter a [green underline]name[/] for the todo group name:")
            let description = AnsiConsole.Ask<string>("Please enter a short [green underline]description[/] for the todo group:")
            (name, description)
        
    let creationResult = TodoGroup.tryCreateTodoGroup todoGroups (fst information) (snd information) 
    
    match creationResult with
    | None ->
        AnsiConsole.MarkupLine("\n\n[red]A todo group already exists with the same name![/]\n\n")
        pressEnterToContinue ()
        clearConsole ()
        todoGroups
    | Some value ->
        let newTodoGroup = value :: todoGroups
        AnsiConsole.MarkupLine($"\n\n[green]Course group \"{(fst information)}\" successfully created![/]\n\n")
        pressEnterToContinue ()
        clearConsole ()
        newTodoGroup

/// <summary>
/// Action method for deleting a tοdo group.
/// </summary>
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
        AnsiConsole.MarkupLine($"[red]Could not find todo group with the name \'{name}\'[/]")
        pressEnterToContinue ()
        clearConsole ()
        todoGroups
    | _ ->
        AnsiConsole.MarkupLine($"\n\n[green]Course group \"{name}\" successfully created![/]\n\n")
        pressEnterToContinue ()
        clearConsole ()
        newTodoGroupList
    



/// <summary>
/// Function that takes in user input and determines what action function to execute.
/// </summary>
let rec execute (argv: string array) (todoGroups: TodoGroup list) : unit =
    
    let actionMap = FunctionMapper.createModuleActionMap "list"
    let quitAction = " . Quit"
    let actions = quitAction :: (List.map fst (Map.toList actionMap)) 
    
    let prompt = "Select what action you would like to perform:"
    let selectionPrompt = SelectionPrompt<string>(Title = prompt).AddChoices(actions)
    let selectedAction = AnsiConsole.Prompt(selectionPrompt)
    
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