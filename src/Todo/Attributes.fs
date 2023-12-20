// Attributes.fs

namespace Todo.Attributes

open System
open Microsoft.FSharp.Core

module ActionTree =
    /// <summary>
    /// Specifies that a module is an action module.
    /// </summary>
    [<AttributeUsage(AttributeTargets.Class, AllowMultiple = false)>]
    type ActionModuleAttribute(moduleName: string) =
        inherit Attribute()
       
        /// The name of the action module.  
        member val ModuleName: string = moduleName with get
  
    /// <summary>
    /// Specifies that a module is the <b>default</b> action module. The function marked with the
    /// <c>DefaultActionFunction</c> attribute is called when no input is passed in by the user.
    /// </summary>
    /// <remarks>
    /// <ol>
    /// <li>
    /// A module marked with this attribute <b>HAS</b> to have exactly one function that is marked with the
    /// <c>DefaultActionFunction</c> attribute.
    /// </li>
    /// </ol> 
    /// </remarks>
    [<AttributeUsage(AttributeTargets.Class, AllowMultiple = false)>]
    type DefaultActionModuleAttribute() =
        inherit Attribute()
    
    /// <summary>
    /// Specifies that an action module is meant to only have one and only one action function. 
    /// </summary>
    [<AttributeUsage(AttributeTargets.Class, AllowMultiple = false)>]
    type SingleActionAttribute() =
        inherit Attribute() 
        
    /// <summary>
    /// Specifies that a function is an action function.
    /// </summary>
    [<AttributeUsage(AttributeTargets.Method, AllowMultiple = false)>]
    type ActionFunctionAttribute(name: string, prompt: string) =
        inherit Attribute()
        
        /// The name of the action being performed.
        member val Name: string = name with get 
        
        /// The prompt that is displayed to the user. 
        member val Prompt: string = prompt with get
        
    /// <summary>
    /// Specifies that a function is meant to act as the "default function" of the action module.
    /// </summary>
    [<AttributeUsage(AttributeTargets.Method, AllowMultiple = false)>]
    type DefaultActionFunctionAttribute() =
        inherit Attribute()
        
    
[<AttributeUsage(AttributeTargets.Field, AllowMultiple = false)>]
type DefaultValueOfAttribute(recordType: Type) =
    inherit Attribute()
   
    // The record type of the default value 
    member val Type: Type = recordType



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
[<Obsolete>]
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