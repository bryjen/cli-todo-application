[<RequireQualifiedAccess>]
module Todo.Cli.Commands.EditItemGroup

open System
open Argu
open FsToolkit.ErrorHandling
open Todo
open Todo.ItemGroup
open Todo.Cli.Commands.Arguments

let private addLabel (appData: AppData) (path: string list) (labelName: string) : Result<AppData, Exception> =
    let labelToAdd = List.tryFind (fun label -> label.Name = labelName) appData.Labels  // See if the label exists first

    match labelToAdd with
    | Some label ->
        result {
            let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
            let modify = ItemGroup.AddLabel label
            let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path
            return { appData with ItemGroups = newRootItemGroup.SubItemGroups } 
        }
    | None ->
        Error (Exception(sprintf $"The label \"%s{labelName}\" does not exist!"))
        
let private removeLabel (appData: AppData) (path: string list) (labelName: string) : Result<AppData, Exception> =
    result {
        let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
        let modify = ItemGroup.RemoveLabel labelName
        let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path
        return { appData with ItemGroups = newRootItemGroup.SubItemGroups } 
    }
    
let private renameItemGroup
    (appData: AppData)
    (path: string list)
    (subItemGroupName: string)
    (newName: string)
    : Result<AppData, Exception> =
    result {
        let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
        let modify = ItemGroup.RenameSubItemGroup subItemGroupName newName
        let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path 
        return { appData with ItemGroups = newRootItemGroup.SubItemGroups } 
    }

let private changeDescription (appData: AppData) (path: string list) (newDescription: string) : Result<AppData, Exception> =
    result {
        let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
        let modify = ItemGroup.ChangeDescription newDescription 
        let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path 
        return { appData with ItemGroups = newRootItemGroup.SubItemGroups } 
    }
    
let internal editItemGroupLabel
    (appData: AppData)
    (path: string list)
    (parseResults: ParseResults<LabelArguments>)
    : Result<AppData, Exception> =
        
    result {
        let labelAction = List.head (parseResults.GetAllResults())
        
        match labelAction with
        | Add labelName ->
            return! addLabel appData path labelName
        | Remove labelName ->
            return! removeLabel appData path labelName
    } 

let editItemGroup (appData: AppData) (itemGroupParseResults: ParseResults<ItemGroupArguments>) : Result<AppData, Exception> =
    let propertyToEdit = itemGroupParseResults.GetAllResults() |> List.rev |> List.head
    let path = itemGroupParseResults.TryGetResult ItemGroupArguments.Path
    
    if path.IsSome then
        match propertyToEdit with
        | ItemGroupArguments.Label innerParseResults ->
            editItemGroupLabel appData (path.Value) innerParseResults
        | ItemGroupArguments.Name newName ->
            let (itemGroupName, adjustedPath) = Todo.Utilities.List.splitLast (path.Value) 
            renameItemGroup appData adjustedPath itemGroupName newName
        | ItemGroupArguments.Description newDescription ->
            changeDescription appData (path.Value) newDescription
        | _ ->
            Error (Exception(sprintf $"oopsie: %A{itemGroupParseResults.GetAllResults()}")) 
    else
        Error (Exception("You must specify a path!"))