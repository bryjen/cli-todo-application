module Todo.UI.Forms.LabelSelectionForm

open Spectre.Console
open Todo.ItemGroup

/// Prompts the user what labels they want from a list of labels.
let promptForLabels (labels: Label list) : Label list =
    
    let multiSelectionPrompt = MultiSelectionPrompt<Label>()
    multiSelectionPrompt.Title <- "Please select the [blue]labels[/] you wish to select."
    multiSelectionPrompt.Required <- false
    multiSelectionPrompt.PageSize <- 50     // some high number
    multiSelectionPrompt.MoreChoicesText <- "[grey](Move up and down to reveal more fruits)[/]"
    multiSelectionPrompt.InstructionsText <- "[grey](Press [blue]<space>[/] to toggle a fruit, [green]<enter>[/] to accept)[/]"
    multiSelectionPrompt.Converter <- (fun label -> label.Name)
    multiSelectionPrompt.AddChoices(labels) |> ignore
    
    let selectedChoices = AnsiConsole.Prompt(multiSelectionPrompt.AddChoices(labels))
    List.ofSeq selectedChoices