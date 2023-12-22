namespace Todo.Utilities

open System
open System.Globalization

type DueDate =
    | DateDue of DateTime
    | WeekDue of int

module Week =
    
    let fromDateTime (dateTime: DateTime) : int =
        CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
        
    