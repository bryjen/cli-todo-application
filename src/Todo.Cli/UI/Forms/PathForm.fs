module Todo.UI.Forms.PathForm

open Spectre.Console
open Todo.UI.Components.Widgets

/// Prompts the user for a path.
/// This path is used to navigate the tree of sub item groups.
let  promptForPath () : string list =
    
    // Just for display
    generateLine "Creating a [blue]Todo group[/]"
    printfn "\n"
    
    let promptMessage = "Please enter the [blue]path[/] where the new item group will be placed: (separate by commas)\n>\t"
    let rawPath = AnsiConsole.Ask<string>(promptMessage)
    
    rawPath
    |> (_.Split(","))
    |> Array.map (_.Trim())
    |> Array.toList 