namespace Todo

open System

module Attributes =
    
    module Command =
        
        /// Attribute indicating that a function returns the configuration for a console command.
        [<AttributeUsage(AttributeTargets.Method, AllowMultiple = false)>]
        type CommandInformationAttribute() =
            inherit Attribute()