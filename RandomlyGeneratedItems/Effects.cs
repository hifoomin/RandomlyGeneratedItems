using UnityEngine;
using RoR2;
using R2API;
using System.Collections.Generic;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using static Facepunch.Steamworks.Inventory.Item;
using UnityEngine.EventSystems;

namespace RandomlyGeneratedItems
{
    public class Effect
    {
        #region VALUES

        public string description;
        public float num1;
        public float num2;
        public float stat1;
        public float stat2;
        public float stat3;
        public float chance;
        // public static float staticChance;

        #endregion VALUES

        public static Xoroshiro128Plus rng = Main.rng; // rng
        public bool conditions = false;

        #region CALLBACK_TYPES

        public delegate bool ConditionCallback(CharacterBody body);

        public delegate void StatEffectCallback(RecalculateStatsAPI.StatHookEventArgs args, int stacks, CharacterBody body);

        public delegate void OnEliteCallback(DamageInfo info);

        public delegate void OnHealCallback(HealthComponent healthComponent);

        public delegate void OnKillCallback(DamageInfo info);

        public delegate void OnTakeDamageCallback(DamageInfo info);

        public delegate void OnHitCallback(DamageInfo info);

        #endregion CALLBACK_TYPES

        #region CHOSEN_CALLBACKS

        public ConditionCallback condition;
        public StatEffectCallback statEffect;
        public OnHitCallback onHitEffect;

        #endregion CHOSEN_CALLBACKS

        #region PREFABS

        public GameObject missilePrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileProjectile.prefab").WaitForCompletion(), "RandomMissile");
        public GameObject potPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBoss/ClayPotProjectile.prefab").WaitForCompletion(), "RandomClayPot");
        public GameObject voidPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpVoidspikeProjectile.prefab").WaitForCompletion(), "RandomVoidSpike");
        public GameObject sawPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Saw/Sawmerang.prefab").WaitForCompletion(), "RandomSaw");
        public GameObject nadePrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoGrenadeProjectile.prefab").WaitForCompletion(), "RandomNade");
        public GameObject fireballPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LemurianBruiser/LemurianBigFireball.prefab").WaitForCompletion(), "RandomFireball");

        #endregion PREFABS

        #region CHOSEN_PREFABS

        public BuffDef buff;
        public BuffDef debuff;
        public GameObject chosenPrefab;
        public string projectileName;

        #endregion CHOSEN_PREFABS

        public EffectType effectType = EffectType.Passive;

        public enum EffectType
        {
            // OnKill,
            Passive,

            // OnElite,
            // OnHurt,
            OnHit,

            // OnHeal
        }

        #region CALLBACK_LISTS

        public List<ConditionCallback> conditionCallbackList;

        public List<OnKillCallback> onKillCallbackList;
        public List<OnEliteCallback> eliteCallbackList;
        public List<OnHealCallback> healCallbackList;
        public List<OnHitCallback> onHitCallbackList = new();

        public List<OnTakeDamageCallback> onHurtCallbackList;

        #endregion CALLBACK_LISTS

        #region DESC_DICTIONARIES

        public Dictionary<string, GameObject> projNameMap;

        public List<StatEffectCallback> statCallbackList;

        #endregion DESC_DICTIONARIES

        public void Generate(Xoroshiro128Plus rng, float tierMult, float stackMult)
        {
            conditions = rng.nextBool;
            num1 = Mathf.Ceil(rng.RangeFloat(5f, 15f) * tierMult);
            num2 = Mathf.Ceil(rng.RangeFloat(100f, 170f) * tierMult);
            stat1 = Mathf.Ceil(rng.RangeFloat(7f, 15f)) * tierMult;
            chance = Mathf.Ceil(rng.RangeFloat(4f, 13f) * tierMult);
            // staticChance = chance;

            // generate maps
            GenerateMapsAndCallbacks(stackMult);

            string[] prefabs =
            { // TODO: this is partially broken - the projectile name doesnt exist for some reason
                "<style=cIsDamage>Missile</style>",
                "<style=cIsVoid>Void Spike</style>",
                "<style=cIsDamage>Clay Pot</style>",
                "<style=cDeath>Sawblade</style>",
                "<style=cIsDamage>Grenade</style>",
                "<style=cIsDamage>Fireball</style>"
            };

            projectileName = prefabs[rng.RangeInt(0, 6)];
            projNameMap.TryGetValue(projectileName, out chosenPrefab);

            effectType = (EffectType)rng.RangeInt(0, 2);

            string condesc = "";
            if (conditions)
            {
                condition = conditionCallbackList[rng.RangeInt(0, conditionCallbackList.Count)];
                conditionmap.TryGetValue(condition, out condesc);
            }

            string statdesc = "";
            string onKillDesc = "";
            string onHealDesc = "";
            string onEliteDesc = "";
            string onHitDesc = "";
            string onHurtDesc = "";

            switch (effectType)
            {
                case EffectType.Passive:
                    statEffect = statCallbackList[rng.RangeInt(0, statCallbackList.Count)];
                    statmap.TryGetValue(statEffect, out statdesc);

                    description = condesc + statdesc;
                    break;

                case EffectType.OnHit:
                    onHitEffect = onHitCallbackList[rng.RangeInt(0, onHitCallbackList.Count)];
                    onhitmap.TryGetValue(onHitEffect, out onHitDesc);

                    description = condesc + onHitDesc;
                    break;

                default:
                    break;
            }
        }

        public bool ConditionsMet(CharacterBody body)
        {
            if (!conditions)
            {
                return true;
            }
            else
            {
                return condition(body);
            }
        }

        public Dictionary<ConditionCallback, string> conditionmap;
        public Dictionary<StatEffectCallback, string> statmap;

        public Dictionary<OnHitCallback, string> onhitmap;

        // condition callbacks

        public ConditionCallback shieldMore;
        public ConditionCallback ood;
        public ConditionCallback ooc;
        public ConditionCallback moving;
        public ConditionCallback notMoving;
        public ConditionCallback underHalfHp;
        public ConditionCallback atFullHp;

        // stateffect callbacks
        public StatEffectCallback attackSpeedBoost;

        public StatEffectCallback speedBoost;

        public StatEffectCallback healthBoost;
        public StatEffectCallback damageBoost;
        public StatEffectCallback shieldBoost;
        public StatEffectCallback armorBoost;
        public StatEffectCallback regenBoost;
        public StatEffectCallback critBoost;
        public StatEffectCallback secondaryCdrBoost;
        public StatEffectCallback utilityCdrBoost;
        public StatEffectCallback specialCdrBoost;
        public StatEffectCallback allSkillCdrBoost;

        // on hit callbacks
        public static OnHitCallback fireProjectile;

        public void GenerateMapsAndCallbacks(float stackMult)
        {
            // callbacks
            // condition callbacks

            shieldMore = (body) =>
            {
                return body.healthComponent.shield > 0;
            };

            ood = (body) =>
            {
                return body.outOfDanger;
            };

            ooc = (body) =>
            {
                return body.outOfCombat;
            };

            moving = (body) =>
            {
                return body.GetNotMoving() == false;
            };

            notMoving = (body) =>
            {
                return body.GetNotMoving() == true;
            };

            underHalfHp = (body) =>
            {
                return body.healthComponent.combinedHealthFraction <= 0.5f;
            };

            atFullHp = (body) =>
            {
                return body.healthComponent.combinedHealthFraction >= 1f;
            };

            // stateffect callbacks
            attackSpeedBoost = (args, stacks, body) =>
            {
                args.baseAttackSpeedAdd += ((stat1 * stackMult) * 0.01f);
            };

            speedBoost = (args, stacks, body) =>
            {
                args.moveSpeedMultAdd += ((stat1 * stackMult) * 0.01f);
            };

            healthBoost = (args, stacks, body) =>
            {
                args.healthMultAdd += ((stat1 * stackMult) * 0.01f);
            };

            damageBoost = (args, stacks, body) =>
            {
                args.damageMultAdd += ((stat1 * stackMult) * 0.01f);
            };

            shieldBoost = (args, stacks, body) =>
            {
                float amount = body.healthComponent.fullHealth * ((stat1 * stackMult) * 0.01f);
                args.baseShieldAdd += amount;
            };

            armorBoost = (args, stacks, body) =>
            {
                args.armorAdd += stat1 * stackMult * 0.8f * 0.01f;
            };

            regenBoost = (args, stacks, body) =>
            {
                args.regenMultAdd += stat1 * stackMult * 4 * 0.01f;
            };

            critBoost = (args, stacks, body) =>
            {
                args.critAdd += stat1 * stackMult;
            };

            secondaryCdrBoost = (args, stacks, body) =>
            {
                args.secondaryCooldownMultAdd += stat1 * 0.7f * stackMult * 0.01f;
            };

            utilityCdrBoost = (args, stacks, body) =>
            {
                args.utilityCooldownMultAdd += stat1 * 0.7f * stackMult * 0.01f;
            };

            specialCdrBoost = (args, stacks, body) =>
            {
                args.specialCooldownMultAdd += stat1 * 0.7f * stackMult * 0.01f;
            };

            allSkillCdrBoost = (args, stacks, body) =>
            {
                args.cooldownMultAdd += stat1 * 0.4f * stackMult * 0.01f;
            };

            // on hit callbacks
            OnHitCallback fireProjectile = (DamageInfo info) =>
            {
                FireProjectileInfo proj = new()
                {
                    damage = info.damage * num2 * 0.01f,
                    owner = info.attacker,
                    speedOverride = 100,
                    // medium speed
                    fuseOverride = 2,
                    rotation = Util.QuaternionSafeLookRotation(info.attacker.GetComponent<CharacterBody>().equipmentSlot.GetAimRay().direction),
                    position = info.attacker.GetComponent<CharacterBody>().corePosition + new Vector3(0, 1, 0),
                    damageColorIndex = DamageColorIndex.Item,
                    projectilePrefab = chosenPrefab
                };

                ProjectileManager.instance.FireProjectile(proj);
            };

            /// generate maps
            onhitmap = new()
            {
                {fireProjectile, $" gain a <style=cIsDamage>{chance}%</style> chance on hit to fire a {projectileName} for <style=cIsDamage>{num2}% base damage</style>."}
            };

            statmap = new()
            {
                {healthBoost, $" gain <style=cIsHealing>{stat1}%</style> <style=cStack>(+{stat1 * stackMult}% per stack)</style> <style=cIsHealing>maximum health</style>."},
                {attackSpeedBoost, $" increase <style=cIsDamage>attack speed</style> by <style=cIsDamage>{stat1}%</style> <style=cStack>(+{stat1 * stackMult}% per stack)</style>."},
                {speedBoost, $" gain <style=cIsUtility>{stat1}%</style> <style=cStack>(+{stat1 * stackMult}% per stack)</style> <style=cIsUtility>movement speed</style>."},
                {damageBoost, $" increase <style=cIsDamage>base damage</style> by <style=cIsDamage>{stat1}%</style> <style=cStack>(+{stat1 * stackMult}% per stack)</style>."},
                {shieldBoost, $" gain a <style=cIsHealing>shield</style> equal to <style=cIsHealing>{stat1}%</style> <style=cStack>(+{stat1 * stackMult}% per stack)</style> of your maximum health."},
                {armorBoost, $" gain <style=cIsHealing>{stat1 * 0.8f}</style> <style=cStack>(+{stat1 * 0.8f * stackMult} per stack)</style> <style=cIsHealing>armor</style>."},
                {regenBoost, $" increase <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>{stat1 * 4}%</style> <style=cStack>(+{stat1 * 4 * stackMult} per stack)." },
                {critBoost, $" gain <style=cIsDamage>{stat1}%</style> <style=cStack>(+{stat1 * stackMult} per stack)</style> <style=cIsDamage>critical chance</style>." },
                {secondaryCdrBoost, $" <style=cIsUtility>reduce secondary skill cooldown</style> by <style=cIsUtility>{stat1 * 0.7f * stackMult}%</style> <style=cStack>(+{stat1 * 0.7f * stackMult}% per stack)" },
                {utilityCdrBoost, $" <style=cIsUtility>reduce utility skill cooldown</style> by <style=cIsUtility>{stat1 * 0.7f * stackMult}%</style> <style=cStack>(+{stat1 * 0.7f * stackMult}% per stack)" },
                {specialCdrBoost, $" <style=cIsUtility>reduce special skill cooldown</style> by <style=cIsUtility>{stat1 * 0.7f * stackMult}%</style> <style=cStack>(+{stat1 * 0.7f * stackMult}% per stack)" },
                {allSkillCdrBoost, $" <style=cIsUtility>reduce skill cooldowns</style> by <style=cIsUtility>{stat1 * 0.4f * stackMult}%</style> <style=cStack>(+{stat1 * 0.4f * stackMult}% per stack)" }
            };

            conditionmap = new()
            {
                {shieldMore, $"While you have a <style=cIsHealing>shield</style>,"},
                {moving, "While <style=cIsUtility>moving</style>,"},
                {notMoving, "After standing still for <style=cIsHealing>1</style> second,"},
                {ooc, "While <style=cIsUtility>out of combat</style>,"},
                {ood, "While <style=cIsUtility>out of danger</style>,"},
                {underHalfHp, "While below <style=cIsHealth>50% health</style>,"},
                {atFullHp, "While at <style=cIsHealth>full health</style>,"}
            };

            statCallbackList = new()
            {
                speedBoost,
                healthBoost,
                attackSpeedBoost,
                damageBoost,
                shieldBoost,
                armorBoost,
                regenBoost
            };

            conditionCallbackList = new()
            {
                shieldMore,
                ooc,
                ood,
                moving,
                notMoving,
                underHalfHp,
                atFullHp
            };

            onHitCallbackList = new()
            {
                fireProjectile
            };

            projNameMap = new()
            {
                {"<style=cIsDamage>Missile</style>", missilePrefab},
                {"<style=cIsVoid>Void Spike</style>", voidPrefab},
                {"<style=cIsDamage>Clay Pot</style>", potPrefab},
                {"<style=cDeath>Sawblade</style>", sawPrefab},
                {"<style=cIsDamage>Grenade</style>", nadePrefab},
                {"<style=cIsDamage>Fireball</style>", fireballPrefab}
            };
        }
    }
}