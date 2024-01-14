namespace Todo.Cli

open Todo
open Todo.Cli

type CommandTemplate =
    { CommandName: string
      HelpString: string
      InjectData: ApplicationConfiguration -> ApplicationSettings -> AppData -> CommandFunction }
    
/// DU indicating whether the user command changes the application data or not.
/// A user command always takes a string array as input. 
and CommandFunction =
    | ChangesData of (string array -> AppData)
    | NoDataChange of (string array -> unit)
