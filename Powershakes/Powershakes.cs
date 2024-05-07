using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Powershakes
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInProcess("valheim.exe")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class PowershakesPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "TinyOak.Powershakes";
        public const string PluginName = "Powershakes";
        public const string PluginVersion = "1.2.0";
        
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();
        private readonly Harmony _harmonyPatcher = new(PluginGUID);

        //Asset Bundles
        private static AssetBundle _powershakeAssets;

        //Custom Status Effects
        public static CustomStatusEffect SailingPowerStatusEffect;
        public static CustomStatusEffect EikthyrPowerStatusEffect;

        //VFX/SFX
        public static GameObject GuckshakeSFXPrefab;

        //RPCs
        public static CustomRPC RpcGuckEffect;

        private void Awake()
        {
            Jotunn.Logger.LogInfo("Loading...");

            _powershakeAssets = AssetUtils.LoadAssetBundleFromResources("powershakes");

            GuckshakeSFXPrefab = _powershakeAssets.LoadAsset<GameObject>("assets/powershakes/sfx_guckshake_gaseffects.prefab");

            PrefabManager.OnVanillaPrefabsAvailable += AddCustomItems;
            _harmonyPatcher.PatchAll();
            AddCustomStatusEffects();

            RpcGuckEffect = NetworkManager.Instance.AddRPC("RpcGuckEffect", RpcGuckEffectServerReceive, RpcGuckEffectClientReceive);

            Jotunn.Logger.LogInfo("Loaded successfully!");
        }

        #region RPCS
        private IEnumerator RpcGuckEffectServerReceive(long sender, ZPackage package)
        {
            string playerName = package.ReadString();
            Jotunn.Logger.LogInfo($"Server received from {playerName}");

            ZPackage serverPackage = new();
            serverPackage.Write(playerName);

            RpcGuckEffect.SendPackage(ZNet.instance.m_peers, serverPackage);
            PowershakeEffectsPatch.CreateGuckEffectsForPlayer(playerName);
            yield return null;
        }

        private IEnumerator RpcGuckEffectClientReceive(long sender, ZPackage package)
        {
            PowershakeEffectsPatch.CreateGuckEffectsForPlayer(package.ReadString());
            yield return null;
        }
        #endregion

        #region CUSTOM ITEMS
        /// <summary>
        /// Adds all Custom Items.
        /// </summary>
        private void AddCustomItems()
        {
            AddGuckshakeItem();
            AddBuckshakeItem();

            PrefabManager.OnVanillaPrefabsAvailable -= AddCustomItems;
        }

        /// <summary>
        /// Add the Guckshake item, based on the ShocklateSmoothie prefab.
        /// Provides the SailingPowerStatusEffect when consumed.
        /// </summary>
        private void AddGuckshakeItem()
        {
            ItemConfig guckShakeConfig = new ItemConfig();
            guckShakeConfig.Name = "$item_guckshake";
            guckShakeConfig.Description = "$item_guckshake_desc";
            guckShakeConfig.CraftingStation = CraftingStations.Cauldron;
            guckShakeConfig.AddRequirement(new RequirementConfig("Guck", 1));
            guckShakeConfig.AddRequirement(new RequirementConfig("Raspberry", 2));
            guckShakeConfig.AddRequirement(new RequirementConfig("Blueberries", 2));

            CustomItem guckShake = new("Guckshake", "ShocklateSmoothie", guckShakeConfig);
            guckShake.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = SailingPowerStatusEffect.StatusEffect;

            ItemManager.Instance.AddItem(guckShake);

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"item_guckshake", "Guckshake"},
                {"item_guckshake_desc", "Gotta go fast!"}
            });
        }

        private void AddBuckshakeItem()
        {
            ItemConfig buckShakeConfig = new ItemConfig();
            buckShakeConfig.Name = "$item_buckshake";
            buckShakeConfig.Description = "$item_buckshake_desc";
            buckShakeConfig.CraftingStation = CraftingStations.Cauldron;
            buckShakeConfig.AddRequirement(new RequirementConfig("TrophyDeer", 1));
            buckShakeConfig.AddRequirement(new RequirementConfig("Raspberry", 2));
            buckShakeConfig.AddRequirement(new RequirementConfig("Blueberries", 2));

            CustomItem buckShake = new("Buckshake", "ShocklateSmoothie", buckShakeConfig);
            buckShake.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = EikthyrPowerStatusEffect.StatusEffect;

            ItemManager.Instance.AddItem(buckShake);

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"item_buckshake", "Buckshake"},
                {"item_buckshake_desc", "Move those trotters!"}
            });
        }
        #endregion

        #region CUSTOM STATUS EFFECTS
        /// <summary>
        /// Adds all Custom Status Effects
        /// </summary>
        private void AddCustomStatusEffects()
        {
            AddSailingPowerStatusEffect();
            AddEikthyrPowerStatusEffect();
        }

        /// <summary>
        /// Adds an effect that provides Moder's sailing power for 300 seconds and
        /// causes accompanying audio/visual effects to occur.
        /// </summary>
        private void AddSailingPowerStatusEffect()
        {
            StatusEffect effect = ScriptableObject.CreateInstance<StatusEffect>();
            effect.name = "Gut Blaster Effect";
            effect.m_name = "$gutblaster_effectname";
            effect.m_ttl = 300;
            effect.m_tooltip = "$gutblaster_tooltip";
            effect.m_startMessage = "$gutblaster_effectstart";
            effect.m_stopMessage = "$gutblaster_effectstop";
            effect.m_startMessageType = MessageHud.MessageType.Center;
            effect.m_stopMessageType = MessageHud.MessageType.Center;
            effect.m_attributes = StatusEffect.StatusAttribute.SailingPower;
            effect.m_icon = _powershakeAssets.LoadAsset<Sprite>("assets/powershakes/biohazard.png");

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"gutblaster_effectname", "Gut Blaster"},
                {"gutblaster_effectstart", "You feel the guck hit your stomach..."},
                {"gutblaster_effectstop", "You feel the effects of the guck waning"},
                {"gutblaster_tooltip", "Your bodily gases will power the sails of your ship."}
            });

            SailingPowerStatusEffect = new CustomStatusEffect(effect, false);

            ItemManager.Instance.AddStatusEffect(SailingPowerStatusEffect);
        }

        private void AddEikthyrPowerStatusEffect()
        {
            SE_Stats effect = ScriptableObject.CreateInstance<SE_Stats>();
            effect.name = "Eikthyr Power";
            effect.m_name = "$eikthyrpower_effectname";
            effect.m_ttl = 300;
            effect.m_tooltip = "$eikthyrpower_tooltip";
            effect.m_startMessage = "$eikthyrpower_effectstart";
            effect.m_stopMessage = "$eikthyrpower_effectstop";
            effect.m_startMessageType = MessageHud.MessageType.Center;
            effect.m_stopMessageType = MessageHud.MessageType.Center;
            effect.m_runStaminaDrainModifier = -0.6f;
            effect.m_jumpStaminaUseModifier = -0.6f;
            effect.m_icon = _powershakeAssets.LoadAsset<Sprite>("assets/powershakes/eikthyr.png");

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"eikthyrpower_effectname", "Eikthyr"},
                {"eikthyrpower_effectstart", "The power of Eikthyr suffuses your veins"},
                {"eikthyrpower_effectstop", "You feel Eikthyr's power fading"},
                {"eikthyrpower_tooltip", "Running and jumping use 60% less stamina."}
            });

            EikthyrPowerStatusEffect = new CustomStatusEffect(effect, false);

            ItemManager.Instance.AddStatusEffect(EikthyrPowerStatusEffect);
        }
        #endregion

        #region PATCHES STATUS EFFECTS
        /// <summary>
        /// Whenever the player eats, we first check if it's one of the powershakes and then propagate any 
        /// effects to the server.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.EatFood))]
        class PowershakeEffectsPatch
        {
            static void Prefix(Player __instance, ItemDrop.ItemData item)
            {
                StatusEffect foodStatusEffect = item?.m_shared?.m_consumeStatusEffect;

                if (foodStatusEffect == null || !__instance.CanEat(item, false))
                {
                    return;
                }

                if (foodStatusEffect.m_name == "$gutblaster_effectname")
                {
                    ZPackage package = new ZPackage();
                    package.Write(__instance.GetPlayerName());

                    RpcGuckEffect.SendPackage(ZNet.GetUID(), package);
                }
            }

            public static void CreateGuckEffectsForPlayer(string playerName)
            {
                Player targetPlayer = FindObjectsOfType<Player>().First(n => n.GetPlayerName() == playerName);
                if (targetPlayer == null)
                {
                    Jotunn.Logger.LogWarning($"Failed to create effects on player: '{playerName}' not found.");
                    return;
                }

                GameObject randomSFXPlayer = new();
                randomSFXPlayer.transform.parent = targetPlayer.gameObject.transform;

                TimedDestruction timer = randomSFXPlayer.AddComponent<TimedDestruction>();
                timer.m_timeout = PowershakesPlugin.SailingPowerStatusEffect.StatusEffect.m_ttl;
                timer.Trigger();

                RandomSFXPlayer sfx = randomSFXPlayer.AddComponent<RandomSFXPlayer>();
                sfx.Trigger(targetPlayer.transform);

                GameObject gutBlasterEffectsVFX = Instantiate(_powershakeAssets.LoadAsset<GameObject>("assets/powershakes/vfx_guckshake_gaseffects.prefab"), targetPlayer.gameObject.transform);
                gutBlasterEffectsVFX.transform.localPosition = new Vector3(0, 0.9f, 0);
            }
        }
        #endregion 
    }
}

