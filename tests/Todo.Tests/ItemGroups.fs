[<AutoOpen>]
module Todo.Tests.ItemGroupValues

open System
open Todo.Models

let itemGroups1 =
    let comp345_a1 = { Name = "Assignment #1"; Path = "University/COMP 345/Assignments"; Description = None; DueDate = DueDate.ByDateTime DateTime.Now }
    let comp345_a2 = { Name = "Assignment #2"; Path = "University/COMP 345/Assignments"; Description = None; DueDate = DueDate.ByDateTime DateTime.Now }
    let comp345_assignments = { Name = "Assignments"; Path = "University/COMP 345"; Description = None; SubItemGroups = List.empty; Items  = [ comp345_a1; comp345_a2 ] }
    
    let comp345_m1 = { Name = "Template metaprogramming"; Path = "University/COMP 345/Material"; Description = None; DueDate = DueDate.ByDateTime DateTime.Now }
    let comp345_m2 = { Name = "Proper usage of smart pointers"; Path = "University/COMP 345/Material"; Description = None; DueDate = DueDate.ByDateTime DateTime.Now }
    let comp345_material = { Name = "Proper usage of smart pointers"; Path = "University/COMP 345"; Description = None; SubItemGroups = List.empty; Items = [ comp345_m1; comp345_m2 ] }
    
    let comp345 = { Name = "COMP 345"; Path = "University"; Description = Some "Advanced program design with C++"; SubItemGroups = [ comp345_assignments; comp345_material ]; Items = List.empty }
    
    
    let comp353_a1 = { Name = "Assignment #1"; Path = "University/COMP 353/Assignments"; Description = None; DueDate = DueDate.ByDateTime DateTime.Now }
    let comp353_a2 = { Name = "Assignment #2"; Path = "University/COMP 353/Assignments"; Description = None; DueDate = DueDate.ByDateTime DateTime.Now }
    let comp353_assignments = { Name = "Assignments"; Path = "University/COMP 353"; Description = None; SubItemGroups = List.empty; Items  = [ comp353_a1; comp353_a2 ] }
    
    let comp353_m1 = { Name = "Normal forms"; Path = "University/COMP 353/Material"; Description = None; DueDate = DueDate.ByDateTime DateTime.Now }
    let comp353_material = { Name = "Proper usage of smart pointers"; Path = "University/COMP 345"; Description = None; SubItemGroups = List.empty; Items = [ comp353_m1 ] }
    
    let comp353 = { Name = "COMP 345"; Path = "University"; Description = Some "Advanced program design with C++"; SubItemGroups = [ comp353_assignments; comp353_material ]; Items = List.empty }
    
    let university = { Name = "University"; Path = ""; Description = None; SubItemGroups = [ comp345; comp353 ]; Items = List.empty }
    
    
    let todo1 = { Name = "Add new feature re #12"; Path = "cli todo app"; Description = None; DueDate = DueDate.ByDateTime DateTime.Now }
    let todo2 = { Name = "Fix performance bug re #9"; Path = "cli todo app"; Description = None; DueDate = DueDate.ByDateTime DateTime.Now }
    let todo3 = { Name = "Plan new feature #7"; Path = "cli todo app"; Description = None; DueDate = DueDate.ByDateTime DateTime.Now }
    let cliTodoApp = { Name = "cli todo app"; Path = ""; Description = None; SubItemGroups = List.empty; Items = [ todo1; todo2; todo3 ] }
    
    [ university; cliTodoApp ]