// Execute dotnet fsi from the solution folder
// dotnet fsi --use:"scripts/references.fsx"

#I "../src/Todo/bin/Debug/net8.0/"
#I "../src/Todo.Cli/bin/Debug/net8.0/"
#r "Todo.dll"
#r "todos.dll"