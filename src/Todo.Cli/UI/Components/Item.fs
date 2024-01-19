/// Contains functions for displaying information about an item to the console.
module Todo.UI.Components.Item

open Todo.ItemGroup
open Todo.UI.Components.Widgets

let viewItem (item: Item) : unit =
    
    // Get text to display
    let descriptionAsText = match item.Description with | Some value -> value | None -> ""
    let labelsAsText = Label.FormatLabels item.Labels
    let dueDateAsText = "[red]" + (Item.GetFormattedDueDate item) + "[/]"
    
    // Render to console
    generateLine "Viewing an [blue]Todo[/]"
    
    Spectre.Console.Table()
    |> Table.setDefaultProperties
    |> Table.addColumns 2u
    |> Table.addRows   [ "Name"; $"{item.Name}" ]             ""
    |> Table.addRows   [ "Description"; descriptionAsText ]   "[grey]No description provided[/]"
    |> Table.addRows   [ "Labels"; labelsAsText ]             "[grey]Item has no labels[/]"
    |> Table.addRows   [ "Due Date"; dueDateAsText ]          ""
    |> Table.addRows   [ "Path"; item.Path ]          ""
    |> Table.render