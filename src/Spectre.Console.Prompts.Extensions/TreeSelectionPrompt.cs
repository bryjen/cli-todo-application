namespace Spectre.Console.Prompts.Extensions;

/// <summary>
/// Contains functions that parse <c>Spectre.Console.Tree</c> objects into <c>Spectre.Console.SelectionPrompt</c>
/// objects.
/// </summary>
public static class TreeSelectionPrompt
{
    public static SelectionPrompt<string> ToSelectionPrompt(IAnsiConsole console, Tree tree)
    { 
        IEnumerable<string> choices = AnsiBuilder.BuildLines(console, tree);
        
        var selectionPrompt = new SelectionPrompt<string>
        {
            PageSize = 30
        };
        return selectionPrompt.AddChoices(choices);
    }
}