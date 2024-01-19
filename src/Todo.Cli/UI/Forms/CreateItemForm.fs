[<RequireQualifiedAccess>]
module Todo.Cli.UI.Forms.CreateItemForm

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
/// Prompts the user for details about an item.
/// </summary>
/// <remarks>
/// The created item has <b>NO path</b>.
/// </remarks>
let displayForm () : Item =
    
    // Just for display
    generateLine "Creating a [blue]Todo item[/]"
    printfn "\n"
    
    let name = promptForName ()
    let description = promptForDescription ()
    
    { Item.Default with Name = name; Description = Some description }
