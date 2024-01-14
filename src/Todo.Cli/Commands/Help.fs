[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Help

open Spectre.Console
open Todo.Cli

// The config for the 'help' command WITHOUT the actual function to execute.
// Work around against circular dependency
let tempCommandTemplate : CommandTemplate =
    { CommandName = "help"
      HelpString = "Prints this list of help strings for each command."
      InjectData = (fun _ _ _ -> CommandFunction.NoDataChange (fun _ -> ())) }

let private commandConfigs : CommandTemplate list = [
    viewCommandTemplate
    createCommandTemplate
    deleteCommandTemplate
    editCommandTemplate
    tempCommandTemplate
]
    
/// <summary>
/// Implements the logic for the 'help' command.
/// </summary>
let printCommandsHelp (_: string array) : unit =
    let grid = Grid().AddColumn().AddColumn()

    let gridWithValues =
        commandConfigs
        |> List.map (fun commandTemplate -> [| commandTemplate.CommandName; commandTemplate.HelpString |])
        |> List.fold (fun (grid: Grid) (row: string array) -> grid.AddRow row) grid
    
    AnsiConsole.MarkupLine "\nApplication commands:"
    AnsiConsole.Write gridWithValues
    AnsiConsole.MarkupLine ""
    
let helpCommandTemplate : CommandTemplate =
      { tempCommandTemplate with InjectData = (fun _ _ _ -> CommandFunction.NoDataChange printCommandsHelp) }