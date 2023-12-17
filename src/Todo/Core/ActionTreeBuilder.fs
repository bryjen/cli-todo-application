module Todo.Core.ActionTreeBuilder

open System
open System.Reflection
open Microsoft.FSharp.Core
open Todo.Core.Utilities.Attributes.ActionTree
open Todo.Core.Utilities.Exceptions.ActionTree

type ActionTree =
    | InternalNode of string * MethodInfo option * ActionTree list 
    | Action of string * MethodInfo
    
    
module AttributeFinder =
    /// <summary>
    /// Gets all modules with the attribute "ActionModule".
    /// </summary>
    /// <remarks>
    /// Any other source construct other than a module (UnionTypes, RecordTypes, etc.) will be ignored.
    /// </remarks>>
    let getActionModuleTypes () : Type array =
        
        let hasCompilationMappingAttribute (sourceConstructFlag: SourceConstructFlags) (_type: Type) : bool =
            if _type.GetCustomAttributes(typeof<CompilationMappingAttribute>, false).Length > 0 then
               let foundAttribute = _type.GetCustomAttribute(typeof<CompilationMappingAttribute>) :?> CompilationMappingAttribute
               foundAttribute.SourceConstructFlags = sourceConstructFlag 
            else
                false
        
        Assembly.GetExecutingAssembly().GetTypes()
        |> Array.filter (fun _type -> _type.IsClass)
        |> Array.filter (fun _type -> _type.GetCustomAttributes(typeof<ActionModuleAttribute>, false).Length > 0)
        |> Array.filter (hasCompilationMappingAttribute SourceConstructFlags.Module)
    
    /// <summary>
    /// Gets all functions inside a module that have the "ActionFunction" attribute.
    /// </summary>
    let getActionFunctions (_type: Type) =
        _type.GetMethods()
        |> Array.filter (fun methodInfo -> methodInfo.GetCustomAttributes(typeof<ActionFunctionAttribute>, false).Length > 0)
        
    /// <summary>
    /// Gets all functions inside a module that have the "DefaultActionFunction" AND "ActionFunction" attribute.
    /// </summary>
    let getDefaultActionFunctions (_type: Type) =
        _type.GetMethods()
        |> Array.filter (fun methodInfo -> methodInfo.GetCustomAttributes(typeof<DefaultActionFunctionAttribute>, false).Length > 0)
        |> Array.filter (fun methodInfo -> methodInfo.GetCustomAttributes(typeof<ActionFunctionAttribute>, false).Length > 0)
        
    let getActionFunctionName (methodInfo: MethodInfo) =
        (methodInfo.GetCustomAttribute(typeof<ActionFunctionAttribute>, false) :?> ActionFunctionAttribute).Name
        
    let getActionModuleName (_type: Type) =
        (_type.GetCustomAttribute(typeof<ActionModuleAttribute>, false) :?> ActionModuleAttribute).ModuleName
        
        
    
            
        
        
module TreeBuilding =
   
    open AttributeFinder
        
    // An intermediate representation of an action module to be able to easier verify constraints.
    type internal ModuleData =
        { ActionModule: ActionModuleAttribute
          DefaultActionFunctions: MethodInfo list 
          ActionFunctions: MethodInfo list
          Attributes: PresentAttributes
           }
    and PresentAttributes =
        { SingleActionAttribute: SingleActionAttribute option }
   
    // Takes a list, filters elements that have duplicates in the list, returns a sub-list containing those duplicates
    let private getDuplicates list =
        list
        |> List.groupBy id
        |> List.filter (fun (_, group) -> List.length group > 1)
        |> List.collect snd
        |> List.distinct
       
    // Given a <c>ModuleData</c> object, returns a list of pairs containing an action function name and its
    // corresponding method info.
    let private getFunctionsFromModuleData (moduleData: ModuleData) : (string * MethodInfo) list =
        let names =
            moduleData.ActionFunctions
            |> List.map (fun methodInfo -> methodInfo.GetCustomAttribute(typeof<ActionFunctionAttribute>) :?> ActionFunctionAttribute)
            |> List.map (fun attribute -> attribute.Name)
            
        List.zip names moduleData.ActionFunctions
        
    // Given a <c>ModuleData</c> object, returns a tuple containing the name of the action module and a <c>None</c>.
    // Used when parsing a ModuleData object into information used for the final ActionTree object. Internal nodes have,
    // by default, no method attached to them hence
    let private getModuleNamesFromModuleData (moduleData: ModuleData) : string * MethodInfo option =
        (moduleData.ActionModule.ModuleName, None)
    
    /// <summary>
    /// Attempts to parse the given type into a <c>ModuleData</c> record type.
    /// </summary>
    /// <remarks>
    /// <ul>
    /// <li>The function fails if the passed type does not contain the <c>ActionModule</c> attribute.</li>
    /// </ul>
    /// </remarks>
    let internal parseIntoModuleData (_type: Type) : ModuleData option =
        if _type.GetCustomAttributes(typeof<ActionModuleAttribute>, false).Length > 0 then
            
            let actionModule = _type.GetCustomAttribute(typeof<ActionModuleAttribute>) :?> ActionModuleAttribute
            let defaultActionFunctions = Array.toList (getDefaultActionFunctions _type)
            let actionFunctions = Array.toList (getActionFunctions _type)
            
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
        
    /// <summary>
    /// Examines the passed <c>ModuleData</c> record instance, and returns some <c>ActionTreeException</c> if there are
    /// any errors. Otherwise, returns <c>None</c>, indicating that the record is valid.
    /// </summary>
    let internal validateModuleData (moduleData: ModuleData) : ActionTreeException option =
        //  Store into more descriptive local variables
        let moduleName = moduleData.ActionModule.ModuleName
        let actionFunctionsCount = List.length moduleData.ActionFunctions
        let defaultActionFunctionsCount = List.length moduleData.DefaultActionFunctions
        
        let actionFunctionNames =
            moduleData.ActionFunctions
            |> List.map (fun methodInfo -> methodInfo.GetCustomAttribute(typeof<ActionFunctionAttribute>))
            |> List.map (fun actionFunctionAttribute -> (actionFunctionAttribute :?> ActionFunctionAttribute).Name)
            
            
        //  Validation logic for individual action modules
        if defaultActionFunctionsCount > 1 then
            Some (MultipleDefaultActionFunctionsException(moduleName))
            
        elif (Option.isSome moduleData.Attributes.SingleActionAttribute) && (actionFunctionsCount <> 1) then
            Some (InvalidSingleActionAttributeException(moduleName))
            
        elif (actionFunctionsCount = 0) then
            Some (NoActionFunctionException(moduleName))
            
        //  We compare the lengths of the list with the same list, but all duplicate elements removed.
        //  Inequality implies action functions with duplicate names
        elif (List.length actionFunctionNames <> List.length (List.distinct actionFunctionNames)) then
            Some (IdenticalActionFunctionNamesException(moduleName, List.head (getDuplicates actionFunctionNames)))
            
        else
            None
            
    /// <summary>
    /// Parses the provided list of <c>ModuleData</c> into an <c>ActionTree</c> instance. Note that this instance is
    /// <b>NOT</b> valid yet, and needs to be re-validated for errors that can occur between modules (ex. two modules
    /// sharing the same name).
    /// </summary>
    let internal parseIntoActionTree (modulesData: ModuleData list) : ActionTree =
        let leaves =
            modulesData
            |> List.map getFunctionsFromModuleData
            |> List.map (List.map ActionTree.Action) 
            
        let nodes =
            modulesData
            |> List.map (fun moduleData -> (moduleData.ActionModule.ModuleName, Option<MethodInfo>.None))
            
        let paired = List.zip nodes leaves
        
        let branches =
            paired
            |> List.map (fun pair ->
                let nodeInfo, leaves = pair
                let name, methodInfo = nodeInfo
                ActionTree.InternalNode (name, methodInfo, leaves))
            
        ActionTree.InternalNode ("root", None, branches)
        
    /// <summary>
    /// Builds the action tree.
    /// </summary>
    let buildActionTree () : Result<ActionTree, ActionTreeException> =
        let modulesData =
            getActionModuleTypes ()
            |> Array.toList
            |> List.map parseIntoModuleData
            |> List.filter Option.isSome
            |> List.map Option.get
            
        let validStatus =
            modulesData
            |> List.map validateModuleData
            
            
        if (List.exists Option.isSome validStatus) then
            Error (validStatus
                   |> List.filter Option.isSome
                   |> List.head
                   |> Option.get)
        else
            Ok (parseIntoActionTree modulesData)
            
    /// <summary>
    /// Prints a formatted representation of an action tree.
    /// </summary>
    let printActionTree (actionTree: ActionTree) =
        
        // recursive helper function
        let rec print actionTree depth : unit =
            let tabs = String.replicate depth "\t"
            
            match actionTree with
            | InternalNode(str, _, actionTrees) ->
                let functionInformation = $"(FUNC: \"%s{str}\")"
                printfn $"%s{tabs}{str} {functionInformation}"
                
                for branch in actionTrees do
                    print branch (depth + 1)
                    
            | Action(str, _) ->
                let functionInformation = $"(FUNC: \"%s{str}\")"
                printfn $"%s{tabs}{str} {functionInformation}"
            
        print actionTree 0