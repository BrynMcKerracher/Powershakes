using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Powershakes
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInProcess("valheim.exe")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class PowershakesPlugin : BaseUnityPlugin
    {
        //Plugin Data
        public const string PluginGUID = "TinyOak.Powershakes";
        public const string PluginName = "Powershakes";
        public const string PluginVersion = "1.3.0";
        
        //Localization
        public CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();
        
        //Asset Bundles
        private AssetBundle _powershakeAssets;

        //Custom Status Effects
        private CustomStatusEffect GutBlasterStatusEffect;
        private CustomStatusEffect TreeMurdererStatusEffect;
        private CustomStatusEffect DeerGodStatusEffect;

        //Harmony Patcher
        private readonly Harmony _harmonyPatcher = new(PluginGUID);

        /// <summary>
        /// All initialisation for the mod happens in Awake().
        /// </summary>
        private void Awake()
        {
            Jotunn.Logger.LogInfo($"Loading...");

            //We make sure all assetbundle objects are available before working to add custom content.
            _powershakeAssets = AssetUtils.LoadAssetBundleFromResources("powershakes");

            AddCustomStatusEffects();
            PrefabManager.OnVanillaPrefabsAvailable += AddCustomItems;

            _harmonyPatcher.PatchAll();

            Jotunn.Logger.LogInfo("Loaded successfully!");
        }

        #region CUSTOM ITEMS
        /// <summary>
        /// Adds all Custom Items.
        /// </summary>
        private void AddCustomItems()
        {
            AddGuckshakeItem();
            AddGuckshake2000Item();
            AddBuckshakeItem();
            AddBuckshake2000Item();
            AddChuckshakeItem();
            AddChuckshakeItem2000();

            PrefabManager.OnVanillaPrefabsAvailable -= AddCustomItems;
        }

        /// <summary>
        /// Add the Guckshake item, based on the ShocklateSmoothie prefab.
        /// Provides the SailingPowerStatusEffect when consumed.
        /// </summary>
        private void AddGuckshakeItem()
        {
            ItemConfig guckShakeConfig = new()
            {
                Name = "$item_guckshake",
                Description = "$item_guckshake_desc",
                CraftingStation = CraftingStations.Cauldron,
                Icon = _powershakeAssets.LoadAsset<Sprite>("assets/powershakes/icon/guckshakeicon.png")
            };

            guckShakeConfig.AddRequirement(new RequirementConfig("Guck", 1));
            guckShakeConfig.AddRequirement(new RequirementConfig("Raspberry", 2));
            guckShakeConfig.AddRequirement(new RequirementConfig("Blueberries", 2));

            CustomItem guckShake = new(_powershakeAssets, "assets/powershakes/shakemodels/guckshake.prefab", false, guckShakeConfig);
            guckShake.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = _powershakeAssets.LoadAsset<StatusEffect>("assets/powershakes/statuseffects/GP_Custom_Moder.asset");

            ItemManager.Instance.AddItem(guckShake);

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"item_guckshake", "Guckshake"},
                {"item_guckshake_desc", "Gotta go fast!"}
            });
        }

        private void AddGuckshake2000Item()
        {
            ItemConfig guckShake2000Config = new()
            {
                Name = "$item_guckshake2000",
                Description = "$item_guckshake2000_desc",
                CraftingStation = CraftingStations.Cauldron,
                Icon = _powershakeAssets.LoadAsset<Sprite>("assets/powershakes/icon/guckshake2000icon.png"),
                Weight = 10
            };

            guckShake2000Config.AddRequirement(new RequirementConfig("Guck", 10));
            guckShake2000Config.AddRequirement(new RequirementConfig("Raspberry", 20));
            guckShake2000Config.AddRequirement(new RequirementConfig("Blueberries", 20));

            CustomItem guckShake2000 = new(_powershakeAssets, "assets/powershakes/shakemodels/guckshake2000.prefab", false, guckShake2000Config);
            guckShake2000.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = GutBlasterStatusEffect.StatusEffect;

            ItemManager.Instance.AddItem(guckShake2000);

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"item_guckshake2000", "Guckshake2000"},
                {"item_guckshake2000_desc", "Gotta go really fast!"}
            });
        }

        /// <summary>
        /// Add the Buckshake item, based on the ShocklateSmoothie prefab.
        /// Provides the EikthyrPowerStatusEffect when consumed.
        /// </summary>
        private void AddBuckshakeItem()
        {
            ItemConfig buckShakeConfig = new()
            {
                Name = "$item_buckshake",
                Description = "$item_buckshake_desc",
                CraftingStation = CraftingStations.Cauldron,
                Icon = _powershakeAssets.LoadAsset<Sprite>("assets/powershakes/icon/buckshakeicon.png")
            };

            buckShakeConfig.AddRequirement(new RequirementConfig("TrophyDeer", 1));
            buckShakeConfig.AddRequirement(new RequirementConfig("Raspberry", 2));
            buckShakeConfig.AddRequirement(new RequirementConfig("Blueberries", 2));

            CustomItem buckShake = new(_powershakeAssets, "assets/powershakes/shakemodels/buckshake.prefab", false, buckShakeConfig);
            buckShake.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = _powershakeAssets.LoadAsset<StatusEffect>("assets/powershakes/statuseffects/GP_Custom_Eikthyr.asset");

            ItemManager.Instance.AddItem(buckShake);

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"item_buckshake", "Buckshake"},
                {"item_buckshake_desc", "Move those trotters!"}
            });
        }

        private void AddBuckshake2000Item()
        {
            ItemConfig buckShakeConfig = new()
            {
                Name = "$item_buckshake2000",
                Description = "$item_buckshake2000_desc",
                CraftingStation = CraftingStations.Cauldron,
                Icon = _powershakeAssets.LoadAsset<Sprite>("assets/powershakes/icon/buckshake2000icon.png"),
                Weight = 10
            };

            buckShakeConfig.AddRequirement(new RequirementConfig("TrophyDeer", 10));
            buckShakeConfig.AddRequirement(new RequirementConfig("Raspberry", 20));
            buckShakeConfig.AddRequirement(new RequirementConfig("Blueberries", 20));

            CustomItem buckShake = new(_powershakeAssets, "assets/powershakes/shakemodels/buckshake2000.prefab", false, buckShakeConfig);
            buckShake.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = DeerGodStatusEffect.StatusEffect;

            ItemManager.Instance.AddItem(buckShake);

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"item_buckshake2000", "Buckshake2000"},
                {"item_buckshake2000_desc", "Oh deer!"}
            });
        }

        private void AddChuckshakeItem()
        {
            ItemConfig chuckShakeConfig = new()
            {
                Name = "$item_chuckshake",
                Description = "$item_chuckshake_desc",
                CraftingStation = CraftingStations.Cauldron,
                Icon = _powershakeAssets.LoadAsset<Sprite>("assets/powershakes/icon/chuckshakeicon.png")
            };

            chuckShakeConfig.AddRequirement(new RequirementConfig("AncientSeed", 1));
            chuckShakeConfig.AddRequirement(new RequirementConfig("Raspberry", 2));
            chuckShakeConfig.AddRequirement(new RequirementConfig("Blueberries", 2));

            CustomItem chuckShake = new(_powershakeAssets, "assets/powershakes/shakemodels/chuckshake.prefab", false, chuckShakeConfig);
            chuckShake.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = _powershakeAssets.LoadAsset<StatusEffect>("assets/powershakes/statuseffects/GP_Custom_TheElder.asset");

            ItemManager.Instance.AddItem(chuckShake);

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"item_chuckshake", "Chuckshake"},
                {"item_chuckshake_desc", "How much wood can a woodchuck chuck?"}
            });
        }

        private void AddChuckshakeItem2000()
        {
            ItemConfig chuckShakeConfig = new()
            {
                Name = "$item_chuckshake2000",
                Description = "$item_chuckshake2000_desc",
                CraftingStation = CraftingStations.Cauldron,
                Icon = _powershakeAssets.LoadAsset<Sprite>("assets/powershakes/icon/chuckshake2000icon.png"),
                Weight = 10
            };

            chuckShakeConfig.AddRequirement(new RequirementConfig("AncientSeed", 10));
            chuckShakeConfig.AddRequirement(new RequirementConfig("Raspberry", 20));
            chuckShakeConfig.AddRequirement(new RequirementConfig("Blueberries", 20));

            CustomItem chuckShake = new(_powershakeAssets, "assets/powershakes/shakemodels/chuckshake2000.prefab", false, chuckShakeConfig);
            chuckShake.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = TreeMurdererStatusEffect.StatusEffect;

            ItemManager.Instance.AddItem(chuckShake);

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"item_chuckshake2000", "Chuckshake2000"},
                {"item_chuckshake2000_desc", "A lot, as it turns out."}
            });
        }
        #endregion

        #region CUSTOM STATUS EFFECTS
        /// <summary>
        /// Adds all Custom Status Effects
        /// </summary>
        private void AddCustomStatusEffects()
        {
            AddGutBlasterStatusEffect();
            AddTreeMurdererStatusEffect();
            AddDeerGodStatusEffect();
        }

        /// <summary>
        /// Adds an effect that provides Moder's Forsaken power for 300 seconds.
        /// </summary>
        private void AddGutBlasterStatusEffect()
        {
            StatusEffect effect = _powershakeAssets.LoadAsset<StatusEffect>("assets/powershakes/statuseffects/GP_Custom_GutBlaster.asset");

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"gutblaster_effectname", "Gut Blaster"},
                {"gutblaster_effectstart", "You feel the guck hit your stomach..."},
                {"gutblaster_effectstop", "You feel the effects of the guck waning"},
                {"gutblaster_tooltip", "Your bodily gases will power the sails of your ship."}
            });

            GutBlasterStatusEffect = new CustomStatusEffect(effect, false);

            ItemManager.Instance.AddStatusEffect(GutBlasterStatusEffect);
        }

        private void AddTreeMurdererStatusEffect()
        {
            StatusEffect effect = _powershakeAssets.LoadAsset<StatusEffect>("assets/powershakes/statuseffects/GP_Custom_Chuckshake2000.asset");

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"treemurderer_effectname", "Tree Murderer"},
                {"treemurderer_effectstart", "You feel the ancient seeds rage in your mind..."},
                {"treemurderer_effectstop", "The rage of the ancient seeds begins to fade"},
                {"treemurderer_tooltip", "You do 2000% damage to trees."}
            });

            TreeMurdererStatusEffect = new CustomStatusEffect(effect, false);

            ItemManager.Instance.AddStatusEffect(TreeMurdererStatusEffect);
        }

        private void AddDeerGodStatusEffect()
        {
            SE_Stats effect = _powershakeAssets.LoadAsset<SE_Stats>("assets/powershakes/statuseffects/GP_Custom_Buckshake2000.asset");

            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"deergod_effectname", "Deer God"},
                {"deergod_effectstart", "You feel a need, a need for speed..."},
                {"deergod_effectstop", "The need for speed begins to fade"},
                {"deergod_tooltip", "Running and jumping use 80% less stamina. You move 100% faster and jump 40% higher and take 90% less fall damage."}
            });

            DeerGodStatusEffect = new CustomStatusEffect(effect, false);

            ItemManager.Instance.AddStatusEffect(DeerGodStatusEffect);
        }
        #endregion

        #region PATCHES 
        [HarmonyPatch(typeof(Ship), nameof(Ship.GetSailForce))]
        class Guckshake2000ShipEffectsPatch
        {
            static void Postfix(Ship __instance, ref Vector3 __result)
            {
                Player targetPlayer = Player.GetPlayer(__instance.m_shipControlls.GetUser());
                if (targetPlayer == null || !targetPlayer.m_seman.HaveStatusEffect("GP_Custom_GutBlaster".GetStableHashCode())) return;

                __result *= 5;
            }
        }
        #endregion
    }
}

