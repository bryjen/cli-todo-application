
open Microsoft.FSharp.Collections

open Todo.Modules
open Todo.Modules.Data.JsonData


[<EntryPoint>]
let main argv =

    //  Load data
    let loadResults = loadTodoGroups todoGroupsFilePath
    
    let loadedTodoGroups =
        match loadResults with
        | Ok data -> data
        | Error errorMessage -> failwith errorMessage 
    
    
    //  Determine what action to perform
    match (Array.tryHead argv) with
    | Some value when value.ToLower() = "list" ->
        TodoList_Interface.execute (Array.skip 1 argv) loadedTodoGroups
    | None -> ()
    | _ -> ()
    0