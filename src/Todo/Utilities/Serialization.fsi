module Todo.Utilities.Serialization



/// Serializes an object into json with indented formatting.
val jsonSerialize : obj: obj -> string 

/// Attempts to deserialize a json string into the indicated object. 
val jsonDeserialize<'T> : rawJson: string -> Option<'T>

/// Returns a function that takes in an object and deserializes it into json in the provided file path.
val getJsonSerializer : filePath: string -> (obj -> unit) 

/// Returns a function that reads and deserializes data from the provided file path.
val getJsonDeserializer<'T> : filePath: string -> (unit -> Option<'T>) 