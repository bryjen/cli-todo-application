namespace Todo

open Microsoft.FSharp.Core

type CommandFunction =
    | ChangesData of (string array -> AppData)  //  command function changes program data
    | NoDataChange of (string array -> unit)    //  command function DOES NOT change program data 

[<RequireQualifiedAccess>]
module Command =

    /// The configuration of a command
    type Config =
        { Command: string
          Help: string
          Function: CommandFunction }
        
    /// Type that stores a list of Command.Config records and has some member functions/methods that provided convenient
    /// operations on the list of stored records.
    type CommandMap (configList: Config list) =
            
        member val Configs: Config list = configList with get
        
        /// Gets the Command.Config record that matches the specified command name.
        member this.getConfig (commandName: string) : Config option =
            
            //  recursive helper function 
            let rec findConfigInList (configList: Config list) : Config option =
                match List.length configList with
                | length when length >= 1 -> 
                    let head = List.head configList
                    if head.Command = commandName then
                        Some head
                    else
                        findConfigInList (List.tail configList)
                | _ ->
                    None
             
            findConfigInList this.Configs
            
        /// Returns a list of all the valid/recognizable commands 
        member this.commands () : string list =
            this.Configs
            |> List.map (fun (config: Config) -> config.Command)