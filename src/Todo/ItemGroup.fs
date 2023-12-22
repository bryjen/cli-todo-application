namespace Todo

open System.Text
open Todo

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
      DueDate: Utilities.DueDate
      Labels: Label list }
    
module ItemGroupFunctions =
    
    let getTabs (num: int) : string =
        String.replicate num "\t"
    
    let itemToString (item: Item) : string =
        sprintf $"%s{item.Name}"
        
    let itemGroupToString (itemGroup: ItemGroup) : string =
        
        // helper function
        let rec formatItemGroup (itmGrp: ItemGroup) (depth: int) (stringBuilder: StringBuilder) : string =
            stringBuilder.Append(sprintf $"%s{getTabs depth}[%s{itmGrp.Name}]\n") |> ignore
            List.map (fun (item: Item) -> stringBuilder.Append(sprintf $"%s{getTabs (depth + 1)}%s{itemToString item}\n")) |> ignore
            
            if List.length itmGrp.SubItemGroups = 0 then
                stringBuilder.ToString()
            else    
                for subItemGroup in itmGrp.SubItemGroups do
                    formatItemGroup subItemGroup (depth + 1) stringBuilder |> ignore
                ""
    
        let stringBuilder = StringBuilder()
        formatItemGroup itemGroup 0 stringBuilder
    