[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Delete

open System
open Argu
open FsToolkit.ErrorHandling

open Spectre.Console
open Todo
open Todo.Cli.Utilities
            
/// <summary>
/// Parses the array of CLI arguments.
/// </summary>
let internal parse (argv: string array) : Result<ParseResults<DeleteArguments>, Exception> =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<DeleteArguments>(errorHandler = errorHandler)
    
    try
        let parsedArgs = parser.Parse argv
        Ok parsedArgs
    with
        | :? ArguParseException as ex -> Error ex
        | ex -> Error ex
        
/// <summary>
/// Attempts to <b>delete</b> the specified item from the given arguments. Returns an exception, or the updated list of
/// item groups.
/// </summary>
/// <param name="appData">The current state/data of the application.</param>
/// <param name="parseResults">Results of parsing the CLI arguments.</param>
let internal delete (appData: AppData) (parseResults: ParseResults<DeleteArguments>) : Result<ItemGroup list, Exception> =
    let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    
    let modifyResult = 
        match parseResults.TryGetResult DeleteArguments.Item_Group with
        | Some itemGroupParseResults -> // Delete an item group
            let unprocessedPath = itemGroupParseResults.GetResult DeleteItemGroupArguments.Path
            let toDelete = unprocessedPath |> List.rev |> List.head
            let path = unprocessedPath |> List.rev |> List.tail |> List.rev
            
            let modify = ItemGroup.RemoveItemGroup toDelete
            ItemGroup.Modify tempRootItemGroup modify path
        | None -> // Delete an item
            let itemParseResults = parseResults.GetResult DeleteArguments.Item
            let unprocessedPath = itemParseResults.GetResult DeleteItemArguments.Path
            let toDelete = unprocessedPath |> List.rev |> List.head
            let path = unprocessedPath |> List.rev |> List.tail |> List.rev
            
            let modify = ItemGroup.RemoveItem toDelete
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
/// Implements the logic for the 'delete' command.
/// </summary>
/// <param name="argv">List of arguments to be parsed.</param>
let execute (argv: string array) : AppData =
    let appData = match Files.loadAppData Files.filePath with | Ok appData -> appData | Error err -> raise err
    
    let deletionResult = 
        result {
            let! parsedArgs = parse argv
            let! newItemGroups = delete appData parsedArgs
            return! update appData newItemGroups
        }
        
    match deletionResult with
    | Ok newAppData ->
        AnsiConsole.MarkupLine "Successfully deleted the specified item group." 
        newAppData
    | Error err ->
        AnsiConsole.MarkupLine (sprintf $"Deletion failed with message: %s{err.Message}")
        appData // return 'old' app data
    
let config : Command.Config =
    { Command = "delete"
      Help = "Deletes an item/item group."
      Function = CommandFunction.ChangesData execute}
