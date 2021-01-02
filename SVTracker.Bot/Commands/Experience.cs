using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System;
using SpookVooper.Api.Entities;
using System.Timers;
using DSharpPlus;

namespace SVTracker.Commands
{
    public class Experience : BaseCommandModule
    {
        Timer timer;
        [Command("experience"), EnableBlacklist]
        [Priority(1)]
        [Aliases("xp", "x")]
        public async Task ExperienceAll(CommandContext ctx, DiscordUser discordUser)
        {
            string discordName = discordUser.Username;
            string discordPFP = discordUser.AvatarUrl;
            ulong discordID = discordUser.Id;
            string SVID = await User.GetSVIDFromDiscordAsync(discordID);
            User user = new User(SVID);
            var data = await user.GetSnapshotAsync();
            int Total_XP = data.post_likes + data.comment_likes + (data.twitch_message_xp * 4) + (data.discord_commends * 5) + (data.discord_message_xp * 2) + (data.discord_game_xp / 100);
#pragma warning disable IDE0004
            decimal Ratio_Messages = (decimal)data.discord_message_xp / (decimal)data.discord_message_count;
#pragma warning restore IDE0004
            decimal multiplier = (decimal)Math.Pow(10, Convert.ToDouble(2));
            decimal Ratio_Messages_Rounded = (Math.Ceiling(Ratio_Messages * multiplier) / multiplier);

            await ctx.TriggerTypingAsync();
            DiscordEmbedBuilder.EmbedAuthor iconURL = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = discordName + " XP",
                IconUrl = discordPFP,
            };

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = $"Total XP: {Total_XP}\nMessage to XP: 1 : {Ratio_Messages_Rounded * 2}",
                Description = $"XP for [{discordName}](https://spookvooper.com/User/Info?svid={SVID})'s SpookVooper account",
                Color = new DiscordColor("#965d4a"),
                Author = iconURL
            };
            await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("experience"), EnableBlacklist]
        [Priority(0)]
        public async Task ExperienceUser(CommandContext ctx, [RemainingText] string Inputname)
        {
            if (Inputname != null)
            {
                string SVID = await User.GetSVIDFromUsernameAsync(Inputname);

                if (SVID == null)
                {
                    await ctx.Channel.SendMessageAsync($"{Inputname} is not a user!").ConfigureAwait(false);
                }
                else
                {
                    User user = new User(SVID);
                    var data = await user.GetSnapshotAsync();
                    string name = data.UserName;
                    string PFP = data.image_url;
                    int Total_XP = data.post_likes + data.comment_likes + (data.twitch_message_xp * 4) + (data.discord_commends * 5) + (data.discord_message_xp * 2) + (data.discord_game_xp / 100);
#pragma warning disable IDE0004
                    decimal Ratio_Messages = (decimal)data.discord_message_xp / (decimal)data.discord_message_count;
#pragma warning restore IDE0004
                    decimal multiplier = (decimal)Math.Pow(10, Convert.ToDouble(2));
                    decimal Ratio_Messages_Rounded = (Math.Ceiling(Ratio_Messages * multiplier) / multiplier);

                    await ctx.TriggerTypingAsync();
                    DiscordEmbedBuilder.EmbedAuthor iconURL = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = name + " XP",
                        IconUrl = PFP,
                    };

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                    {
                        Title = $"Total XP: {Total_XP}\nMessage to XP: 1 : {Ratio_Messages_Rounded * 2}",
                        Description = $"XP for [{name}](https://spookvooper.com/User/Info?svid={SVID})'s SpookVooper account",
                        Color = new DiscordColor("#965d4a"),
                        Author = iconURL
                    };
                    await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
                }
            }
            else
            {
                string discordName = ctx.Member.Username;
                string discordPFP = ctx.Member.AvatarUrl;
                ulong discordID = ctx.Member.Id;
                string SVID = await User.GetSVIDFromDiscordAsync(discordID);
                User user = new User(SVID);
                var data = await user.GetSnapshotAsync();
                int Total_XP = data.post_likes + data.comment_likes + (data.twitch_message_xp * 4) + (data.discord_commends * 5) + (data.discord_message_xp * 2) + (data.discord_game_xp / 100);
#pragma warning disable IDE0004
                decimal Ratio_Messages = (decimal)data.discord_message_xp / (decimal)data.discord_message_count;
#pragma warning restore IDE0004
                decimal multiplier = (decimal)Math.Pow(10, Convert.ToDouble(2));
                decimal Ratio_Messages_Rounded = (Math.Ceiling(Ratio_Messages * multiplier) / multiplier);

                await ctx.TriggerTypingAsync();
                DiscordEmbedBuilder.EmbedAuthor iconURL = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = discordName + " XP",
                    IconUrl = discordPFP,
                };

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = $"Total XP: {Total_XP}\nMessage to Message XP: 1 : {Ratio_Messages_Rounded * 2}",
                    Description = $"XP for [{discordName}](https://spookvooper.com/User/Info?svid={SVID})'s SpookVooper account",
                    Color = new DiscordColor("#965d4a"),
                    Author = iconURL
                };
                await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
            }
        }

        [Command("experienceloop"), EnableBlacklist]
        [Description("Loops through the experience of a SV user with a asigning interval between each message and a begining time")]
        [Aliases("xploop", "xpl", "xloop", "xl")]
        [RequirePermissions(Permissions.Administrator)]
        [Priority(1)]
        public async Task ExperienceDiscordLoop(CommandContext ctx,
    [Description("Interval between each experience check in minutes (decimals can be used).")] double interval,
    [Description("When the loop start at a current point in a hour (in minutes). Goes to next hour if the point in the hour has already passed.")] float start,
    [Description("The SV Username or Discord user of the person who you wish to get the xp of.")] DiscordMember discordUser)
        {
            string SVID = await User.GetSVIDFromDiscordAsync(discordUser.Id);

            if (SVID == null)
            {
                await ctx.Channel.SendMessageAsync($"{discordUser.DisplayName} is not a SV user!").ConfigureAwait(false);
            }
            else
            {
                DateTime when;
                DateTime now = DateTime.Now;

                var timeSpan = TimeSpan.FromMinutes(start);
                int mm = timeSpan.Minutes;
                int ss = timeSpan.Seconds - (mm * 60);
                //Turns minutes into minutes and leftover seconds

                if (start > now.Minute + (now.Second / 60)) { when = new DateTime(now.Year, now.Month, now.Day, now.Hour, mm, ss); }
                else { when = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, mm, ss); }
                //Creates the user's desired start time into DateTime. If the start time is smaller then the current minutes it is moved for next hour

                TimeSpan delay = when.Subtract(now);
                //Gets difference between disered time and current time 

                await Task.Delay(delay.Milliseconds);
                await ExperienceUpdaterAsync(ctx, SVID);
                //Delays until users time is reached and then awaits ExperienceUpdaterAsync as timer doesn't when initiated

                if (interval >= 0.01)
                {
                    timer = new Timer();
                    timer.Interval = (float)(interval * 60000);
                    timer.Enabled = true;
#pragma warning disable CS4014
                    timer.Elapsed += (sender, e) => ExperienceUpdaterAsync(ctx, SVID);
#pragma warning restore CS4014
                }
                else { await ctx.Channel.SendMessageAsync($"Cannot have a interval of {interval}. Needs to be 0.01 or higher to not DDOS Spike!").ConfigureAwait(false); }
            }
        }

        [Command("experienceloop"), EnableBlacklist]
        [Priority(0)]
        public async Task ExperienceSVLoop(CommandContext ctx,
            [Description("Interval between each experience check in minutes (decimals can be used).")] double interval,
            [Description("When the loop start at a current point in a hour (in minutes). Goes to next hour if the point in the hour has already passed.")] float start,
            [Description("The SV Username of the person who you wish to get the xp of.")] string Inputname)
        {
            if (Inputname != null)
            {
                string SVID = await User.GetSVIDFromUsernameAsync(Inputname);

                if (SVID == null)
                {
                    await ctx.Channel.SendMessageAsync($"{Inputname} is not a SV username!").ConfigureAwait(false);
                }
                else
                {
                    DateTime when;
                    DateTime now = DateTime.Now;

                    var timeSpan = TimeSpan.FromMinutes(start);
                    int mm = timeSpan.Minutes;
                    int ss = timeSpan.Seconds - (mm * 60);
                    //Turns minutes into minutes and leftover seconds

                    if (start > now.Minute + (now.Second / 60)){ when = new DateTime(now.Year, now.Month, now.Day, now.Hour, mm, ss); }
                    else { when = new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, mm, ss); }
                    //Creates the user's desired start time into DateTime. If the start time is smaller then the current minutes it is moved for next hour

                    TimeSpan delay = when.Subtract(now);
                    //Gets difference between disered time and current time 

                    await Task.Delay(delay.Milliseconds);
                    await ExperienceUpdaterAsync(ctx, SVID);
                    //Delays until users time is reached and then awaits ExperienceUpdaterAsync as timer doesn't when initiated

                    if (interval >= 0.01)
                    {
                        timer = new Timer();
                        timer.Interval = (float)(interval * 60000);
                        timer.Enabled = true;
#pragma warning disable CS4014
                        timer.Elapsed += (sender, e) => ExperienceUpdaterAsync(ctx, SVID);
#pragma warning restore CS4014
                    }
                    else { await ctx.Channel.SendMessageAsync($"Cannot have a interval of {interval}. Needs to be 0.01 or higher to not DDOS Spike!").ConfigureAwait(false); }
                }
            }
        }

        private async Task ExperienceUpdaterAsync(CommandContext ctx, string SVID)
        {
            User user = new User(SVID);
            var data = await user.GetSnapshotAsync();
            int Total_XP = data.post_likes + data.comment_likes + (data.twitch_message_xp * 4) + (data.discord_commends * 5) + (data.discord_message_xp * 2) + (data.discord_game_xp / 100); string name = ctx.Member.Username;
#pragma warning disable IDE0004
            decimal Ratio_Messages = (decimal)data.discord_message_xp / (decimal)data.discord_message_count;
#pragma warning restore IDE0004
            decimal multiplier = (decimal)Math.Pow(10, Convert.ToDouble(2));
            decimal Ratio_Messages_Rounded = (Math.Ceiling(Ratio_Messages * multiplier) / multiplier);

            DateTime time = DateTime.Now;
            await ctx.Channel.SendMessageAsync($"{time.Day}/{time.Month}/{time.Year} {time.Hour}:{time.Minute}:{time.Second}\nXP: {Total_XP}\nRatio: {Ratio_Messages_Rounded * 2}\nMessage Count: {data.discord_message_count}").ConfigureAwait(false);
        }
    }
}

