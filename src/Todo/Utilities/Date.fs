namespace Todo.Utilities

open System
open System.Globalization

type DueDate =
    | DateDue of DateTime
    | WeekDue of int
    
module Date =
    let getDaysRemainingAsString (dateTime: DateTime) =
        let difference = dateTime - DateTime.Now
        match difference with
        | timeSpan when timeSpan.Minutes < 0 -> "OVERDUE"
        | timeSpan when timeSpan.Hours <= 24 && timeSpan.Days < 1 -> $"DUE IN %d{timeSpan.Hours} HOURS"
        | timeSpan -> $"DUE IN %d{timeSpan.Days} DAYS"

module Week =
    
    let fromDateTime (dateTime: DateTime) : int =
        CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
        
    let getWeeksRemainingAsString (week: int) =
        let difference = week - (fromDateTime DateTime.Now)
        match difference with
        | value when value < 0 -> "OVERDUE"
        | value when value = 0 -> "DUE THIS WEEK"
        | value when value = 1 -> $"DUE IN %d{value} WEEK"
        | value -> $"DUE IN %d{value} WEEKS"
