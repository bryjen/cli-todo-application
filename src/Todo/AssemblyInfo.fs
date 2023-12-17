module Todo.AssemblyInfo

open System.Runtime.CompilerServices

//  Exposes types marked as internal to the test project
[<assembly: InternalsVisibleTo("Todo.Tests")>]
do ()