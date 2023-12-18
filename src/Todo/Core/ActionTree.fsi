namespace Todo.Core.ActionTree

open System.Reflection


/// <summary>
/// Implementation of the tree that represents the path of action the user can take.
/// </summary>
type ActionTree =
    | InternalNode of string * MethodInfo option * ActionTree list 
    | Action of string * MethodInfo

module ActionTreeFunctions =
    
    val getAllInternalNodes : ActionTree -> (string * MethodInfo option * ActionTree list) list 
