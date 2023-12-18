module Todo.Tests.Core.ActionTreeBuilderTests

open NUnit.Framework
open Todo.Core.ActionTreeBuilder

[<TestFixture>]
type ActionTreeBuilderTests() =
        
    [<Test>]
    member this.``Test Action Tree Builder`` () =
        match TreeBuilding.buildActionTree () with
        | Ok actionTree ->
            TreeBuilding.printActionTree actionTree
            Assert.Pass()
        | Error actionTreeException ->
            printfn $"%s{actionTreeException.StackTrace}"
            raise actionTreeException