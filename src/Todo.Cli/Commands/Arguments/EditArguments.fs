namespace Todo.Cli.Commands.Arguments

open Argu

[<CliPrefix(CliPrefix.None)>]
type EditArguments =
    | Item_Group of ParseResults<ItemGroupArguments>
    | Item of ParseResults<ItemArguments>
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Item_Group _ -> "Edit a todo group."
            | Item _ -> "Edit a todo."
            
and ItemGroupArguments =
    | [<Last; CliPrefix(CliPrefix.None)>] Label of ParseResults<LabelArguments>
    | [<Last>] Name of newName:string 
    | [<Last>] Description of newDesc:string 
    | [<Unique; Mandatory; AltCommandLine("-p")>] Path of path:string list
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Label _ -> "Subcommand to either 'add' or 'remove' a label from a todo group."
            | Name _ -> "Subcommand to change the name of todo group."
            | Description _ -> "Subcommand to change the description of a todo group."
            | Path _ -> "The path of the todo group."

and ItemArguments =
    | [<Last; CliPrefix(CliPrefix.None)>] Label of ParseResults<LabelArguments>
    | [<Last>] Name of newName:string 
    | [<Last>] Description of newDesc:string 
    | [<Unique; Mandatory; AltCommandLine("-p")>] Path of path:string list
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Label _ -> "Subcommand to either 'add' or 'remove' a label from a todo item."
            | Name _ -> "Subcommand to change the name of todo item."
            | Description _ -> "Subcommand to change the description of a todo item."
            | Path _ -> "The path of the todo item."
            
and LabelArguments =
    | [<CliPrefix(CliPrefix.None)>] Add of labelName:string 
    | [<CliPrefix(CliPrefix.None)>] Remove of labelName:string
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Add _ -> "Adds a label to an item group"
            | Remove _ -> "Removes a label from an item group"