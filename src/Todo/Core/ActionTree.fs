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
    
    // Given an action tree, get the name/string at the root node.
    let private getName actionTree =
        match actionTree with
        | InternalNode(string, _, _) -> string
        | Action(string, _) -> string
        
    //  Gets the sub action trees (branches)
    //  YOU MUST PASS IN AN INTERNAL NODE
    let private getNextActionTreeByString actionTree branchName =
        match actionTree with
        | InternalNode(_, _, branches) ->
            branches
            |> List.map getName
            |> List.zip branches
            |> List.filter (fun pair -> (snd pair) = branchName)
            |> List.map fst
        | Action _ ->
            failwith "You must pass in an 'ActionTree.InternalNode'!"
        
    
    let getAllInternalNodeData actionTree =
        
        let rec getInternalNodes actionTree =
            match actionTree with
            | InternalNode(str, methodInfoOption, branches) ->
                [(str, methodInfoOption, branches)] @ List.concat (List.map getInternalNodes branches)
            | Action(_1, _2) ->
                []
                
        getInternalNodes actionTree
                
    let rec getAction actionTree argv : Result<string * MethodInfo, ActionTreeException> =
        match actionTree with
        | InternalNode(str, methodInfoOption, _) ->
            if (Array.length argv) = 0 && Option.isSome methodInfoOption then
                Ok (str, Option.get methodInfoOption)
            elif (Array.length argv) = 0 && Option.isNone methodInfoOption then
                Error (InvalidActionException(argv)) 
            else
                //  (Array.length argv) > 0
                let nextActionTrees = getNextActionTreeByString actionTree argv[0]
                if (List.length nextActionTrees) = 1 then
                    getAction (List.head nextActionTrees) (Array.tail argv)
                else
                    Error (InvalidActionException(argv))
                    
        | Action(str, methodInfo) ->
            if (Array.length argv) = 0 then
                Ok (str, methodInfo)
            else
                Error (InvalidActionException(argv))
            
