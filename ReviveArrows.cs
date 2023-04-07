using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Revive Arrows", "Dana", "2.1.0")]
    [Description("Shoot your teammates with love and healing.")]

    public class ReviveArrows : RustPlugin
    {
        #region Fields

        private static ReviveArrows _instance;
        private static Configuration _config;

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Revive Arrow Button")]
            public string ReviveArrowButton { get; set; }

            [JsonProperty("Instant Heal Amount")]
            public float InstantHealAmount { get; set; }

            [JsonProperty("Heal Amount Over Time")]
            public float HealAmountOverTime { get; set; }

            [JsonProperty("Can Revive")]
            public bool CanRevive { get; set; }

            [JsonProperty("Revive Sound Effect")]
            public string ReviveSoundEffect { get; set; }

            [JsonProperty("Consumable Items")]
            public List<ConsumableOptions> ConsumableItems { get; set; }

            [JsonIgnore]
            public BUTTON Button
            {
                get
                {
                    return (BUTTON)Enum.Parse(typeof(BUTTON), ReviveArrowButton);
                }
            }
        }

        private class ConsumableOptions
        {
            [JsonProperty("Item Shortname")]
            public string ItemShortname { get; set; }

            [JsonProperty("Amount To Consume")]
            public int AmountToConsume { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                ReviveArrowButton = "USE",
                InstantHealAmount = 15f,
                HealAmountOverTime = 20f,
                CanRevive = true,
                ReviveSoundEffect = "assets/prefabs/tools/medical syringe/effects/inject_friend.prefab",
                ConsumableItems = new List<ConsumableOptions>()
                    {
                        new ConsumableOptions
                        {
                            ItemShortname = "syringe.medical",
                            AmountToConsume = 1,
                        },
                        new ConsumableOptions
                        {
                            ItemShortname = "rope",
                            AmountToConsume = 1,
                        },
                    }
            };
        }

        private void UpdateConfig()
        {
            PrintWarning("Detected changes in configuration! Updating...");
            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            if (string.Compare(_config.Version, "2.1.0") < 0)
            {
                _config.ReviveArrowButton = defaultConfig.ReviveArrowButton;
                _config.ReviveSoundEffect = defaultConfig.ReviveSoundEffect;
            }

            PrintWarning("Configuration update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _instance = this;
            PermissionUtils.Register();
        }

        private void Unload()
        {
            _config = null;
            _instance = null;
        }

        private void OnPlayerAttack(BasePlayer player, HitInfo hitInfo)
        {
            if (!player.IsValid() || !hitInfo.HitEntity.IsValid())
                return;

            if (!CanUseHealArrow(player, hitInfo))
                return;

            BasePlayer targetPlayer = hitInfo.HitEntity.ToPlayer();
            if (!targetPlayer.IsValid())
                return;

            Dictionary<Item, int> itemsToConsume;
            if (!VerifyHasConsumableItem(player, out itemsToConsume))
            {
                ReplyToPlayer(player, Lang.ReviveInsufficientConsumable);
                return;
            }

            ConsumeItems(player, itemsToConsume);

            hitInfo.damageTypes.ScaleAll(0);
            HealPlayer(targetPlayer);
            ReplyToPlayer(player, Lang.ReviveSuccess);
        }

        #endregion Oxide Hooks

        #region Functions

        private bool CanUseHealArrow(BasePlayer player, HitInfo hitInfo)
        {
            if (!player.serverInput.IsDown(_config.Button))
                return false;

            if (!PermissionUtils.Verify(player))
                return false;

            if (!hitInfo.Weapon.ShortPrefabName.ToLower().Contains("bow"))
                return false;

            return true;
        }

        private bool VerifyHasConsumableItem(BasePlayer player, out Dictionary<Item, int> items)
        {
            items = new Dictionary<Item, int>();
            foreach (ConsumableOptions consumable in _config.ConsumableItems)
            {
                Item item = player.inventory.FindItemID(consumable.ItemShortname);
                if (item == null || item.amount < consumable.AmountToConsume)
                    return false;
                else
                    items.Add(item, consumable.AmountToConsume);
            }

            return true;
        }

        private void ConsumeItems(BasePlayer player, Dictionary<Item, int> items)
        {
            foreach (var item in items)
                player.inventory.Take(null, item.Key.info.itemid, item.Value);
        }

        private void HealPlayer(BasePlayer player)
        {
            player.Heal(_config.InstantHealAmount);
            player.metabolism.ApplyChange(MetabolismAttribute.Type.HealthOverTime, _config.HealAmountOverTime, 1f);

            if (_config.CanRevive && player.IsWounded())
                player.StopWounded();

            PlayEffect(_config.ReviveSoundEffect, player);
            ReplyToPlayer(player, Lang.ReviveSuccess);
        }

        private void PlayEffect(string effectPrefab, BasePlayer player)
        {
            if (string.IsNullOrEmpty(effectPrefab))
                return;

            Effect.server.Run(effectPrefab, player, 827230707, Vector3.up, new Vector3(), player.net.connection, false);
        }

        #endregion Functions

        #region Helper Classes

        private static class PermissionUtils
        {
            public const string USE = "revivearrows.use";

            public static void Register()
            {
                _instance.permission.RegisterPermission(USE, _instance);
            }

            public static bool Verify(BasePlayer player, string permissionName = USE)
            {
                if (_instance.permission.UserHasPermission(player.UserIDString, permissionName))
                    return true;

                _instance.ReplyToPlayer(player, Lang.NoPermission);
                return false;
            }
        }

        #endregion Helper Classes

        #region Localization

        private class Lang
        {
            public const string NoPermission = "NoPermission";
            public const string ReviveSuccess = "Revive.Success";
            public const string ReviveInsufficientConsumable = "Revive.InsufficientConsumable";
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [Lang.NoPermission] = "You lack the necessary permission to use heal arrows.",
                [Lang.ReviveSuccess] = "Your arrow has done its job, the target has been healed!",
                [Lang.ReviveInsufficientConsumable] = "Sorry, you cannot use revive revive without the required materials!",

            }, this, "en");
        }

        private void ReplyToPlayer(BasePlayer player, string messageName, params object[] args)
        {
            SendReply(player, string.Format(GetLang(messageName, player.UserIDString), args));
        }

        private string GetLang(string messageName, string playerId = null)
        {
            return lang.GetMessage(messageName, this, playerId);
        }

        #endregion Localization
    }
}