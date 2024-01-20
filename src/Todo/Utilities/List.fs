[<AutoOpen>]
module Todo.Utilities.List

open System

// ["first"; "next"; "after"; "last"] -> ("last", ["first"; "next"; "after"])
let splitLast (lst: string list) : string * string list =
    match lst with
    | [] ->
        ("", List.empty)
    | _ ->
        let last = lst |> List.rev |> List.head
        let rest = lst |> List.rev |> List.tail |> List.rev
        (last, rest)
        
/// <summary>
/// Asserts that a list of strings contains non-empty values. Otherwise, an exception is returned.
/// </summary>
/// <param name="strings">A list of strings to check.</param>
/// <param name="exceptionToReturn">The exception that is returned in case a string is empty.</param>
let assertNonEmptyStringList 
    (strings: string list)
    (exceptionToReturn: Exception) =
        
    match List.exists (String.IsNullOrWhiteSpace) strings with
    | true -> Error exceptionToReturn
    | false -> Ok () 

/// Returns true if a list has duplicates, false otherwise
let hasDuplicates list =
    let distinctCount = list |> Seq.distinct |> Seq.length
    distinctCount <> List.length list
    
/// Returns the first occurrence of a duplicate, if one exists.
let findFirstDuplicate list =
    list
    |> List.groupBy id
    |> List.choose (fun (key, grp) -> if List.length grp > 1 then Some key else None)
    |> List.tryHead