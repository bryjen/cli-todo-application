module Todo.Modules.List

open Todo.Core.Utilities.Attributes.ActionTree

[<ActionModule("list")>]
module ListMenu =
    
    [<ActionFunction("create", "Create a new todo")>]
    let create (argv: string array) =
        ()
        
    [<ActionFunction("delete", "Delete a new todo")>]
    let delete (argv: string array) =
        ()
        
    [<ActionFunction("summary", "View summary of todos")>]
    let summary (argv: string array) =
        ()
