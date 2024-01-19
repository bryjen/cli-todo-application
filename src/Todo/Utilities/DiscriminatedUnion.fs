[<AutoOpen>]
module Todo.Utilities.DiscriminatedUnion

open Microsoft.FSharp.Reflection

let getAllDUValues<'T> () =
    if FSharpType.IsUnion(typeof<'T>) then
        FSharpType.GetUnionCases(typeof<'T>)
        |> Array.choose (fun caseInfo ->
            if caseInfo.GetFields().Length = 0 then
                Some (FSharpValue.MakeUnion(caseInfo, [||]) :?> 'T)
            else
                None) // Ignore DU cases with parameters
    else
        failwith "Provided type is not a Discriminated Union"
