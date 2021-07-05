// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using RaaLabs.Edge.Modules.Configuration;


namespace RaaLabs.Edge.ModuleTemplate
{
    [Name("configuration.json")]
    [RestartOnChange]
    public class ModuleTemplateConfiguration : IConfiguration
    {
        public int SampleConfigValue { get; set; }
    }
}
