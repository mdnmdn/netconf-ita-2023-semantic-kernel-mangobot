﻿// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace MangoBot.SemanticSample.Plugins;

public sealed class MenuPlugin
{
    [SKFunction, Description("Provides a list of specials from the menu.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate",
        Justification = "Too smart")]
    public string GetSpecials()
    {
        return @"
Special Soup: Clam Chowder
Special Salad: Cobb Chowder
Special Drink: Chai Tea
";
    }

    [SKFunction, Description("Provides the price of the requested menu item.")]
    public string GetItemPrice(
        [Description("The name of the menu item.")]
        string menuItem)
    {
        return "$9.99";
    }
}