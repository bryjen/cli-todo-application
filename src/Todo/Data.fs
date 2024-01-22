namespace Todo

open Todo.ItemGroup

/// Record type containing program and user data
type AppData =
    { ItemGroups: ItemGroup list
      Labels: Label list }
    
    /// Default/placeholder app data, used for testing/development purposes.
    static member Default =
          { ItemGroups = List.empty
            Labels = List.empty }