namespace Todo.Cli.Commands.Arguments

open Argu
open Todo.Formatters

type ViewArguments =
    | [<AltCommandLine("-i")>]
        Interactive
    | [<AltCommandLine("-d", "-f", "-t")>]
        Type of view_type:ParseResults<DisplayFormatArguments>
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Interactive -> "Makes it so that the display is interactive."
            | Type _ -> "Determines how the todo information will be displayed."

and DisplayFormatArguments =
    | [<CliPrefix(CliPrefix.None)>]
        Plain_Tree 
    | [<CliPrefix(CliPrefix.None)>]
        Pretty_Tree 
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Plain_Tree -> "Display todo information in a 'plain' tree format." 
            | Pretty_Tree -> "Display todo information in a 'pretty' tree format." 
        
    static member ToDisplayFormatDU (displayFormat: DisplayFormatArguments) =
        match displayFormat with
        | Plain_Tree -> DisplayFormat.PlainTree 
        | Pretty_Tree -> DisplayFormat.PrettyTree 
