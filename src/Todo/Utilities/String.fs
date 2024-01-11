[<AutoOpen>]
module Todo.Utilities.String

// Prepends the provided string with indentation.
let indent (num: int) (str: string) : string =
    let indent = String.replicate 4 " "
    let totalIndents = String.replicate num indent
    totalIndents + str