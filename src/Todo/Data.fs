namespace Todo

open Todo.ItemGroup

/// Record type containing program and user data
type AppData =
    { ItemGroups: ItemGroup list
      Labels: Label list }
  
module AppDataFunctions =
    
    /// Default/placeholder app data, used for testing/development purposes.
    let defaultAppData = {
        ItemGroups = List.empty
        Labels = List.empty
    }