namespace Todo

open System
open Spectre.Console

[<AutoOpen>]
type Label =
    { Name: string 
      Color: Spectre.Console.Color }
    
    // Character prefixed to the label.
    // Indicates to the end user that the string/token represents a label.
    static member private Symbol: char = '@' 
    
    /// <summary>
    /// Attempts to create a <c>Label</c> record.
    /// </summary>
    /// <param name="name">The name of the label.</param>
    /// <param name="color">The color of the label.</param>
    static member Create (name: string) (color: Color) : Label =
        { Name = name; Color = color }
        
    /// <summary>
    /// Attempts to create a <c>Label</c> record from a color name as string.
    /// </summary>
    /// <param name="name">The name of the label.</param>
    /// <param name="colorName">The color of the label, as a string.</param>
    static member CreateFromString (name: string) (colorName: string) : Result<Label, Exception> =
        let simplifiedColorName = colorName.ToLower().Trim()
        let asColorOption = ColorTable.tryParseFromString simplifiedColorName
        
        match asColorOption with
        | Some color -> Ok { Name = name; Color = color } 
        | None -> Error (Exception(sprintf $"Unknown color: %s{colorName}"))
        
    static member FormatLabels (labels: Label list) : string =
        labels
        |> List.map (fun label -> label.ToString())
        |> (fun labelStrings -> (" ", labelStrings)) // first part of tuple is the joining character
        |> String.Join 
        
    /// <summary>
    /// Returns the label with <c>Spectre.Console</c> markups.
    /// </summary>
    override this.ToString () : string =
        sprintf "[%s]%c%s[/]" (this.Color.ToMarkup()) Label.Symbol this.Name