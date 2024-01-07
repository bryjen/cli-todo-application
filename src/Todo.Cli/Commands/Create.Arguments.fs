namespace Todo.Cli.Commands.Arguments

open System
open Argu

open Todo.ItemGroup

/// <summary>
/// Arguments for the 'create' command.
/// </summary>
type CreateArguments =
    | [<First; CliPrefix(CliPrefix.None)>] Item_Group of ParseResults<CreateItemGroupArguments>
    | [<First; CliPrefix(CliPrefix.None)>] Item of ParseResults<CreateItemArguments>
    | [<First; CliPrefix(CliPrefix.None)>] Label of ParseResults<CreateLabelArguments>
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Item_Group _ -> "Creates an item group."
            | Item _ -> "Creates an item."
            | Label _ -> "Creates a label."
    
    
    
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



/// Arguments for the 'create label' subcommand
and CreateLabelArguments =
    | [<Unique; Mandatory; AltCommandLine("-n")>] Name of name:string
    | [<Unique; Mandatory; AltCommandLine("-c")>] Color of color:string
    
    /// <summary>
    /// Converts the parse results into a label record.
    /// </summary>
    static member ToLabel (parseResults: ParseResults<CreateLabelArguments>) : Result<Label, Exception> =
        let name = parseResults.GetResult CreateLabelArguments.Name
        let colorString = parseResults.GetResult CreateLabelArguments.Color
        Label.CreateFromString name colorString 
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "The name of the label."
            | Color _ -> "The color associated with the label."