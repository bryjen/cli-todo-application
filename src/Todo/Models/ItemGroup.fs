namespace Todo.Models

/// <summary>
/// An entity that can hold tοdo items. An item group can also hold inner/sub item groups. 
/// </summary>
/// <remarks>
/// When displaying to the user, it is called a '<b>tοdo</b>' group.
/// </remarks>
[<AutoOpen>]
type ItemGroup =
    { Name: string
      Path: string
      Description: string option
      SubItemGroups: ItemGroup list
      Items: Item list }
    
    /// A placeholder value for a 'ItemGroup' type 
    static member Root =
        { Name = "root"
          Path = ""
          Description = Some "A temporary todo group." 
          SubItemGroups = List.empty
          Items = List.empty }
        
    /// <inheritdoc />
    override this.ToString () =
        let indentation (num: int) : string =
            String.replicate num "    "
        
        let rec formatItemGroup (itmGrp: ItemGroup) (depth: int) : string =
            let itmGrpString =
                ($"{(indentation depth)}[[{itmGrp.Name}]]\n") 
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
    