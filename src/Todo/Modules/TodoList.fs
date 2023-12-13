module Todo.Modules.TodoList

open System

type Todo = {
    Name: string
    Description: string
    DueDate: DateTime
}



type TodoGroup = {
    Name: string
    Description: string
    Todos: Todo list
}


module Todo =
    /// Tries to create a new tοdo item given information from a tοdo group. If the item can be created, a new instance is
    /// returned, otherwise nothing is returned.
    let tryCreateTodo (todoGroup: TodoGroup) (name: string, description: string, dueDate: DateTime) : Todo option =
        let equalTodos = List.filter (fun (todo: Todo) -> todo.Name = name) todoGroup.Todos
        
        match (List.length equalTodos) with
        | 0 -> Some { Name = name; Description = description; DueDate = dueDate }
        | _ -> None

    /// <summary>
    /// Prepends a record type instance to the passed tοdo group record. Returns the newly constructed tοdo group record.
    /// </summary>
    /// <remarks>
    /// Make sure that another tοdo item of the same name exists already, as it can cause confusions and complications.
    /// Try using <see cref="tryCreateTodo"/> to see if a tοdo with the same name already exists.
    /// </remarks>
    let addTodo (todoGroup: TodoGroup) (todo: Todo) : TodoGroup =
        let newTodoList = todo :: todoGroup.Todos
        { todoGroup with Todos = newTodoList }
    
    /// Returns a new tοdo record group with specified tοdos removed. 
    let deleteTodos (todoGroup: TodoGroup) ([<ParamArray>] todosToDelete: string array) =
        let newTodoList = List.filter (fun (todo: Todo) -> not (Array.contains todo.Name todosToDelete)) todoGroup.Todos
        { todoGroup with Todos = newTodoList }
        
    /// Returns a string representation of a tοdo record.
    let toString (todo: Todo) : string =
        sprintf $"%s{todo.Name} | %s{todo.DueDate.ToString()}"
        
        
        
module TodoGroup =
    /// Tries to create a course group. Takes in a list of course groups. If the given name is already taken, then the
    /// course group cannot be created.
    let tryCreateTodoGroup (todoGroupsList: TodoGroup list) (name: string) (description: string) : TodoGroup option =
        let equalGroups = List.filter (fun (todoGroup: TodoGroup) -> todoGroup.Name = name) todoGroupsList
        
        match (List.length equalGroups) with
        | 0 -> Some { Name = name; Description = description; Todos = [] }
        | _ -> None 
    
    /// Tries to get a specific course group by name from a list of course groups.
    let tryGetTodoGroup (todoGroupsList: TodoGroup list) (name: string) : TodoGroup option =
        let equalGroups = List.filter (fun (todoGroup: TodoGroup) -> todoGroup.Name = name) todoGroupsList
       
        match (List.length equalGroups) with
        | 0 -> Some (List.head todoGroupsList)
        | _ -> None
        
    /// Returns a string representation of a tοdo group.
    let toString (todoGroup: TodoGroup) : string =
        sprintf $"%s{todoGroup.Name} | %d{List.length todoGroup.Todos} pending todos"