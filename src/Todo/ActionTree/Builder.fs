// Builder.fs
//
// Contains functionality mainly used for building the action tree.
// Contained functionality inaccessible outside of assembly. 

namespace Todo.ActionTree

open System
open System.Reflection
open Microsoft.FSharp.Core
open FsToolkit.ErrorHandling

open Todo.Attributes.ActionTree
open Todo.Exceptions.ActionTree

/// <summary>
/// This module contains functions that build the action tree.
/// </summary>
module internal Builder =
    
    //  Given a module data record type, returns a list of action function names paired with its corresponding methodinfo
    let private getFunctionsFromModuleData
        (moduleData: ModuleData)
        : (string * MethodInfo) list =
        let names =
            moduleData.ActionFunctions
            |> List.map (fun methodInfo -> methodInfo.GetCustomAttribute(typeof<ActionFunctionAttribute>) :?> ActionFunctionAttribute)
            |> List.map (fun attribute -> attribute.Name)
            
        List.zip names moduleData.ActionFunctions
    
    // Attempts to parse the passed types into a module data record type. Does NOT return exceptions, only 'None'.
    let internal parseIntoModuleData
        (_type: Type)
        : ModuleData option =
        if _type.GetCustomAttributes(typeof<ActionModuleAttribute>, false).Length > 0 then
            let actionModule = _type.GetCustomAttribute(typeof<ActionModuleAttribute>) :?> ActionModuleAttribute
            let defaultActionFunctions = Array.toList (AttributeFinder.getDefaultActionFunctions _type)
            let actionFunctions = Array.toList (AttributeFinder.getActionFunctions _type)
            
            let singleActionAttribute =
                match _type.GetCustomAttribute(typeof<SingleActionAttribute>) with
                | null -> None
                | value -> Some (value :?> SingleActionAttribute)
                
            let presentAttributes = { SingleActionAttribute = singleActionAttribute }
            
            Some { ActionModule = actionModule
                   ActionFunctions = actionFunctions
                   DefaultActionFunctions = defaultActionFunctions
                   Attributes = presentAttributes }
        else
            None
            
    //  Gets a list of module data. Does so by searching for types with specific attributes.
    let internal getModuleDatalist
        ()
        : ModuleData list =
        AttributeFinder.getActionModuleTypes ()
        |> Array.map parseIntoModuleData
        |> Array.filter Option.isSome
        |> Array.map Option.get
        |> Array.toList
        
    //  Validates each module data in the list. If an exception is returned from any of the module data, propagate the exception.
    let internal validateModuleDataList
        (moduleDataList: ModuleData list)
        : Result<ModuleData list, ActionTreeException> =
        let validationStatuses = List.map Validation.ModuleData.validate moduleDataList
        
        let invalidModuleDataList = validationStatuses
                                    |> List.filter (fun result -> match result with | Ok _ -> false | Error _ -> true)
                                    |> List.map (fun result -> match result with | Ok _ -> failwith "error" | Error err -> err)
        
        match List.length invalidModuleDataList with
        | 0 -> Ok moduleDataList
        | _ -> Error (List.head invalidModuleDataList)
        
    //  Parses hte list of module data records into the initial action tree.
    let internal parseIntoActionTree 
        (moduleDataList: ModuleData list)
        : Result<ActionTree, ActionTreeException> =
        let leaves = moduleDataList
                     |> List.map getFunctionsFromModuleData
                     |> List.map (List.map ActionTree.Action) 
            
        let nodes = moduleDataList
                    |> List.map (fun moduleData -> (moduleData.ActionModule.ModuleName, Option<MethodInfo>.None))
            
        let paired = List.zip nodes leaves
        
        
        let branches = paired
                       |> List.map (fun pair ->
                                    let nodeInfo, leaves = pair
                                    let name, methodInfo = nodeInfo
                                    ActionTree.InternalNode (name, methodInfo, leaves))
            
        Ok (ActionTree.InternalNode ("root", None, branches))
         
        
    /// <summary>
    /// Builds the action tree.
    /// </summary>
    let buildActionTree
        ()
        : Result<ActionTree, ActionTreeException> =
        getModuleDatalist ()
        |> validateModuleDataList
        |> Result.bind validateModuleDataList
        |> Result.bind parseIntoActionTree
        |> Result.bind Validation.validateUniqueActionModuleNames 
        |> Result.bind Validation.validateDefaultModuleConfiguration 