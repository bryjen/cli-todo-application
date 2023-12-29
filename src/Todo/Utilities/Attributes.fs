namespace Todo.Utilities

open System

module Attributes =
    
    [<AutoOpen>]
    module Command =
        
        /// Attribute indicating that a function returns the configuration for a console command.
        [<Obsolete>]
        [<AttributeUsage(AttributeTargets.Method, AllowMultiple = false)>]
        type CommandInformationAttribute() =
            inherit Attribute()