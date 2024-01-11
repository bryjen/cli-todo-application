/// Contains functions that parse item groups into strings.
module Todo.Formatters.ItemGroup.Default

open Todo.ItemGroup

/// <summary>
/// Returns a single line, string representation of an <b>Item Group</b>. 
/// </summary>
/// <remarks>
/// Does <b>NOT</b> account an item group's <b>sub item groups AND items</b>.
/// </remarks> 
let internal formatItemGroup (itemGroup: ItemGroup) : string =
    sprintf "[[%s]]" itemGroup.Name 
