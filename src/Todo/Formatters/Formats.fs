namespace Todo.Formatters

open Todo.ItemGroup

type DisplayFormat =
    | PlainTree
    | PrettyTree 
    
    /// Attempts to parse the given string into a <c>DisplayFormat</c> type. Returns <c>None</c> if invalid.
    static member TryParseString (str: string) : DisplayFormat option =
        let newStr = str |> (_.Trim()) |> (_.ToLower())
        
        match newStr with
        | "plaintree" -> Some DisplayFormat.PlainTree
        | "prettytree" -> Some DisplayFormat.PrettyTree
        | _ -> None
        
    /// Formats into a string and then split it into lines. Formatting is based on the provided display format.
    static member ToLines (displayFormat: DisplayFormat) (itemGroup: ItemGroup) : string list =
         match displayFormat with
         | PlainTree -> Tree.PlainTree.toLines itemGroup 
         | PrettyTree -> Tree.PrettyTree.toLines itemGroup 