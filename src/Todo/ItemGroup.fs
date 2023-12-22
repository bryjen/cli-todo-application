namespace Todo

type ItemGroup =
    { SubItemGroups: ItemGroup list
      Items: Item list }
and Item =
    { Title: string
      Description: string }
