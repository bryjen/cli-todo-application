[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Delete

open System
open Argu
open FsToolkit.ErrorHandling
open Spectre.Console
open Todo
open Todo.ItemGroup
open Todo.Utilities
open Todo.Cli
open Todo.Cli.Utilities
open Todo.Cli.Commands.Arguments

let internal parseArgv (argv: string array) : Result<ParseResults<DeleteArguments>, Exception> =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<DeleteArguments>(errorHandler = errorHandler)
    
    try
        let parsedArgs = parser.Parse argv
        Ok parsedArgs
    with
        | :? ArguParseException as ex -> Error ex
        | ex -> Error ex
        
let private deleteItemGroup (appData: AppData) (parseResults: ParseResults<DeleteItemGroupArguments>) : Result<ItemGroup list, Exception> =
    let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    
    result {
        let (toDelete, path) = splitLast (parseResults.GetResult DeleteItemGroupArguments.Path) 
        let modify = ItemGroup.RemoveItemGroup toDelete 
        let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path 
        return! (fun itemGroup -> Ok itemGroup.SubItemGroups) newRootItemGroup
    }
    
let private deleteItem (appData: AppData) (parseResults: ParseResults<DeleteItemArguments>) : Result<ItemGroup list, Exception> =
    let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    
    result {
        let (toDelete, path) = splitLast (parseResults.GetResult DeleteItemArguments.Path) 
        let modify = ItemGroup.RemoveItem toDelete 
        let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path 
        return! (fun itemGroup -> Ok itemGroup.SubItemGroups) newRootItemGroup
    }
        
let private updateAppData (appData: AppData) (parseResults: ParseResults<DeleteArguments>) : Result<AppData, Exception> =
    result {
        let toDelete = List.head (parseResults.GetAllResults())
        
        match toDelete with
        | Item_Group innerParseResults ->
            let! newItemGroups = deleteItemGroup appData innerParseResults
            return { appData with ItemGroups = newItemGroups }
            
        | Item innerParseResults ->
            let! newItemGroups = deleteItem appData innerParseResults
            return { appData with ItemGroups = newItemGroups }
    }
        
/// <summary>
/// Returns a function that implements the underlying logic for the 'delete' command. 
/// </summary>
/// <param name="appData">The application data.</param>
let internal injectDelete 
    (_: ApplicationConfiguration)
    (_: ApplicationSettings)
    (appData: AppData)
    : CommandFunction =
        
    let execute (argv: string array) : AppData =
        let deletionResult = 
            result {
                let! parsedArgs = parseArgv argv
                return! updateAppData appData parsedArgs
            }
            
        match deletionResult with
        | Ok newAppData ->
            AnsiConsole.MarkupLine "Successfully deleted the specified item group." 
            newAppData
        | Error err ->
            AnsiConsole.MarkupLine (sprintf $"Deletion failed with message: %s{err.Message}")
            appData // return 'old' app data
            
    CommandFunction.ChangesData execute
    
let deleteCommandTemplate : CommandTemplate =
    { CommandName = "delete"
      HelpString = "Deletes an item/item group/label."
      InjectData = injectDelete }