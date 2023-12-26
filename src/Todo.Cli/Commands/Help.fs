module Todo.Cli.Commands.Help

open Spectre.Console
open Todo
open Todo.Utilities.Attributes.Command

let printCommandsHelp (_: string array) : unit =
    let commandMap = match Command.getCommandMap () with | Ok commandMap -> commandMap | Error ex -> raise ex
    let commandConfigs = commandMap.Configs
    
    let grid = Grid().AddColumn().AddColumn()

    let gridWithValues =
        commandConfigs
        |> List.map (fun (config: Command.Config) -> [|config.Command; config.Help|])
        |> List.fold (fun (grid: Grid) (row: string array) -> grid.AddRow row) grid
    
    AnsiConsole.Write gridWithValues

[<CommandInformation>]
let ``'help' Command Config`` () : Command.Config =
    { Command = "help"
      Help = "Prints help strings of each command."
      Function = CommandFunction.NoDataChange printCommandsHelp}