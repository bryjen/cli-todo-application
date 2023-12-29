namespace Todo

open System
open Todo.Utilities
open FsToolkit.ErrorHandling

[<AutoOpen>]
type ItemGroup =
    { Name: string
      Description: string option
      SubItemGroups: ItemGroup list
      Items: Item list
      Labels: Label list }
    
    /// <summary>
    /// Default values for a <c>ItemGroup</c> record.
    /// </summary>
    static member Default =
        { Name = ""
          Description = None
          SubItemGroups = List.empty
          Items = List.empty
          Labels = List.empty }
        
    // Attempts to get an item group with the specified name from a list of item groups.
    static member private tryGetSubItemGroup (itemGroups: ItemGroup list) (name: string) =
        if (List.length itemGroups) = 0 then
            None
        else
            let head = List.head itemGroups
            if (head.Name = name) then (Some head) else (ItemGroup.tryGetSubItemGroup (List.tail itemGroups) name) 

    /// <summary>
    /// Returns the string representation of the <c>ItemGroup</c> record.
    /// </summary>
    override this.ToString () =
        let indentation (num: int) : string =
            String.replicate num "    "
        
        let rec formatItemGroup (itmGrp: ItemGroup) (depth: int) : string =
            let itmGrpString =
                (sprintf $"%s{indentation depth}[[%s{itmGrp.Name}]]\n")
                    +
                (itmGrp.Items
                 |> List.map (fun (item: Item) -> sprintf $"%s{indentation (depth + 1)}%s{item.ToString()}\n")
                 |> List.fold (fun acc str -> acc + str) "")
            
            if List.length itmGrp.SubItemGroups = 0 then
                itmGrpString
            else
                let subItemGroupsString =
                    itmGrp.SubItemGroups
                    |> List.map (fun subItemGroup -> formatItemGroup subItemGroup (depth + 1))  
                    |> List.fold (fun acc str -> acc + str) ""
                    
                itmGrpString + subItemGroupsString
    
        formatItemGroup this 0
        
    /// <summary>
    /// Attempts to get the item group with the specified path.
    /// </summary>
    /// <example>
    /// Passing the following path <c>["University"; "COMP 345"]</c> attempts to first find the item group named
    /// "University", then the item group "COMP 345" inside of that.
    /// </example> 
    member this.getSubItemGroup (path: string list) =
        if (List.length path) = 0 then
            Some this 
        else
            let nextItemGroup = ItemGroup.tryGetSubItemGroup this.SubItemGroups (List.head path)
            match nextItemGroup with
            | Some subItemGrp -> subItemGrp.getSubItemGroup (List.tail path)
            | None -> None
            
    // Searches this item group's sub item groups for one that has the same name as that of the replacement. Replaces
    // that item group, with the replacement.
    // If no sub item group matches, nothing happens.
    member private this.onReplacement (replacement: ItemGroup) : ItemGroup =
        let newSubList = List.map (fun itemGrp -> if itemGrp.Name = replacement.Name then replacement else itemGrp) this.SubItemGroups  // idk why use 'this', just works
        { this with SubItemGroups = newSubList }
            
    /// <summary>
    /// Attempts to add a sub- item group.
    /// </summary>
    /// <param name="newItemGroup">The new item group to add.</param>
    /// <param name="path">
    /// The location to put the item group. Ex. The path ["University"; "Courses"] indicates that there should be a top
    /// level item group named "University", and that it should have a sub item group "Courses". The new item group will
    /// be placed as a sub item group to "Courses".
    /// </param>
    member this.tryAddSubItemGroup (newItemGroup: ItemGroup) (path: string list) : ItemGroup option =
        if List.isEmpty path then
            let newSubList = newItemGroup :: this.SubItemGroups
            Some { this with SubItemGroups = newSubList }
        else 
            option {
                let matchingSubItemGroups = List.filter (fun itemGroup -> itemGroup.Name = (List.head path)) this.SubItemGroups
                let! subItemGroup = match List.length matchingSubItemGroups with | 1 -> Some (List.head matchingSubItemGroups) | _ -> None
                let! newSubItemGroup = subItemGroup.tryAddSubItemGroup newItemGroup path.Tail
                return this.onReplacement newSubItemGroup 
            }
        
    /// <summary>
    /// Attempts to delete a sub- item group.
    /// </summary>
    /// <param name="path">
    /// The location of the item group to delete. Ex. The path ["University"; "Courses"] indicates that there should be a top
    /// level item group named "University", and that it should have a sub item group "Courses". The item group "Courses" 
    /// is going to be deleted.
    /// </param>
    member this.tryDeleteSubItemGroup (path: string list) : ItemGroup option =
        if List.isEmpty path then
            None
        elif (List.length path = 1) then
            let newSubList = List.filter (fun itemGroup -> itemGroup.Name <> path.Head) this.SubItemGroups
            Some { this with SubItemGroups = newSubList }
        else
            option {
                let matchingSubItemGroups = List.filter (fun itemGroup -> itemGroup.Name = (List.head path)) this.SubItemGroups
                let! subItemGroup = match List.length matchingSubItemGroups with | 1 -> Some (List.head matchingSubItemGroups) | _ -> None
                let! newSubItemGroup = subItemGroup.tryDeleteSubItemGroup path.Tail
                return this.onReplacement newSubItemGroup 
            }
            
    // Adds the item to the item group's list of items.
    member private this.addItem (item: Item) : ItemGroup =
        let newItemsList = item :: this.Items
        { this with Items = newItemsList }
            
    /// <summary>
    /// Tries to add the provided item using the given path.
    /// </summary>
    /// <param name="newItem">The new item to add.</param>
    /// <param name="path">
    /// The path of the item to add. Ex. The path ["University"; "COMP 345"] implies that the item will be placed in the
    /// item group "COMP 345", which, itself, should be inside an in item group "University".
    /// </param>
    member this.tryAddItem (newItem: Item) (path: string list) : ItemGroup option =
        if List.isEmpty path then
            Some (this.addItem newItem)
        else
            option {
                let matchingSubItemGroups = List.filter (fun itemGroup -> itemGroup.Name = (List.head path)) this.SubItemGroups
                let! subItemGroup = match List.length matchingSubItemGroups with | 1 -> Some (List.head matchingSubItemGroups) | _ -> None
                let! newSubItemGroup = subItemGroup.tryAddItem newItem path.Tail
                return this.onReplacement newSubItemGroup 
            }
           
    // Deletes the item from the item group's list of item.
    // If the item does not exist, nothing is deleted. 
    member private this.deleteItem (itemName: string) : ItemGroup =
        let newItemList = List.filter (fun (item: Item) -> item.Name <> itemName) this.Items
        { this with Items = newItemList }
        
    /// <summary>
    /// Tries to remove the provided item using the provided path.
    /// </summary>
    /// <param name="path">
    /// The path of the item to add. Ex. The path ["University"; "COMP 345"; "Do this"] implies that the item "Do this"
    /// is to be deleted. This item should be found in the "COMP 345" item group, which, itself, should be inside an item
    /// group "University".
    /// </param>
    member this.tryDeleteItem (path: string list) : ItemGroup option =
        if List.isEmpty path then
            None
        elif (List.length path) = 1 then
            option {
                let matchingItems = List.filter (fun (item: Item) -> item.Name = (List.head path)) this.Items
                let! item = match List.length matchingItems with | 1 -> Some (List.head matchingItems) | _ -> None
                return this.deleteItem item.Name
            } 
        else
            option {
                let matchingSubItemGroups = List.filter (fun itemGroup -> itemGroup.Name = (List.head path)) this.SubItemGroups
                let! subItemGroup = match List.length matchingSubItemGroups with | 1 -> Some (List.head matchingSubItemGroups) | _ -> None
                let! newSubItemGroup = subItemGroup.tryDeleteItem path.Tail
                return this.onReplacement newSubItemGroup 
            }
        
        
        
and Item =
    { Name: string
      Description: string option
      DueDate: DueDate
      Labels: Label list }
    
    static member Default =
        { Name = ""
          Description = None
          DueDate = DueDate.DateDue DateTime.MinValue 
          Labels = List.empty }
        
    override this.ToString () =
        let dueDateFormatted =
            match this.DueDate with
            | DateDue dateTime ->  Date.getDaysRemainingAsString dateTime 
            | WeekDue week -> Week.getWeeksRemainingAsString week 
            
        sprintf $"[red]%s{dueDateFormatted}[/] %s{this.Name}"