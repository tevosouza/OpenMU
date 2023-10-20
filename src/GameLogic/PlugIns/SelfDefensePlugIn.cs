﻿// <copyright file="SelfDefensePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

using MUnique.OpenMU.GameLogic.NPC;

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Updates the state of the active self defenses on every second and every hit.
/// </summary>
[PlugIn(nameof(SelfDefensePlugIn), "Updates the state of the self defense system.")]
[Guid("3E702A15-653A-48EF-899C-4CDB2239A90C")]
public class SelfDefensePlugIn : IPeriodicTaskPlugIn, IAttackableGotHitPlugIn
{
    private readonly TimeSpan _selfDefenseTime = TimeSpan.FromSeconds(60);

    /// <inheritdoc />
    public async ValueTask ExecuteTaskAsync(GameContext gameContext)
    {
        var timedOut = gameContext.SelfDefenseState.Where(s => DateTime.UtcNow.Subtract(s.Value) >= _selfDefenseTime).ToList();
        foreach (var (pair, lastAttack) in timedOut)
        {
            if (gameContext.SelfDefenseState.Remove(pair, out _))
            {
                await this.EndSelfDefenseAsync(pair.Attacker, pair.Defender).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public void AttackableGotHit(IAttackable attackable, IAttacker attacker, HitInfo hitInfo)
    {
        var defender = attackable as Player ?? (attackable as Monster)?.SummonedBy;
        var attackerPlayer = attacker as Player ?? (attackable as Monster)?.SummonedBy;
        if (defender is null || attackerPlayer is null)
        {
            return;
        }

        if (defender.SelectedCharacter?.State >= HeroState.PlayerKiller1stStage)
        {
            // PKs have no right to self defense.
            return;
        }

        if (attackerPlayer.IsSelfDefenseActive(defender))
        {
            // Attacking during self defense period does not initiate another self defense.
            return;
        }

        if (hitInfo is { HealthDamage: 0, ShieldDamage: 0 } || hitInfo.Attributes.HasFlag(DamageAttributes.Reflected))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var gameContext = defender.GameContext;
        gameContext.SelfDefenseState.AddOrUpdate((attackerPlayer, defender), tuple =>
        {
            _ = this.BeginSelfDefenseAsync(attackerPlayer, defender);
            return now;
        }, (tuple, time) => now);
    }

    private async ValueTask BeginSelfDefenseAsync(Player attacker, Player defender)
    {
        var message = $"Self defense is initiated by {attacker.Name}'s attack to {defender.Name}!";
        await defender.InvokeViewPlugInAsync<IShowMessagePlugIn>(p => p.ShowMessageAsync(message, MessageType.BlueNormal));
        await attacker.InvokeViewPlugInAsync<IShowMessagePlugIn>(p => p.ShowMessageAsync(message, MessageType.BlueNormal));
    }

    private async ValueTask EndSelfDefenseAsync(Player attacker, Player defender)
    {
        var message = $"Self defense of {defender.Name} against {attacker.Name} diminishes.";
        await defender.InvokeViewPlugInAsync<IShowMessagePlugIn>(p => p.ShowMessageAsync(message, MessageType.BlueNormal)).ConfigureAwait(false);
        await attacker.InvokeViewPlugInAsync<IShowMessagePlugIn>(p => p.ShowMessageAsync(message, MessageType.BlueNormal)).ConfigureAwait(false);
    }
}