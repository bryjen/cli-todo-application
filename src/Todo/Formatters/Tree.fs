/// Contains functions and modules that parse an item group into some tree.
module Todo.Formatters.Tree

open Spectre.Console
open Spectre.Console.Prompts.Extensions
open Todo.Utilities.String
open Todo.ItemGroup
open Todo.ItemGroup.Utilities
open Todo.Formatters.Item.Default
open Todo.Formatters.ItemGroup.Default

/// Functions for printing the 'plain' version of the tree representation.
module PlainTree =
    
    let toLines (itemGroup: ItemGroup) : string list =
        
        let rec split (depth: int) (itemGroup: ItemGroup) : string list =
            let name = (formatItemGroup itemGroup) |> indent depth 
            let items = List.map (formatItem >> (indent (depth + 1)))  itemGroup.Items
            let subItemGroups = List.map (split (depth + 1)) itemGroup.SubItemGroups
            [ name ] @ items @ (List.collect id subItemGroups) 
        
        split 0 itemGroup
        
    let pairLinesWithObject (itemGroup: ItemGroup) : (string * ItemOrItemGroup) list =
        let lines = toLines itemGroup
        let objects = parseIntoDuList itemGroup
        List.zip lines objects
        
        
        
/// Functions for printing the 'pretty' version of the tree representation.
/// Mainly uses 'Spectre.Console''s 'Tree' class and rendering functionality.
module PrettyTree =
    
    /// <summary>
    /// Parses an item group into a Spectre.Console.Tree object.
    /// </summary>
    /// <param name="baseItemGroup">The item group to parse.</param>
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
        
    /// <summary>
    /// Parses an item group into rendered string lines.
    /// </summary>
    /// <param name="itemGroup">The item group to parse.</param>
    let toLines (itemGroup: ItemGroup) : string list =
        let asTree = toTree itemGroup
        let lines = List.ofSeq (AnsiBuilder.BuildLines(AnsiConsole.Console, asTree))
        
        // There is an extra line at the very end
        // Get all elements but the last
        List.rev lines |> List.tail |> List.rev 
        
    let pairLinesWithObject (itemGroup: ItemGroup) : (string * ItemOrItemGroup) list =
        let lines = toLines itemGroup
        let objects = parseIntoDuList itemGroup
        List.zip lines objects
