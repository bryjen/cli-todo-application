[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module Todo.Cli.Commands.Create

open System
open Argu

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
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "The name of the item."
            | Description _ -> "An accompanying description."
            | Path _ -> "The path of the item group to put the produced item in. Ex. [\"University\"; \"Clubs\"] will " +
                        "put the item in the \"Clubs\" item group of the \"University\" item group, if they exist."

/// Updates the app data by creating the specified item group
let rec private createItemGroup (appData: AppData) (itemGroupArgs: ParseResults<CreateItemGroupArguments>) : AppData =
    printfn "Creating item group!"
    
    // Creating the new item group
    let defaultItemGroup = ItemGroup.Default
    let newName = match (itemGroupArgs.TryGetResult CreateItemGroupArguments.Name) with | Some name -> name | None -> defaultItemGroup.Name 
    let newDesc = match (itemGroupArgs.TryGetResult CreateItemGroupArguments.Description) with | Some descOption -> descOption | None -> None
    // ...
    let newItemGroup = { defaultItemGroup with Name = newName; Description = newDesc }
    
    
    // Set all item groups to a similar parent so we can use some functions
    let tempItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
    let newItemGroupConfigOption = tempItemGroup.tryAddSubItemGroup newItemGroup (itemGroupArgs.GetResult CreateItemGroupArguments.Path)
    
    match newItemGroupConfigOption with
    | Some newItemGroupConfig ->
        printfn "Successfully created new item group!"
        { appData with ItemGroups = newItemGroupConfig.SubItemGroups }
    | None ->
        printfn "Creation failed, womp womp"
        appData
    
/// Updates the app data by creating the specified item
let private createItem (appData: AppData) (itemArgs: ParseResults<CreateItemArguments>) : AppData =
    printfn $"create item: %A{itemArgs}"
    appData

/// <summary>
/// Implements the logic for the 'list' command.
/// </summary>
/// <param name="argv">List of arguments to be parsed.</param>
let create (argv: string array) : AppData =
    
    // Loading app data
    let appData = 
        match Files.loadAppData Files.filePath with
        | Ok appData -> appData
        | Error err -> raise err
        
    // Creating the parser
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<CreateArguments>(errorHandler = errorHandler)
    
    // Tries parsing
    try
        let parsedArgs = parser.Parse argv
       
        match parsedArgs.TryGetResult Item_Group with
        | _ when (List.length (parsedArgs.GetAllResults())) = 0 ->
            printfn "%s" (parser.PrintUsage("You must enter some arguments!"))
            appData
        | None ->
            createItem appData (parsedArgs.GetResult Item) 
        | Some _ ->
            createItemGroup appData (parsedArgs.GetResult Item_Group) 
    with
        | :? ArguParseException as ex ->
            printfn $"%s{ex.Message}"
            appData
        | ex ->
            printfn "Unexpected exception"
            printfn $"%s{ex.Message}"
            appData

[<CommandInformation>]
let ``'create' Command Config`` () : Command.Config =
    { Command = "create"
      Description = "Creates something."
      Help = None
      Function = CommandFunction.ChangesData create}