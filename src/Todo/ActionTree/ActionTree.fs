// ActionTree.fs
//
// Contains functionality mainly for interacting with the action tree.

namespace Todo.ActionTree

open System.Reflection

open Todo.Exceptions.ActionTree


module ActionTreeFunctions =
    
    // Given an action tree, get the name/string at the root node.
    let private getName
        (actionTree: ActionTree)
        : string =
        match actionTree with
        | InternalNode(string, _, _) -> string
        | Action(string, _) -> string
        
    //  Gets the sub action trees (branches)
    //  YOU MUST PASS IN AN INTERNAL NODE
    let private getNextActionTreeByString
        (actionTree: ActionTree)
        (branchName: string)
        : ActionTree list =
        match actionTree with
        | InternalNode(_, _, branches) ->
            branches
            |> List.map getName
            |> List.zip branches
            |> List.filter (fun pair -> (snd pair) = branchName)
            |> List.map fst
        | Action _ ->
            failwith "You must pass in an 'ActionTree.InternalNode'!"
        
    /// <summary>
    /// Builds the action tree.
    /// </summary>
    let buildActionTree
        ()
        : Result<ActionTree, ActionTreeException> =
        Builder.buildActionTree ()
    
    /// <summary>
    /// Gets a list of all the internal node data. 
    /// </summary>
    let getAllInternalNodeData
        (actionTree: ActionTree) =
        
        let rec getInternalNodes actionTree =
            match actionTree with
            | InternalNode(str, methodInfoOption, branches) ->
                [(str, methodInfoOption, branches)] @ List.concat (List.map getInternalNodes branches)
            | Action(_1, _2) ->
                []
                
        getInternalNodes actionTree
                
    /// <summary>
    /// Attempts to traverse through the passed action tree with the given input.
    /// </summary>
    let rec getAction
        (actionTree: ActionTree)
        (argv: string array)
        : Result<string * MethodInfo, ActionTreeException> =
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
                
    /// <summary>
    /// Prints a formatted representation of an action tree.
    /// </summary>
    let printActionTree
        (actionTree: ActionTree)
        : unit =
    
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
