[<Microsoft.FSharp.Core.AutoOpen>]
[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Create

open System
open Argu
open Spectre.Console
open FsToolkit.ErrorHandling

open Todo
open Todo.Cli.Utilities
open Todo.Utilities.Attributes.Command

// TODO: add support for label creation

/// <summary>
/// Arguments for the 'create' command.
/// </summary>
type CreateArguments =
    | [<First; CliPrefix(CliPrefix.None)>] Item_Group of ParseResults<CreateItemGroupArguments>
    | [<First; CliPrefix(CliPrefix.None)>] Item of ParseResults<CreateItemArguments>
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Item_Group _ -> "Creates an item group."
            | Item _ -> "Creates an item."
    
/// Arguments for the 'create item-group' subcommand
and CreateItemGroupArguments =
    | [<Unique; Mandatory; AltCommandLine("-n")>] Name of name:string
    | [<Unique; AltCommandLine("-d")>] Description of description:string option
    | [<Unique; Mandatory; AltCommandLine("-p")>] Path of path:string list
    
    /// <summary>
    /// Converts the parse results into a item group record.
    /// </summary>
    static member ToItemGroup (parseResults: ParseResults<CreateItemGroupArguments>) : ItemGroup =
        let defaultItemGroup = ItemGroup.Default
        let newName = match (parseResults.TryGetResult CreateItemGroupArguments.Name) with | Some name -> name | None -> defaultItemGroup.Name 
        let newDesc = match (parseResults.TryGetResult CreateItemGroupArguments.Description) with | Some descOption -> descOption | None -> None
        // ...
        { defaultItemGroup with Name = newName; Description = newDesc }
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "The name of the item group."
            | Description _ -> "An accompanying description."
            | Path _ -> "The path of the item group to put the produced item in.\n" +
                        "\t - [ ] Places the item group at the top, with no 'parent' item group.\n" +
                        "\t - [\"University\"; \"2nd Sem\"] Places the item group inside the \"2nd Sem\" item group inside" +
                        "the \"University\" item group, if they exist." 
            
/// Arguments for the 'create item' subcommand
and CreateItemArguments =
    | [<Unique; Mandatory; AltCommandLine("-n")>] Name of name:string
    | [<Unique; AltCommandLine("-d")>] Description of description:string option
    | [<Unique; Mandatory; AltCommandLine("-p")>] Path of path:string list
    
    /// <summary>
    /// Converts the parse results into a item record.
    /// </summary>
    static member ToItem (parseResults: ParseResults<CreateItemArguments>) : Item =
        let defaultItem = Item.Default
        let newName = match (parseResults.TryGetResult CreateItemArguments.Name) with | Some name -> name | None -> defaultItem.Name 
        let newDesc = match (parseResults.TryGetResult CreateItemArguments.Description) with | Some descOption -> descOption | None -> None
        { defaultItem with Name = newName; Description = newDesc }
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "The name of the item."
            | Description _ -> "An accompanying description."
            | Path _ -> "The path of the item group to put the produced item in. Ex. [\"University\"; \"Clubs\"] will " +
                        "put the item in the \"Clubs\" item group of the \"University\" item group, if they exist."
        
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
    match parseResults.TryGetResult CreateArguments.Item_Group with
    // create item group
    | Some itemGroupParseResults -> 
        let newItemGroup = CreateItemGroupArguments.ToItemGroup itemGroupParseResults 
        let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
        let newRootItemGroupOption = tempRootItemGroup.tryAddSubItemGroup newItemGroup (itemGroupParseResults.GetResult CreateItemGroupArguments.Path)
        
        match newRootItemGroupOption with
        | Some itemGroup -> Ok itemGroup.SubItemGroups
        | None -> Error (Exception("Could not create item group."))
        
    // create item 
    | None ->
        let itemParseResults = parseResults.GetResult CreateArguments.Item  //  guaranteed to be ok
        let newItem = CreateItemArguments.ToItem itemParseResults 
        let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
        let newRootItemGroupOption = tempRootItemGroup.tryAddItem newItem (itemParseResults.GetResult CreateItemArguments.Path)
        
        match newRootItemGroupOption with
        | Some itemGroup -> Ok itemGroup.SubItemGroups
        | None -> Error (Exception("Could not create item."))
        
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