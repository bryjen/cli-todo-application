// Types.fs
//
// Contains the type of the action tree itself, as well as the other types involved in building/verifying the action tree.

namespace Todo.ActionTree

open System.Reflection

open Todo.Attributes.ActionTree

// An intermediate representation of an action module to be able to easier verify constraints.
type internal ModuleData =
    { ActionModule: ActionModuleAttribute
      DefaultActionFunctions: MethodInfo list 
      ActionFunctions: MethodInfo list
      Attributes: PresentAttributes
       }
and PresentAttributes =
    { SingleActionAttribute: SingleActionAttribute option }

/// <summary>
/// Implementation of the tree that represents the path of action the user can take.
/// </summary>
type ActionTree =
    | InternalNode of string * MethodInfo option * ActionTree list 
    | Action of string * MethodInfo