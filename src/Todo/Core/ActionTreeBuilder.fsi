namespace Todo.Core.ActionTreeBuilder

open Todo.Core.ActionTree
open Todo.Core.Utilities.Exceptions.ActionTree

module TreeBuilding =
   
    /// <summary>
    /// Builds the action tree.
    /// </summary>
    val buildActionTree : unit -> Result<ActionTree, ActionTreeException>
            
    /// <summary>
    /// Prints a formatted representation of an action tree.
    /// </summary>
    val printActionTree : ActionTree -> unit