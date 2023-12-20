namespace Todo.Cli.Modules

open Todo.Attributes.ActionTree

[<DefaultActionModule>]
[<ActionModule("defaultentry")>]
module DefaultEntry =
    
    [<DefaultActionFunction>]
    [<ActionFunction("", "")>]
    let execute (argv: string array) =
        ()
        