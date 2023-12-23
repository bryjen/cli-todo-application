namespace Todo.Cli.Utilities

module Arguments =
    
    /// Splits argv into the command and its arguments
    let splitToCommandAndArgs (argv: string array) : (string * string array) option =
        match Array.length argv with
        | 0 -> None
        | 1 -> Some ((Array.head argv), Array.empty<string>)
        | _ -> Some ((Array.head argv), (Array.tail argv))
