using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SVTracker.Commands;
using DSharpPlus.CommandsNext.Exceptions;
using System;
using DSharpPlus.Entities;
using SpookVooper.Api.Economy;

namespace SVTracker
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }

        public CommandsNextExtension Commands { get; private set; }

        public async Task RunAsync()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            ConfigJson ConfigJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            DiscordConfiguration config = new DiscordConfiguration
            {
                Token = ConfigJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            };

#pragma warning disable IDE0003
            this.Client = new DiscordClient(config);
#pragma warning restore IDE0003

            Client.Ready += OnClientReady;

            CommandsNextConfiguration commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { ConfigJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = true,
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.CommandErrored += CmdErroredHandler;
            Commands.SetHelpFormatter<CustomHelpFormatter>();

            // Basic:
            Commands.RegisterCommands<Basic>();
            // Economy:
            Commands.RegisterCommands<Balance>();
            Commands.RegisterCommands<Experience>();
            Commands.RegisterCommands<Leaderboards>();
            Commands.RegisterCommands<Transactions>();

            await Client.ConnectAsync();

            // Deserializes Transactions Hooks
            await Transactions.TransactionStartup(Client);
            // Create transaction hub object
            TransactionHub tHub = new TransactionHub();
            // Hook transaction event to method
            tHub.OnTransaction += async (transaction) => await Transactions.HandleTransactionAsync(transaction);

            await Task.Delay(-1);
        }

        private async Task CmdErroredHandler(CommandsNextExtension _, CommandErrorEventArgs e)
        {
            if (e.Exception.Message == "Response failed: HTTP Code BadGateway")
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = $"SpookVooper Error",
                    Description = $"SpookVooper cannot be reached",
                    Color = DiscordColor.Red
                };
                await e.Context.RespondAsync(embed: embed).ConfigureAwait(false);
            }
            else if (e.Exception.Message == "Could not find a suitable overload for the command.")
            {
                var json = string.Empty;
                using (var fs = File.OpenRead("config.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync().ConfigureAwait(false);

                ConfigJson ConfigJson = JsonConvert.DeserializeObject<ConfigJson>(json);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = $"The arguments for this command are invalid",
                    Description = $"Please do '{ConfigJson.Prefix}help {e.Command.Name}' to see all needed arguments",
                    Color = DiscordColor.Yellow
                };
                await e.Context.RespondAsync(embed: embed).ConfigureAwait(false);
            }
            else if (e.Exception.Message != "Specified command was not found.")
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = $"SVTracker Error",
                    Description = $"While attempting to run the command the following error has happened:\n{e.Exception.Message}",
                    Color = DiscordColor.Red
                };
                await e.Context.RespondAsync(embed: embed).ConfigureAwait(false);
            }
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}