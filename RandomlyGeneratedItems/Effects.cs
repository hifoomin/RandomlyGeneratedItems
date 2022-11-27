using UnityEngine;
using RoR2;
using R2API;
using System.Collections.Generic;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using System.Linq;

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

        public delegate void OnEliteCallback(DamageInfo info, int stacks);

        public delegate void OnHealCallback(HealthComponent healthComponent, int stacks);

        public delegate void OnKillCallback(DamageInfo info, int stacks);

        public delegate void OnTakeDamageCallback(GameObject victim, int stacks);

        public delegate void OnHitCallback(DamageInfo info, int count);

        public delegate void OnSkillUseCallback(CharacterBody body, int stacks);

        #endregion CALLBACK_TYPES

        #region CHOSEN_CALLBACKS

        public ConditionCallback condition;
        public StatEffectCallback statEffect;
        public OnHitCallback onHitEffect;
        public OnTakeDamageCallback onHurtEffect;
        public OnHealCallback onHealEffect;
        public OnKillCallback onKillEffect;
        public OnEliteCallback onEliteEffect;
        public OnSkillUseCallback onSkillUseEffect;

        #endregion CHOSEN_CALLBACKS

        #region PREFABS

        public GameObject missilePrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileProjectile.prefab").WaitForCompletion(), "RandomMissile");
        public static GameObject potPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBoss/ClayPotProjectile.prefab").WaitForCompletion(), "RandomClayPot");
        public GameObject voidPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpVoidspikeProjectile.prefab").WaitForCompletion(), "RandomVoidSpike");
        public static GameObject sawPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Saw/Sawmerang.prefab").WaitForCompletion(), "RandomSaw");
        public GameObject nadePrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoGrenadeProjectile.prefab").WaitForCompletion(), "RandomNade");
        public GameObject fireballPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LemurianBruiser/LemurianBigFireball.prefab").WaitForCompletion(), "RandomFireball");

        #endregion PREFABS

        public static void ModifyPrefabs()
        {
            sawPrefab.GetComponent<ProjectileDotZone>().resetFrequency = 0;
            potPrefab.GetComponent<ProjectileImpactExplosion>().blastRadius = 9f;
        }

        // ░░░░░▄▄▄▄▀▀▀▀▀▀▀▀▄▄▄▄▄▄░░░░░░░
        // ░░░░░█░░░░▒▒▒▒▒▒▒▒▒▒▒▒░░▀▀▄░░░░
        // ░░░░█░░░▒▒▒▒▒▒░░░░░░░░▒▒▒░░█░░░
        // ░░░█░░░░░░▄██▀▄▄░░░░░▄▄▄░░░░█░░
        // ░▄▀▒▄▄▄▒░█▀▀▀▀▄▄█░░░██▄▄█░░░░█░
        // █░▒█▒▄░▀▄▄▄▀░░░░░░░░█░░░▒▒▒▒▒░█
        // █░▒█░█▀▄▄░░░░░█▀░░░░▀▄░░▄▀▀▀▄▒█
        // ░█░▀▄░█▄░█▀▄▄░▀░▀▀░▄▄▀░░░░█░░█░
        // ░░█░░░▀▄▀█▄▄░█▀▀▀▄▄▄▄▀▀█▀██░█░░
        // ░░░█░░░░██░░▀█▄▄▄█▄▄█▄████░█░░░
        // ░░░░█░░░░▀▀▄░█░░░█░█▀██████░█░░
        // ░░░░░▀▄░░░░░▀▀▄▄▄█▄█▄█▄█▄▀░░█░░
        // ░░░░░░░▀▄▄░▒▒▒▒░░░░░░░░░░▒░░░█░
        // ░░░░░░░░░░▀▀▄▄░▒▒▒▒▒▒▒▒▒▒░░░░█░
        // ░░░░░░░░░░░░░░▀▄▄▄▄▄░░░░░░░░█░░

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
            OnHurt,

            OnHit,
            OnSkillUse,

            OnHeal
        }

        #region CALLBACK_LISTS

        public List<ConditionCallback> conditionCallbackList;

        public List<OnKillCallback> onKillCallbackList;
        public List<OnEliteCallback> eliteCallbackList;
        public List<OnHealCallback> healCallbackList;
        public List<OnHitCallback> onHitCallbackList;
        public List<OnSkillUseCallback> onSkillUseCallbackList;

        public List<OnTakeDamageCallback> onHurtCallbackList;

        #endregion CALLBACK_LISTS

        #region DESC_DICTIONARIES

        public Dictionary<string, GameObject> projNameMap;
        public Dictionary<OnTakeDamageCallback, string> onHurtMap;
        public Dictionary<OnEliteCallback, string> onEliteMap;
        public Dictionary<OnHealCallback, string> onHealMap;
        public Dictionary<OnSkillUseCallback, string> onSkillUseMap;

        public List<StatEffectCallback> statCallbackList;

        #endregion DESC_DICTIONARIES

        public void Generate(Xoroshiro128Plus rng, float tierMult, float stackMult)
        {
            conditions = rng.nextBool;
            num1 = Mathf.Ceil(rng.RangeFloat(5f, 15f) * tierMult);
            num2 = Mathf.Ceil(rng.RangeFloat(100f, 170f) * tierMult);
            stat1 = Mathf.Ceil(rng.RangeFloat(7f, 15f)) * tierMult;
            chance = Mathf.Ceil(rng.RangeFloat(4f, 13f) * tierMult);
            stat2 = Mathf.Ceil(rng.RangeFloat(1f, 5f) * tierMult);

            if (conditions)
            {
                num1 *= Mathf.Ceil(rng.RangeFloat(0.5f, 1f));
                num2 *= Mathf.Ceil(rng.RangeFloat(0.5f, 1f));
                stat1 *= Mathf.Ceil(rng.RangeFloat(0.5f, 1f));
                chance *= Mathf.Ceil(rng.RangeFloat(0.2f, 0.5f));
            }

            // buff = BuffCatalog.buffDefs[rng.RangeInt(0, BuffCatalog.buffDefs.Length)];

            string[] prefabs =
            {
                "<style=cIsDamage>Missile</style>",
                "<style=cIsVoid>Void Spike</style>",
                "<style=cIsDamage>Clay Pot</style>",
                "<style=cDeath>Sawblade</style>",
                "<style=cIsDamage>Grenade</style>",
                "<style=cIsDamage>Fireball</style>"
            };

            projectileName = prefabs[rng.RangeInt(0, prefabs.Length)];

            // generate maps and callbacks after choosing projname or wont work
            GenerateMapsAndCallbacks(stackMult);

            projNameMap.TryGetValue(projectileName, out chosenPrefab);

            effectType = (EffectType)rng.RangeInt(0, 5);

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
            string onSkillUseDesc = " ";

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

                case EffectType.OnHurt:
                    onHurtEffect = onHurtCallbackList[rng.RangeInt(0, onHurtCallbackList.Count)];
                    onHurtMap.TryGetValue(onHurtEffect, out onHurtDesc);

                    description = condesc + onHurtDesc;
                    break;

                case EffectType.OnSkillUse:
                    onSkillUseEffect = onSkillUseCallbackList[rng.RangeInt(0, onSkillUseCallbackList.Count)];
                    onSkillUseMap.TryGetValue(onSkillUseEffect, out onSkillUseDesc);

                    description = condesc + onSkillUseDesc;
                    break;

                case EffectType.OnHeal:
                    onHealEffect = healCallbackList[rng.RangeInt(0, healCallbackList.Count)];
                    onHealMap.TryGetValue(onHealEffect, out onHealDesc);

                    description = condesc + onHealDesc;
                    break;

                default:
                    break;
            }

            if (!conditions)
            {
                description.Remove(0);
                description.Replace(description.ElementAt(0), description.ElementAt(0).ToString().ToUpper()[0]);
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

        public OnSkillUseCallback fireProjSkill;
        public OnTakeDamageCallback retaliateProjectile;

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
                args.baseAttackSpeedAdd += ((stat1 * (stackMult * stacks)) * 0.01f);
            };

            speedBoost = (args, stacks, body) =>
            {
                args.moveSpeedMultAdd += ((stat1 * (stackMult * stacks)) * 0.01f);
            };

            healthBoost = (args, stacks, body) =>
            {
                args.healthMultAdd += ((stat1 * (stackMult * stacks)) * 0.01f);
            };

            damageBoost = (args, stacks, body) =>
            {
                args.damageMultAdd += ((stat1 * (stackMult * stacks)) * 0.01f);
            };

            shieldBoost = (args, stacks, body) =>
            {
                float amount = body.healthComponent.fullHealth * ((stat1 * (stackMult * stacks)) * 0.01f);
                args.baseShieldAdd += amount;
            };

            armorBoost = (args, stacks, body) =>
            {
                args.armorAdd += stat1 * (stackMult * stacks) * 0.8f * 0.01f;
            };

            regenBoost = (args, stacks, body) =>
            {
                args.regenMultAdd += stat1 * (stackMult * stacks) * 4 * 0.01f;
            };

            critBoost = (args, stacks, body) =>
            {
                args.critAdd += stat1 * (stackMult * stacks);
            };

            secondaryCdrBoost = (args, stacks, body) =>
            {
                args.secondaryCooldownMultAdd += stat1 * 0.7f * (stackMult * stacks) * 0.01f;
            };

            utilityCdrBoost = (args, stacks, body) =>
            {
                args.utilityCooldownMultAdd += stat1 * 0.7f * (stackMult * stacks) * 0.01f;
            };

            specialCdrBoost = (args, stacks, body) =>
            {
                args.specialCooldownMultAdd += stat1 * 0.7f * (stackMult * stacks) * 0.01f;
            };

            allSkillCdrBoost = (args, stacks, body) =>
            {
                args.cooldownMultAdd += stat1 * 0.4f * (stackMult * stacks) * 0.01f;
            };

            // on hit callbacks
            OnHitCallback fireProjectile = (DamageInfo info, int stacks) =>
            {
                FireProjectileInfo proj = new()
                {
                    damage = info.damage * num2 * (stackMult * stacks) * 0.01f,
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

            // on hurt callbacks
            OnTakeDamageCallback retaliateProjectile = (GameObject victim, int stacks) =>
            {
                FireProjectileInfo proj = new()
                {
                    damage = victim.GetComponent<CharacterBody>().damage * num2 * (stackMult * stacks) * 0.01f,
                    owner = victim,
                    speedOverride = 100,
                    // medium speed
                    fuseOverride = 2,
                    rotation = Util.QuaternionSafeLookRotation(victim.GetComponent<CharacterBody>().equipmentSlot.GetAimRay().direction),
                    position = victim.GetComponent<CharacterBody>().corePosition + new Vector3(0, 1, 0),
                    damageColorIndex = DamageColorIndex.Item,
                    projectilePrefab = chosenPrefab
                };

                ProjectileManager.instance.FireProjectile(proj);
            };

            // on skill use callbacks
            OnSkillUseCallback fireProjSkill = (CharacterBody body, int stacks) =>
            {
                FireProjectileInfo proj = new()
                {
                    damage = body.damage * (num2 * (stackMult * stacks)) * 0.01f,
                    owner = body.gameObject,
                    speedOverride = 100,
                    // medium speed
                    fuseOverride = 2,
                    rotation = Util.QuaternionSafeLookRotation(body.equipmentSlot.GetAimRay().direction),
                    position = body.corePosition + new Vector3(0, 1, 0),
                    damageColorIndex = DamageColorIndex.Item,
                    projectilePrefab = chosenPrefab
                };

                ProjectileManager.instance.FireProjectile(proj);
            };

            // on heal callbacks
            OnHealCallback barrier = (HealthComponent com, int stacks) =>
            {
                float increase = (stat2 * 0.01f * 0.8f) * (stacks * stackMult);
                com.AddBarrier(com.fullHealth * increase);
            };

            OnHealCallback bonus = (HealthComponent com, int stacks) =>
            {
                float increase = (stat2 * 0.01f * 0.1f) * (stacks * stackMult);
                ProcChainMask mask = new();
                mask.AddProc(Main.HealingBonus);
                com.Heal(com.fullHealth * increase, mask, true);
            };

            /// generate maps
            onhitmap = new()
            {
                {fireProjectile, $"Gain a <style=cIsDamage>{chance}%</style> chance on hit to fire a {projectileName} for <style=cIsDamage>{num2}%</style> <style=cStack>(+{num2 * stackMult}% per stack)</style> <style=cIsDamage>base damage</style>."}
            };

            onHealMap = new() {
                {barrier, $"Receive <style=cIsHealing>{stat2 * 0.8f}%</style> <style=cStack>(+{stat2 * stackMult * 0.8f}% per stack)</style> of your maximum health as <style=cIsDamage>barrier</style> upon being <style=cIsHealing>healed</style>."},
                {bonus, $"Receive <style=cIsHealing>bonus healing</style> equal to <style=cIsHealing>{stat2 * 0.1f}%</style> <style=cStack>(+{stat2 * stackMult * 0.1f}% per stack)</style> of your maximum <style=cIsHealing>health</style> upon being <style=cIsHealing>healed</style>"}
            };

            statmap = new()
            {
                {healthBoost, $"Gain <style=cIsHealing>{stat1}%</style> <style=cStack>(+{stat1 * stackMult}% per stack)</style> <style=cIsHealing>maximum health</style>."},
                {attackSpeedBoost, $"Increase <style=cIsDamage>attack speed</style> by <style=cIsDamage>{stat1}%</style> <style=cStack>(+{stat1 * stackMult}% per stack)</style>."},
                {speedBoost, $"Gain <style=cIsUtility>{stat1}%</style> <style=cStack>(+{stat1 * stackMult}% per stack)</style> <style=cIsUtility>movement speed</style>."},
                {damageBoost, $"Increase <style=cIsDamage>base damage</style> by <style=cIsDamage>{stat1}%</style> <style=cStack>(+{stat1 * stackMult}% per stack)</style>."},
                {shieldBoost, $"Gain a <style=cIsHealing>shield</style> equal to <style=cIsHealing>{stat1}%</style> <style=cStack>(+{stat1 * stackMult}% per stack)</style> of your maximum health."},
                {armorBoost, $"Gain <style=cIsHealing>{stat1 * 0.8f}</style> <style=cStack>(+{stat1 * 0.8f * stackMult} per stack)</style> <style=cIsHealing>armor</style>."},
                {regenBoost, $"Increase <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>{stat1 * 4}%</style> <style=cStack>(+{stat1 * 4 * stackMult} per stack)." },
                {critBoost, $"Gain <style=cIsDamage>{stat1}%</style> <style=cStack>(+{stat1 * stackMult} per stack)</style> <style=cIsDamage>critical chance</style>." },
                {secondaryCdrBoost, $"<style=cIsUtility>Reduce secondary skill cooldown</style> by <style=cIsUtility>{stat1 * 0.7f * stackMult}%</style> <style=cStack>(+{stat1 * 0.7f * stackMult}% per stack)" },
                {utilityCdrBoost, $"<style=cIsUtility>Reduce utility skill cooldown</style> by <style=cIsUtility>{stat1 * 0.7f * stackMult}%</style> <style=cStack>(+{stat1 * 0.7f * stackMult}% per stack)" },
                {specialCdrBoost, $"<style=cIsUtility>Reduce special skill cooldown</style> by <style=cIsUtility>{stat1 * 0.7f * stackMult}%</style> <style=cStack>(+{stat1 * 0.7f * stackMult}% per stack)" },
                {allSkillCdrBoost, $"<style=cIsUtility>Reduce skill cooldowns</style> by <style=cIsUtility>{stat1 * 0.4f * stackMult}%</style> <style=cStack>(+{stat1 * 0.4f * stackMult}% per stack)" }
            };

            conditionmap = new()
            {
                {shieldMore, $"While you have a <style=cIsHealing>shield</style>, "},
                {moving, "While <style=cIsUtility>moving</style>, "},
                {notMoving, "After standing still for <style=cIsHealing>1</style> second, "},
                {ooc, "While <style=cIsUtility>out of combat</style>, "},
                {ood, "While <style=cIsUtility>out of danger</style>, "},
                {underHalfHp, "While below <style=cIsHealth>50% health</style>, "},
                {atFullHp, "While at <style=cIsHealth>full health</style>, "}
            };

            onHurtMap = new() {
              {retaliateProjectile, $"Upon <style=cDeath>taking damage</style>, fire a {projectileName} for <style=cIsDamage>{num2}%</style> <style=cStack>(+{num2*stackMult}% per stack)</style> damage."}
            };

            onSkillUseMap = new()
            {
                {fireProjSkill, $"Gain a <style=cIsDamage>{chance}%</style> chance on skill use to fire a {projectileName} for <style=cIsDamage>{num2*0.5f}%</style> <style=cStack>(+{num2*stackMult*0.5f}% per stack)</style> <style=cIsDamage>base damage</style>."}
            };

            healCallbackList = new() {
                barrier,
                bonus
            };

            statCallbackList = new()
            {
                speedBoost,
                healthBoost,
                attackSpeedBoost,
                damageBoost,
                shieldBoost,
                armorBoost,
                regenBoost,
                critBoost,
                secondaryCdrBoost,
                utilityCdrBoost,
                specialCdrBoost,
                allSkillCdrBoost
            };

            onSkillUseCallbackList = new() {
                fireProjSkill
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

            onHurtCallbackList = new() {
                retaliateProjectile
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