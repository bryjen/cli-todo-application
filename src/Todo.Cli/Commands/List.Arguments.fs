namespace Todo.Cli.Commands.Arguments

open Argu

/// <summary>
/// Arguments for the 'list' command 
/// </summary>
type ListArguments =
    | [<AltCommandLine("-i")>] Interactive
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Interactive -> "Makes it so that the display is interactive."
