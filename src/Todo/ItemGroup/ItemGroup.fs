namespace Todo.ItemGroup

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
        
    /// <summary>
    /// Returns the string representation of the <c>ItemGroup</c> record.
    /// </summary>
    override this.ToString () =
        let indentation (num: int) : string =
            String.replicate num "    "
        
        let rec formatItemGroup (itmGrp: ItemGroup) (depth: int) : string =
            let itmGrpString =
                (sprintf "%s[[%s]]  %s\n" (indentation depth) itmGrp.Name (Label.FormatLabels itmGrp.Labels)) 
                    +
                (itmGrp.Items
                 |> List.map (fun (item: Item) -> sprintf $"%s{indentation (depth + 1)}%s{item.ToString()}\n")
                 |> List.fold (fun acc str -> acc + str) "")
                
            match (List.length itmGrp.SubItemGroups) = 0 with
            | true ->
                itmGrpString
            | false ->
                let subItemGroupsString =
                    itmGrp.SubItemGroups
                    |> List.map (fun subItemGroup -> formatItemGroup subItemGroup (depth + 1))  
                    |> List.fold (fun acc str -> acc + str) ""
                    
                itmGrpString + subItemGroupsString
    
        formatItemGroup this 0
        
    // Replaces the base item group's sub item groups with the replacement (If their name matches the IDENTIFIER).
    static member private ReplaceSubItemGroupByIdentifier
        (baseItemGroup: ItemGroup)
        (replacement: ItemGroup)
        (identifier: string)
        : ItemGroup =
            
        let replacementFunction = (fun itemGroup -> if itemGroup.Name = identifier then replacement else itemGroup)
        let newSubItemGroups = List.map replacementFunction baseItemGroup.SubItemGroups
        { baseItemGroup with SubItemGroups = newSubItemGroups }
        
    // Replaces the base item group's sub item groups with the replacement (If they have the SAME NAME).
    static member private ReplaceSubItemGroup (baseItemGroup: ItemGroup) (replacement: ItemGroup) : ItemGroup =
        ReplaceSubItemGroupByIdentifier baseItemGroup replacement replacement.Name 
    
    // Recursively locates a specified (sub) item group.
    static member private Locate (baseItemGroup: ItemGroup) (path: string list) : Result<ItemGroup, Exception> =
        match path with
        | [] ->
            Ok baseItemGroup
        | requiredName :: rest -> 
            match List.tryFind (fun itemGroup -> itemGroup.Name = requiredName) baseItemGroup.SubItemGroups with
            | Some subItemGroup ->
                Locate subItemGroup rest
            | None ->
                let message = sprintf $"Could not find the sub item group \"%s{requiredName}\" in the item group \"%s{baseItemGroup.Name}\""
                Error (Exception(message))
    
    // Recursively replaces the indicated sub item group with its replacement.
    // Reflects a change to a single (sub) item group.
    static member private Update (baseItemGroup: ItemGroup) (path: string list) (replacement: ItemGroup) : ItemGroup =
        
        // ** DOES NOT HANDLE ERRORS FROM INVALID PATH. Error meant to be handled from a previous call to 'Locate' **
        
        match path with
        | [] ->
            replacement
        | [requiredName] ->
            let newSubItemGroups = List.map (fun itemGroup -> if itemGroup.Name = requiredName then replacement else itemGroup) baseItemGroup.SubItemGroups
            { baseItemGroup with SubItemGroups = newSubItemGroups }
        | requiredName :: rest ->
            let subItemGroup = List.find (fun itemGroup -> itemGroup.Name = requiredName) baseItemGroup.SubItemGroups
            ReplaceSubItemGroup baseItemGroup (Update subItemGroup rest replacement)
    
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
    static member Modify
        (itemGroup: ItemGroup)
        (modify: ItemGroup -> Result<ItemGroup, Exception>)
        (path: string list)
        : Result<ItemGroup, Exception> =
            
        result {
            let! requiredItemGroup = ItemGroup.Locate itemGroup path  // locate
            let! newItemGroup = modify requiredItemGroup              // modify
            return ItemGroup.Update itemGroup path newItemGroup       // update
        }
    
    /// <summary>
    /// Adds a sub item group to the passed item group.
    /// </summary>
    /// <param name="newItemGroup">The new sub item group to add.</param>
    /// <param name="baseItemGroup">The item group to be modified.</param>
    static member AddItemGroup (newItemGroup: ItemGroup) (baseItemGroup: ItemGroup) : Result<ItemGroup, Exception> =
        let newSubItemGroups = newItemGroup :: baseItemGroup.SubItemGroups
        Ok { baseItemGroup with SubItemGroups = newSubItemGroups }
        
    /// <summary>
    /// Attempts to remove a sub item group from the passed item group.
    /// </summary>
    /// <param name="name">The name of the sub item group to remove.</param>
    /// <param name="baseItemGroup">The item group to be modified.</param>
    static member RemoveItemGroup (name: string) (baseItemGroup: ItemGroup) : Result<ItemGroup, Exception> =
        let newSubItemGroups = List.filter (fun itemGroup -> itemGroup.Name <> name) baseItemGroup.SubItemGroups
        
        match (List.length newSubItemGroups) = (List.length baseItemGroup.SubItemGroups) with
        | true -> Error (Exception(sprintf $"Could not find the sub-item group with the name %s{name}")) 
        | false -> Ok { baseItemGroup with SubItemGroups = newSubItemGroups } 
        
    /// <summary>
    /// Adds an item to the passed item group.
    /// </summary>
    /// <param name="newItem">The new item to add.</param>
    /// <param name="baseItemGroup">The item group to be modified.</param>
    static member AddItem (newItem: Item) (baseItemGroup: ItemGroup) : Result<ItemGroup, Exception> =
        let newItems = newItem :: baseItemGroup.Items
        Ok { baseItemGroup with Items = newItems }
        
    /// <summary>
    /// Attempts to remove an item from the passed item group.
    /// </summary>
    /// <param name="name">The name of the item to remove.</param>
    /// <param name="baseItemGroup">The item group to be modified.</param>
    static member RemoveItem (name: string) (baseItemGroup: ItemGroup) : Result<ItemGroup, Exception> =
        let newItems = List.filter (fun (item: Item) -> item.Name <> name) baseItemGroup.Items
        
        match (List.length newItems) = (List.length baseItemGroup.Items) with
        | true -> Error (Exception(sprintf $"Could not find the sub-item group with the name %s{name}")) 
        | false -> Ok { baseItemGroup with Items = newItems }
        
    /// <summary>
    /// Attempts to add a label to the passed item group.
    /// </summary>
    /// <param name="newLabel">The label to add to the item group.</param>
    /// <param name="baseItemGroup">The item group to be modified.</param>
    static member AddLabel (newLabel: Label) (baseItemGroup: ItemGroup) : Result<ItemGroup, Exception> =
        let newLabels = newLabel :: baseItemGroup.Labels
        Ok { baseItemGroup with Labels = newLabels }
        
    /// <summary>
    /// Attempts to remove a label from the passed item group.
    /// </summary>
    /// <param name="name">The name of the label to remove.</param>
    /// <param name="baseItemGroup">The item group to be modified.</param>
    static member RemoveLabel (name: string) (baseItemGroup: ItemGroup) : Result<ItemGroup, Exception> =
        let newLabels = List.filter (fun (label: Label) -> label.Name <> name) baseItemGroup.Labels
        
        match (List.length newLabels) = (List.length baseItemGroup.Labels) with
        | true -> Error (Exception(sprintf $"Could not find label with the name \"%s{name}\""))
        | false -> Ok { baseItemGroup with Labels = newLabels }
        
    /// <summary>
    /// Attempts to rename a specified sub item group.
    /// </summary>
    /// <param name="subItemGroupName">The name of the sub item group to replace.</param>
    /// <param name="newName">The new name of the sub item group.</param>
    /// <param name="baseItemGroup">The item group to be modified.</param>
    static member RenameSubItemGroup
        (subItemGroupName: string)
        (newName: string)
        (baseItemGroup: ItemGroup)
        : Result<ItemGroup, Exception> =
            
        (* Note that we do not change the name of the base item group itself because since the replacement algorithm
           in 'Locate' and 'Update' that the modified item group has the same name - otherwise they break. *)
        
        match List.tryFind (fun itemGroup -> itemGroup.Name = subItemGroupName) baseItemGroup.SubItemGroups with
        | Some itemGroup ->
            let oldName = itemGroup.Name
            let newItemGroup = { itemGroup with Name = newName }
            Ok (ReplaceSubItemGroupByIdentifier baseItemGroup newItemGroup oldName)
        | None ->
            Error (Exception(sprintf $"Could not find the sub item group \"%s{subItemGroupName}\""))

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newDescription"></param>
    /// <param name="baseItemGroup"></param>
    static member ChangeDescription (newDescription: string) (baseItemGroup: ItemGroup) : Result<ItemGroup, Exception> =
        Ok { baseItemGroup with Description = Some newDescription }
        
        
        
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
        
