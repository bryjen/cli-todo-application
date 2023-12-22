namespace Todo


/// Record type containing program and user data
type AppData =
    { ItemGroups: ItemGroup list }
  
module AppDataFunctions =
    
    /// Default/placeholder app data, used for testing/development purposes.
    let defaultAppData = { ItemGroups = List.empty<ItemGroup> }