[<RequireQualifiedAccess>]
module Todo.Cli.UI.Forms.Interactive.ItemMenu

open Spectre.Console
open Todo.Utilities.DiscriminatedUnion

/// Represents the actions that a user can take, given they selected an item group in the interactive session.
type SelectedAction =
    | Quit
    | ChangeDescription
    
    /// Returns a function that converts a 'SelectedAction' DU into a string. This string representation is a prompt
    /// that is meant to be displayed to the user.
    static member Converter : SelectedAction -> string =
        fun selectedAction -> 
            match selectedAction with
            | Quit -> "Quit." 
            | ChangeDescription -> "Change todo description." 

/// Prompts the user for their next action, after selecting an item group.
let promptUserForAction () =
    let selectionPrompt = SelectionPrompt<SelectedAction>()
    selectionPrompt.Converter <- SelectedAction.Converter
    
    let choices = getAllDUValues<SelectedAction> ()
    selectionPrompt.AddChoices(choices) |> ignore
    
    AnsiConsole.Prompt(selectionPrompt) 