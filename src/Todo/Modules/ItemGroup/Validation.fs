/// <summary>
/// Module containing functions used to <b>validate</b> <c>ItemGroup</c> objects.
/// </summary>
module Todo.Modules.ItemGroup.Validation

open System
open FsToolkit.ErrorHandling
open Todo.Utils
open Todo.Models

[<AutoOpen>]
module internal Assertions =
    
    // Asserts that an item group has a non-empty name.
    let assertNonEmptyName (itemGroup: ItemGroup) =
        let exceptionToReturn = Exception("An item group must have a non-empty name!")
        assertNonEmptyStringList [itemGroup.Name] exceptionToReturn
        
    // Asserts that an item group has sub item groups with non-empty names.
    let assertNonEmptySubItemGroupNames (itemGroup: ItemGroup) =
        let exceptionToReturn = Exception("An item group must have a non-empty name!")
        assertNonEmptyStringList (List.map (_.Name) itemGroup.SubItemGroups) exceptionToReturn
        
    // Assert that an item group has items with non-empty names.
    let assertNonEmptyItemNames (itemGroup: ItemGroup) =
        let exceptionToReturn = Exception("An item must have a non-empty name!")
        let getItemName = (fun (item: Item) -> item.Name)
        assertNonEmptyStringList (List.map getItemName itemGroup.Items) exceptionToReturn
        
    // Assert that an item group has sub item groups with unique names.
    let assertUniqueSubItemGroupNames (itemGroup: ItemGroup) =
        let subItemGroupNames = List.map (_.Name) itemGroup.SubItemGroups
        
        match findFirstDuplicate subItemGroupNames with
        | Some duplicateName -> Error (Exception($"An item group with the name \"{duplicateName}\" already exists!")) 
        | None -> Ok () 
            
    // Assert that an item group has items with unique names.
    let assertUniqueItemNames (itemGroup: ItemGroup) =
        let getItemName = (fun (item: Item) -> item.Name)
        let itemNames = List.map getItemName itemGroup.Items
        
        match findFirstDuplicate itemNames with
        | Some duplicateName -> Error (Exception($"An item with the name \"{duplicateName}\" already exists!")) 
        | None -> Ok ()
    
/// Asserts that an 'ItemGroup' object is valid.
let validateItemGroup (itemGroup: ItemGroup) : Result<unit, Exception> =
    result {
        do! assertNonEmptyName itemGroup 
        do! assertNonEmptySubItemGroupNames itemGroup 
        do! assertNonEmptyItemNames itemGroup 
        do! assertUniqueSubItemGroupNames itemGroup 
        do! assertUniqueItemNames itemGroup 
    }
    
/// Asserts that a list of 'ItemGroup' objects are valid.
let validateItemGroups (itemGroups: ItemGroup list) : Result<unit, Exception> =
    let results = List.map validateItemGroup itemGroups
    
    match List.tryFind Result.isError results with
    | None -> Ok () 
    | Some err -> err 