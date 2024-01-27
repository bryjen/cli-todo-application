namespace Todo.Cli.Commands.Arguments

open Argu

[<CliPrefix(CliPrefix.None)>]
type DeleteArguments =
    | Item_Group of ParseResults<DeleteItemGroupArguments>
    | Item of ParseResults<DeleteItemArguments>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Item_Group _ -> "Deletes an item group."
            | Item _ -> "Deletes an item."
            
//  BUG: What looks like to be some Argu parsing bug: cannot parse 'Path' if it is the sole argument
and DeleteItemGroupArguments =
    | [<Hidden>] Hidden 
    | [<ExactlyOnce; AltCommandLine("-p")>] Path of path:string list
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Hidden -> ""
            | Path _ -> "The path of the item group to delete."
    
and DeleteItemArguments =
    | [<Hidden>] Hidden 
    | [<Unique; Mandatory; AltCommandLine("-p")>] Path of path:string list
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Hidden -> ""
            | Path _ -> "The path of the item to delete. The last token is the item, and the tokens before that are item groups.\n" +
                        "Ex. [\"University\"; \"Assignment #1\"] will delete the item 'Assignment #1' in the item group 'University'" 