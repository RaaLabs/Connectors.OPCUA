// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Modules.EdgeHub;
using RaaLabs.Edge.Modules.Configuration;


namespace RaaLabs.Edge.ModuleTemplate
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main(string[] args)
        {
            var application = new ApplicationBuilder()
                .WithModule<EventHandling>()
                .WithModule<Configuration>()
                .WithModule<EdgeHub>()
                .WithHandler<ModuleTemplateHandler>()
                .Build();

            application.Run().Wait();
        }
    }
}
