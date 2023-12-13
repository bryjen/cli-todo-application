/// Attributes.fs

module Todo.Utilities.Attributes

open System
open Microsoft.FSharp.Core

/// <summary>
/// 
/// </summary>
[<AttributeUsage(AttributeTargets.Field, AllowMultiple = false)>]
type DefaultValueOfAttribute(recordType: Type) =
    inherit Attribute()
   
    // The record type of the default value 
    member val Type: Type = recordType


[<AttributeUsage(AttributeTargets.Module, AllowMultiple = false)>]
type ActionModuleAttribute(moduleName: string) =
    inherit Attribute()
   
    /// The name of the module.  
    member val Name = moduleName


[<AttributeUsage(AttributeTargets.Method, AllowMultiple = false)>]
type ActionFunctionAttribute(action: string, prompt: string) =
    inherit Attribute()
    
    /// The name of the action being performed.
    member val Action = action
    
    /// The prompt that is displayed to the user. 
    member val Prompt = prompt



/// <summary>
/// <para>
/// An attribute that is meant to be used on functions that represent a specific action that a user can take. Contains
/// properties to separate code between modules. This allows action functions to be dynamically obtained and mapped
/// using reflection at runtime. This makes it so that you do not have to hard code actions, which reduces verbosity and
/// makes additions/removals less tedious.
/// </para>
/// <para>
/// For an example implementation, see <see cref="Modules.Data.TodoList.Interface.fs"/>.
/// </para>
/// </summary>
/// <remarks>
/// These functions are not usually <b>not pure</b> as most of these functions have code that take user input.
/// </remarks>
[<Sealed>]
[<AttributeUsage(AttributeTargets.Method, AllowMultiple = false)>]
type ActionSignatureAttribute(_module: string, action: string, prompt: string) =
    inherit Attribute()
    
    /// The subset of the application where the action function is located.
    /// Ex. The 'TodoList' module of the application has the module value 'list'
    member val Module = _module
    
    /// The specific action inside a module. This value is unique among a module.
    member val Action = action 
    
    /// The prompt that is displayed to the user.
    member val Prompt = prompt