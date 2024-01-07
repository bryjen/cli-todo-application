module Todo.Cli.Commands.Edit_ItemGroup

open System
open Argu
open FsToolkit.ErrorHandling

open Todo
open Todo.ItemGroup
open Todo.Cli.Commands
open Todo.Cli.Commands.Arguments

let private addLabel (appData: AppData) (path: string list) (labelName: string) : Result<ItemGroup list, Exception> =
    let labelToAdd = List.tryFind (fun label -> label.Name = labelName) appData.Labels  // See if the label exists first

    match labelToAdd with
    | Some label ->
        result {
            let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
            let modify = ItemGroup.AddLabel label
            let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path
            return newRootItemGroup.SubItemGroups
        }
    | None ->
        Error (Exception(sprintf $"The label \"%s{labelName}\" does not exist!"))
        
let private removeLabel (appData: AppData) (path: string list) (labelName: string) : Result<ItemGroup list, Exception> =
    result {
        let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
        let modify = ItemGroup.RemoveLabel labelName
        let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path
        return newRootItemGroup.SubItemGroups
    }
    
let private renameItemGroup
    (appData: AppData)
    (path: string list)
    (subItemGroupName: string)
    (newName: string)
    : Result<ItemGroup list, Exception> =
    result {
        let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
        let modify = ItemGroup.RenameSubItemGroup subItemGroupName newName
        let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path 
        return newRootItemGroup.SubItemGroups
    }

let private changeDescription (appData: AppData) (path: string list) (newDescription: string) : Result<ItemGroup list, Exception> =
    result {
        let tempRootItemGroup = { ItemGroup.Default with SubItemGroups = appData.ItemGroups }
        let modify = ItemGroup.ChangeDescription newDescription 
        let! newRootItemGroup = ItemGroup.Modify tempRootItemGroup modify path 
        return newRootItemGroup.SubItemGroups
    }
    
let internal editItemGroupLabel
    (appData: AppData)
    (path: string list)
    (parseResults: ParseResults<LabelArguments>)
    : Result<ItemGroup list, Exception> =
        
    result {
        let labelAction = List.head (parseResults.GetAllResults())
        
        match labelAction with
        | Add labelName ->
            return! addLabel appData path labelName
        | Remove labelName ->
            return! removeLabel appData path labelName
    } 

let internal editItemGroup
    (appData: AppData)
    (parseResults: ParseResults<ItemGroupArguments>)
    : Result<ItemGroup list, Exception> =
        
    result {
        let editAction = parseResults.GetAllResults() |> List.rev |> List.head
        
        match editAction with
        | Label innerParseResults ->
            let path = parseResults.GetResult ItemGroupArguments.Path
            return! editItemGroupLabel appData path innerParseResults
        | Name newName ->
            let (itemGroupName, path) = Todo.Utilities.List.splitLast (parseResults.GetResult ItemGroupArguments.Path)
            return! renameItemGroup appData path itemGroupName newName
        | Description newDescription ->
            let path = parseResults.GetResult ItemGroupArguments.Path
            return! changeDescription appData path newDescription
        | _ ->
            failwith (sprintf $"oopsie: %A{parseResults.GetAllResults()}") |> ignore
            return appData.ItemGroups
    } 
