namespace Todo

open Todo.Utilities

type Label =
    { Name: string }

type ItemGroup =
    { Name: string
      SubItemGroups: ItemGroup list
      Items: Item list
      Labels: Label list }
and Item =
    { Name: string
      Description: string option
      DueDate: DueDate
      Labels: Label list }
    
module ItemGroupFunctions =
    
    let getIndentation (num: int) : string =
        String.replicate num "    "
    
    let itemToString (item: Item) : string =
        let dueDateFormatted =
            match item.DueDate with
            | DateDue dateTime ->  Date.getDaysRemainingAsString dateTime 
            | WeekDue week -> Week.getWeeksRemainingAsString week 
            
        sprintf $"[red]%s{dueDateFormatted}[/] %s{item.Name}"
        
    let itemGroupToString (itemGroup: ItemGroup) : string =
        
        // helper function
        let rec formatItemGroup (itmGrp: ItemGroup) (depth: int) : string =
            let itmGrpString =
                (sprintf $"%s{getIndentation depth}[[%s{itmGrp.Name}]]\n")
                    +
                (itmGrp.Items
                 |> List.map (fun (item: Item) -> sprintf $"%s{getIndentation (depth + 1)}%s{itemToString item}\n")
                 |> List.fold (fun acc str -> acc + str) "")
            
            if List.length itmGrp.SubItemGroups = 0 then
                itmGrpString
            else
                let subItemGroupsString =
                    itmGrp.SubItemGroups
                    |> List.map (fun subItemGroup -> formatItemGroup subItemGroup (depth + 1))  
                    |> List.fold (fun acc str -> acc + str) ""
                    
                itmGrpString + subItemGroupsString
    
        formatItemGroup itemGroup 0 
    