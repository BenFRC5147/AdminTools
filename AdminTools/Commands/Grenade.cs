﻿using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;

namespace AdminTools.Commands
{
    using Exiled.API.Enums;
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features.Pickups.Projectiles;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Grenade : ICommand, IUsageProvider
    {
        public string Command { get; } = "grenade";

        public string[] Aliases { get; } = new string[] { "gn" };

        public string Description { get; } = $"Spawns a {string.Join("/", Enum.GetNames(typeof(ProjectileType)))} grenade on a user or users";

        public string[] Usage { get; } = new string[] { "%player%", string.Join(", ", Enum.GetNames(typeof(ProjectileType))), "FuseTime" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender.CheckPermission("at.grenade"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 2 || arguments.Count > 3)
            {
                response = "Usage: grenade ((player id / name) or (all / *)) (ProjectileType) [grenade time = default]";
                return false;
            }

            if (!Enum.TryParse(arguments.At(1), true, out ProjectileType type))
            {
                response = $"Invalid value for projectile type: {arguments.At(1)}\n{string.Join(", ",Enum.GetNames(typeof(ProjectileType)))}.";
                return false;
            }

            float? fusetime = null;
            if (float.TryParse(arguments.At(2), out float value))
                fusetime = value;

            IEnumerable<Player> players = Player.GetProcessedData(arguments);

            foreach (Player player in players)
            {
                if (player.IsDead)
                    continue;
                if (Projectile.CreateAndSpawn(type, player.Position, player.Rotation).Is(out TimeGrenadeProjectile timeGrenadeProjectile) && fusetime.HasValue)
                    fusetime = timeGrenadeProjectile.FuseTime = fusetime.Value;
            }

            response = $"{type} has been sent to the followed player{(fusetime.HasValue ? $". Will explode in {fusetime}s" : string.Empty)}: {Extensions.LogPlayers(players)}";
            return true;
        }
    }
}
