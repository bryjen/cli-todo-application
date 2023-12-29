// Execute dotnet fsi from the solution folder
// dotnet fsi --use:"scripts/console_commands.fsx"

#load "references.fsx"

open System
open Todo.Cli.Commands

let list ([<ParamArray>] argv: string array) =
    List.execute argv

let create ([<ParamArray>] argv: string array) =
    Create.execute argv

let delete ([<ParamArray>] argv: string array) =
    Delete.execute argv

let help ([<ParamArray>] argv: string array) =
    Help.printCommandsHelp argv