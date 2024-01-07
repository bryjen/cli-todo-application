namespace Todo.ItemGroup.Representations.Tree

open Spectre.Console
open Spectre.Console.Prompts.Extensions

open Todo.ItemGroup

/// Provides functions that convert ItemGroups/Items into strings.
/// Specifically used for creating the tree representation of an ItemGroup.
[<RequireQualifiedAccess>]
module Converter =
    
    // Prepends the provided string with indentation.
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
    
    
    /// <summary>
    /// Expands an item group then lists each item/sub item group 's type in a list.
    /// </summary>
    /// <example>
    /// <p>
    /// Consider the following item group tree representation
    /// </p>
    /// <code>
    /// [University]                                                // ItemGroup 
    ///     DUE THIS WEEK Finish report                             // Item
    ///     [COMP 345]                                              // ItemGroup
    ///         DUE IN 4 DAYS Programming assignment 1              // Item
    ///         DUE IN 3 DAYS Theory assignment 2                   // Item
    ///     [COMP 353]                                              // ItemGroup
    ///         DUE THIS WEEK Cover material                        // Item
    /// </code>
    /// <p>
    /// <br></br>
    /// Suppose we pass the uppermost item group record, the following list is returned:
    /// <code>
    /// [ItemOrItemGroup.ItemGroup; ItemOrItemGroup.Item; ItemOrItemGroup.ItemGroup; ItemOrItemGroup.Item;
    /// ItemOrItemGroup.Item; ItemOrItemGroup.ItemGroup; ItemOrItemGroup.Item ] 
    /// </code>
    /// </p>
    /// </example>
    let parseIntoDuList (baseItemGroup: ItemGroup) : ItemOrItemGroup list =
        
        let rec split (itemGroup: ItemGroup) : ItemOrItemGroup list =
            let parentItemGroup = ItemOrItemGroup.ItemGroup itemGroup 
            let items = List.map ItemOrItemGroup.Item itemGroup.Items
            let subItemGroups = List.map split itemGroup.SubItemGroups
            [ parentItemGroup ] @ items @ (List.collect id subItemGroups) 
        
        split baseItemGroup 
     
   
    
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