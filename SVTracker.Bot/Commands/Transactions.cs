using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus;
using System.Threading.Tasks;
using System;
using SpookVooper.Api.Economy;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace SVTracker.Commands
{
    public class Transactions : BaseCommandModule
    {
        private static Dictionary<string, Tuple<ulong, string>> hooks = new Dictionary<string, Tuple<ulong, string>>();
        private static DiscordClient client;
        private static readonly string hooksFilename = "transactionhooks.json";
        private static readonly List<string> messageTypes = new List<string>()
        {
            "embed",
            "none"
        };

        [Group("transactions")]
        [Description("Group of commands for transaction logging")]
        [Aliases("transaction", "transact", "trans", "tran", "tr", "t")]
        public class ExampleExecutableGroup : BaseCommandModule
        {
            [Command("create"), EnableBlacklist]
            [Description("Creates a new hook or logger")]
            [Aliases("add", "creat", "cre", "cr", "c")]
            [RequirePermissions(Permissions.Administrator)]
            public async Task TransactionCreate(CommandContext ctx,
                [Description("Label used to identify the hook (created log). Must be unique")] string label,
                [Description("Type of message sent when transaction is logged. Options are embed or none.")] string type)
            {
                if (messageTypes.Contains(type.ToLower()))
                {
                    if (!hooks.ContainsKey(label))
                    {
                        hooks.Add(label, Tuple.Create(ctx.Channel.Id, type.ToLower()));
                        using FileStream createStream = File.Create(hooksFilename);
                        await JsonSerializer.SerializeAsync(createStream, hooks).ConfigureAwait(false);
                        client = ctx.Client;
                        await ctx.RespondAsync("Logger has been added!").ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.RespondAsync($"Label already exists! Use a different label.").ConfigureAwait(false);
                    }
                }
                else
                {
                    await ctx.RespondAsync($"That type isn't possible. Use types: {string.Join(", ", messageTypes)}").ConfigureAwait(false);
                }
            }

            [Command("remove"), EnableBlacklist]
            [Aliases("delete", "del", "d", "rem", "r")]
            [Description("Removes specific transaction hook/loggers")]
            [RequirePermissions(Permissions.Administrator)]
            public async Task TransactionRemove(CommandContext ctx,
                [Description("Label used to identify the hook (log to delete)")] string label)
            {
                if (hooks.ContainsKey(label))
                {
                    hooks.Remove(label);
                    using FileStream createStream = File.Create(hooksFilename);
                    await JsonSerializer.SerializeAsync(createStream, hooks).ConfigureAwait(false);
                    client = ctx.Client;
                    await ctx.RespondAsync("Logger has been removed!").ConfigureAwait(false);
                }
                else
                {
                    await ctx.RespondAsync($"Label doesn't exists! Try a different label.").ConfigureAwait(false);
                }
            }

            [Command("view"), EnableBlacklist]
            [Aliases("see", "s", "v")]
            [Description("Views all active hooks/loggers")]
            public async Task HookView(CommandContext ctx)
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = "Active Hooks",
                    Color = new DiscordColor("#965d4a")
                };
                foreach (var hook in hooks)
                {
                    embed.AddField($"{hook.Key}:",
    $"Channel ID: {hook.Value.Item1}\nType: {hook.Value.Item2}");
                }
                await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
            }
        }

        static public async Task HandleTransactionAsync(Transaction transaction)
        {
            DateTime time = DateTime.Now;

            //tax calculation
            float tax = 0;
            switch (transaction.Tax)
            {
                case ApplicableTax.CapitalGains:
                    tax = (float)transaction.Amount * ((float)0.10 / 100);
                    break;
                case ApplicableTax.Corporate:
                    tax = (float)transaction.Amount * ((float)2.50 / 100);
                    break;
                case ApplicableTax.Payroll:
                    tax = (float)transaction.Amount * ((float)1.50 / 100);
                    break;
                case ApplicableTax.Sales:
                    tax = (float)transaction.Amount * ((float)2.50 / 100);
                    break;
            }
            // text base
            string text;
            if (!transaction.Force)
            {
                text = $@"{time.Day}/{time.Month}/{time.Year} {time.Hour}:{time.Minute}:{time.Second} {time.Millisecond}ms
sent ¢{transaction.Amount} from {transaction.FromAccount} ({await SVTools.SVIDToName(transaction.FromAccount)}) to {transaction.ToAccount} ({await SVTools.SVIDToName(transaction.ToAccount)}) as {transaction.Detail}";
            }
            else
            {
                text = $@"{time.Day}/{time.Month}/{time.Year} {time.Hour}:{time.Minute}:{time.Second} {time.Millisecond}ms
force sent ¢{transaction.Amount} from {transaction.FromAccount} ({await SVTools.SVIDToName(transaction.FromAccount)}) to {transaction.ToAccount} ({await SVTools.SVIDToName(transaction.ToAccount)}) as {transaction.Detail}";
            }

            // embed base
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = $"¢{transaction.Amount} {transaction.Detail} from {await SVTools.SVIDToName(transaction.FromAccount)} to {await SVTools.SVIDToName(transaction.ToAccount)}",
                Description = $"{time.Day}/{time.Month}/{time.Year} {time.Hour}:{time.Minute}:{time.Second} {time.Millisecond}ms",
                Color = new DiscordColor("#00eb08")
            };
            embed.AddField("SVIDs",
                $"From: {transaction.FromAccount}\nTo: {transaction.ToAccount}");
            // additions to base
            if (tax != 0)
            {
                text = $@"{text} with a tax of {tax} ({transaction.Tax})";
                embed.AddField("Tax",
                    $"Amount: {tax}\nType: {transaction.Tax}");
            }

            if (transaction.Force)
            {
                embed.Description = $"Forced {embed.Description} ";
            }


            foreach (var hook in hooks)
            {
                DiscordChannel channel = await client.GetChannelAsync(hook.Value.Item1);

                if (hook.Value.Item2 == "none")
                {
                    await channel.SendMessageAsync(text).ConfigureAwait(false);

                }
                else if (hook.Value.Item2 == "embed")
                {
                    await channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                }
                else
                {
                    await channel.SendMessageAsync($@"Type does not exist!").ConfigureAwait(false);
                }
            }
        }

        static public async Task TransactionStartup(DiscordClient Client)
        {
            client = Client;
            using FileStream openStream = File.OpenRead(hooksFilename);
            hooks = await JsonSerializer.DeserializeAsync<Dictionary<string, Tuple<ulong, string>>>(openStream);
        }
    }
}

