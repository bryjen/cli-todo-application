module Todo.Cli.Program

open Todo.ActionTree.ActionTreeFunctions

[<EntryPoint>]
let main argv =
    
    let result = buildActionTree ()
    match result with
    | Ok actionTree -> printActionTree actionTree
    | Error error -> raise error
    
    0