namespace Todo

open System
open System.Reflection
open Microsoft.FSharp.Core
open Todo.Attributes.Command

type CommandFunction =
    | ChangesData of (string array -> AppData)  //  command function changes program data
    | NoDataChange of (string array -> unit)    //  command function DOES NOT change program data 

[<RequireQualifiedAccess>]
module Command =

    /// The configuration of a command
    type Config =
        { Command: string
          Description: string
          Help: string option
          Function: CommandFunction }
        
    /// Type that stores a list of Command.Config records and has some member functions/methods that provided convenient
    /// operations on the list of stored records.
    type CommandMap internal (configList: Config list) =
            
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
        
        
        
    /// <summary>
    /// Builds the command map.
    /// </summary>
    /// <remarks>
    /// Command.Config data is found through functions marked with the 'CommandInformation' attribute. These functions
    /// are called/invoked and the value they return is then casted into a Command.Config record. 
    /// </remarks>
    let getCommandMap () : Result<CommandMap, Exception> =
        try
            let commandConfigs =
                Assembly.GetEntryAssembly().GetTypes()
                |> Array.collect (fun t -> t.GetMethods())
                |> Array.filter (fun methodInfo -> methodInfo.GetCustomAttributes(typeof<CommandInformationAttribute>, false).Length > 0)
                |> Array.map (fun methodInfo -> methodInfo.Invoke(null, null) :?> Config)
                |> Array.toList
            
            // Logic for config list validation goes here
            
            Ok (CommandMap(commandConfigs))
        with
            | :? NullReferenceException as ex -> Error ex
            | :? InvalidCastException as ex -> Error ex