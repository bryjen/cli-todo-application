/// FunctionMapper.fs
///
/// Contains functions that map action functions to their unique string representations. This allows dynamic addition 
/// of functionality instead of having them hardcoded.

module Todo.Modules.Data.FunctionMapper

open System
open System.Reflection
open Todo.Core.Utilities.Attributes

/// <summary>
/// Returns all functions with the <c>ActionSignatureAttribute</c> attribute. 
/// </summary>
let getFunctionsWithActionSignatureAttribute () : MethodInfo array =
    Assembly.GetExecutingAssembly().GetTypes()
    |> Array.collect (fun _type -> _type.GetMethods())
    |> Array.filter (fun func -> func.GetCustomAttributes(typeof<ActionSignatureAttribute>, true).Length > 0)
    
/// <summary>
/// Returns all functions with the <c>ActionSignatureAttribute</c> attribute with the specified module type. 
/// </summary>
/// <param name="_module">The module value of the function to match.</param>
let getFunctionsWithModule (_module: string) : MethodInfo array =
    let attributeFilter = (fun (func: MethodInfo) ->
        let attribute = func.GetCustomAttribute(typeof<ActionSignatureAttribute>) :?> ActionSignatureAttribute
        attribute.Module = _module)
    
    getFunctionsWithActionSignatureAttribute ()
    |> Array.filter attributeFilter
   
let createModuleActionMap (_module: string) : Map<string, MethodInfo> =
    //  Get the functions / MethodInfo objects
    let functions = getFunctionsWithModule _module
    
    //  Get the corresponding prompts for the functions
    let promptStrings = Array.map
                            (fun (func: MethodInfo) ->
                                let attribute = func.GetCustomAttribute(typeof<ActionSignatureAttribute>) :?> ActionSignatureAttribute
                                attribute.Prompt)
                            functions
                           
    //  Pair them together & create a map 
    Map.ofArray (Array.zip promptStrings functions) 