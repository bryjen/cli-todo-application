[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Help

open Todo
open Spectre.Console

// The config for the 'help' command WITHOUT the actual function to execute.
// Work around against circular dependency
let private tempHelpConfig: Command.Config =
    { Command = "help"
      Help = "Prints this list of help strings for each command."
      Function = CommandFunction.NoDataChange (fun _ -> ()) }

let private commandConfigs = [
    List.config
    Create.config
    Delete.config
    Edit.config
    tempHelpConfig
]
    
/// <summary>
/// Implements the logic for the 'help' command.
/// </summary>
let printCommandsHelp (_: string array) : unit =
    let grid = Grid().AddColumn().AddColumn()

    let gridWithValues =
        commandConfigs
        |> List.map (fun (config: Command.Config) -> [|config.Command; config.Help|])
        |> List.fold (fun (grid: Grid) (row: string array) -> grid.AddRow row) grid
    
    AnsiConsole.MarkupLine "\nApplication commands:"
    AnsiConsole.Write gridWithValues
    AnsiConsole.MarkupLine ""
    
let config : Command.Config =
      { tempHelpConfig with Function = CommandFunction.NoDataChange printCommandsHelp }