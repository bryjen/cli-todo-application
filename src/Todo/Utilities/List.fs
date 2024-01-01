[<Microsoft.FSharp.Core.AutoOpen>]
module Todo.Utilities.List

// ["first"; "next"; "after"; "last"] -> ("last", ["first"; "next"; "after"])
let splitLast (lst: string list) : string * string list =
    match lst with
    | [] ->
        ("", List.empty)
    | _ ->
        let last = lst |> List.rev |> List.head
        let rest = lst |> List.rev |> List.tail |> List.rev
        (last, rest)