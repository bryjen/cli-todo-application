[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Create

open System
open Argu
open Spectre.Console
open FsToolkit.ErrorHandling

open Todo
open Todo.Cli.Utilities

/// <summary>
/// Parses the array of CLI arguments.
/// </summary>
let internal parse (argv: string array) : Result<ParseResults<CreateArguments>, Exception> =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<CreateArguments>(errorHandler = errorHandler)
    
    try
        let parsedArgs = parser.Parse argv
        Ok parsedArgs
    with
        | :? ArguParseException as ex -> Error ex
        | ex -> Error ex
        
/// <summary>
/// Attempts to create the specified item from the given arguments. Returns an exception, or the updated list of item
/// groups.
/// </summary>
/// <param name="appData">The current state/data of the application.</param>
/// <param name="parseResults">Results of parsing the CLI arguments.</param>
let internal create (appData: AppData) (parseResults: ParseResults<CreateArguments>) : Result<ItemGroup list, Exception> =
    let modifyResult = 
        match parseResults.TryGetResult CreateArguments.Item_Group with
        // create item group
        | Some itemGroupParseResults -> 
            let newItemGroup = CreateItemGroupArguments.ToItemGroup itemGroupParseResults 
            let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
            let path = itemGroupParseResults.GetResult CreateItemGroupArguments.Path
            
            let modify = ItemGroup.AddItemGroup newItemGroup
            ItemGroup.Modify tempRootItemGroup modify path
            
        // create item 
        | None ->
            let itemParseResults = parseResults.GetResult CreateArguments.Item  //  guaranteed to be ok
            let newItem = CreateItemArguments.ToItem itemParseResults 
            let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
            let path = itemParseResults.GetResult CreateItemArguments.Path
            
            let modify = ItemGroup.AddItem newItem
            ItemGroup.Modify tempRootItemGroup modify path
        
    Result.bind (fun itemGroup -> Ok itemGroup.SubItemGroups) modifyResult // get modified sub-item groups if ok 
        
/// <summary>
/// Update the current application state/data with new new list of item groups.
/// </summary>
/// <param name="appData">The current state/data of the application.</param>
/// <param name="newItemGroups">The new list of item groups.</param>
let internal update (appData: AppData) (newItemGroups: ItemGroup list) : Result<AppData, Exception> =
    Ok { appData with ItemGroups = newItemGroups }
    
/// <summary>
/// Implements the logic for the 'create' command.
/// </summary>
/// <param name="argv">List of arguments to be parsed.</param>
let execute (argv: string array) : AppData =
    let appData = match Files.loadAppData Files.filePath with | Ok appData -> appData | Error err -> raise err
        
    match
        result {
            let! parsedArgs = parse argv
            let! newItemGroups = create appData parsedArgs
            return! update appData newItemGroups 
        }
    with
    | Ok newAppData ->
        AnsiConsole.MarkupLine "Successfully created the specified item group." 
        newAppData
    | Error err ->
        AnsiConsole.MarkupLine (sprintf $"Creation failed with message: %s{err.Message}")
        appData

let config : Command.Config =
    { Command = "create"
      Help = "Creates an item/item group."
      Function = CommandFunction.ChangesData execute}