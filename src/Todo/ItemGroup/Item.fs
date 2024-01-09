namespace Todo.ItemGroup

open System

open Todo.Utilities

type Item =
    { Name: string
      Description: string option
      DueDate: DueDate
      Labels: Label list }
    
    static member Default =
        { Name = ""
          Description = None
          DueDate = DueDate.DateDue DateTime.MinValue 
          Labels = List.empty }
        
    static member GetFormattedDueDate (item: Item) =
        match item.DueDate with
        | DateDue dateTime ->  Date.getDaysRemainingAsString dateTime 
        | WeekDue week -> Week.getWeeksRemainingAsString week 
        
    override this.ToString () =
        sprintf $"[red]%s{Item.GetFormattedDueDate this}[/] %s{this.Name}"
