/// Module contains functions that generate and/or render <c>Spectre.Console</c> widgets to the console.
module Todo.UI.Components.Widgets 

open System
open Spectre.Console
    
/// <summary>
/// Writes a line across the console screen with the given text centered. 
/// </summary>
/// <param name="str">The string to be displayed. Can have markup.</param>
let generateLine (str: string) : unit =
    let rule = Rule(str)
    AnsiConsole.Write(rule)
    
/// <summary>
/// Replaces the passed string with the replacement if the string is null or empty (series of whitespaces).
/// </summary>
/// <param name="replacement">The replacement string.</param>
/// <param name="str">The string to test.</param>
let replaceIfEmpty (replacement: string) (str: string) : string =
    match String.IsNullOrWhiteSpace str with
    | true -> replacement
    | false -> str 
    

module Table =
    
    /// <summary>
    /// Sets the property values for the passed table object. 
    /// </summary>
    let setDefaultProperties (table: Spectre.Console.Table) : Spectre.Console.Table =
        table.Caption <- TableTitle("Press 'ENTER' to continue")
        table.ShowHeaders <- false
        table
    
    /// <summary>
    /// Adds the specified number of empty (no-name) columns to the table.
    /// </summary>
    /// <param name="numberOfColumns">The number of columns to add.</param>
    /// <param name="table">The table to modify.</param>
    let addColumns (numberOfColumns: uint) (table: Spectre.Console.Table) : Spectre.Console.Table =
        let numberOfColumnsAsInt = Convert.ToInt32(numberOfColumns)
        for _ in [0 .. (numberOfColumnsAsInt - 1)] do
            table.AddColumn("") |> ignore
            
        table
        
    /// <summary>
    /// Adds a row to the table.
    /// </summary>
    /// <param name="values">The values to add.</param>
    /// <param name="defaultValue">If a value is empty, replace with this value.</param>
    /// <param name="table">The table to modify.</param>
    let addRows (values: string list) (defaultValue: string) (table: Spectre.Console.Table) : Spectre.Console.Table =
        let valuesArray =
            values
            |> List.map (replaceIfEmpty defaultValue)
            |> List.toArray 
        
        table.AddRow(valuesArray) |> ignore
        table.AddEmptyRow() |> ignore
        table
        
    /// Prints the table to the console. Is vertically and horizontally aligned.
    let render (table: Spectre.Console.Table) : unit =
        let aligned = Align(table, HorizontalAlignment.Center, VerticalAlignment.Middle)
        AnsiConsole.Write(aligned)
        ()