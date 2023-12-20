namespace Todo.Cli.Modules

open Todo.Attributes.ActionTree

[<ActionModule("list")>]
module List =
    
    [<DefaultActionFunction>]
    [<ActionFunction("", "")>]
    let interactive
        (argv: string array)
        : unit =
        ()

    [<ActionFunction("create", "Create a new todo")>]
    let create  (argv: string array) =
        ()
        
    [<ActionFunction("delete", "Delete a new todo")>]
    let delete (argv: string array) =
        ()
        
    [<ActionFunction("summary", "View summary of todos")>]
    let summary (argv: string array) =
        ()