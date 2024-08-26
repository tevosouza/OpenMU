﻿// <copyright file="AddItemDropGroupForJewelsSeason6.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// This update creates a specific item drop group for jewels with a default chance of 5%.
/// </summary>
[PlugIn(PlugInName, PlugInDescription)]
[Guid("F958CC5B-C1E6-4F67-B48D-4BF75EC5CAA8")]
public class AddItemDropGroupForJewelsSeason6Update : AddItemDropGroupForJewels075Update
{
    /// <inheritdoc />
    public override UpdateVersion Version => UpdateVersion.AddItemDropGroupForJewelsSeason6;

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        await base.ApplyAsync(context, gameConfiguration).ConfigureAwait(false);
        this.AddJewelToItemDrop(gameConfiguration, "Jewel of Creation");
        this.AddJewelToItemDrop(gameConfiguration, "Jewel of Guardian");
    }
}