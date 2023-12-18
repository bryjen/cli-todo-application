namespace Todo.Core.ActionTree

open System.Reflection

/// <summary>
/// Implementation of the tree that represents the path of action the user can take.
/// </summary>
type ActionTree =
    | InternalNode of string * MethodInfo option * ActionTree list 
    | Action of string * MethodInfo
    
module ActionTreeFunctions =
    
    let getAllInternalNodes actionTree =
        
        let rec getInternalNodes actionTree =
            match actionTree with
            | InternalNode(str, methodInfoOption, branches) ->
                [(str, methodInfoOption, branches)] @ List.concat (List.map getInternalNodes branches)
            | Action(_1, _2) ->
                []
                
        getInternalNodes actionTree
                
