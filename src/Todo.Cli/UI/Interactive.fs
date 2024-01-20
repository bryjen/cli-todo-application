/// Module contains functions related to processing actions in the interactive view.
module Todo.Interactive

open System
open FsToolkit.ErrorHandling
open Spectre.Console
open Spectre.Console.Prompts.Extensions
open Todo.Cli.UI.Forms
open Todo.Utilities
open Todo.ItemGroup
open Todo.ItemGroup.Utilities
open Todo.Formatters.Tree
open Todo.UI.Forms
open Todo.Cli.UI.Forms.Interactive

// UI functions that take an item/item group and display it to the console
let private viewItem = Todo.UI.Components.Item.viewItem
let private viewItemGroup = Todo.UI.Components.ItemGroup.viewItemGroup



/// Displays and handles the interactive view.
let rec interactive (appData: AppData) : AppData =
    let rootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    
    // Creating the tree selection prompt
    let itemGroupToReturnValues = parseIntoDuList >> (fun lst -> ResizeArray(lst)) 
    let selectionPrompt = TreeSelectionPrompt<ItemGroup, ItemOrItemGroup>(PrettyTree.toTree, itemGroupToReturnValues, rootItemGroup)
    
    let selectedObject = selectionPrompt.Show(AnsiConsole.Console) 
        
    let actionResult = 
        match selectedObject with
        | Item item ->
            viewItem item
            let selectedAction = ItemMenu.promptUserForAction ()
            handleItemAction appData item selectedAction
        | ItemGroup itemGroup ->
            viewItemGroup itemGroup
            let selectedAction = ItemGroupMenu.promptUserForAction ()
            handleItemMenuAction appData itemGroup selectedAction

    match actionResult with
    | Ok newAppData ->
        newAppData
    | Error err ->
        printfn "%A" err 
        appData

/// Handle the selected action in the item group menu.
and handleItemMenuAction
    (appData: AppData)
    (selectedItemGroup: ItemGroup)
    (selectedAction: ItemGroupMenu.SelectedAction)
    : Result<AppData, Exception> =
        
    let rootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    let path = selectedItemGroup.Path.Split('/') |> Array.toList 
    
    result {
        match selectedAction with
        | ItemGroupMenu.Quit ->
            return appData
        | ItemGroupMenu.ReturnToView ->
            // Recursive call to the main interactive function
            return interactive appData 
        | ItemGroupMenu.CreateNewItemGroup ->
            // Create the new item group object
            let tempItemGroup = CreateItemGroupForm.displayForm ()
            let newPath = path @ [ tempItemGroup.Name ] |> List.toArray
            let newItemGroup = { tempItemGroup with Path = String.Join('/', newPath) }
            
            // Attempt to insert it into existing structure
            let modify = ItemGroup.AddItemGroup newItemGroup
            let! newRootItemGroup = ItemGroup.Modify rootItemGroup modify path
            return { appData with ItemGroups = newRootItemGroup.SubItemGroups }
        | ItemGroupMenu.DeleteThisItemGroup -> 
            let (toRemove, adjustedPath) = List.splitLast path
            let modify = ItemGroup.RemoveItemGroup toRemove 
            let! newRootItemGroup = ItemGroup.Modify rootItemGroup modify adjustedPath 
            return { appData with ItemGroups = newRootItemGroup.SubItemGroups }
        | ItemGroupMenu.DeleteSubItemGroup ->
            let toRemove = AnsiConsole.Ask<string>("What [blue]item group[/] do you want to delete:\t");
            let modify = ItemGroup.RemoveItemGroup toRemove 
            let! newRootItemGroup = ItemGroup.Modify rootItemGroup modify path 
            return { appData with ItemGroups = newRootItemGroup.SubItemGroups }
        | ItemGroupMenu.CreateItem ->
            // Create the new item object
            let tempItem = CreateItemForm.displayForm ()
            let newItem = { tempItem with Path = String.Join('/', path) }
            
            // Attempt to insert it into existing structure
            let modify = ItemGroup.AddItem newItem
            let! newRootItemGroup = ItemGroup.Modify rootItemGroup modify path
            return { appData with ItemGroups = newRootItemGroup.SubItemGroups }
        | ItemGroupMenu.DeleteItem ->
            let toRemove = AnsiConsole.Ask<string>("What [blue]item[/] do you want to delete:\t");
            let modify = ItemGroup.RemoveItem toRemove 
            let! newRootItemGroup = ItemGroup.Modify rootItemGroup modify path 
            return { appData with ItemGroups = newRootItemGroup.SubItemGroups }
    }

/// Handle the selected action in the item menu.
and handleItemAction 
    (appData: AppData)
    (selectedItem: Item)
    (selectedAction: ItemMenu.SelectedAction)
    : Result<AppData, Exception> =
        
    let rootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    let path = selectedItem.Path.Split('/') |> Array.toList
    
    result {
        match selectedAction with
        | ItemMenu.Quit ->
            return appData 
        | ItemMenu.ChangeDescription ->
            // Create a new item with altered description
            let newDescription = AnsiConsole.Ask<string>("New todo [blue]description[/]:\t")
            let alteredItem = { selectedItem with Description = Some newDescription }
            
            // Delete from existing structure
            let deleteModify = ItemGroup.RemoveItem selectedItem.Name
            let! tempRootItemGroup = ItemGroup.Modify rootItemGroup deleteModify path
            
            // Re-insert into existing structure
            let insertModify = ItemGroup.AddItem alteredItem
            let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup insertModify path
            return { appData with ItemGroups = newRootItemGroup.SubItemGroups }
    }
