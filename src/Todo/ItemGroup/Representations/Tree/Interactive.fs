namespace Todo.ItemGroup.Representations.Tree

open System
open Spectre.Console

open Spectre.Console.Prompts.Extensions
open Todo.ItemGroup
open Todo.Utilities

module Interactive =
    
    // TODO: Proof of concept, fully flesh out
    let viewItem (item: Item) : unit =
        let title = Rule("Viewing an [blue]Todo[/]")
        AnsiConsole.Write(title)
        
        let table = Table()
        table.Caption <- TableTitle("Press 'ENTER' to continue")
        table.ShowHeaders <- false
        
        // Adds headers, no text since they aren't shown anyways 
        table.AddColumn("") |> ignore
        table.AddColumn("{}") |> ignore
      
        // Row about the item's "name" 
        table.AddRow("Name", $"{item.Name}") |> ignore 
        table.AddEmptyRow() |> ignore
        
        // Row about the item's "description" 
        let descriptionText = match item.Description with | Some desc -> desc | None -> "[grey]No description provided[/]"
        table.AddRow("Description", descriptionText) |> ignore
        table.AddEmptyRow() |> ignore
        
        // Row about the item's "labels" 
        let formattedLabels = Label.FormatLabels item.Labels
        let labelsText = match String.IsNullOrWhiteSpace formattedLabels with
                         | true -> "[grey]Item has no labels[/]"
                         | false -> formattedLabels
        table.AddRow("Labels", labelsText) |> ignore
        table.AddEmptyRow() |> ignore
        
        // Row about the item's "due date" 
        let dueDateText = Item.GetFormattedDueDate item 
        table.AddRow("Due date", "[red]" + dueDateText + "[/]") |> ignore
        
        // Alignment and rendering/writing
        let tableAligned = Align(table, HorizontalAlignment.Center, VerticalAlignment.Middle)
        AnsiConsole.Write(tableAligned)
        ()
    
    // TODO: Proof of concept, fully flesh out
    let viewItemGroup (itemGroup: ItemGroup) : unit =
        let layout = Layout("Root")
        
        // Create partitions
        layout.SplitRows(Layout("Top"), Layout("Bot")) |> ignore
        layout["Bot"].SplitColumns(Layout("Left"), Layout("Right")) |> ignore
        
        // Configuring partitions
        layout["Bot"].Ratio <- 2
        
        // Text for the top row
        let labelsMarkup = (Label.FormatLabels itemGroup.Labels)
        let description =
            match itemGroup.Description with
            | Some value -> "Description:\n" + value
            | None -> "No description provided." 
        let topMarkupText = Markup($"[blue]{itemGroup.Name}[/]    {labelsMarkup}\n{itemGroup.Path}\n\n{description}")
        let topPanel = Panel(Align.Left(topMarkupText, VerticalAlignment.Top))
        topPanel.Expand <- true
        layout["Top"].Update(topPanel) |> ignore
        
        // Text for the left column
        let itemsText =
            itemGroup.Items
            |> List.map Converter.formatItem
            |> (fun itemStrings -> ("\n", itemStrings)) // first part of tuple is the joining character
            |> String.Join
        let leftMarkupText = Markup($"Items:\n\n{itemsText}")
        let leftPanel = Panel(Align.Left(leftMarkupText, VerticalAlignment.Top))
        leftPanel.Expand <- true
        layout["Left"].Update(leftPanel) |> ignore
       
        // Text for the right column 
        let subItemGroupsText =
            itemGroup.SubItemGroups
            |> List.map Converter.formatItemGroup
            |> (fun itemGroupStrings -> ("\n", itemGroupStrings)) // first part of tuple is the joining character
            |> String.Join
        let rightMarkupText = Markup($"Sub- item groups:\n\n{subItemGroupsText}")
        let rightPanel = Panel(Align.Left(rightMarkupText, VerticalAlignment.Top))
        rightPanel.Expand <- true
        layout["Right"].Update(rightPanel) |> ignore 
        
        AnsiConsole.Write(layout)
        ()
    
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