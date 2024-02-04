namespace Todo.Models

open System

/// Represents a tοdo item.
[<AutoOpen>]
type Item =
    { Name: string
      Path: string
      Description: string option
      DueDate: DueDate }
    
    /// An item record instance with default values.
    static member Default =
        { Name = ""
          Path = ""
          Description = None
          DueDate = DueDate.ByDateTime DateTime.MinValue  }
        
    /// <inheritdoc /> 
    override this.ToString () =
        let formattedDueDate = DueDate.GetDueMessage this.DueDate 
        $"{formattedDueDate} {this.Name}"
        
    /// Returns a string representation of the current object that has markdown that can be printed with "Spectre.Console".
    member this.ToMarkupString () =
        let formattedDueDate = DueDate.GetDueMessage this.DueDate 
        $"[red]{formattedDueDate}[/] {this.Name}"