using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using R2API;
using R2API.Utils;
using RoR2;
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

        public static ProcType HealingBonus = (ProcType)89; // hopefully no other mod uses a proc type of 89 because r2api doesnt have proctypeapi
        private static ulong seed;
        public static Xoroshiro128Plus rng;

        private static ItemDef itemDef;

        public static ConfigEntry<ulong> seedConfig { get; set; }

        private static List<GameObject> itemModels = new();
        private static List<Sprite> itemIcons = new();

        private static List<ItemDef> myItemDefs = new();
        private static Dictionary<string, Effect> map = new();

        // gonna make the statMult and tierMult configurable later down the line

        public void Awake()
        {
            RGILogger = Logger;
            RGIConfig = Config; // seedconfig does nothing right now because config.bind.value returns a bepinex.configentry<ulong> instead of a plain ulong???
            seedConfig = Config.Bind<ulong>("Configuration:", "Seed", 0, "The seed that will be used for random generation. This MUST be the same between all clients in multiplayer!!! A seed of 0 will generate a random seed instead");
            if (seedConfig.Value != 0)
            {
                seed = seedConfig.Value;
            }
            else
            {
                seed = (ulong)Random.RandomRangeInt(0, 10000) ^ (ulong)Random.RandomRangeInt(1, 10) << 16;
            }

            rng = new(seed);
            Logger.LogFatal("seed is " + seed);

            // int maxItems = itemNamePrefix.Count < itemName.Count ? itemNamePrefix.Count : itemName.Count;
            int maxItems = Config.Bind("Configuration:", "Maximum Items", 30, "The maximum amount of items the mod will generate.").Value;

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

            On.RoR2.GlobalEventManager.ServerDamageDealt += GlobalEventManager_ServerDamageDealt;

            On.RoR2.UI.MainMenu.BaseMainMenuScreen.Awake += (orig, self) =>
            {
                orig(self);
                foreach (ItemDef def in myItemDefs)
                {
                    UserProfile.defaultProfile.DiscoverPickup(def.CreatePickupDef().pickupIndex);
                }
            };

            On.RoR2.CharacterBody.Update += (orig, self) =>
            {
                orig(self);
                if (UnityEngine.Networking.NetworkServer.active)
                {
                    if (self.characterMotor && self.characterMotor.velocity.magnitude != -self.characterMotor.lastVelocity.magnitude)
                    {
                        if (self.isPlayerControlled)
                        {
                            self.RecalculateStats();
                            // :thonk:
                            // you know forcing recalcstats is terrible for the trout population
                        }
                    }
                }
            };

            On.RoR2.CharacterBody.OnSkillActivated += (orig, sender, slot) =>
            {
                orig(sender, slot);
                if (sender && sender.inventory && UnityEngine.Networking.NetworkServer.active)
                {
                    foreach (ItemIndex index in sender.inventory.itemAcquisitionOrder)
                    {
                        ItemDef def = ItemCatalog.GetItemDef(index);
                        Effect effect;

                        /*foreach (KeyValuePair<string, Effect> res in map) {
                            Debug.Log("Key: " + res.Key);
                            Debug.Log("Description: " + res.Value.description);
                            Debug.Log("=====================================");
                        }

                        Debug.Log("Current Item: " + def.nameToken); */

                        bool found = map.TryGetValue(def.nameToken, out effect);
                        if (found && effect.effectType == Effect.EffectType.OnSkillUse && Util.CheckRoll(effect.chance, sender.master))
                        {
                            if (effect.ConditionsMet(sender))
                            {
                                effect.onSkillUseEffect(sender, sender.inventory.GetItemCount(def));
                            }
                        }
                    }
                }
            };

            On.RoR2.HealthComponent.Heal += (orig, self, amount, mask, nonRegen) =>
            {
                CharacterBody sender = self.body;
                if (sender && sender.inventory && UnityEngine.Networking.NetworkServer.active && !mask.HasProc(HealingBonus))
                {
                    foreach (ItemIndex index in sender.inventory.itemAcquisitionOrder)
                    {
                        ItemDef def = ItemCatalog.GetItemDef(index);
                        Effect effect;

                        /*foreach (KeyValuePair<string, Effect> res in map) {
                            Debug.Log("Key: " + res.Key);
                            Debug.Log("Description: " + res.Value.description);
                            Debug.Log("=====================================");
                        }

                        Debug.Log("Current Item: " + def.nameToken); */

                        bool found = map.TryGetValue(def.nameToken, out effect);
                        if (found && effect.effectType == Effect.EffectType.OnHeal)
                        {
                            if (effect.ConditionsMet(sender))
                            {
                                effect.onHealEffect(self, sender.inventory.GetItemCount(def));
                            }
                        }
                    }
                }
                return orig(self, amount, mask, nonRegen);
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
            ItemTier tier;

            int attempts = 0;

            string xmlSafeItemName = "E";

            while (attempts <= 5)
            {
                var prefixRng2 = rng.RangeInt(0, NameSystem.itemNamePrefix.Count);
                var nameRng2 = rng.RangeInt(0, NameSystem.itemName.Count);
                itemName = "";

                itemName += NameSystem.itemNamePrefix[prefixRng2] + " ";
                // itemNamePrefix.RemoveAt(prefixRng2);

                itemName += NameSystem.itemName[nameRng2];
                // Main.itemName.RemoveAt(nameRng2);

                xmlSafeItemName = itemName.ToUpper();
                xmlSafeItemName = xmlSafeItemName.Replace(" ", "_").Replace("'", "").Replace("&", "AND");

                Effect buffer;
                if (map.TryGetValue("ITEM_" + xmlSafeItemName + "_NAME", out buffer))
                {
                    attempts++;
                }
                else
                {
                    break;
                }
            }

            if (attempts > 5)
            {
                return;
            }

            tier = (ItemTier)rng.RangeInt(0, 3);

            string translatedTier = "";
            float mult = 1f;
            float stackMult = 1f;
            Effect effect = new();

            int objects = rng.RangeInt(2, 9);
            PrimitiveType[] prims = {
                PrimitiveType.Sphere,
                PrimitiveType.Capsule,
                PrimitiveType.Cylinder,
                PrimitiveType.Cube,
            };
            Color32[] colors =
            {
                // top colors from paint.net with S turned down by 20 and V turned down by 30
                // black is 0, 0, 15 hsv            //
                // white is 0, 0, 85 hsv           //
                new(178, 35, 35, 255),            // red
                new(178, 92, 35, 255),           // orange
                new(178, 154, 35, 255),         // yellow
                new(35, 178, 52, 255),         // green
                new(35, 178, 178, 255),       // blue
                new(35, 57, 178, 255),       // dark blue
                new(73, 35, 178, 255),      // violet
                new(133, 35, 178, 255),    // magenta
                new(38, 38, 38, 255),     // black
                new(216, 216, 216, 255), // white
            };

            GameObject first = GameObject.CreatePrimitive(prims[rng.RangeInt(0, prims.Length)]);
            first.GetComponent<MeshRenderer>().material.color = colors[rng.RangeInt(0, colors.Length)];
            for (int i = 0; i < objects; i++)
            {
                GameObject prim = GameObject.CreatePrimitive(prims[rng.RangeInt(0, prims.Length)]);
                prim.GetComponent<MeshRenderer>().material.color = colors[rng.RangeInt(0, colors.Length)];
                prim.transform.SetParent(first.transform);
                prim.transform.localPosition = new Vector3(rng.RangeFloat(-1, 1), rng.RangeFloat(-1, 1), rng.RangeFloat(-1, 1));
                prim.transform.localRotation = Quaternion.Euler(new Vector3(rng.RangeFloat(-360, 360), rng.RangeFloat(-360, 360), rng.RangeFloat(-360, 360)));
            }

            GameObject prefab = PrefabAPI.InstantiateClone(first, $"{xmlSafeItemName}-model", false);
            DontDestroyOnLoad(prefab);

            Texture2D tex = new(512, 512);

            Color[] col = new Color[512 * 512];

            float y = 0.0F;

            float sx = rng.RangeFloat(0, 10000);
            float sy = rng.RangeFloat(0, 10000);

            float scale = rng.RangeFloat(1, 1);

            Color color = colors[rng.RangeInt(0, colors.Length)];
            Color tierCol;

            switch (tier)
            {
                case ItemTier.Tier1:
                    tierCol = Color.white;
                    translatedTier = "Common";
                    mult = 1f;
                    stackMult = 1f;
                    break;

                case ItemTier.Tier2:
                    tierCol = Color.green;
                    translatedTier = "Uncommon";
                    mult = 3f;
                    stackMult = 0.5f;
                    break;

                case ItemTier.Tier3:
                    tierCol = Color.red;
                    translatedTier = "Legendary";
                    mult = 12f;
                    stackMult = 0.15f;
                    break;

                case ItemTier.Lunar:
                    tierCol = Color.blue;
                    translatedTier = "Lunar";
                    mult = 1.8f;
                    stackMult = 0.5f;
                    break;

                case ItemTier.VoidTier1:
                    tierCol = Color.magenta;
                    translatedTier = "Void Common";
                    mult = 0.9f;
                    stackMult = 1f;
                    break;

                case ItemTier.VoidTier2:
                    tierCol = Color.magenta;
                    translatedTier = "Void Uncommon";
                    mult = 1.5f;
                    stackMult = 0.75f;
                    break;

                case ItemTier.VoidTier3:
                    tierCol = Color.magenta;
                    translatedTier = "Void Legendary";
                    mult = 2f;
                    stackMult = 0.45f;
                    break;

                case ItemTier.VoidBoss:
                    tierCol = Color.magenta;
                    translatedTier = "Void Yellow";
                    mult = 2f;
                    stackMult = 0.6f;
                    break;

                default:
                    tierCol = Color.black;
                    translatedTier = "BRO THIS SHOULDN'T BE HAPPENING";
                    mult = 1f;
                    stackMult = 1f;
                    break;
            }

            effect.Generate(rng, mult, stackMult);

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
            DontDestroyOnLoad(tex);
            DontDestroyOnLoad(icon);

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
            Logger.LogDebug("Generated a " + translatedTier + " item named " + itemName);

            //On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory && UnityEngine.Networking.NetworkServer.active)
            {
                foreach (ItemIndex index in sender.inventory.itemAcquisitionOrder)
                {
                    ItemDef def = ItemCatalog.GetItemDef(index);
                    Effect effect;

                    /*foreach (KeyValuePair<string, Effect> res in map) {
                        Debug.Log("Key: " + res.Key);
                        Debug.Log("Description: " + res.Value.description);
                        Debug.Log("=====================================");
                    }

                    Debug.Log("Current Item: " + def.nameToken); */

                    bool found = map.TryGetValue(def.nameToken, out effect);
                    if (found && effect.effectType == Effect.EffectType.Passive)
                    {
                        if (effect.ConditionsMet(sender))
                        {
                            effect.statEffect(args, sender.inventory.GetItemCount(def), sender);
                        }
                    }
                }
            }
        }

        private void GlobalEventManager_ServerDamageDealt(On.RoR2.GlobalEventManager.orig_ServerDamageDealt orig, DamageReport report)
        {
            orig(report);
            DamageInfo info = report.damageInfo;
            if (UnityEngine.Networking.NetworkServer.active && info.attacker)
            {
                CharacterBody sender = info.attacker.GetComponent<CharacterBody>();

                if (sender && sender.inventory && UnityEngine.Networking.NetworkServer.active && info.damageColorIndex != DamageColorIndex.Item)
                {
                    foreach (ItemIndex index in sender.inventory.itemAcquisitionOrder)
                    {
                        ItemDef def = ItemCatalog.GetItemDef(index);
                        Effect effect;

                        /*foreach (KeyValuePair<string, Effect> res in map) {
                            Debug.Log("Key: " + res.Key);
                            Debug.Log("Description: " + res.Value.description);
                            Debug.Log("=====================================");
                        }

                        Debug.Log("Current Item: " + def.nameToken); */

                        bool found = map.TryGetValue(def.nameToken, out effect);
                        if (found && effect.effectType == Effect.EffectType.OnHit)
                        {
                            if (effect.ConditionsMet(sender) && Util.CheckRoll(effect.chance, sender.master))
                            {
                                effect.onHitEffect(info, sender.inventory.GetItemCount(def));
                            }
                        }
                    }
                }

                GameObject victim = report.victimBody.gameObject;

                sender = victim.GetComponent<CharacterBody>();

                if (sender && sender.inventory && UnityEngine.Networking.NetworkServer.active && info.damageColorIndex != DamageColorIndex.Item)
                {
                    foreach (ItemIndex index in sender.inventory.itemAcquisitionOrder)
                    {
                        ItemDef def = ItemCatalog.GetItemDef(index);
                        Effect effect;

                        /*foreach (KeyValuePair<string, Effect> res in map) {
                            Debug.Log("Key: " + res.Key);
                            Debug.Log("Description: " + res.Value.description);
                            Debug.Log("=====================================");
                        } */

                        bool found = map.TryGetValue(def.nameToken, out effect);
                        if (found && effect.effectType == Effect.EffectType.OnHurt)
                        {
                            Debug.Log("Current Item: " + def.nameToken);
                            Debug.Log("Effect: " + effect.description);
                            Debug.Log("Conditions: " + effect.ConditionsMet(sender));
                            if (effect.ConditionsMet(sender))
                            {
                                effect.onHurtEffect(victim, sender.inventory.GetItemCount(def));
                            }
                        }
                    }
                }
            }
        }
    }
}