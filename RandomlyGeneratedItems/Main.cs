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
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(ItemAPI), nameof(PrefabAPI), nameof(RecalculateStatsAPI))]
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
        public static Xoroshiro128Plus rng;

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
        private static Dictionary<string, Effect> map = new();

        public void Awake()
        {
            RGILogger = Logger;
            RGIConfig = Config; // seedconfig does nothing right now because config.bind.value returns a bepinex.configentry<ulong> instead of a plain ulong???
            seedConfig = Config.Bind<ulong>("Configuration:", "Seed", 0, "the seed that will be used for random generation. this MUST be the same between all clients in multiplayer!!!. a seed of 0 will generate a random seed instead");
            seed = (ulong)Random.RandomRangeInt(0, 10000) ^ (ulong)Random.RandomRangeInt(1, 10) << 16;
            rng = new(seed);
            Logger.LogFatal("seed is " + seed);

            // int maxItems = itemNamePrefix.Count < itemName.Count ? itemNamePrefix.Count : itemName.Count;
            int maxItems = Config.Bind<int>("Configuration:", "Maximum Items", 350, "the maximum amount of items the mod will generate").Value;

            On.RoR2.ItemCatalog.Init += ItemCatalog_Init;

            for (int i = 0; i < maxItems; i++)
            {
                GenerateItem();
                if (i == maxItems - 1)
                {
                    Logger.LogWarning("Max item amount of " + maxItems + " reached");
                }
            }

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;

            On.RoR2.CharacterBody.Update += (orig, self) => {
                orig(self);
                if (UnityEngine.Networking.NetworkServer.active) {
                    if (self.characterMotor && self.characterMotor.velocity.magnitude != -self.characterMotor.lastVelocity.magnitude) {
                        if (self.isPlayerControlled) {
                            self.RecalculateStats();
                        }
                    }
                }
            }; 
        }

        private void ItemCatalog_Init(On.RoR2.ItemCatalog.orig_Init orig)
        {
            orig();

            /* foreach (ItemDef def in ContentManager._itemDefs)
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
                var iconRng2 = iconRng.Next(itemIcons.Count); */
                /*
                if (itemModels.Count > 0)
                {
                    itemDef.pickupModelPrefab = itemModels[modelRng2];
                    itemModels.RemoveAt(modelRng2);

                    Logger.LogFatal("itemmodels[modelRng2] is " + itemModels[modelRng2]);
                    itemModels.RemoveAt(modelRng2);
                }
                */
                /* if (itemIcons.Count > 0)
                {
                    itemDef.pickupIconSprite = itemIcons[iconRng2];
                    Logger.LogWarning("itemicons[iconRng2] is " + itemIcons[iconRng2]);
                    itemIcons.RemoveAt(iconRng2);
                } 
            } */
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

            int attempts = 0;

            string xmlSafeItemName = "E";

            while (attempts <= 5) {
                var prefixRng2 = prefixRng.Next(NameSystem.itemNamePrefix.Count);
                var nameRng2 = nameRng.Next(NameSystem.itemName.Count);
                itemName = "";

                itemName += NameSystem.itemNamePrefix[prefixRng2] + " ";
                // itemNamePrefix.RemoveAt(prefixRng2);

                itemName += NameSystem.itemName[nameRng2];
                // Main.itemName.RemoveAt(nameRng2);

                xmlSafeItemName = itemName.ToUpper();
                xmlSafeItemName = xmlSafeItemName.Replace(" ", "_").Replace("'", "").Replace("&", "AND");

                Effect buffer;
                if (map.TryGetValue("ITEM_" + xmlSafeItemName + "_NAME", out buffer)) {
                    attempts++;
                }
                else {
                    break;
                }
            }

            if (attempts > 5) {
                return;
            }

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
            float mult = 1f;
            Effect effect = new();
            switch (tier) {
                case ItemTier.Tier2:
                    mult = 1.5f;
                    break;
                case ItemTier.Tier3:
                    mult = 2f;
                    break;
                default:
                    mult = 1f;
                    break;
            }
            effect.Generate(rng, mult);

            int objects = rng.RangeInt(3, 9);
            PrimitiveType[] prims = {
                PrimitiveType.Sphere,
                PrimitiveType.Capsule,
                PrimitiveType.Cylinder,
                PrimitiveType.Cube,
            };
            Color[] colors = {
                Color.cyan,
                Color.grey,
                Color.yellow,
                Color.black,
                Color.Lerp(Color.gray, Color.cyan, 5),
                Color.Lerp(Color.red, Color.magenta, 2),
                Color.Lerp(Color.white, Color.black, 7),
            };

            GameObject first = GameObject.CreatePrimitive(prims[rng.RangeInt(0, prims.Length)]);
            first.GetComponent<MeshRenderer>().material.color = colors[rng.RangeInt(0, colors.Length)];
            for (int i = 0; i < objects; i++) {
                GameObject prim = GameObject.CreatePrimitive(prims[rng.RangeInt(0, prims.Length)]);
                prim.GetComponent<MeshRenderer>().material.color = colors[rng.RangeInt(0, colors.Length)];
                prim.transform.SetParent(first.transform);
                prim.transform.localPosition = new Vector3(rng.RangeFloat(-1, 1), rng.RangeFloat(-1, 1), rng.RangeFloat(-1, 1));
                prim.transform.localRotation = Quaternion.Euler(new Vector3(rng.RangeFloat(-360, 360), rng.RangeFloat(-360, 360), rng.RangeFloat(-360, 360)));
            }

            GameObject prefab = PrefabAPI.InstantiateClone(first, $"{xmlSafeItemName}-model");
            GameObject.DontDestroyOnLoad(prefab);

            Texture2D tex = new(512, 512);

            Color[] col = new Color[512 * 512];

            float y = 0.0F;

            float sx = rng.RangeFloat(0, 10000);
            float sy = rng.RangeFloat(0, 10000);

            float scale = rng.RangeFloat(1, 1);

            Color color = colors[rng.RangeInt(0, colors.Length)];
            Color tierCol;
            
            switch (tier) {
                case ItemTier.Tier1:
                    tierCol = Color.white;
                    break;
                case ItemTier.Tier2:
                    tierCol = Color.green;
                    break;
                case ItemTier.Tier3:
                    tierCol = Color.red;
                    break;
                case ItemTier.Lunar:
                    tierCol = Color.blue;
                    break;
                case ItemTier.VoidTier1:
                    tierCol = Color.magenta;
                    break;
                case ItemTier.VoidTier2:
                    tierCol = Color.magenta;
                    break;
                case ItemTier.VoidTier3:
                    tierCol = Color.magenta;
                    break;
                case ItemTier.VoidBoss:
                    tierCol = Color.magenta;
                    break;
                default:
                    tierCol = Color.black;
                    break;
            }

            while (y < tex.height)
            {
                float x = 0.0F;
                while (x < tex.width)
                {  
                    float xCoord = sx + x / tex.width * scale;
                    float yCoord = sy + y / tex.height * scale;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    col[(int)y * tex.width + (int)x] = Color.Lerp(color, tierCol, sample);
                    x++;
                }
                y++;
            }

            tex.SetPixels(col);
            tex.Apply();
            
            Sprite icon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            GameObject.DontDestroyOnLoad(tex);
            GameObject.DontDestroyOnLoad(icon);

            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "ITEM_" + xmlSafeItemName;
            itemDef.nameToken = "ITEM_" + xmlSafeItemName + "_NAME";
            itemDef.pickupToken = "ITEM_" + xmlSafeItemName + "_PICKUP";
            itemDef.descriptionToken = "ITEM_" + xmlSafeItemName + "_DESCRIPTION";
            itemDef.loreToken = "ITEM_" + xmlSafeItemName + "_LORE";
            itemDef.pickupModelPrefab = prefab;
            itemDef.pickupIconSprite = icon;
            itemDef.hidden = false;
            itemDef.deprecatedTier = tier;

            map.Add(itemDef.nameToken, effect);

            LanguageAPI.Add(itemDef.nameToken, itemName);
            LanguageAPI.Add(itemDef.pickupToken, effect.description);
            LanguageAPI.Add(itemDef.descriptionToken, effect.description);

            ItemAPI.Add(new CustomItem(itemDef, CreateItemDisplayRules()));
            myItemDefs.Add(itemDef);
            Logger.LogWarning("Generated a " + translatedTier + " item named " + itemName);

            //On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;

            // RoR2.UserProfile.defaultProfile.DiscoverPickup(itemDef.CreatePickupDef().pickupIndex);
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
            if (sender && sender.inventory && UnityEngine.Networking.NetworkServer.active)
            {
                foreach (ItemIndex index in sender.inventory.itemAcquisitionOrder) {
                    ItemDef def = ItemCatalog.GetItemDef(index);
                    Effect effect;

                    /*foreach (KeyValuePair<string, Effect> res in map) {
                        Debug.Log("Key: " + res.Key);
                        Debug.Log("Description: " + res.Value.description);
                        Debug.Log("=====================================");
                    }

                    Debug.Log("Current Item: " + def.nameToken); */

                    bool found = map.TryGetValue(def.nameToken, out effect);
                    if (found && effect.effectType == Effect.EffectType.Passive) {
                        Debug.Log("effect was found for " + def.nameToken + " : " + effect.description);
                        Debug.Log("conditions: " + effect.ConditionsMet(sender));
                        if (effect.ConditionsMet(sender)) {
                            effect.statEffect(args, sender.inventory.GetItemCount(def), sender);
                        }
                    }
                }
            }
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo info, GameObject victim) {
            orig(self, info, victim);
            if (UnityEngine.Networking.NetworkServer.active && info.attacker) {
                CharacterBody sender = info.attacker.GetComponent<CharacterBody>();

                if (sender && sender.inventory && UnityEngine.Networking.NetworkServer.active && info.damageColorIndex != DamageColorIndex.Item)
                {
                    foreach (ItemIndex index in sender.inventory.itemAcquisitionOrder) {
                        ItemDef def = ItemCatalog.GetItemDef(index);
                        Effect effect;

                        /*foreach (KeyValuePair<string, Effect> res in map) {
                            Debug.Log("Key: " + res.Key);
                            Debug.Log("Description: " + res.Value.description);
                            Debug.Log("=====================================");
                        }

                        Debug.Log("Current Item: " + def.nameToken); */

                        bool found = map.TryGetValue(def.nameToken, out effect);
                        if (found && effect.effectType == Effect.EffectType.OnHit) {
                            Debug.Log("effect was found for " + def.nameToken + " : " + effect.description);
                            Debug.Log("conditions: " + effect.ConditionsMet(sender));
                            if (effect.ConditionsMet(sender) && Util.CheckRoll(effect.chance, sender.master)) {
                                effect.onHitEffect(info);
                            }
                        }
                    }
            }
            }
        }
    }
}