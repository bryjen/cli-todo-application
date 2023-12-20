// Validation.fs
//
// Contains functionality mainly for validation of the action tree and its constraints. Also contains validation logic
// for the underlying types used to create the action tree (ex. ModuleData)
// Contained functionality inaccessible outside of assembly. 

namespace Todo.ActionTree

open System
open System.Reflection
open Microsoft.FSharp.Core
open FsToolkit.ErrorHandling

open Todo.ActionTree
open Todo.Attributes.ActionTree
open Todo.Exceptions.ActionTree

//  Module containing functions that validates and enforces constraints of the action tree and its constituent module data
module internal Validation =
    
    // Nested module containing functions related to validating constraints on an module data record instances.
    module internal ModuleData =
        
        // Takes a list, filters elements that have duplicates in the list, returns a sub-list containing those duplicates.
        let private getDuplicates list =
            list
            |> List.groupBy id
            |> List.filter (fun (_, group) -> List.length group > 1)
            |> List.collect snd
            |> List.distinct
        
        //  Ensures that the action module has at the very least one action function.
        let internal ensureAtLeastOneActionFunction
            (moduleData: ModuleData)
            : Result<ModuleData, ActionTreeException> =
            match List.length moduleData.ActionFunctions with
            | 0 -> Error (NoActionFunctionException(moduleData.ActionModule.ModuleName))
            | _ -> Ok moduleData
        
        //  Ensures that the action module contains exactly 0 or 1 default action functions.
        let internal ensureCorrectNumberOfDefaultActionFunctions 
            (moduleData: ModuleData)
            : Result<ModuleData, ActionTreeException> =
            match List.length moduleData.DefaultActionFunctions with
            | 0 -> Ok moduleData
            | 1 -> Ok moduleData
            | _ -> Error (MultipleDefaultActionFunctionsException(moduleData.ActionModule.ModuleName))
            
        // Ensures that each action function in a module has distinct action function names.
        let internal ensureDistinctActionFunctionNames
            (moduleData: ModuleData)
            : Result<ModuleData, ActionTreeException> =
            let actionFunctionNames =
                moduleData.ActionFunctions
                |> List.map (fun methodInfo -> methodInfo.GetCustomAttribute(typeof<ActionFunctionAttribute>))
                |> List.map (fun actionFunctionAttribute -> (actionFunctionAttribute :?> ActionFunctionAttribute).Name)
                
            match (List.length actionFunctionNames) - (List.length (List.distinct actionFunctionNames)) with
            | 0 -> Ok moduleData    //  difference of zero means List.distinct did not remove any elements -> no dupes
            | _ -> Error (IdenticalActionFunctionNamesException(moduleData.ActionModule.ModuleName, List.head (getDuplicates actionFunctionNames)))

        //  Ensures that, if the module contains the 'SingleAction' attribute, the module follows the attribute's constraints.
        let internal validateSingleActionAttributeConstraint
            (moduleData: ModuleData)
            : Result<ModuleData, ActionTreeException> =
            if Option.isSome moduleData.Attributes.SingleActionAttribute then
                match List.length moduleData.ActionFunctions with
                | 1 -> Ok moduleData
                | _ -> Error (InvalidSingleActionAttributeException(moduleData.ActionModule.ModuleName))
            else
                Ok moduleData
                
        /// <summary>
        /// Examines the passed <c>ModuleData</c> record instance, and returns some <c>ActionTreeException</c> if there are
        /// any errors. Otherwise, returns <c>None</c>, indicating that the record is valid.
        /// </summary>
        let validate 
            (moduleData: ModuleData)
            : Result<ModuleData, ActionTreeException> =
            moduleData
            |> Ok   // wrap into monadic value
            |> Result.bind ensureAtLeastOneActionFunction
            |> Result.bind ensureCorrectNumberOfDefaultActionFunctions
            |> Result.bind ensureDistinctActionFunctionNames
            |> Result.bind validateSingleActionAttributeConstraint
                
    //  Validates a type that it matches the constraints of the default action module, that being it contains one and only
    //  one action function. 
    let private validateDefaultActionModule
        (actionTree: ActionTree)
        (actionModuleType: Type)
        : Result<ActionTree, ActionTreeException> =
            
        let defaultActionFunctions =
            actionModuleType.GetMethods()
            |> Array.filter (fun methodInfo -> methodInfo.GetCustomAttributes(typeof<DefaultActionFunctionAttribute>, false).Length > 0)
            |> Array.filter (fun methodInfo -> methodInfo.GetCustomAttributes(typeof<ActionFunctionAttribute>, false).Length > 0)

        match Array.length defaultActionFunctions with
        | 1 -> Ok actionTree 
        | _ -> Error (InvalidDefaultActionModuleException())
        
    /// <summary>
    /// Validates that an action tree has a valid default action module.
    /// </summary>
    let validateDefaultModuleConfiguration
        (actionTree: ActionTree)
        : Result<ActionTree, ActionTreeException> =
        let defaultActionModuleTypes = AttributeFinder.getDefaultActionModuleTypes ()
        match Array.length defaultActionModuleTypes with
        | 0 -> Ok actionTree
        | 1 -> validateDefaultActionModule actionTree defaultActionModuleTypes[0]
        | _ -> Error (InvalidDefaultActionModuleException())
            
    // TODO: Implement
    /// <summary>
    /// Validates no action module has the same name.
    /// </summary>
    let validateUniqueActionModuleNames
        (actionTree: ActionTree)
        : Result<ActionTree, ActionTreeException> =
        Ok actionTree 
