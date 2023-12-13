module Todo.Tests.Utilities.SerializationTests

open NUnit.Framework

open Todo.Utilities.Serialization



//  Sample record type used for testing
type TestUser = {
    Id: int
    Username: string
    PasswordHash: string
}



[<TestFixture>]
type SerializationTests () =
    
    
    //  Returns a string representation of a 'TestUser' record
    let toString (testUser: TestUser) : string =
        sprintf $"[%d{testUser.Id}, %s{testUser.Username}, %s{testUser.PasswordHash}]"
    
    
    [<Test>]
    member this.``Test Round-Trip Serialization, Json`` () =
         
         let testCases = [
             { Id = 0; Username = "getmeout8341"; PasswordHash = "$2y$10$UBRahwzqgqzI/RWWQxq.gehyBq6AfSTysHy9uNhBQJd2HsfX6qTX2" }
             { Id = 1; Username = "BlakeUplift";  PasswordHash = "$2y$10$/3eldA.vbU60foae8qzOcOrEvHpPxewNAwC1Jg8um5XUM5E2iVHJK" }
             { Id = 2; Username = "w-bellamy";    PasswordHash = "$2y$10$sy0uVadb6beM.Qj63UIGF.Swz9BYuVrVJJAjNos.8.aOIHqEiF9fu" }
         ]
         
         let processedTestCases = List.map (jsonSerialize >> jsonDeserialize<TestUser>) testCases
         let pairedCases = List.zip testCases processedTestCases
        
         if (List.exists (fun result -> result = None) processedTestCases) then
             // One of the serialized strings failed to deserialize (return a None value)
             printfn "Invalid records: (could not deserialize)"
             
             pairedCases
             |> List.filter (fun pair -> (snd pair) = None)
             |> List.map fst
             |> List.map (fun testUser -> printfn $"%s{toString testUser}")
             |> ignore
             
             Assert.Fail()
             ()
             
         else
             // Now we make sure that the deserialized data is equal to that of the original data
             let testCasesResults =
                 pairedCases
                 |> List.map (fun pair -> ((fst pair), Option.get (snd pair)))
                 |> List.map (fun pair -> (fst pair) = (snd pair))
                 |> List.zip testCases
             
             if (List.exists (fun pair -> (snd pair) = false) testCasesResults) then
                //  One of the deserialized strings is unequal to the original record 
                printfn "Invalid records: (does not equal original)"
                
                testCasesResults
                |> List.filter (fun pair -> (snd pair) = false)
                |> List.map fst
                |> List.map (fun testUser -> printfn $"%s{toString testUser}")
                |> ignore
                
                Assert.Fail()
                ()
             else
                printfn "All tests passed, used records"
                
                testCasesResults
                |> List.map fst
                |> List.map (fun testUser -> printfn $"%s{toString testUser}")
                |> ignore
             ()
         
    [<Test>]    
    member this.``Test Serializer, Json`` () =
        let testUser = { Id = 0; Username = "getmeout8341"; PasswordHash = "$2y$10$UBRahwzqgqzI/RWWQxq.gehyBq6AfSTysHy9uNhBQJd2HsfX6qTX2" }
        let jsonSerializer = getJsonSerializer "data/test.json"
        
        jsonSerializer testUser
        ()