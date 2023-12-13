module Todo.Tests.Modules.Data.FunctionMapperTests

open System.Reflection
open NUnit.Framework

open Todo.Modules.Data

[<TestFixture>]
type FunctionMapperTests () =

    [<Test>]
    member this.``Test Get All Functions`` () =
        FunctionMapper.getFunctionsWithActionSignatureAttribute ()
        |> Array.map (fun (func: MethodInfo) -> printfn $"%s{func.Name}")
        |> ignore
        Assert.Pass()