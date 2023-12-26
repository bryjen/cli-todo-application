[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Delete

open System
open Argu

open Todo
open Todo.Cli.Utilities
open Todo.Utilities.Attributes.Command

type DeleteArguments =
    | [<First; CliPrefix(CliPrefix.None)>] Item_Group of ParseResults<DeleteItemGroupArguments>
    | [<First; CliPrefix(CliPrefix.None)>] Item of ParseResults<DeleteItemArguments>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Item_Group _ -> "Deletes an item group."
            | Item _ -> "Deletes an item."
            
//  BUG: What looks like to be some Argu parsing bug: cannot parse 'Path' if it is the sole argument
and DeleteItemGroupArguments =
    | [<Hidden>] Hidden 
    | [<ExactlyOnce; AltCommandLine("-p")>] Path of path:string list
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Hidden -> ""
            | Path _ -> "The path of the item group to delete."
    
and DeleteItemArguments =
    | [<Hidden>] Hidden 
    | [<Unique; Mandatory; AltCommandLine("-p")>] Path of path:string list
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Hidden -> ""
            | Path _ -> "The path of the item to delete. The last token is the item, and the tokens before that are item groups.\n" +
                        "Ex. [\"University\"; \"Assignment #1\"] will delete the item 'Assignment #1' in the item group 'University'" 
            
/// Updates the app data by creating the specified item group
let private deleteItemGroup (appData: AppData) (itemGroupArgs: ParseResults<DeleteItemGroupArguments>) : AppData =
    // Set all item groups to a similar parent so we can use some functions
    let tempItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    let newItemGroupConfigOption = tempItemGroup.tryDeleteSubItemGroup (itemGroupArgs.GetResult DeleteItemGroupArguments.Path)
    
    match newItemGroupConfigOption with
    | Some newItemGroupConfig ->
        printfn "Successfully deleted the item group!"
        { appData with ItemGroups = newItemGroupConfig.SubItemGroups }
    | None ->
        printfn "Deletion of the item group failed, doo doo"
        appData
    
/// Updates the app data by creating the specified item
let private deleteItem (appData: AppData) (itemArgs: ParseResults<DeleteItemArguments>) : AppData =
    // Set all item groups to a similar parent so we can use some functions
    let tempItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    let newItemGroupConfigOption = tempItemGroup.tryDeleteItem (itemArgs.GetResult DeleteItemArguments.Path)
    
    match newItemGroupConfigOption with
    | Some newItemGroupConfig ->
        printfn "Successfully deleted the item!"
        { appData with ItemGroups = newItemGroupConfig.SubItemGroups }
    | None ->
        printfn "Deletion of the item failed, doo doo"
        appData
            
/// <summary>
/// Implements the logic for the 'list' command.
/// </summary>
/// <param name="argv">List of arguments to be parsed.</param>
let delete (argv: string array) : AppData =
    
    // Loading app data
    let appData = 
        match Files.loadAppData Files.filePath with
        | Ok appData -> appData
        | Error err -> raise err
        
    // Creating the parser
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<DeleteArguments>(errorHandler = errorHandler)
    
    // Tries parsing
    try
        let parsedArgs = parser.Parse argv
       
        match parsedArgs.TryGetResult Item_Group with
        | _ when (List.length (parsedArgs.GetAllResults())) = 0 ->
            printfn "%s" (parser.PrintUsage("You must enter some arguments!"))
            appData
        | None ->
            deleteItem appData (parsedArgs.GetResult Item) 
        | Some _ ->
            deleteItemGroup appData (parsedArgs.GetResult Item_Group) 
    with
        | :? ArguParseException as ex ->
            printfn $"%s{ex.Message}"
            appData
        | ex ->
            printfn "Unexpected exception"
            printfn $"%s{ex.Message}"
            appData

[<CommandInformation>]
let ``'delete' Command Config`` () : Command.Config =
    { Command = "delete"
      Help = "Deletes an item/item group."
      Function = CommandFunction.ChangesData delete}
