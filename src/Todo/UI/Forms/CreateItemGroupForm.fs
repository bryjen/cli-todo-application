[<RequireQualifiedAccess>]
module Todo.UI.Forms.CreateItemGroupForm

open Spectre.Console
open Todo.ItemGroup
open Todo.UI.Components.Widgets

let private promptForName () : string =
    AnsiConsole.Ask<string>("Name:\t\t")
    
let private promptForDescription () : string =
    let textPrompt = TextPrompt<string>("[grey][[Optional]][/]Description:\t")
    textPrompt.AllowEmpty <- true
    
    AnsiConsole.Prompt(textPrompt)
    
/// <summary>
/// Prompts the user for details about an item group.
/// </summary>
/// <remarks>
/// The created item group has <b>NO path, and NO items</b>.
/// </remarks>
let displayForm () : ItemGroup =
    
    // Just for display
    generateLine "Creating a [blue]Todo group[/]"
    printfn "\n"
    
    ItemGroup.Default