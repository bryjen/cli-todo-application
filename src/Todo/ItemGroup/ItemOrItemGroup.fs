namespace Todo.ItemGroup

/// DU indicating that an item is either an item or an item group.
type ItemOrItemGroup =
    | Item of Item
    | ItemGroup of ItemGroup
    
(* This DU is mainly used when parsing a complex item group, which have layers of inner/sub item groups that contain
   multiple items and even more inner item groups. *)