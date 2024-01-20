﻿[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Create

open System
open Argu
open Spectre.Console
open FsToolkit.ErrorHandling
open Todo
open Todo.ItemGroup
open Todo.Cli
open Todo.Cli.Commands.Arguments

let private createItemGroup (appData: AppData) (parseResults: ParseResults<CreateItemGroupArguments>) : Result<ItemGroup list, Exception> =
    let newItemGroup = CreateItemGroupArguments.ToItemGroup parseResults 
    let path = parseResults.GetResult CreateItemGroupArguments.Path
    let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups; Path = String.concat "/" path }
    
    result {
        let modify = ItemGroup.AddItemGroup newItemGroup
        let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path          // perform change
        return! (fun rootItemGroup -> Ok rootItemGroup.SubItemGroups) newRootItemGroup  // get the sub-item groups
    }
    
let private createItem (appData: AppData) (parseResults: ParseResults<CreateItemArguments>) : Result<ItemGroup list, Exception> =
    let newItem = CreateItemArguments.ToItem parseResults 
    let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    let path = parseResults.GetResult CreateItemArguments.Path
    
    result {
        let modify = ItemGroup.AddItem newItem
        let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path          // perform change
        return! (fun rootItemGroup -> Ok rootItemGroup.SubItemGroups) newRootItemGroup  // get the sub-item groups
    }
    
let private createLabel (appData: AppData) (parseResults: ParseResults<CreateLabelArguments>) : Result<Label list, Exception> =
    result {
        let! newLabel = CreateLabelArguments.ToLabel parseResults
        return! (fun label -> Ok (label :: appData.Labels)) newLabel
    }

let private parseArgv (argv: string array) : Result<ParseResults<CreateArguments>, Exception> =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<CreateArguments>(errorHandler = errorHandler)
    
    try
        let parsedArgs = parser.Parse argv
        Ok parsedArgs
    with
        | :? ArguParseException as ex -> Error ex
        | ex -> Error ex
        
let private updateAppData (appData: AppData) (parseResults: ParseResults<CreateArguments>) : Result<AppData, Exception> =
    result {
        let toCreate = List.head (parseResults.GetAllResults())
        
        match toCreate with 
        | Item_Group innerParseResults ->
            let! newItemGroups = createItemGroup appData innerParseResults
            return { appData with ItemGroups = newItemGroups }
            
        | Item innerParseResults -> 
            let! newItemGroups = createItem appData innerParseResults
            return { appData with ItemGroups = newItemGroups}
            
        | Label innerParseResults ->
            let! newLabels = createLabel appData innerParseResults
            return { appData with Labels = newLabels }
    } 
    
/// <summary>
/// Returns a function that implements the underlying logic for the 'create' command. 
/// </summary>
/// <param name="appData">The application data.</param>
let internal injectCreate 
    (_: ApplicationConfiguration)
    (_: ApplicationSettings)
    (appData: AppData)
    : CommandFunction =
        
    (* Actual execute function, *)
    let execute (argv: string array) : AppData =
        let creationResult = 
            result {
                let! parseResults = parseArgv argv
                return! updateAppData appData parseResults
            }
            
        // process the result
        match creationResult with
        | Ok newAppData ->
            AnsiConsole.MarkupLine "Successfully created the specified item group." 
            newAppData
        | Error err ->
            AnsiConsole.MarkupLine (sprintf $"Creation failed with message: %s{err.Message}")
            appData

    CommandFunction.ChangesData execute
    
let createCommandTemplate : CommandTemplate =
    { CommandName = "create"
      HelpString = "Creates an item/item group/label."
      InjectData = injectCreate }