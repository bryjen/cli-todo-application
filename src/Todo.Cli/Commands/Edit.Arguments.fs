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

type EditArguments =
    | [<CliPrefix(CliPrefix.None)>] Item_Group of ParseResults<ItemGroupArguments>
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Item_Group _ -> "Edit an item group."
            
and ItemGroupArguments =
    | [<Unique; Mandatory; AltCommandLine("-p")>] Path of path:string list
    | Label of ParseResults<LabelArguments>
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Path _ -> "The path of the item group"
            | Label _ -> "Subcommand to either 'add' or 'remove' a label from an item group."
