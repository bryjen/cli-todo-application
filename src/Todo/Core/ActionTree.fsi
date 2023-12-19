namespace Todo.Core.ActionTree

open System.Reflection

open Todo.Core.Utilities.Exceptions.ActionTree

/// <summary>
/// Implementation of the tree that represents the path of action the user can take.
/// </summary>
type ActionTree =
    | InternalNode of string * MethodInfo option * ActionTree list 
    | Action of string * MethodInfo

module ActionTreeFunctions =
    
    val getAllInternalNodeData : ActionTree -> (string * MethodInfo option * ActionTree list) list
    
    val getAction : ActionTree -> string array -> Result<string * MethodInfo, ActionTreeException>
