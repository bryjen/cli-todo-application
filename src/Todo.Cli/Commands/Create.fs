﻿[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Create

open Todo
open Todo.Attributes.Command

//  Replace with app data
let create (argv: string array) : AppData =
    printf "You just executed create!"
    AppDataFunctions.defaultAppDat

[<CommandInformation>]
let command_data
    ()
    : Command.Config =
        
    { Command = "create"
      Description = "Creates something."
      Help = None
      Function = CommandFunction.ChangesData create}
