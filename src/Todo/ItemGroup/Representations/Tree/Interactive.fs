namespace Todo.ItemGroup.Representations.Tree

open Spectre.Console

open Todo.ItemGroup

module Interactive =
    
    let treeInteractive
        (rootItemGroup: ItemGroup)
        (converter: ItemGroup -> string list)
        : ItemGroup =
            
        //  Pair the prompt to be displayed with the resulting item selected 
        let lines = converter rootItemGroup
        let objects = Converter.parseIntoDuList rootItemGroup
        let pairs = List.zip lines objects
            
        let selectionPrompt = SelectionPrompt<string>().AddChoices(lines)
        selectionPrompt.PageSize <- 30
        let choice = selectionPrompt.Show(AnsiConsole.Console)
        
        let selectedObject =
            pairs
            |> List.find (fun (line: string, _) -> line = choice)
            |> snd
            
        match selectedObject with
        | Item _ -> printfn "You selected an item!"
        | ItemGroup _ -> printfn "You selected and item group!"
        System.Console.ReadLine() |> ignore 
            
        rootItemGroup