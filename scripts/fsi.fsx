// Execute dotnet fsi from the solution folder
// dotnet fsi --use:"scripts/fsi.fsx"

#load "packages.fsx"
#load "references.fsx"

open System
open Spectre.Console
open Argu

open Todo
open Todo.Cli.Commands
open Todo.Cli.Commands.Arguments
open Todo.Cli.Utilities

// Other
let fromresult<'T, 'Error> (someResult: Result<'T, 'Error>) = someResult |> Result.toList |> List.head   


// Wrap file related functions
let filePath = Files.filePath
let loadappdata () = Files.loadAppData filePath
let saveappdata (appdata: AppData) = Files.saveAppData filePath appdata 
let appdata = loadappdata () |> Result.toList |> List.head
let rootitemgroup = { ItemGroup.Default with SubItemGroups = appdata.ItemGroups }



// Wrap console functions. Makes them easier to call
let printmkup = AnsiConsole.MarkupLine 
let printitemgroup (itemGroup: ItemGroup) = printmkup (sprintf $"%s{itemGroup.ToString()}") 



// Creating 'Label' record(s) for testing
let testLabelResult = Label.CreateFromString "project" "purple"
let testLabel = testLabelResult |> Result.toList |> List.head

let parser = ArgumentParser.Create<EditArguments>()


// Wraps user commands 
let list ([<ParamArray>] argv: string array) =
    List.execute argv

let create ([<ParamArray>] argv: string array) =
    Create.execute argv

let delete ([<ParamArray>] argv: string array) =
    Delete.execute argv
    
let edit ([<ParamArray>] argv: string array) =
    Edit.execute argv

let help ([<ParamArray>] argv: string array) =
    Help.printCommandsHelp argv