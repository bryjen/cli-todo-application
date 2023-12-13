/// Console.fs
///
/// Contains functions that manipulate the console to provided formatted output, interactive prompts, etc.
/// Extensively uses the library 'spectreconsole' (https://github.com/spectreconsole/spectre.console).

module Todo.Utilities.Console

open System
open System.Globalization
open Microsoft.FSharp.Core

open Spectre.Console

open Todo.Utilities.Attributes


/// Unboxes an optional reference type. Returns null or actual value.
/// todo re-write into extensions module whenever possible
let private unboxNullable<'T when 'T : null> (option: 'T option) =
    match option with
    | None -> null 
    | Some value -> value


/// <summary>
/// Utility module containing wrapper types and functions for interoping with <c>Spectre.Console.SelectionPrompt</c>
/// objects. 
/// </summary>
module SelectionPrompt = 
    /// <summary>
    /// Record class for representing the configuration on how a <b>selection</b> prompt is presented to the user.
    /// </summary>
    type SelectionPromptConfig =
        { PageSize:                int
          WrapAround:              bool
          HighlightStyle:          Style        option
          DisabledStyle:           Style        option
          MoreChoicesText:         string       option
          Mode:                    SelectionMode }
    
    /// <summary>
    /// Record class containing information on what choices are there and how they are presented to the user.
    /// </summary>
    type SelectionPromptTypeConfig<'T> =
        { Choices:                 'T list
          Converter:               ('T -> string)            option}
        
    [<DefaultValueOf(typeof<SelectionPromptConfig>)>]
    let defaultSelectionPromptConfig =
        { PageSize = 10
          WrapAround = false
          HighlightStyle = Some (Style(foreground = Color.Plum3))
          DisabledStyle = None
          MoreChoicesText = None
          Mode = SelectionMode.Leaf }
        
    /// <summary>
    /// Wraps the specified choices into a <c>SelectionPromptTypeConfig</c> record with default values.
    /// </summary>
    let wrapChoices<'T> (choices: 'T list) : SelectionPromptTypeConfig<'T> =
        { Choices = choices; Converter = None; }
        
    /// <summary>
    /// Builds a <c>Spectre.Console.SelectionPrompt</c> object from the given configuration.
    /// </summary>
    let buildPrompt<'T> (config: SelectionPromptConfig) (typeConfig: SelectionPromptTypeConfig<'T>) (prompt: string) =
        let selectionPrompt = SelectionPrompt<'T>()
                                  .AddChoices(System.Collections.Generic.List<'T>(typeConfig.Choices))
        selectionPrompt.Title <- prompt 
        selectionPrompt.PageSize <- config.PageSize
        selectionPrompt.WrapAround <- config.WrapAround
        selectionPrompt.HighlightStyle <- unboxNullable config.HighlightStyle
        selectionPrompt.DisabledStyle <- unboxNullable config.DisabledStyle
        selectionPrompt.MoreChoicesText <- unboxNullable config.MoreChoicesText
        selectionPrompt.Mode <- config.Mode
        
        selectionPrompt.Converter <-
            match typeConfig.Converter with
            | None -> null 
            | Some func ->  Func<_, string>(func)
        
        selectionPrompt
        
    /// <summary>
    /// Builds a <c>Spectre.Console.SelectionPrompt</c> object with the default style configuration. 
    /// </summary>
    /// <param name="choices"> A list of options that the user can select. </param>
    /// <param name="prompt"> The prompt that is displayed to the user. </param>
    /// <returns> One of the objects in 'choices' </returns>
    let defaultPrompt<'T> (choices: 'T list) (prompt: string) : SelectionPrompt<'T> =
        buildPrompt defaultSelectionPromptConfig (wrapChoices choices) prompt
    
    
    
/// <summary>
/// Utility module containing wrapper types and functions for interoping with <c>Spectre.Console.TextPrompt</c>
/// objects. 
/// </summary>
module TextPrompt =
    /// <summary>
    /// Record class for representing the configuration on how a <b>text</b> prompt is presented to the user.
    /// </summary>
    type TextPromptConfig =
        { PromptStyle:             Style        option
          Culture:                 CultureInfo  option
          InvalidChoiceMessage:    string
          IsSecret:                bool
          Mask:                    char         option
          ValidationErrorMessage:  string
          ShowChoices:             bool
          ShowDefaultValue:        bool
          AllowEmpty:              bool
          DefaultValueStyle:       Style        option
          ChoicesStyle:            Style        option }
        
    /// <summary>
    /// Record class containing information on what choices are there and how they are presented to the user.
    /// </summary>
    type TextPromptTypeConfig<'T> =
        { Choices:                 'T list
          Converter:               ('T -> string)            option
          Validator:               ('T -> ValidationResult)  option }
        
    /// Contains the default text prompt configuration
    [<DefaultValueOf(typeof<TextPromptConfig>)>]
    let defaultTextPromptConfig =
        { PromptStyle = None
          Culture = None
          InvalidChoiceMessage = "[red]Please select one of the available options[/]"
          IsSecret = false
          Mask = Some '*'
          ValidationErrorMessage = "[red]Invalid input[/]"
          ShowChoices = true
          ShowDefaultValue = true
          AllowEmpty = false
          DefaultValueStyle = None
          ChoicesStyle = None }
        
    /// <summary>
    /// Wraps the specified choices into a <c>TextPromptTypeConfig</c> record with default values.
    /// </summary>
    let wrapChoices<'T> (choices: 'T list) : TextPromptTypeConfig<'T> =
        { Choices = choices; Converter = None; Validator = None }
        
    /// <summary>
    /// Constructs a <see cref="Spectre.Console.TextPrompt"/> object from the given configuration records.
    /// </summary>
    let buildPrompt<'T> (config: TextPromptConfig) (typeConfig: TextPromptTypeConfig<'T>) (prompt: string) =
        let textPrompt = TextPrompt<'T>(prompt)
                             .AddChoices(System.Collections.Generic.List<'T>(typeConfig.Choices))
        
        textPrompt.PromptStyle <- unboxNullable config.PromptStyle
        textPrompt.Culture <- unboxNullable config.Culture
        textPrompt.InvalidChoiceMessage <- config.InvalidChoiceMessage
        textPrompt.IsSecret <- config.IsSecret
        textPrompt.Mask <- Option.toNullable config.Mask
        textPrompt.ValidationErrorMessage <- config.ValidationErrorMessage
        textPrompt.ShowChoices <- config.ShowChoices
        textPrompt.ShowDefaultValue <- config.ShowDefaultValue
        textPrompt.AllowEmpty <- config.AllowEmpty
        textPrompt.DefaultValueStyle <- unboxNullable config.DefaultValueStyle
        textPrompt.ChoicesStyle <- unboxNullable config.ChoicesStyle
        
        textPrompt.Converter <-
            match typeConfig.Converter with
            | None -> null 
            | Some func ->  Func<_, string>(func)
        
        textPrompt.Validator <-
            match typeConfig.Validator with
            | None -> null 
            | Some func ->  Func<_, ValidationResult>(func)
        
        textPrompt

    /// <summary>
    /// Builds a <c>Spectre.Console.SelectionPrompt</c> object with the default style configuration. 
    /// </summary>
    /// <param name="choices"> A list of options that the user can select. </param>
    /// <param name="prompt"> The prompt that is displayed to the user. </param>
    /// <returns> One of the objects in 'choices' </returns>
    let defaultPrompt<'T> (choices: 'T list) (prompt: string) : TextPrompt<'T> =
        buildPrompt defaultTextPromptConfig (wrapChoices choices) prompt



/// Discriminated Union representing the possible prompt configurations that can be passed to Console.PromptUser<'T>
type PromptOptions<'T> =
    //  Text Prompt 
    | TextPrompt of Spectre.Console.TextPrompt<'T>
    | TextPromptConfig of config: TextPrompt.TextPromptConfig * typeConfig: TextPrompt.TextPromptTypeConfig<'T> * prompt: string
    | DefaultTextPrompt of choices: 'T list * prompt: string
    //  Selection Prompt
    | SelectionPrompt of Spectre.Console.SelectionPrompt<'T>
    | SelectionPromptConfig of config: SelectionPrompt.SelectionPromptConfig * typeConfig: SelectionPrompt.SelectionPromptTypeConfig<'T> * prompt: string
    | DefaultSelectionPrompt of choices: 'T list * prompt: string



/// <summary>
/// Provides functions and types that help formatting console output and provide interactive behavior.
/// </summary>
module Console =
   
    /// <summary>
    /// Prompts the user given the passed in prompt configuration.
    /// </summary>
    let rec promptUser<'T> (promptType: PromptOptions<'T>) : 'T =
        match promptType with
        | TextPrompt textPrompt ->
            AnsiConsole.Prompt(textPrompt)
        | SelectionPrompt selectionPrompt ->
            AnsiConsole.Prompt(selectionPrompt)
            
        | TextPromptConfig(config, typeConfig, prompt) ->
            let textPrompt = TextPrompt.buildPrompt config typeConfig prompt
            promptUser (PromptOptions<'T>.TextPrompt textPrompt)
        | DefaultTextPrompt(choices, prompt) ->
            let typeConfig = (TextPrompt.wrapChoices choices)
            let textPrompt = TextPrompt.buildPrompt TextPrompt.defaultTextPromptConfig typeConfig prompt
            promptUser (PromptOptions<'T>.TextPrompt textPrompt)
            
        | SelectionPromptConfig(config, typeConfig, prompt) ->
            let selectionPrompt = SelectionPrompt.buildPrompt config typeConfig prompt 
            promptUser (PromptOptions<'T>.SelectionPrompt selectionPrompt)
        | DefaultSelectionPrompt(choices, prompt) ->
            let selectionPrompt = SelectionPrompt.defaultPrompt choices prompt 
            promptUser (PromptOptions<'T>.SelectionPrompt selectionPrompt)

    /// <summary>
    /// Asks the user for a response given a prompt
    /// </summary>
    let ask<'T> (prompt: string) =
        AnsiConsole.Ask<'T> prompt
        
    /// <summary>
    /// Prints a message in green.
    /// </summary>
    let printSuccess (message: string) : unit =
        AnsiConsole.MarkupLine("[green]" + message + "[/]")
        
    /// <summary>
    /// Print a message in red. Typically used for errors.
    /// </summary>
    let printError (message: string) : unit =
        AnsiConsole.MarkupLine("[red]" + message + "[/]") 
    
    /// <summary>
    /// Prompts the user to press <c>Enter</c> to continue program execution.
    /// </summary>
    let pressEnterToContinue () : unit =
        let textPrompt = Spectre.Console.TextPrompt<string>("Press [green]Enter[/] to continue:")
        textPrompt.IsSecret <- true
        textPrompt.AllowEmpty <- true
        textPrompt.Mask <- ' '
        
        AnsiConsole.Prompt(textPrompt) |> ignore
        
    let clearConsole () : unit =
        AnsiConsole.Clear()