using Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

/*
 * Rewritten from scratch and maintained to present by VisEntities
 * Originally created by redBDGR, up to version 1.0.1
 */

namespace Oxide.Plugins
{
    [Info("Revive Arrows", "VisEntities", "3.0.0")]
    [Description("Heal and revive the wounded from a distance.")]
    public class ReviveArrows : RustPlugin
    {
        #region Fields

        private static ReviveArrows _plugin;
        private static Configuration _config;

        private const string FX_INJECT_FRIEND = "assets/prefabs/tools/medical syringe/effects/inject_friend.prefab";

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Instant Health Increase")]
            public float InstantHealthIncrease { get; set; }

            [JsonProperty("Health Increase Over Time")]
            public float HealthIncreaseOverTime { get; set; }

            [JsonProperty("Can Revive Wounded")]
            public bool CanReviveWounded { get; set; }
            
            [JsonProperty("Arrow Ingredients")]
            public List<ItemInfo> ArrowIngredients { get; set; }
        }

        public class ItemInfo
        {
            [JsonProperty("Shortname")]
            public string Shortname { get; set; }

            [JsonProperty("Amount")]
            public int Amount { get; set; }

            [JsonIgnore]
            private bool _validated;

            [JsonIgnore]
            private ItemDefinition _itemDefinition;

            [JsonIgnore]
            public ItemDefinition ItemDefinition
            {
                get
                {
                    if (!_validated)
                    {
                        ItemDefinition matchedItemDefinition = ItemManager.FindItemDefinition(Shortname);
                        if (matchedItemDefinition != null)
                            _itemDefinition = matchedItemDefinition;
                        else
                            return null;

                        _validated = true;
                    }

                    return _itemDefinition;
                }
            }

            public int GetItemAmount(ItemContainer container)
            {
                return container.GetAmount(ItemDefinition.itemid, true);
            }

            public void GiveItem(BasePlayer player, ItemContainer container)
            {
                container.GiveItem(ItemManager.CreateByItemID(ItemDefinition.itemid, Amount));
                player.Command("note.inv", ItemDefinition.itemid, Amount);
            }

            public int TakeItem(BasePlayer player, ItemContainer container)
            {
                int amountTaken = container.Take(null, ItemDefinition.itemid, Amount);
                player.Command("note.inv", ItemDefinition.itemid, -amountTaken);
                return amountTaken;
            }
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

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                InstantHealthIncrease = 15f,
                HealthIncreaseOverTime = 20f,
                CanReviveWounded = true,
                ArrowIngredients = new List<ItemInfo>
                {
                    new ItemInfo
                    {
                        Shortname = "syringe.medical",
                        Amount = 1
                    },
                    new ItemInfo
                    {
                        Shortname = "rope",
                        Amount = 1
                    }
                }
            };
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
            PermissionUtil.RegisterPermissions();
        }

        private void Unload()
        {
            _config = null;
            _plugin = null;
        }

        private void OnActiveItemChanged(BasePlayer player, Item oldItem, Item newItem)
        {
            if (player == null || !PermissionUtil.VerifyHasPermission(player))
                return;

            if (!newItem.info.shortname.Contains("bow"))
                return;

            SendGameTip(player, lang.GetMessage(Lang.HealArrowUsage, this, player.UserIDString), 5f);
        }

        private object OnPlayerAttack(BasePlayer player, HitInfo hitInfo)
        {
            if (player == null || hitInfo == null)
                return null;

            if (!PermissionUtil.VerifyHasPermission(player))
                return null;

            BasePlayer hitPlayer = hitInfo.HitEntity.ToPlayer();
            if (hitPlayer == null || hitPlayer.IsNpc)
                return null;

            if (!hitInfo.Weapon.ShortPrefabName.Contains("bow"))
                return null;

            if (!player.serverInput.IsDown(BUTTON.USE))
                return null;

            foreach (ItemInfo ingredient in _config.ArrowIngredients)
            {
                int amount = ingredient.GetItemAmount(player.inventory.containerMain);
                if (amount < ingredient.Amount)
                {
                    SendReplyToPlayer(player, Lang.InsufficientIngredients, ingredient.Shortname, ingredient.Amount);
                    return null;
                }
            }

            foreach (ItemInfo ingredient in _config.ArrowIngredients)
            {
                ingredient.TakeItem(player, player.inventory.containerMain);
            }

            Heal(hitPlayer);
            RunEffect(FX_INJECT_FRIEND, hitPlayer, boneId: 698017942);
            SendGameTip(player, lang.GetMessage(Lang.PlayerHealed, this, player.UserIDString), 5f, hitPlayer.displayName, _config.InstantHealthIncrease);

            return true;
        }

        #endregion Oxide Hooks

        #region Functions

        private void Heal(BasePlayer player)
        {
            player.Heal(_config.InstantHealthIncrease);
            player.metabolism.ApplyChange(MetabolismAttribute.Type.HealthOverTime, _config.HealthIncreaseOverTime, 1f);
            
            if (_config.CanReviveWounded)
            {
                player.StopWounded();
            }
        }

        #endregion Functions

        #region Helper Functions

        private void SendGameTip(BasePlayer player, string message, float durationSeconds, params object[] args)
        {
            message = string.Format(message, args);

            player.SendConsoleCommand("gametip.showgametip", message);
            timer.Once(durationSeconds, () =>
            {
                if (player != null)
                    player.SendConsoleCommand("gametip.hidegametip");
            });
        }

        private static void RunEffect(string prefab, BaseEntity entity, uint boneId = 0, Vector3 localPosition = default(Vector3), Vector3 localDirection = default(Vector3), Connection effectRecipient = null, bool sendToAll = false)
        {
            Effect.server.Run(prefab, entity, boneId, localPosition, localDirection, effectRecipient, sendToAll);
        }

        #endregion Helper Functions

        #region Helper Classes

        private static class PermissionUtil
        {
            public const string USE = "revivearrows.use";

            public static void RegisterPermissions()
            {
                _plugin.permission.RegisterPermission(USE, _plugin);
            }

            public static bool VerifyHasPermission(BasePlayer player, string permissionName = USE)
            {
                return _plugin.permission.UserHasPermission(player.UserIDString, permissionName);
            }
        }

        #endregion Helper Classes

        #region Localization

        private class Lang
        {
            public const string InsufficientIngredients = "InsufficientIngredients";
            public const string PlayerHealed = "PlayerHealed ";
            public const string HealArrowUsage = "HealArrowUsage ";
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [Lang.InsufficientIngredients] = "You don't have enough <color=#FABE28>{0}</color>. Required: <color=#FABE28>{1}</color>",
                [Lang.PlayerHealed] = "You healed <color=#FABE28>{0}</color> by <color=#FABE28>{1}</color> health points",
                [Lang.HealArrowUsage] = "Hold down <color=#FABE28>use</color> to heal a friend with an arrow",

            }, this, "en");
        }

        private void SendReplyToPlayer(BasePlayer player, string messageKey, params object[] args)
        {
            string message = lang.GetMessage(messageKey, this, player.UserIDString);
            if (args.Length > 0)
                message = string.Format(message, args);

            SendReply(player, message);
        }

        #endregion Localization
    }
}