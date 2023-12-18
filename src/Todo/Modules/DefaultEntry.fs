/// DefaultEntry.fs
///
/// Contains the module that defines the behavior when a user does not specify a specific action to perform.

module Todo.Modules.DefaultEntry

open Todo.Core.Utilities.Attributes.ActionTree

[<DefaultActionModule>]
module DefaultEntry =
    
    [<DefaultActionFunction>]
    [<ActionFunction("", "")>]
    let execute (argv: string array) =
        ()
        