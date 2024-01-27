namespace Todo.Cli.Commands

open System
open Argu
open FsToolkit.ErrorHandling
open Spectre.Console
open Spectre.Console.Prompts.Extensions
open Todo
open Todo.Cli
open Todo.Utilities
open Todo.Cli.Commands.Arguments

(* Exception thrown when no cli args are provided. Signals to display the custom help text.
   Replacement to Argu's ArguParseException with a specific error code, however it is internal. *)
type private HelpTextException() =
    inherit Exception("")
    member val ErrorCode = ErrorCode.HelpText with get

(* Note you cannot bring out the 'CliPrefix(CliPrefix.None)' attribute to the outside, otherwise Argu will complain that
   you have overriden the default value of '--help', which we do not. *)
type ProgramArguments =
    | [<CliPrefix(CliPrefix.None)>]
        View of view_args:ParseResults<ViewArguments>
    | [<CliPrefix(CliPrefix.None)>]
        Create of create_args:ParseResults<CreateArguments>
    | [<CliPrefix(CliPrefix.None)>]
        Delete of delete_args:ParseResults<DeleteArguments> 
    | [<CliPrefix(CliPrefix.None)>]
        Edit of edit_args:ParseResults<EditArguments> 
    | [<CliPrefix(CliPrefix.None); AltCommandLine("-h")>]
        Help
        
    interface IArgParserTemplate with
        member this.Usage =
            ProgramArguments.GetUsageString this
    
    static member private GetUsageString (programArguments: ProgramArguments) =
        match programArguments with
        | View _ -> "Displays a view of the current todos."
        | Create _ -> "Creates an item/item group/label."
        | Delete _ -> "Deletes an item/item group/label."
        | Edit _ -> "Edits an existing item/item group/label." 
        | Help -> "Prints this list of help strings for each command."
        
    static member internal CustomHelpString =
        let commandNames = getDUNames<ProgramArguments> () 
        let helpStrings =  getAllDUValuesWithParameters<ProgramArguments> () |> Array.map ProgramArguments.GetUsageString
        let pairs = Array.zip commandNames helpStrings |> Array.toList
        
        let grid = Grid().AddColumn().AddColumn()
        let addPairToGrid = fun (grid: Grid) (pair: string * string) -> grid.AddRow [| fst pair; snd pair |]
        let gridWithValues = List.fold addPairToGrid grid pairs
        
        AnsiBuilder.Build(AnsiConsole.Console, gridWithValues)
        |> sprintf "\nApplication Commands:\n%s"
        
    static member private ValidateParsedArguments (parseResults: ParseResults<ProgramArguments>) : Result<unit, Exception> =
        match parseResults.GetAllResults () |> List.length with
        | 0 ->
            raise (HelpTextException())
        | 1 ->
            Ok ()
        | _ ->
            let exceptionMessage = "You can only pass one main command!"
            Error (ArgumentException(exceptionMessage + ProgramArguments.CustomHelpString))
        
    static member Parse (argv: string array) : Result<ParseResults<ProgramArguments>, Exception> =
        let parser = ArgumentParser.Create<ProgramArguments>(programName = "todos", usageStringCharacterWidth = 999)
        
        try
            result {
                let parseResults = parser.Parse argv
                do! ProgramArguments.ValidateParsedArguments parseResults
                return parseResults
            }
        with
        | :? HelpTextException as _ ->
            Error (Exception(ProgramArguments.CustomHelpString))
        | :? ArguParseException as ex ->
            // ex.Message returns the error message + help string, we just want the error message
            let exceptionMessage = ex.Message.Split('\n')[0]
            
            if (argv.Length = 0 || argv.Length = 1) then 
                Error (Exception(exceptionMessage + "\n" + ProgramArguments.CustomHelpString))
            else
                Error (Exception(ex.Message))

    static member Execute  
        (appConfig: ApplicationConfiguration)
        (appSettings: ApplicationSettings)
        (appData: AppData)
        (parseResults: ParseResults<ProgramArguments>)
        : unit =

        let toExecute = List.head (parseResults.GetAllResults())
        
        match toExecute with
        | ProgramArguments.View viewArguments ->
            View.executeViewCommand appConfig appSettings appData viewArguments
        | ProgramArguments.Create createArguments ->
            Create.executeCreateCommand appConfig appData createArguments 
        | ProgramArguments.Delete deleteArguments ->
            Delete.executeDeleteCommand appConfig appData deleteArguments
        | ProgramArguments.Edit editArguments ->
            Edit.executeEditCommand appConfig appData editArguments 
        | _ -> failwith "unknown command"