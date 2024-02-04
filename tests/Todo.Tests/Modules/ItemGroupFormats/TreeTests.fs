module Todo.Tests.Modules.ItemGroupFormats.TreeTests

open NUnit.Framework
open Spectre.Console
open Todo.Models
open Todo.Modules.ItemGroupFormats
open Todo.Tests

[<SetUp>]
let Setup () =
    ()

[<Test>]
let Test1 () =
    let rootItemGroup = { ItemGroup.Root with SubItemGroups = itemGroups1 }
    AnsiConsole.MarkupLine(Tree.toMarkupString rootItemGroup)
    Assert.Pass()