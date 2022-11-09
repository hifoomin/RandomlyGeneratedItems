using System.Collections.Generic;

namespace RandomlyGeneratedItems
{
    public static class NameSystem
    {
        public static List<string> itemNamePrefix = new()
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
            "Nebby's", "MonsterSkinMan's", "Pseudopulse's",

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

        public static List<string> itemName = new()
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
    }
}