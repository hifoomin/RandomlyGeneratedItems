using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using UnityEngine;

namespace RandomlyGeneratedItems
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(ItemAPI))]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public const string PluginAuthor = "HIFU";
        public const string PluginName = "RandomlyGeneratedItems";
        public const string PluginVersion = "0.0.1";

        public static ConfigFile RGIConfig;
        public static ManualLogSource RGILogger;

        private string version = PluginVersion;

        private static ulong seed;
        private static Xoroshiro128Plus rng;

        private static ItemDef itemDef;
        private static System.Random statValueRng;

        private static GameObject model;
        private static Sprite icon;

        private static System.Random modelRng;

        private static System.Random iconRng;

        // yeah i know im using system.random, placeholder for now
        // still dont know how im gonna network this, maybe not ever lol

        public static ConfigEntry<ulong> seedConfig { get; set; }

        private static List<GameObject> itemModels = new();
        private static List<Sprite> itemIcons = new();

        private static List<ItemDef> myItemDefs = new();

        public void Awake()
        {
            RGILogger = Logger;
            RGIConfig = Config;
            seed = (ulong)Random.RandomRangeInt(0, 10000) ^ (ulong)Random.RandomRangeInt(1, 10) << 16;
            rng = new(seed);
            Logger.LogFatal("seed is " + seed);
            seedConfig = Config.Bind("Seed number", "_", seed, "_");

            int maxItems = NameSystem.itemNamePrefix.Count < NameSystem.itemName.Count ? NameSystem.itemNamePrefix.Count : NameSystem.itemName.Count;

            On.RoR2.ItemCatalog.Init += ItemCatalog_Init;

            for (int i = 0; i < maxItems; i++)
            {
                GenerateItem();
                if (i == maxItems - 1)
                {
                    Logger.LogWarning("Max item amount of " + maxItems + " reached");
                }
            }
        }

        private void ItemCatalog_Init(On.RoR2.ItemCatalog.orig_Init orig)
        {
            orig();

            foreach (ItemDef def in ContentManager._itemDefs)
            {
                if (def.pickupModelPrefab) itemModels.Add(def.pickupModelPrefab);
                if (def.pickupIconSprite) itemIcons.Add(def.pickupIconSprite);
            }

            foreach (BuffDef def in ContentManager._buffDefs)
            {
                if (def.iconSprite)
                {
                    var clone = Instantiate(def);
                    itemIcons.Add(clone.iconSprite);
                }
            }

            foreach (EquipmentDef def in ContentManager._equipmentDefs)
            {
                if (def.pickupModelPrefab)
                {
                    itemModels.Add(def.pickupModelPrefab);
                    itemModels.Add(def.pickupModelPrefab);
                    itemModels.Add(def.pickupModelPrefab);
                }

                if (def.pickupIconSprite)
                {
                    itemIcons.Add(def.pickupIconSprite);
                    itemIcons.Add(def.pickupIconSprite);
                }
            }

            Logger.LogFatal("modelList has " + itemModels.Count + " elements");
            Logger.LogFatal("iconList has " + itemIcons.Count + " elements");

            foreach (ItemDef itemDef in myItemDefs)
            {
                var modelRng2 = modelRng.Next(itemModels.Count);
                var iconRng2 = iconRng.Next(itemIcons.Count);
                /*
                if (itemModels.Count > 0)
                {
                    itemDef.pickupModelPrefab = itemModels[modelRng2];
                    itemModels.RemoveAt(modelRng2);

                    Logger.LogFatal("itemmodels[modelRng2] is " + itemModels[modelRng2]);
                    itemModels.RemoveAt(modelRng2);
                }
                */

                // changing model NREs for some reasonok imm
                if (itemIcons.Count > 0)
                {
                    itemDef.pickupIconSprite = itemIcons[iconRng2];
                    Logger.LogWarning("itemicons[iconRng2] is " + itemIcons[iconRng2]);
                    itemIcons.RemoveAt(iconRng2);
                }
            }
        }

        private ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        private void GenerateItem()
        {
            string itemName = "";
            string itemPickup = "Gain ";
            string itemDesc = "Gain ";
            ItemTier tier;

            //System.Random modelRng = new();
            //System.Random iconRng = new();

            // load a random prefab through addressables and assign it to model?
            // load a random sprite through addressables and assign it to icon?

            System.Random prefixRng = new();
            System.Random nameRng = new();
            System.Random tierRng = new();
            statValueRng = new();
            modelRng = new();
            iconRng = new();

            var prefixRng2 = prefixRng.Next(NameSystem.itemNamePrefix.Count);
            var nameRng2 = nameRng.Next(NameSystem.itemName.Count);

            itemName += NameSystem.itemNamePrefix[prefixRng2] + " ";
            NameSystem.itemNamePrefix.RemoveAt(prefixRng2);

            itemName += NameSystem.itemName[nameRng2];
            NameSystem.itemName.RemoveAt(nameRng2);

            string xmlSafeItemName = itemName.ToUpper();
            xmlSafeItemName = xmlSafeItemName.Replace(" ", "_").Replace("'", "").Replace("&", "AND");

            tier = (ItemTier)tierRng.Next(0, 3);

            string translatedTier = "";

            switch (tier)
            {
                case (ItemTier)0:
                    statValueRng.Next(8, 14);
                    translatedTier = "Common";
                    break;

                case (ItemTier)1:
                    statValueRng.Next(20, 35);
                    translatedTier = "Uncommon";
                    break;

                case (ItemTier)2:
                    statValueRng.Next(40, 60);
                    translatedTier = "Legendary";
                    break;
            }

            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "ITEM_" + xmlSafeItemName;
            itemDef.nameToken = "ITEM_" + xmlSafeItemName + "_NAME";
            itemDef.pickupToken = "ITEM_" + xmlSafeItemName + "_PICKUP";
            itemDef.descriptionToken = "ITEM_" + xmlSafeItemName + "_DESCRIPTION";
            itemDef.loreToken = "ITEM_" + xmlSafeItemName + "_LORE";
            itemDef.pickupModelPrefab = null;
            itemDef.pickupIconSprite = null;
            itemDef.hidden = false;
            itemDef.deprecatedTier = tier;

            LanguageAPI.Add(itemDef.nameToken, itemName);
            LanguageAPI.Add(itemDef.pickupToken, itemPickup);
            LanguageAPI.Add(itemDef.descriptionToken, itemDesc);

            ItemAPI.Add(new CustomItem(itemDef, CreateItemDisplayRules()));
            myItemDefs.Add(itemDef);
            Logger.LogWarning("Generated a " + translatedTier + " item named " + itemName);

            //On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;

            //RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, Language self, string token)
        {
            if (token == "ITEM_" + NameSystem.itemName + "_PICKUP")
            {
            }
            if (token == "ITEM_" + NameSystem.itemName + "_DESCRIPTION")
            {
            }
            return orig(self, token);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory)
            {
                var stack = sender.inventory.GetItemCount(itemDef);
                if (stack > 0)
                {
                }
            }
        }
    }
}