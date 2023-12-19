module Todo.Tests.Core.ActionTreeTests

open NUnit.Framework

open Todo.Core.ActionTreeBuilder
open Todo.Core.ActionTree

[<TestFixture>]
type ActionTreeTests() =
    
    [<Test>]
    member this.``Test getAction`` () =
        match TreeBuilding.buildActionTree () with
        | Ok actionTree ->
            match ActionTreeFunctions.getAction actionTree [| "list"; "create" |] with
            | Ok value -> printfn $"%s{(fst value)}"
            | Error error -> raise error
            
            Assert.Pass()
        | Error actionTreeException ->
            printfn $"%s{actionTreeException.StackTrace}"
            raise actionTreeException
