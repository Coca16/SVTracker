﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class EnableBlacklist : CheckBaseAttribute
{
    public EnableBlacklist() { }

    readonly List<float> blacklist = new List<float>
    {
    470203136771096596 //Asdia
    };

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        bool blacklisted = false;
        foreach (var id in blacklist)
        {
            if (id == ctx.User.Id)
                blacklisted = true;
        }
        if (ctx.Channel.Id != 782470733058015262){ blacklisted = true; }
        return Task.FromResult(blacklisted != true);
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class DeveloperOnly : CheckBaseAttribute
{
    public DeveloperOnly() { }

    readonly List<float> whitelist = new List<float>
    {
    };

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        bool whitelisted = false;
        foreach (var id in whitelist)
        {
            if (id == ctx.User.Id)
                whitelisted = true;
        }
        return Task.FromResult(whitelisted == true);
    }
}