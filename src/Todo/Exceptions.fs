// Exceptions.fs

namespace Todo.Exceptions

open System

module ActionTree =
    /// <summary>
    /// An exception that is thrown then the action tree builder detects some error.
    /// </summary>
    type ActionTreeException(message: string) =
        inherit Exception(message)
    
    /// <summary>
    /// An exception that is thrown when the action tree builder detects that two action modules declare the same name.
    /// </summary>
    /// <remarks>
    /// Takes in a string list containing the namespaces of the modules with the duplicate attribute name.
    /// </remarks>
    type IdenticalActionModuleNamesException(moduleName: string, namespaces: string list) =
        inherit ActionTreeException(IdenticalActionModuleNamesException.formatMessage moduleName namespaces)
            
        member val ModuleName: string = moduleName with get
        member val Namespaces: string list = namespaces with get
        
        static member formatMessage (moduleName: string) (namespaces: string list) : string =
            sprintf $"There are action modules with the name \"%s{moduleName}\" in the namespaces: %A{namespaces}."
    
    /// <summary>
    /// An exception that is thrown when the action tree builder detects that two action functions declare the same name
    /// in the same action module. 
    /// </summary>
    /// <remarks>
    /// Takes in a tuple: (name of action module * name of duplicate action functions).
    /// </remarks>
    type IdenticalActionFunctionNamesException(moduleName: string, functionName: string) =
        inherit ActionTreeException(IdenticalActionFunctionNamesException.formatMessage moduleName functionName)
        
        member val ModuleName: string = moduleName with get
        member val FunctionName: string = functionName with get
        
        static member formatMessage (moduleName: string) (functionName: string) : string =
            sprintf $"There are multiple action functions with the name \"%s{functionName}\" in the action module \"%s{moduleName}\"."
        
    /// <summary>
    /// An exception that is thrown when the action tree builder detects that an action module has no action functions
    /// declared.
    /// </summary>
    /// <remarks>
    /// Takes in the name of the invalid action module.
    /// </remarks>>
    type NoActionFunctionException(moduleName: string) =
        inherit ActionTreeException(NoActionFunctionException.formatMessage moduleName)
        
        member val ModuleName: string = moduleName with get
        
        static member formatMessage (moduleName: string) : string =
            sprintf $"There are no action functions in the action module \"%s{moduleName}\"."
            + "Please define at least one action function."
    
    /// <summary>
    /// An exception that is thrown when the action tree builder detects that there are multiple action functions in an
    /// action module that is marked with the <c>SingleAction</c> attribute (illegal usage).
    /// </summary>
    /// <remarks>
    /// Takes in the name of the invalid action module.
    /// </remarks>
    type InvalidSingleActionAttributeException(moduleName: string) =
        inherit ActionTreeException(InvalidSingleActionAttributeException.formatMessage moduleName)
        
        member val ModuleName: string = moduleName with get
        
        static member formatMessage (moduleName: string) : string =
            sprintf $"The action module \"$%s{moduleName}\" is marked with the \"SingleAction\" attribute."
            + "It should have exactly one function marked with the \"ActionFunction\" attribute."
   
    /// <summary>
    /// An exception that is thrown when the action tree builder detects that there are multiple action functions in an
    /// action module marked with the <c>DefaultAction</c> attribute (illegal usage).
    /// </summary>
    /// <remarks>
    /// Takes in the name of the invalid action module.
    /// </remarks>>
    type MultipleDefaultActionFunctionsException(moduleName: string) =
        inherit ActionTreeException(MultipleDefaultActionFunctionsException.formatMessage moduleName)
        
        member val ModuleName: string = moduleName with get
        
        static member formatMessage (moduleName: string) : string =
            sprintf $"The action module \"$%s{moduleName}\" has multiple functions marked with the \"DefaultAction\" attribute."
            + "No function or exactly one function can be marked with this attribute."
            
    /// <summary>
    /// An exception that is thrown when the action tree builder detects that there is some exception with the default
    /// action module.
    /// </summary>
    type InvalidDefaultActionModuleException() =
        inherit ActionTreeException(InvalidDefaultActionModuleException.errorMessage)
        static member errorMessage : string =
            "\n\nThere is an invalid definition of a default action module. Make sure that: " +
            "\n - There is only one module that is marked with with the \"DefaultActionFunction\" attribute" +
            "\n - The module has EXACTLY one function that is marked with both \"DefaultActionFunction\" and \"ActionFunction\" attributes.\n"
            
    /// <summary>
    /// An exception that is thrown when the user enters an invalid action.
    /// </summary>
    type InvalidActionException(argv: string array) =
        inherit ActionTreeException(InvalidActionException.formatMessage argv)
        
        member val argv: string array = argv with get
        
        static member formatMessage (argv: string array) : string =
            sprintf $"Invalid actions: %A{argv}"