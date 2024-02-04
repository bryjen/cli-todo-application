/// <summary>
/// Module containing functions used to <b>modify</b> <c>ItemGroup</c> objects.
/// </summary>
module Todo.Modules.ItemGroup.Modification

open System
open FsToolkit.ErrorHandling
open Todo.Models
open Todo.Modules.ItemGroup.Validation

[<AutoOpen>]
module private Helpers =
    
    // Replaces the base item group's sub item groups with the replacement (If their name matches the IDENTIFIER).
    let replaceSubItemGroupByIdentifier
        (baseItemGroup: ItemGroup)
        (replacement: ItemGroup)
        (identifier: string)
        : ItemGroup =
            
        let replacementFunction = (fun itemGroup -> if itemGroup.Name = identifier then replacement else itemGroup)
        let newSubItemGroups = List.map replacementFunction baseItemGroup.SubItemGroups
        { baseItemGroup with SubItemGroups = newSubItemGroups }
        
    // Replaces the base item group's sub item groups with the replacement (If they have the SAME NAME).
    let replaceSubItemGroup (baseItemGroup: ItemGroup) (replacement: ItemGroup) : ItemGroup =
        replaceSubItemGroupByIdentifier baseItemGroup replacement replacement.Name 

    // Recursively locates a specified (sub) item group.
    let rec locate (baseItemGroup: ItemGroup) (path: string list) : Result<ItemGroup, Exception> =
        match path with
        | [] ->
            Ok baseItemGroup
        | requiredName :: rest -> 
            match List.tryFind (fun itemGroup -> itemGroup.Name = requiredName) baseItemGroup.SubItemGroups with
            | Some subItemGroup ->
                locate subItemGroup rest
            | None ->
                let message = sprintf $"Could not find the sub item group \"%s{requiredName}\" in the item group \"%s{baseItemGroup.Name}\""
                Error (Exception(message))

    // Recursively replaces the indicated sub item group with its replacement.
    // Reflects a change to a single (sub) item group.
    let rec update (baseItemGroup: ItemGroup) (path: string list) (replacement: ItemGroup) : ItemGroup =
        
        // ** DOES NOT HANDLE ERRORS FROM INVALID PATH. Error meant to be handled from a previous call to 'Locate' **
        
        match path with
        | [] ->
            replacement
        | [requiredName] ->
            let newSubItemGroups = List.map (fun itemGroup -> if itemGroup.Name = requiredName then replacement else itemGroup) baseItemGroup.SubItemGroups
            { baseItemGroup with SubItemGroups = newSubItemGroups }
        | requiredName :: rest ->
            let subItemGroup = List.find (fun itemGroup -> itemGroup.Name = requiredName) baseItemGroup.SubItemGroups
            replaceSubItemGroup baseItemGroup (update subItemGroup rest replacement)
            
    // Adds a sub item group to the passed item group.
    let addItemGroup newItemGroup baseItemGroup =
        let newSubItemGroups = newItemGroup :: baseItemGroup.SubItemGroups
        { baseItemGroup with SubItemGroups = newSubItemGroups }
        
    // Attempts to remove a sub item group from the passed item group.
    let removeItemGroup name baseItemGroup =
        let newSubItemGroups = List.filter (fun itemGroup -> itemGroup.Name <> name) baseItemGroup.SubItemGroups
        
        match (List.length newSubItemGroups) = (List.length baseItemGroup.SubItemGroups) with
        | true -> Error (Exception(sprintf $"Could not find the sub-item group with the name %s{name}")) 
        | false -> Ok { baseItemGroup with SubItemGroups = newSubItemGroups } 
        
    // Adds an item to the passed item group.
    let addItem newItem baseItemGroup =
        let newItems = newItem :: baseItemGroup.Items
        { baseItemGroup with Items = newItems }
        
    // Attempts to remove an item from the passed item group.
    let removeItem name baseItemGroup =
        let newItems = List.filter (fun (item: Item) -> item.Name <> name) baseItemGroup.Items
        
        match (List.length newItems) = (List.length baseItemGroup.Items) with
        | true -> Error (Exception(sprintf $"Could not find the sub-item group with the name %s{name}")) 
        | false -> Ok { baseItemGroup with Items = newItems }
        
    // Attempts to rename a specified sub item group.
    let renameSubItemGroup subItemGroupName newName baseItemGroup =
        
        (* Note that we do not change the name of the base item group itself because since the replacement algorithm
           in 'Locate' and 'Update' that the modified item group has the same name - otherwise they break. *)
        
        match List.tryFind (fun itemGroup -> itemGroup.Name = subItemGroupName) baseItemGroup.SubItemGroups with
        | Some itemGroup ->
            let oldName = itemGroup.Name
            let newItemGroup = { itemGroup with Name = newName }
            Ok (replaceSubItemGroupByIdentifier baseItemGroup newItemGroup oldName)
        | None ->
            Error (Exception(sprintf $"Could not find the sub item group \"%s{subItemGroupName}\""))

    // Changes the description of the passed item group.
    let changeDescription newDescription (baseItemGroup: ItemGroup) =
        { baseItemGroup with Description = Some newDescription }
            
/// <summary>
/// Modifies an item group.
/// </summary>
/// <param name="itemGroup">The item group to modify.</param>
/// <param name="modify">A function that modifies an item group. <c>ItemGroup -> Result&lt;ItemGroup, Exception&gt;</c></param>
/// <param name="path">The 'path' the item group to modify.</param>
/// <example>
/// <code>
/// let modify = ItemGroup.AddItem newItem
/// let modifyResult = ItemGroup.Modify someItemGroup modify ["University"; "COMP 345"; "Assignments"]
/// <br></br>
/// match modifyResult with
/// | Ok newItemGroup -> ... // do something with new item group
/// | Error ex -> ...        // do something with exception
/// </code>
/// <br></br>
/// <br></br>
/// In the above example, we attempt to add a new item to our item group <c>someItemGroup</c>. <c>ItemGroup</c>
/// provides some functions for modifying item groups by default. See <see cref="ItemGroup.AddItemGroup"/>,
/// <see cref="ItemGroup.RemoveItemGroup"/>, <see cref="ItemGroup.AddItem"/>, <see cref="ItemGroup.RemoveItem"/>
/// , etc.
/// </example>
let modifyItemGroup 
    (itemGroup: ItemGroup)
    (modify: ItemGroup -> Result<ItemGroup, Exception>)
    (path: string list)
    : Result<ItemGroup, Exception> =
        
    result {
        let! requiredItemGroup = locate itemGroup path
        let! modifiedItemGroup = modify requiredItemGroup
        return update itemGroup path modifiedItemGroup
    }
 
/// Attempts to add a new item group to the specified path.
let tryAddNewItemGroup baseItemGroup newItemGroup path =
    result {
        let modify = (fun itemGroup -> Ok (Helpers.addItemGroup newItemGroup itemGroup)) // Wrap result in a 'Ok' type 
        let! newItemGroup = modifyItemGroup baseItemGroup modify path
        do! validateItemGroup newItemGroup
        return newItemGroup
    }
    
/// Attempts to remove an item group from the specified path.
let tryRemoveItemGroup baseItemGroup (toDelete: string) path =
    result {
        let modify = Helpers.removeItemGroup toDelete
        let! newItemGroup = modifyItemGroup baseItemGroup modify path
        do! validateItemGroup newItemGroup
        return newItemGroup
    }
    
/// Attempts to add an item to the specified item group.
let tryAddItem baseItemGroup newItem path =
    result {
        let modify = (fun itemGroup -> Ok (Helpers.addItem newItem itemGroup)) // Wrap result in a 'Ok' type 
        let! newItemGroup = modifyItemGroup baseItemGroup modify path
        do! validateItemGroup newItemGroup
        return newItemGroup
    }
    
/// Attempts to remove an item from the specified item group.
let tryRemoveItem baseItemGroup (toDelete: string) path =
    result {
        let modify = Helpers.removeItem toDelete 
        let! newItemGroup = modifyItemGroup baseItemGroup modify path
        do! validateItemGroup newItemGroup
        return newItemGroup
    }
    
/// Attempts to rename a specified sub item group.
let tryRenameSubItemGroup baseItemGroup (toRename: string) newName path =
    result {
        let modify = Helpers.renameSubItemGroup toRename newName 
        let! newItemGroup = modifyItemGroup baseItemGroup modify path
        do! validateItemGroup newItemGroup
        return newItemGroup
    }
    
/// Attempts to change the description of a specified item group.
let tryChangeDescription baseItemGroup newDescription path =
    result {
        let modify = (fun itemGroup -> Ok (Helpers.changeDescription newDescription itemGroup)) // Wrap result in a 'Ok' type 
        let! newItemGroup = modifyItemGroup baseItemGroup modify path
        do! validateItemGroup newItemGroup
        return newItemGroup
    }