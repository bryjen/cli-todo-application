namespace Todo

open System

[<AutoOpen>]
type Label =
    { Name: string 
      ColorComponents: ColorComponent * ColorComponent * ColorComponent } // the RGB value components of the color
    
    // Character prefixed to the label.
    // Indicates to the end user that the string/token represents a label.
    static member private Symbol: char = '#'
    
    /// <summary>
    /// Default values for a <c>Label</c> record.
    /// </summary>
    static member Default: Label =
        { Name = ""
          ColorComponents = ColorComponent.FromColor Spectre.Console.Color.Black }
    
    /// <summary>
    /// Attempts to create a <c>Label</c> record.
    /// </summary>
    /// <param name="name">The name of the label.</param>
    /// <param name="color">The color of the label.</param>
    static member Create (name: string) (color: Spectre.Console.Color) : Label =
        { Name = name; ColorComponents = ColorComponent.FromColor color }
        
    /// <summary>
    /// Attempts to create a <c>Label</c> record from a color name as string.
    /// </summary>
    /// <param name="name">The name of the label.</param>
    /// <param name="colorName">The color of the label, as a string.</param>
    static member CreateFromString (name: string) (colorName: string) : Result<Label, Exception> =
        let simplifiedColorName = colorName.ToLower().Trim()
        let asColorOption = ColorTable.tryParseFromString simplifiedColorName
        
        match asColorOption with
        | Some color -> Ok { Name = name; ColorComponents = ColorComponent.FromColor color } 
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
        let (r, g, b) = this.ColorComponents
        sprintf "[%s]%c%s[/]" ((ColorComponent.ToColor r g b).ToMarkup()) Label.Symbol this.Name
        
        
and ColorComponent =
    { Value: byte }
    
    static member Min: ColorComponent = { Value = byte 0 }
    
    static member Max: ColorComponent = { Value = byte 0 }
    
    static member FromColor (color: Spectre.Console.Color) : ColorComponent * ColorComponent * ColorComponent =
        ({ Value = color.R }, { Value = color.G }, { Value = color.B })
        
    static member ToColor (R: ColorComponent) (G: ColorComponent) (B: ColorComponent) : Spectre.Console.Color =
        Spectre.Console.Color(R.Value, G.Value, B.Value)