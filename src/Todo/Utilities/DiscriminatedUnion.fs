[<AutoOpen>]
module Todo.Utilities.DiscriminatedUnion

open System 
open Microsoft.FSharp.Reflection

let getDUNames<'T>() =
    FSharpType.GetUnionCases(typeof<'T>)
    |> Array.map (_.Name)

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
        
/// <summary>
/// Get all cases in a DU type.
/// </summary>
/// <remarks>
/// For DU cases that return a parameter, a null value is used for the value. Use this method if and only if you will
/// not use the value of the DU case (ex. converting a DU case into its string representation).
/// </remarks>
let getAllDUValuesWithParameters<'T> () =
    let createDefaultArg (typ: Type) = if typ.IsValueType then Activator.CreateInstance(typ) else null
    
    if FSharpType.IsUnion(typeof<'T>) then
        FSharpType.GetUnionCases(typeof<'T>)
        |> Array.map (fun caseInfo ->
            let args = caseInfo.GetFields() |> Array.map (fun fi -> createDefaultArg fi.PropertyType)
            FSharpValue.MakeUnion(caseInfo, args) :?> 'T)
    else
        failwith "Provided type is not a Discriminated Union"
