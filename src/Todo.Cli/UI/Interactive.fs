/// Module contains functions related to processing actions in the interactive view.
module Todo.Interactive

open Spectre.Console
open Spectre.Console.Prompts.Extensions
open Todo.ItemGroup
open Todo.ItemGroup.Utilities
open Todo.Formatters.Tree

// UI functions that take an item/item group and display it to the console
let viewItem = Todo.UI.Components.Item.viewItem
let viewItemGroup = Todo.UI.Components.ItemGroup.viewItemGroup

let treeInteractive
    (rootItemGroup: ItemGroup)
    (converter: ItemGroup -> string list)
    : ItemGroup =
        
    let toReturnValues = parseIntoDuList >> (fun lst -> ResizeArray(lst))    
    let selectionPrompt = TreeSelectionPrompt<ItemGroup, ItemOrItemGroup>(PrettyTree.toTree, toReturnValues, rootItemGroup)
    let selectedObject = selectionPrompt.Show(AnsiConsole.Console) 
        
    match selectedObject with
    | Item item ->
        viewItem item 
    | ItemGroup itemGroup ->
        viewItemGroup itemGroup
        
    System.Console.ReadLine() |> ignore 
        
    rootItemGroup
