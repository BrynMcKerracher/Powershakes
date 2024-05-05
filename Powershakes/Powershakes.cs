using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;
using UnityEngine;
using ValheimModdingWiki;

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
        public const string PluginVersion = "0.0.1";
        
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();
        private readonly Harmony _harmonyPatcher = new(PluginGUID);

        //Asset Bundles
        private static AssetBundle _powershakeAssets;

        //Custom Status Effects
        private CustomStatusEffect _sailingPowerStatusEffect;

        //VFX/SFX
        public static GameObject GuckshakeSFXPrefab;

        private void Awake()
        {
            Jotunn.Logger.LogInfo("Loading...");

            _powershakeAssets = AssetUtils.LoadAssetBundleFromResources("powershakes");

            GuckshakeSFXPrefab = _powershakeAssets.LoadAsset<GameObject>("assets/powershakes/sfx_guckshake_gaseffects.prefab");

            PrefabManager.OnVanillaPrefabsAvailable += AddCustomItems;
            _harmonyPatcher.PatchAll();
            AddCustomStatusEffects();

            Jotunn.Logger.LogInfo("Loaded successfully!");
        }

        #region CUSTOM ITEMS
        /// <summary>
        /// Adds all Custom Items.
        /// </summary>
        private void AddCustomItems()
        {
            AddGuckshakeItem();
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

            CustomItem guckShake = new CustomItem("Guckshake", "ShocklateSmoothie", guckShakeConfig);
            ItemManager.Instance.AddItem(guckShake);

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"item_guckshake", "Guckshake"},
                {"item_guckshake_desc", "Gotta go fast!"}
            });

            PrefabManager.OnVanillaPrefabsAvailable -= AddCustomItems;

            guckShake.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = _sailingPowerStatusEffect.StatusEffect;
        }
        #endregion

        #region CUSTOM STATUS EFFECTS
        /// <summary>
        /// Adds all Custom Status Effects
        /// </summary>
        private void AddCustomStatusEffects()
        {
            AddSailingPowerStatusEffect();
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

            _sailingPowerStatusEffect = new CustomStatusEffect(effect, false);

            ItemManager.Instance.AddStatusEffect(_sailingPowerStatusEffect);
        }
        #endregion

        #region PATCHES STATUS EFFECTS
        [HarmonyPatch(typeof(Player), nameof(Player.EatFood))]
        class PowershakeEffectsPatch
        {
            static void Prefix(Player __instance, ItemDrop.ItemData item)
            {
                if (item.m_shared.m_consumeStatusEffect.m_name == "$gutblaster_effectname" && __instance.CanEat(item, false))
                {
                    GameObject randomSFXPlayer = new();
                    randomSFXPlayer.transform.parent = __instance.gameObject.transform;

                    TimedDestruction timer = randomSFXPlayer.AddComponent<TimedDestruction>();
                    timer.m_timeout = item.m_shared.m_consumeStatusEffect.m_ttl;
                    timer.Trigger();

                    RandomSFXPlayer sfx = randomSFXPlayer.AddComponent<RandomSFXPlayer>();
                    sfx.Trigger(__instance.transform);

                    GameObject gutBlasterEffectsVFX = Instantiate(_powershakeAssets.LoadAsset<GameObject>("assets/powershakes/vfx_guckshake_gaseffects.prefab"), __instance.gameObject.transform);
                    gutBlasterEffectsVFX.transform.localPosition = new Vector3(0, 0.9f, 0);
                }
            }
        }
        #endregion 
    }
}

