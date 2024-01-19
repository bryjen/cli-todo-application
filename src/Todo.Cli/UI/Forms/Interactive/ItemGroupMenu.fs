[<RequireQualifiedAccess>]
module Todo.Cli.UI.Forms.Interactive.ItemGroupMenu

open Spectre.Console
open Todo.Utilities.DiscriminatedUnion

/// Represents the actions that a user can take, given they selected an item group in the interactive session.
type SelectedAction =
    | Quit
    | CreateNewItemGroup
    | DeleteThisItemGroup
    | DeleteSubItemGroup
    | CreateItem
    | DeleteItem
    
    /// Returns a function that converts a 'SelectedAction' DU into a string. This string representation is a prompt
    /// that is meant to be displayed to the user.
    static member Converter : SelectedAction -> string =
        fun selectedAction -> 
            match selectedAction with
            | Quit -> "Quit." 
            | CreateNewItemGroup -> "Create a new todo group." 
            | DeleteThisItemGroup -> "Delete this todo group."
            | DeleteSubItemGroup -> "Delete a sub item group."
            | CreateItem -> "Create an item." 
            | DeleteItem -> "Delete an item."
        
/// Prompts the user for their next action, after selecting an item group.
let promptUserForAction () =
    let selectionPrompt = SelectionPrompt<SelectedAction>()
    selectionPrompt.Converter <- SelectedAction.Converter
    
    let choices = getAllDUValues<SelectedAction> ()
    selectionPrompt.AddChoices(choices) |> ignore
    
    AnsiConsole.Prompt(selectionPrompt) 