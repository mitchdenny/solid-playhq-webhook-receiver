using System.CommandLine;
using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.Hosting;
using System.CommandLine.IO;
using System.Threading;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace Solid.Integrations.PlayHQ.WebhookClient
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static async Task<int> Main(string[] args)
        {
            var endpointOption = new Option<Uri>("--endpoint", "Base URL for server.");
            var tenantIdOption = new Option<Guid>("--tenant-id", "ID of tenant at server");
            var playingSurfaceIdOption = new Option<Guid>("--playing-surface-id", "ID of playing surface from PlayHQ");
            var secretOption = new Option<string>("--secret", "Shared secret for signing configuration request");
            var outputPathOption = new Option<FileInfo>("--output-path", "Fully qualified path to file to output content into");

            var rootCommand = new RootCommand("Solid Displays PlayHQ Webhook Client");
            rootCommand.AddOption(endpointOption);
            rootCommand.AddOption(tenantIdOption);
            rootCommand.AddOption(playingSurfaceIdOption);
            rootCommand.AddOption(secretOption);
            rootCommand.AddOption(outputPathOption);

            rootCommand.SetHandler(async (context) =>
            {
                var endpoint = context.ParseResult.GetValueForOption(endpointOption);
                var tenantId = context.ParseResult.GetValueForOption(tenantIdOption);
                var playingSurfaceId = context.ParseResult.GetValueForOption(playingSurfaceIdOption);
                var secret = context.ParseResult.GetValueForOption(secretOption);
                var outputPath = context.ParseResult.GetValueForOption(outputPathOption);

                var eventPump = new EventPump(endpoint, tenantId, playingSurfaceId, secret, outputPath);
                await eventPump.RunAsync(context.GetCancellationToken());
            });
                
            var builder = new CommandLineBuilder(rootCommand);
            var app = builder.Build();

            return await app.InvokeAsync(args);
        }
    }
}