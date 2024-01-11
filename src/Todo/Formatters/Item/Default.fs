/// Contains functions that parse items into strings.
module Todo.Formatters.Item.Default

open Todo.ItemGroup

/// <summary>
/// Returns a single line, string representation of an <b>Item</b>. 
/// </summary>
/// <remarks>
/// Does <b>NOT</b> account an item group's <b>sub item groups AND items</b>.
/// </remarks> 
let internal formatItem (item: Item) : string =
    (* format as default to string *)
    item.ToString()
