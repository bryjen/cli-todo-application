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
        
    /// Returns a function that parses an item group into formatted lines. Depends on the type of display format.
    static member GetToLinesFunc (displayFormat: DisplayFormat) : ItemGroup -> string list =
         match displayFormat with
         | PlainTree -> Tree.PlainTree.toLines  
         | PrettyTree -> Tree.PrettyTree.toLines  