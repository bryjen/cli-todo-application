[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.List

open Spectre.Console
open Todo
open Todo.Cli.Utilities
open Todo.Utilities.Attributes.Command

let display (argv: string array) : unit =
    let appData =
        match Files.loadAppData Files.filePath with
        | Ok appData -> appData
        | Error err -> raise err
        
    appData.ItemGroups
    |> List.map ItemGroupFunctions.itemGroupToString 
    |> List.map (fun str -> AnsiConsole.MarkupLine($"%s{str}")) 
    |> ignore
    ()

[<CommandInformation>]
let command_data
    ()
    : Command.Config =
        
    { Command = "list"
      Description = "Displays the list of todos."
      Help = None
      Function = CommandFunction.NoDataChange display}