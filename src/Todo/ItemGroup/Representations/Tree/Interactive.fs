namespace Todo.ItemGroup.Representations.Tree

open System
open Spectre.Console
open Spectre.Console.Prompts.Extensions
open Todo.Widgets
open Todo.ItemGroup

/// <summary>
/// Module containing functions related to the <b>tree</b> interactive view. 
/// </summary>
module Interactive =
    
    // Displays information about an item to the console
    let private viewItem (item: Item) : unit =
        
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
        |> Table.render
    
    // Displays information about an item group to the console
    let private viewItemGroup (itemGroup: ItemGroup) : unit =
        
        // Get text to display
        let descriptionAsText = match itemGroup.Description with | Some value -> value | None -> ""
        
        let itemsAsText =
            itemGroup.Items
            |> List.map Converter.formatItem
            |> (fun itemStrings -> ("\n", itemStrings)) 
            |> String.Join
        
        let subItemGroupsAsText =
            itemGroup.SubItemGroups
            |> List.map Converter.formatItemGroup
            |> (fun itemGroupStrings -> ("\n", itemGroupStrings)) 
            |> String.Join
        
        // Render to console
        generateLine "Viewing a [blue]Todo group[/]"
        
        Spectre.Console.Table()
        |> Table.setDefaultProperties
        |> Table.addColumns 2u
        |> Table.addRows   [ "Name"; itemGroup.Name]                   ""
        |> Table.addRows   [ "Description"; descriptionAsText]         "[grey]No description provided[/]"
        |> Table.addRows   [ "Items"; itemsAsText]                     "[grey]This item group has no items.[/]"
        |> Table.addRows   [ "Sub Item Groups"; subItemGroupsAsText]   "[grey]This item group has no sub item group.[/]"
        |> Table.render 
    
    let treeInteractive
        (rootItemGroup: ItemGroup)
        (converter: ItemGroup -> string list)
        : ItemGroup =
            
        let toReturnValues = Converter.parseIntoDuList >> (fun lst -> ResizeArray(lst))    
        let selectionPrompt = TreeSelectionPrompt<ItemGroup, ItemOrItemGroup>(Converter.PrettyTree.toTree, toReturnValues, rootItemGroup)
        let selectedObject = selectionPrompt.Show(AnsiConsole.Console) 
            
        match selectedObject with
        | Item item ->
            viewItem item 
        | ItemGroup itemGroup ->
            viewItemGroup itemGroup
            
        System.Console.ReadLine() |> ignore 
            
        rootItemGroup