module Todo.Cli.Commands.Create

open System
open Argu
open FsToolkit.ErrorHandling
open Todo
open Todo.ItemGroup
open Todo.Cli
open Todo.Cli.Utilities
open Todo.Cli.Commands.Arguments

// Updates the app data, given a new root item group.
let private updateAppData appData newRootItemGroup =
    { appData with ItemGroups = newRootItemGroup.SubItemGroups }

let private tryCreateItemGroup
    (appData: AppData)
    (parseResults: ParseResults<CreateItemGroupArguments>)
    : Result<AppData, Exception> =
        
    let newItemGroup = CreateItemGroupArguments.ToItemGroup parseResults 
    let path = parseResults.GetResult CreateItemGroupArguments.Path
    let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups; Path = String.concat "/" path }
    
    let modify = ItemGroup.AddItemGroup newItemGroup
    let newRootItemGroupResult = ItemGroup.Modify tempRootItemGroup modify path          
    Result.map (updateAppData appData) newRootItemGroupResult
    
let private tryCreateItem
    (appData: AppData)
    (parseResults: ParseResults<CreateItemArguments>)
    : Result<AppData, Exception> =
        
    let newItem = CreateItemArguments.ToItem parseResults 
    let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    let path = parseResults.GetResult CreateItemArguments.Path
    
    let modify = ItemGroup.AddItem newItem
    let newRootItemGroupResult = ItemGroup.Modify tempRootItemGroup modify path
    Result.map (updateAppData appData) newRootItemGroupResult
    
let private tryCreateLabel
    (appData: AppData)
    (parseResults: ParseResults<CreateLabelArguments>)
    : Result<AppData, Exception> =
        
    result {
        let! newLabel = CreateLabelArguments.ToLabel parseResults
        let newListOfLabels = newLabel :: appData.Labels
        return { appData with Labels = newListOfLabels }
    }
    
let private create createArgument appData = 
    match createArgument with
    | CreateArguments.Item_Group innerParseResults -> tryCreateItemGroup appData innerParseResults 
    | CreateArguments.Item innerParseResults -> tryCreateItem appData innerParseResults 
    | CreateArguments.Label innerParseResults -> tryCreateLabel appData innerParseResults
    
let private handleResults (result: Result<'a, Exception>) =
    match result with
    | Ok _ ->
        ()
    | Error err ->
        let exceptionMessage = "An error occurred while trying to create the object: " + err.Message
        printfn "%s" exceptionMessage
    
let internal executeCreateCommand 
    (appConfig: ApplicationConfiguration)
    (appData: AppData)
    (createParseResults: ParseResults<CreateArguments>)
    : unit =
        
    let filePath = appConfig.getDataFilePath()
    let whatToCreate = List.head (createParseResults.GetAllResults())
    
    result {
        let! creationResult = create whatToCreate appData 
        return! Files.saveAppData filePath creationResult
    }
    |> handleResults 