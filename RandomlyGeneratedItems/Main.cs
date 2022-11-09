using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Mono.Cecil;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RandomlyGeneratedItems
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(LanguageAPI))]
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

        private static System.Random rRng = new();
        private static System.Random gRng = new();
        private static System.Random bRng = new();
        private static System.Random aRng = new();
        // yeah i know im using system.random, placeholder for now
        // still dont know how im gonna network this, maybe not ever lol

        public static ConfigEntry<ulong> seedConfig { get; set; }

        private static List<string> itemNamePrefix = new()
        {
            "Armor-Piercing", "Backup", "Bison", "Bundle of", "Bustling", "Cautious", "Delicate", "Energy", "Focus", "Lens-Maker's",
            "Monster", "Oddly-shaped", "Paul's Goat", "Personal", "Power", "Repulsion", "Roll of", "Rusted", "Soldier's", "Sticky",
            "Stun", "Topaz", "Tougher", "Tri-Tip", "AtG", "Berzerker's", "Death", "Fuel", "Ghor's", "Harvester's", "Hopoo", "Hunter's",
            "Ignition", "Kjaro's", "Leeching", "Lepton", "Old", "Old War", "Predatory", "Red", "Regenerating", "Rose", "Runald's",
            "Shipping Request", "Squid", "War", "Wax", "Will of the", "57 Leaf", "Alien", "Ben's", "Bottled", "Brilliant", "Ceremonial",
            "Defensive", "Frost", "Happiest", "Hardlight", "Interstellar", "Laser", "N'kuhana's", "Pocket", "Rejuvenation", "Resonance",
            "Sentient", "Shattering", "Soulbound", "Spare", "Symbiotic", "Unstable", "Wake of", "Charged", "Defense", "Empathy", "Genesis",
            "Halcyon", "Irradiant", "Little", "Mired", "Molten", "Queen's", "Titanic", "Beads of", "Brittle", "Defiant", "Essence of", "Eulogy",
            "Focused", "Gesture of the", "Hooks of", "Light Flux", "Mercurial", "Shaped", "Stone Flux", "Strides of", "Visions of", "Benthic",
            "Encrusted", "Lost Seer's", "Lysate", "Newly Hatched", "Plasma", "Pluripotent", "Safer", "Singularity", "Tenta", "Voidsent", "Weeping",
            "Blast", "Disposable", "Eccentric", "Executive", "Foreign", "Fuel", "Gnarled", "Gorag's", "Jade", "Milky", "Primordial", "Remote", "Royal",
            "Super Massive", "Crowdfunding", "Tropy Hunter's", "Volcanic", "Glowing", "Helfire", "Spinel", "Her", "His", "Ifrit's", "Suspicious",

            "The", "The", "The", "The", "The", "The",

            "Artificial", "Liquated", "Crystallized", "Wild", "Poison", "Primitive", "Stealthy", "Blazing", "Nacreous", "Invisible",

            "HIFU's", "RandomlyAwesome's","Harb's", "iDeath's", "Twiner's", "Jace's", "Groove's", "Noop's", "Dotflare's", "Atlantean", "Gav's", "Mystic's",
            "Nebby's",

            "Scarlet", "Crazy", "Erised", "Froggin'", "Pale", "Black", "Heavy", "Rainbow", "Stranger", "Blood", "Malleable", "Abandoned", "Antler's",
            "Another", "Orange", "Mint", "Snake", "Closer", "Velvet", "Scorpio", "Gold", "Concealing", "Impossible", "Luminary", "Dead", "Ordinary",
            "Little", "Deep", "Divisionary", "Autonomous", "Glass", "Electric", "Hollow", "Cardinal", "Arch", "False", "Makeshift", "Non Human",
            "Outer", "Serial", "Ultrabeat", "Disconnected", "Ephemeral", "Lost", "Burning", "Toxic", "Dying", "Neo", "Doom", "Argent", "Translucent",
            "Soul", "Opaque", "Broken", "Peripheral", "Unprocessed", "Within", "Blue", "Motionless", "White", "Veil of",

            "Snecko", "Ironclad", "Silent", "Defective",
            "Perfected", "Reckless", "First", "Second", "Ghostly", "Infernal", "Flying", "Poisoned", "Quick", "Sneaky", "All-Out", "Endless", "Masterful",
            "Grand", "Piercing", "Bouncing", "Calculated", "Crippling", "Phantasmal", "Infinite", "Noxious", "Well-Laid", "Sweeping", "Genetic", "Reinforced",
            "Amplified", "Defragmented", "Biased", "Based", "Empty",

            "Flurry of", "Barrage of", "Volley of", "Salvo of", "Gate of", "Book of", "Aura of", "Barrier of", "Agony of", "Revival of", "Retrospect of",

            "Annoying", "Awful", "Bloody", "Clean", "Combative", "Courageous", "Cute", "Adorable", "Dead", "Distinct", "Green", "Yellow", "Pink", "Monochromatic",
            "Charming", "Dangerous", "Drab", "Alive", "Bright", "Dark", "Evil", "Enchanting", "Fragile", "Jittery", "Mysterious", "Modern", "Perfect",
            "Plain", "Real", "Fake", "Tender", "Unusual", "Imaginary",

            "Femboy" // for the funny
        };

        private static List<string> itemName = new()
        {
            "Insomnia", "Letter", "Experiment", "Jetpack", "Light", "Materials", "Buttersnips", "Feelings", "Zyglrox", "Racecar", "Passenger", "Groove",
            "Captain", "Eureka", "Muramasa", "Blast", "Facepalm", "Mute", "Ji", "Luck", "Ragnarok", "Diamond", "Bullfish", "Masamune", "Overture", "Ground",
            "Zero", "Parade", "Aura", "Minute", "Ultra", "Heart", "Scourge", "Alpha", "Gravity", "Hell", "Omega", "Price", "Motormouth", "Marigold", "Priestess",
            "Flatline", "Fire", "Prayer", "Lune", "Reptile", "Eagle", "Burner", "Ghost", "Glow", "Covenant", "Haven", "Ghilan", "Millenium", "Division", "Meridian",
            "Prototype", "Void", "Ruins", "Fear", "Waters", "Avatar", "Decay", "Spine", "Sky", "Movements", "Echoes", "Deadrose", "Rain", "Longing", "Grove",
            "Oil", "Scorpio", "Ocean", "Portrait", "Gold", "Fate", "Proxy", "Retrospect", "Nocturne", "Eclipse", "Singularity", "Embers", "Dystopia", "Hexes",
            "Utopia", "Messenger", "Cages", "King", "Orbital", "Mirror Image", "Arrow", "Matter", "Energy", "Blood", "Blood", "Extinction", "Impermanence",
            "Wonder", "Libertine", "Goliath", "Demi God", "Meteor", "Fake", "Skyline", "Snowblood", "Gungrave", "Shadow", "House", "Twilight", "Hymn", "Canvas",
            "Eidolon", "Remnant", "Fiction", "I", "Animus", "Deadnest", "Lavos", "Opiate", "Essence", "Somnus", "Providence", "Harmony", "Cimmerian", "Vein",
            "Club", "Molecule", "Space", "Follower", "Madness", "Face", "Imago", "Brahmastra", "Killer", "Hole", "Wave", "Lotus", "Nightmare", "Scars", "Revenge",
            "World", "Illusions", "Radiance", "Sequence", "Tragedy", "Convulsions", "Gasoline", "Body Bag", "Love", "Parasite", "Slayer", "Rip", "Gate", "Harbinger",
            "Headache", "Flight", "Mikasa", "Sandalphon", "Venom", "Horizon", "Tendinitis", "Blood & Water", "Surrender", "Everything", "Elevation", "Periphery",
            "Tesseract", "Augment", "Stasis", "Variations", "Afterglow", "Destruction", "Termina", "Ruin",

            "Glasses", "Clock", "Fork", "Bracelet", "Socks", "Lamp", "Remote", "Bread", "Credit Card", "Book", "Necronomicon", "Shawl", "Candle",

            "Knife","Cannon", "Mortar", "Machine Gun", "Bola", "Boomerang", "Bow", "Crossbow", "Longbow", "Sling", "Spear", "Flamethrower", "Bayonet", "Halberd", "Lance",
            "Pike", "Quarterstaff", "Sabre", "Sword", "Tomahawk", "Grenade", "Mine", "Shrapnel", "Depth Charge", "C4", "Torpedo", "Trident Missile", "Peacekeeper Missile",
            "Bazooka", "Blowgun", "Blunderbuss", "Carbine", "Gatling gun", "Handgun", "Pistol", "Revolver", "Derringer", "Arquebus", "Musket", "Rifle",
            "Shotgun", "Luger", "Repeater", "Submachine gun", "Shrapnel",

            "Hammer", "Screwdriver", "Mallet", "Axe", "Saw", "Scissors", "Chisel", "Pliers", "Drill", "Iron", "Chainsaw", "Scraper", "Wire", "Nail", "Shovel", "Callipers",
            "Scalpel", "Gloves", "Needle",

            "Brain", "Lungs", "Liver", "Bladder", "Kidney", "Heart", "Stomach", "Eye",

            "Animal", "Balloon", "Battery", "Camera", "Disease", "Drug", "Guitar", "Ice", "Iron", "Quill", "Spoon", "Pen", "Box", "Brush", "Stockings", "Card",

            "Strike", "Bash", "Anger", "Clash", "Cleave", "Wave", "Bludgeon", "Carnage", "Rampage", "Armament", "Grit", "Warcry", "Battle Trance", "Pact", "Barrier",
            "Armor", "Blade", "Rage", "Wind", "Sentinel", "Weakness", "Offering", "Embrace", "Rupture", "Barricade", "Berserk", "Brutality", "Corruption",
            "Bane", "Flechettes", "Skewer", "Grand Finale", "Cloak", "Poison", "Wail", "Flask", "Gamble", "Catalyst", "Cloud", "Distraction", "Plan", "Terror", "Reflex",
            "Caltrops", "Footwork", "Fumes",
            "Lightning", "Barrage", "Claw", "Snap", "Driver", "Rebound", "Streamline", "Beam", "Blizzard", "Bullseye", "Melter", "Sunder", "Surge", "Hyperbeam",
            "Hologram", "Recursion",

            "Load" // for the funny
        };

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

            int maxItems = itemNamePrefix.Count < itemName.Count ? itemNamePrefix.Count : itemName.Count;

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
                    clone.buffColor = new Color32((byte)rRng.Next(0, 255), (byte)gRng.Next(0, 255), (byte)bRng.Next(0, 255), (byte)aRng.Next(150, 255));
                    itemIcons.Add(clone.iconSprite);
                }
            }

            foreach (EquipmentDef def in ContentManager._equipmentDefs)
            {
                if (def.pickupModelPrefab) itemModels.Add(def.pickupModelPrefab);
                if (def.pickupIconSprite) itemIcons.Add(def.pickupIconSprite);
            }

            Logger.LogFatal("modelList has " + itemModels.Count + " elements");
            Logger.LogFatal("iconList has " + itemIcons.Count + " elements");
            var modelRng2 = modelRng.Next(itemModels.Count);
            var iconRng2 = iconRng.Next(itemIcons.Count);

            foreach (ItemDef itemDef in myItemDefs)
            {
                // itemDef.pickupModelPrefab = itemModels[modelRng2];
                // itemModels.RemoveAt(modelRng2);
                // not actually sure why it NREs,
                // also not enough elements to have unique models for everything
                Logger.LogFatal("itemdef is " + itemDef);
                if (itemIcons.Count > 0)
                {
                    // itemDef.pickupIconSprite = itemIcons[iconRng2];
                    // NRE HERE SOMEHOW?? AT ITEMDEF???????????
                    /*
                        IL_023D: ldloc.s   itemDef <<< HERE???
                        IL_023F: ldsfld    class [netstandard] System.Collections.Generic.List`1<class [UnityEngine.CoreModule] UnityEngine.Sprite> RandomlyGeneratedItems.Main::itemIcons
                        IL_0244: ldloc.1
                        IL_0245: callvirt instance !0 class [netstandard] System.Collections.Generic.List`1<class [UnityEngine.CoreModule]
                    */
                    // itemIcons.RemoveAt(iconRng2);
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

            var prefixRng2 = prefixRng.Next(itemNamePrefix.Count);
            var nameRng2 = nameRng.Next(Main.itemName.Count);

            itemName += itemNamePrefix[prefixRng2] + " ";
            itemNamePrefix.RemoveAt(prefixRng2);

            itemName += Main.itemName[nameRng2];
            Main.itemName.RemoveAt(nameRng2);

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
            if (token == "ITEM_" + itemName + "_PICKUP")
            {
            }
            if (token == "ITEM_" + itemName + "_DESCRIPTION")
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