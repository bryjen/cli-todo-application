/// Contains functions for displaying information about an item group to the console.
module Todo.UI.Components.ItemGroup

open System
open Todo
open Todo.ItemGroup
open Todo.UI.Components.Widgets


// Functions that take an item/item group and return some string representation 
let formatItem = Formatters.Item.Default.formatItem
let formatItemGroup = Formatters.ItemGroup.Default.formatItemGroup


let viewItemGroup (itemGroup: ItemGroup) : unit =
    
    // Get text to display
    let descriptionAsText = match itemGroup.Description with | Some value -> value | None -> ""
    
    let itemsAsText =
        itemGroup.Items
        |> List.map formatItem 
        |> (fun itemStrings -> ("\n", itemStrings)) 
        |> String.Join
    
    let subItemGroupsAsText =
        itemGroup.SubItemGroups
        |> List.map formatItemGroup 
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
    |> Table.addRows   [ "Path"; itemGroup.Path ]                  ""
    |> Table.render 