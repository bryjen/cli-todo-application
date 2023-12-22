namespace Todo

open System

/// Record type containing program and user data
type AppData =
    { TestData: string }
  
module AppDataFunctions =
    
    /// Default/placeholder app data, used for testing/development purposes.
    let defaultAppData = { TestData = "Some test data" }