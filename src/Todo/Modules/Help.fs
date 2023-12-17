module Todo.Modules.Help

open Todo.Core.Utilities.Attributes.ActionTree

[<ActionModule("help")>]
module Help =
    
    [<DefaultActionFunction>]
    [<ActionFunction("INTERACTIVE", "")>]
    let interactive (argv: string array) =
        ()
        
        
    [<ActionFunction("about", "")>]
    let about (argv: string array) =
        ()

