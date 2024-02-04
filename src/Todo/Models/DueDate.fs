namespace Todo.Models

open System
open System.Globalization
open FsToolkit.ErrorHandling

[<AutoOpen>]  
type DueDate =
    | ByDateTime of DateTime
    | ByWeek of WeekOfYear
    
    /// Returns a string message indicating when a 'DueDate' type is due. 
    static member GetDueMessage dueDate =
        match dueDate with
        | ByDateTime dateTime -> DueDate.GetDueMessageDateTime dateTime 
        | ByWeek weekOfYear -> WeekOfYear.GetDueMessage weekOfYear
        
    /// Returns a string message indicating when a 'DateTime' type is due. 
    static member private GetDueMessageDateTime dateTime =
        let timespanDifference = dateTime - DateTime.Now
        
        match timespanDifference with
        | timespan when timespan.TotalDays > 1 -> $"DUE IN {timespan.Days} DAYS" 
        | timespan when timespan.TotalHours > 1 -> $"DUE IN {timespan.Hours} HOURS" 
        | timespan when timespan.TotalMinutes > 1 -> $"DUE IN {timespan.Minutes} MINUTES" 
        | timespan when timespan.TotalSeconds > 1 -> $"DUE IN {timespan.Seconds} SECONDS" 
        | _ -> "OVERDUE" 
        
        
and WeekOfYear private =
    { WeekNumber: int 
      Year: int }
    
    /// Attempt to create a 'WeekOfYear' type from the parameters. 
    static member TryCreate weekNumber year =
        let checkWeekNumber weekNumber =
            if weekNumber >= 0 && weekNumber <= 52 then
                Ok ()
            else 
                Error(ArgumentException($"\"{weekNumber}\" is not a valid week. The value must be between 1 and 52 inclusive."))
                
        let checkYear year =
            if year >= 0 then
                Ok ()
            else
                Error(ArgumentException($"\"{year}\" is not a valid year."))
            
        result {
            do! checkWeekNumber weekNumber
            do! checkYear year
            return { WeekNumber = weekNumber; Year = year }
        }
        
    /// Converts a 'DateTime' object into a 'WeekOfYear' record.
    static member FromDateTime dateTime =
        let week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
        let year = dateTime.Year
        { WeekNumber = week; Year = year }
       
    /// Calculates the difference in weeks between two 'WeekOfYear' types. 
    static member GetWeekDifference weekOfYear1 weekOfYear2 =
        let totalWeeks1 = weekOfYear1.WeekNumber + (52 * weekOfYear1.Year)
        let totalWeeks2 = weekOfYear2.WeekNumber + (52 * weekOfYear2.Year)
        totalWeeks2 - totalWeeks1
        
    /// Returns a string message indicating when a 'WeekOfYear' type is due. 
    static member GetDueMessage weekOfYear =
        let weekDifference = WeekOfYear.GetWeekDifference weekOfYear (WeekOfYear.FromDateTime DateTime.Now)
        
        match weekDifference with
        | i when i < 0 -> "OVERDUE" 
        | i when i = 0 -> "DUE THIS WEEK"
        | i when i = 1 -> "DUE NEXT WEEK"
        | _ -> $"DUE IN {weekDifference} WEEKS"