// Execute dotnet fsi from the solution folder
// dotnet fsi --use:"scripts/references.fsx"

#I "../src/Spectre.Console.Prompts.Extensions/bin/Debug/net8.0/"
#I "../src/Todo/bin/Debug/net8.0/"
#I "../src/Todo.Cli/bin/Debug/net8.0/"
#r "Spectre.Console.Prompts.Extensions.dll"
#r "Todo.dll"
#r "todos.dll"