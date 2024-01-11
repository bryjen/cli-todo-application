// Module containing utilities related to labels/items/item groups.
module Todo.ItemGroup.Utilities

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