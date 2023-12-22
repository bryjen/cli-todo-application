[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.List

open Todo
open Todo.Attributes.Command

let display
    (argv: string array)
    : unit =
    printf "You just executed list!"
    ()

[<CommandInformation>]
let command_data
    ()
    : Command.Config =
        
    { Command = "list"
      Description = "Displays the list of todos."
      Help = None
      Function = CommandFunction.NoDataChange display}