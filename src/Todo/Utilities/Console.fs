/// Console.fs
///
/// Contains functions that manipulate the console to provided formatted output, interactive prompts, etc.
/// Extensively uses the library 'spectreconsole' (https://github.com/spectreconsole/spectre.console).

module Todo.Utilities.Console

open System
open System.IO

open Spectre.Console

open System.Runtime.InteropServices


/// <summary>
/// Provides functions and types that help formatting console output and provide interactive behavior.
/// </summary>
module Console =
            
    
    let pressEnterToContinue () : unit =
        let textPrompt = TextPrompt<string>("Press [green]Enter[/] to continue:")
        textPrompt.IsSecret <- true
        textPrompt.AllowEmpty <- true
        textPrompt.Mask <- ' '
        
        AnsiConsole.Prompt(textPrompt) |> ignore
        
    let clearConsole () : unit =
        AnsiConsole.Clear()