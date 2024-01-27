module Todo.Cli.Commands.Delete

open System
open Argu
open FsToolkit.ErrorHandling
open Todo
open Todo.Utilities
open Todo.ItemGroup
open Todo.Cli
open Todo.Cli.Utilities
open Todo.Cli.Commands.Arguments

// Updates the app data, given a new root item group.
let private updateAppData appData newRootItemGroup =
    { appData with ItemGroups = newRootItemGroup.SubItemGroups }

let private tryDeleteItemGroup 
    (appData: AppData)
    (parseResults: ParseResults<DeleteItemGroupArguments>)
    : Result<AppData, Exception> =
        
    let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    let (toDelete, path) = splitLast (parseResults.GetResult DeleteItemGroupArguments.Path)
    
    let modify = ItemGroup.RemoveItemGroup toDelete 
    let newRootItemGroupResult = ItemGroup.Modify tempRootItemGroup modify path
    Result.map (updateAppData appData) newRootItemGroupResult
    
let private tryDeleteItem 
    (appData: AppData)
    (parseResults: ParseResults<DeleteItemArguments>)
    : Result<AppData, Exception> =
        
    let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    let (toDelete, path) = splitLast (parseResults.GetResult DeleteItemArguments.Path)
    
    let modify = ItemGroup.RemoveItem toDelete 
    let newRootItemGroupResult = ItemGroup.Modify tempRootItemGroup modify path
    Result.map (updateAppData appData) newRootItemGroupResult
    
let private delete deleteArgument appData = 
    match deleteArgument with
    | DeleteArguments.Item_Group innerParseResults -> tryDeleteItemGroup appData innerParseResults 
    | DeleteArguments.Item innerParseResults -> tryDeleteItem appData innerParseResults 

let private handleResults (result: Result<'a, Exception>) =
    match result with
    | Ok _ ->
        ()
    | Error err ->
        let exceptionMessage = "An error occurred while trying to delete the object: " + err.Message
        printfn "%s" exceptionMessage
        
let internal executeDeleteCommand 
    (appConfig: ApplicationConfiguration)
    (appData: AppData)
    (deleteParseResults: ParseResults<DeleteArguments>)
    : unit =
        
    let filePath = appConfig.getDataFilePath()
    let whatToDelete = List.head (deleteParseResults.GetAllResults())
    
    result {
        let! creationResult = delete whatToDelete appData 
        return! Files.saveAppData filePath creationResult
    }
    |> handleResults 