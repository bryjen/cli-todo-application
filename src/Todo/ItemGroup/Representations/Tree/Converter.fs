namespace Todo.ItemGroup.Representations.Tree

open Spectre.Console

open Todo.ItemGroup

/// Provides functions that convert ItemGroups/Items into strings.
/// Specifically used for creating the tree representation of an ItemGroup.
[<RequireQualifiedAccess>]
module Converter =
        
    let private indent (num: int) (str: string) : string =
        let indent = String.replicate 4 " "
        let totalIndents = String.replicate num indent
        totalIndents + str
            
    /// <summary>
    /// Returns a single line, string representation of an <b>Item</b>. 
    /// </summary>
    /// <remarks>
    /// Does <b>NOT</b> account an item group's <b>sub item groups AND items</b>.
    /// </remarks> 
    let internal formatItem (item: Item) : string =
        (* format as default to string *)
        item.ToString()
        
    /// <summary>
    /// Returns a single line, string representation of an <b>Item Group</b>. 
    /// </summary>
    /// <remarks>
    /// Does <b>NOT</b> account an item group's <b>sub item groups AND items</b>.
    /// </remarks> 
    let internal formatItemGroup (itemGroup: ItemGroup) : string =
        sprintf "[[%s]]" itemGroup.Name 

    let toLines (itemGroup: ItemGroup) : string list =
        
        let rec split (depth: int) (itemGroup: ItemGroup) : string list =
            let name = (formatItemGroup itemGroup) |> indent depth 
            let items = List.map (formatItem >> (indent (depth + 1)))  itemGroup.Items
            let subItemGroups = List.map (split (depth + 1)) itemGroup.SubItemGroups
            [ name ] @ items @ (List.collect id subItemGroups) 
        
        split 0 itemGroup
        
    let toTree (baseItemGroup: ItemGroup) : Spectre.Console.Tree =
        
        (* The root is a separate type from its children (Tree vs TreeNode).
           So we need to process sub item groups a little bit differently, hence the helper function. *)
        let rec toTreeHelper (itemGroup: ItemGroup) : Spectre.Console.TreeNode =
            let node = TreeNode(Text(formatItemGroup itemGroup))
            
            let itemNodes = List.map (fun (item: Item) -> TreeNode(Text(formatItem item))) itemGroup.Items
            for itemNode in itemNodes do
                node.AddNode(itemNode) |> ignore
                
            let itemGroupNodes = List.map (fun (itemGroup: ItemGroup) -> toTreeHelper itemGroup) itemGroup.SubItemGroups 
            for itemGroupNode in itemGroupNodes do
                node.AddNode(itemGroupNode) |> ignore
                
            node
        
        
        let root = Tree(Text(formatItemGroup baseItemGroup))
        root.Guide <- TreeGuide.Line
        root.Style <- Style.Plain
        
        let itemNodes = List.map (fun (item: Item) -> TreeNode(Text(formatItem item))) baseItemGroup.Items
        for itemNode in itemNodes do
            root.AddNode(itemNode) |> ignore
            
        let itemGroupNodes = List.map (fun (itemGroup: ItemGroup) -> toTreeHelper itemGroup) baseItemGroup.SubItemGroups 
        for itemGroupNode in itemGroupNodes do
            root.AddNode(itemGroupNode) |> ignore
            
        root