module Todo.Utils.AnsiBuilder

open System
open System.Reflection
open Spectre.Console
open FsToolkit.ErrorHandling
open Spectre.Console.Rendering

let private tryGetClassType (assembly: Assembly) =
    let classType = assembly.GetType("Spectre.Console.AnsiBuilder")
    
    if classType <> null then
        Ok classType
    else
        let errorMessage = "Could not find the type 'Spectre.Console.AnsiBuilder' in the assembly 'Spectre.Console'.";
        Error (MissingMemberException(errorMessage))
        
let private tryGetMethodInfo (classType: Type) =
    let parameterTypes = [| typeof<IAnsiConsole>; typeof<IRenderable> |]
    let methodInfo = classType.GetMethod("Build", BindingFlags.Static ||| BindingFlags.Public, null, parameterTypes, null)
    
    if methodInfo <> null then
        Ok methodInfo 
    else
        let errorMessage = "Could not find the method 'Build'.";
        Error (MissingMemberException(errorMessage))
        
let private buildFunction: MethodInfo =
    result {
        let assembly = Assembly.Load("Spectre.Console")
        let! classType = tryGetClassType assembly
        return! tryGetMethodInfo classType 
    }
    |>
    function
    | Ok methodInfo ->
        methodInfo
    | Error err ->
        raise err
        
/// <summary>
/// Renders an <c>IRenderable</c> into a string that can be printed to the console. 
/// </summary>
/// <remarks>
/// Basically a wrapper function to Spectre.Console's AnsiBuilder.Build(IAnsiConsole, IRenderable) method.
/// </remarks>
/// https://github.com/spectreconsole/spectre.console/blob/main/src/Spectre.Console/Internal/Backends/Ansi/AnsiBuilder.cs
let buildRenderable (console: IAnsiConsole) (renderable: IRenderable) =
    let invokeResult = buildFunction.Invoke(null, [| console; renderable |])
    
    match invokeResult with
    | :? string as rendered ->
        rendered
    | _ ->
        let errorMessage = "A call to 'AnsiBuild.Build(IAnsiConsole, IRenderable)' did not return a string.";
        raise (Exception(errorMessage))
        
/// <summary>
/// Renders an <c>IRenderable</c> into a string, and then split it into lines. 
/// </summary>
/// <see cref="AnsiBuilder.buildRenderable"/>
let buildRenderableLines (console: IAnsiConsole) (renderable: IRenderable) =
    buildRenderable console renderable
    |> (_.Split('\n'))
    |> Array.map (_.Replace("\r", ""))
    |> Array.toList