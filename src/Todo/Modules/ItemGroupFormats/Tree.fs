module Todo.Modules.ItemGroupFormats.Tree

open Spectre.Console
open Todo.Models
open Todo.Utils

(* Previously 'ParseIntoDuList' *)
let private toReturnValues (baseItemGroup: ItemGroup) : ItemOrItemGroup list =
    let rec split (itemGroup: ItemGroup) : ItemOrItemGroup list =
        let parentItemGroup = ItemOrItemGroup.ItemGroup itemGroup 
        let items = List.map ItemOrItemGroup.Item itemGroup.Items
        let subItemGroups = List.map split itemGroup.SubItemGroups
        [ parentItemGroup ] @ items @ (List.collect id subItemGroups) 

    split baseItemGroup

/// <summary>
/// Parses an item group into a <see cref="Spectre.Console.Tree"/> object.
/// </summary>
/// <param name="baseItemGroup">The item group to parse.</param>
let toTree (baseItemGroup: ItemGroup) : Spectre.Console.Tree =
    let formatItemGroup itemGroup =   
        $"[[{itemGroup.Name}]]"
    
    (* The root is a separate type from its children (Tree vs TreeNode).
       So we need to process sub item groups a little bit differently, hence the helper function. *)
    let rec toTreeHelper (itemGroup: ItemGroup) : Spectre.Console.TreeNode =
        let node = TreeNode(Text(formatItemGroup itemGroup))
        
        let itemNodes = List.map (fun (item: Item) -> TreeNode(Text(item.ToString()))) itemGroup.Items
        for itemNode in itemNodes do
            node.AddNode(itemNode) |> ignore
            
        let itemGroupNodes = List.map (fun (itemGroup: ItemGroup) -> toTreeHelper itemGroup) itemGroup.SubItemGroups 
        for itemGroupNode in itemGroupNodes do
            node.AddNode(itemGroupNode) |> ignore
            
        node
    
    let root = Tree(Text(formatItemGroup baseItemGroup))
    root.Guide <- TreeGuide.Line
    root.Style <- Style.Plain
    
    let itemNodes = List.map (fun (item: Item) -> TreeNode(Text(item.ToString()))) baseItemGroup.Items
    for itemNode in itemNodes do
        root.AddNode(itemNode) |> ignore
        
    let itemGroupNodes = List.map (fun (itemGroup: ItemGroup) -> toTreeHelper itemGroup) baseItemGroup.SubItemGroups 
    for itemGroupNode in itemGroupNodes do
        root.AddNode(itemGroupNode) |> ignore
        
    root
    
/// Formats the passed 'ItemGroup' object into a tree. 
/// Has 'Spectre.Console' markup syntax which allows for rich text output to the console. 
let toMarkupString(itemGroup: ItemGroup) =
    let asTree = toTree itemGroup
    AnsiBuilder.buildRenderable AnsiConsole.Console asTree

/// Returns a 'SelectionPrompt' object with the tree formatting. 
let toSelectionPrompt(itemGroup: ItemGroup) =
    // lines
    let asTree = toTree itemGroup
    let unprocessedLines = AnsiBuilder.buildRenderableLines AnsiConsole.Console asTree
    let lines = List.rev unprocessedLines |> List.tail |> List.rev
    
    // return values
    let returnValues = toReturnValues itemGroup
    
    let selectionPrompt = SelectionPrompt<(ItemOrItemGroup * string)>()
    selectionPrompt.PageSize <- 30
    selectionPrompt.Converter <- snd
    
    let pairs = List.zip returnValues lines
    selectionPrompt.AddChoices(pairs) |> ignore
    selectionPrompt