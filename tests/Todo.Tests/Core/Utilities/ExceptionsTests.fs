module Todo.Tests.Core.Utilities.ExceptionsTests

open NUnit.Framework
open Todo.Core.Utilities.Exceptions.ActionTree

[<TestFixture>]
type ExceptionsTests () =
    
    [<Test>]
    member this.``IdenticalActionModuleNamesException tests`` () =
        raise (IdenticalActionModuleNamesException("List", ["somenamespace"; "anothernamespace"]))