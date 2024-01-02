namespace Todo.Cli.Commands.Arguments

open Argu

[<CliPrefix(CliPrefix.None)>]
type LabelArguments =
    | Add of labelName:string 
    | Remove of labelName:string
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Add _ -> "Adds a label to an item group"
            | Remove _ -> "Removes a label from an item group"


type ItemGroupArguments =
    | [<Last; CliPrefix(CliPrefix.None)>] Label of ParseResults<LabelArguments>
    | [<Last>] Name of newName:string 
    | [<Last>] Description of newDesc:string 
    | [<Unique; Mandatory; AltCommandLine("-p")>] Path of path:string list
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Label _ -> "Subcommand to either 'add' or 'remove' a label from an item group."
            | Name _ -> "Subcommand change the name of an item group."
            | Description _ -> "Subcommand change the description of an item group."
            | Path _ -> "The path of the item group"


[<CliPrefix(CliPrefix.None)>]
type EditArguments =
    | Item_Group of ParseResults<ItemGroupArguments>
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Item_Group _ -> "Edit an item group."