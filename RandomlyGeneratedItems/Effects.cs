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
        public float damage;
        public float statIncrease;
        public float healOrBarrier;
        public float speedOrBleed;
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

        public delegate void OnHitCallback(DamageInfo info, int count, GameObject victim);

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
            OnKill,
            Passive,

            OnElite,
            OnHurt,

            OnHit,
            OnSkillUse,

            OnHeal
        }

        #region CALLBACK_LISTS

        public List<ConditionCallback> conditionCallbackList;

        public List<OnKillCallback> onKillCallbackList;
        public List<OnEliteCallback> onEliteCallbackList;
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
        public Dictionary<OnKillCallback, string> onKillEffectMap;
        public List<StatEffectCallback> statCallbackList;

        #endregion DESC_DICTIONARIES

        public void Generate(Xoroshiro128Plus rng, float tierMult, float stackMult)
        {
            conditions = rng.nextBool;
            // num1 = Mathf.Ceil(rng.RangeFloat(5f, 15f) * tierMult);
            damage = Mathf.RoundToInt(rng.RangeFloat(100f, 170f) * Mathf.Sqrt(tierMult));
            statIncrease = Mathf.Round(rng.RangeFloat(7f, 15f)) * tierMult;
            chance = Mathf.RoundToInt(rng.RangeFloat(4f, 13f) * tierMult);
            healOrBarrier = Mathf.Round(rng.RangeFloat(1f, 5f) * tierMult);
            speedOrBleed = Mathf.Round(rng.RangeFloat(0.5f, 3f) * tierMult);

            if (conditions)
            {
                // num1 *= Mathf.Ceil(rng.RangeFloat(0.5f, 1f));
                damage += Mathf.RoundToInt(rng.RangeFloat(30f, 50f) * Mathf.Sqrt(tierMult));
                statIncrease += Mathf.Round(rng.RangeFloat(3, 6f) * Mathf.Sqrt(tierMult));
                chance += Mathf.RoundToInt(rng.RangeFloat(1f, 1.5f) * Mathf.Sqrt(tierMult));
                healOrBarrier += Mathf.Round(rng.RangeFloat(0.3f, 1f) * Mathf.Sqrt(tierMult));
                speedOrBleed += Mathf.Round(rng.RangeFloat(0.2f, 0.5f) * Mathf.Sqrt(tierMult));
            }

            if (chance >= 100f)
            {
                chance = 100;
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

            effectType = (EffectType)rng.RangeInt(0, 7);

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

                case EffectType.OnElite:
                    onEliteEffect = onEliteCallbackList[rng.RangeInt(0, onEliteCallbackList.Count)];
                    onEliteMap.TryGetValue(onEliteEffect, out onEliteDesc);

                    description = condesc + onEliteDesc;
                    break;

                case EffectType.OnKill:
                    onKillEffect = onKillCallbackList[rng.RangeInt(0, onKillCallbackList.Count)];
                    onKillEffectMap.TryGetValue(onKillEffect, out onKillDesc);

                    description = condesc + onKillDesc;
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
        public ConditionCallback midair;
        public ConditionCallback debuffed;
        public ConditionCallback firstXSeconds;
        public ConditionCallback tpEvent;

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

            midair = (body) =>
            {
                return body.characterMotor.lastGroundedTime >= Run.FixedTimeStamp.now + 0.2f;
            };
            debuffed = (body) =>
            {
                foreach (BuffIndex index in body.activeBuffsList)
                {
                    if (BuffCatalog.GetBuffDef(index).isDebuff) return true;
                }
                return false;
            };

            firstXSeconds = (body) =>
            {
                return Stage.instance ? Run.instance.fixedTime - Stage.instance.entryTime.t <= 180 : false;
            };

            tpEvent = (body) =>
            {
                if (TeleporterInteraction.instance) return TeleporterInteraction.instance.isCharging;
                return false;
            };

            // stateffect callbacks
            attackSpeedBoost = (args, stacks, body) =>
            {
                args.baseAttackSpeedAdd += ((statIncrease * (stackMult * stacks)) * 0.01f);
            };

            speedBoost = (args, stacks, body) =>
            {
                args.moveSpeedMultAdd += ((statIncrease * (stackMult * stacks)) * 0.01f);
            };

            healthBoost = (args, stacks, body) =>
            {
                args.healthMultAdd += ((statIncrease * (stackMult * stacks)) * 0.01f);
            };

            damageBoost = (args, stacks, body) =>
            {
                args.damageMultAdd += ((statIncrease * (stackMult * stacks)) * 0.01f);
            };

            shieldBoost = (args, stacks, body) =>
            {
                float amount = body.healthComponent.fullHealth * ((statIncrease * (stackMult * stacks)) * 0.01f);
                args.baseShieldAdd += amount;
            };

            armorBoost = (args, stacks, body) =>
            {
                args.armorAdd += statIncrease * (stackMult * stacks) * 0.8f * 0.01f;
            };

            regenBoost = (args, stacks, body) =>
            {
                args.regenMultAdd += statIncrease * (stackMult * stacks) * 11 * 0.01f;
            };

            critBoost = (args, stacks, body) =>
            {
                args.critAdd += statIncrease * (stackMult * stacks);
            };

            secondaryCdrBoost = (args, stacks, body) =>
            {
                args.secondaryCooldownMultAdd += statIncrease * 0.7f * (stackMult * stacks) * 0.01f;
            };

            utilityCdrBoost = (args, stacks, body) =>
            {
                args.utilityCooldownMultAdd += statIncrease * 0.7f * (stackMult * stacks) * 0.01f;
            };

            specialCdrBoost = (args, stacks, body) =>
            {
                args.specialCooldownMultAdd += statIncrease * 0.7f * (stackMult * stacks) * 0.01f;
            };

            allSkillCdrBoost = (args, stacks, body) =>
            {
                args.cooldownMultAdd += statIncrease * 0.4f * (stackMult * stacks) * 0.01f;
            };

            // on hit callbacks
            OnHitCallback fireProjectile = (DamageInfo info, int stacks, GameObject victim) =>
            {
                FireProjectileInfo proj = new()
                {
                    damage = info.damage * damage * (stackMult * stacks) * 0.01f,
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

            OnHitCallback applyBleed = (DamageInfo info, int stacks, GameObject victim) =>
            {
                InflictDotInfo dotInfo = new();
                dotInfo.dotIndex = DotController.DotIndex.Bleed;
                dotInfo.duration = speedOrBleed * (stacks * stackMult);
                dotInfo.totalDamage = info.damage;
                dotInfo.victimObject = victim;

                victim.GetComponent<CharacterBody>().AddTimedBuff(RoR2Content.Buffs.Bleeding, speedOrBleed * (stacks * stackMult));
                DotController.InflictDot(ref dotInfo);
            };

            // on hurt callbacks
            OnTakeDamageCallback retaliateProjectile = (GameObject victim, int stacks) =>
            {
                FireProjectileInfo proj = new()
                {
                    damage = victim.GetComponent<CharacterBody>().damage * damage * (stackMult * stacks) * 0.01f,
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

            OnTakeDamageCallback speedBonus = (GameObject victim, int stacks) =>
            {
                float duration = speedOrBleed * (1 + (stacks * stackMult));
                victim.GetComponent<CharacterBody>().AddTimedBuff(RoR2.RoR2Content.Buffs.CloakSpeed, duration);
            };

            // on skill use callbacks
            OnSkillUseCallback fireProjSkill = (CharacterBody body, int stacks) =>
            {
                FireProjectileInfo proj = new()
                {
                    damage = body.damage * (damage * (stackMult * stacks)) * 0.01f,
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

            // on elite kill callbacks
            OnEliteCallback barrierOnElite = (DamageInfo info, int stacks) =>
            {
                float increase = ((healOrBarrier * 3) * 0.01f) * (stacks * stackMult);
                HealthComponent com = info.attacker.GetComponent<HealthComponent>();
                com.AddBarrier(com.fullHealth * increase);
                com.body.AddTimedBuff(Buffs.NoDecay.buffDef, 10);
            };

            // on heal callbacks
            OnHealCallback barrier = (HealthComponent com, int stacks) =>
            {
                float increase = (healOrBarrier * 0.01f * 0.8f) * (stacks * stackMult);
                com.AddBarrier(com.fullHealth * increase);
            };

            OnHealCallback bonus = (HealthComponent com, int stacks) =>
            {
                float increase = (healOrBarrier * 0.01f * 0.1f) * (stacks * stackMult);
                ProcChainMask mask = new();
                mask.AddProc(Main.HealingBonus);
                com.Heal(com.fullHealth * increase, mask, true);
            };

            // on kill callbacks
            OnKillCallback projOnKill = (DamageInfo info, int stacks) =>
            {
                CharacterBody body = info.attacker.GetComponent<CharacterBody>();
                FireProjectileInfo proj = new()
                {
                    damage = body.damage * (damage * (stackMult * stacks)) * 0.01f * 2,
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

            OnKillCallback healOnkill = (DamageInfo info, int stacks) =>
            {
                CharacterBody body = info.attacker.GetComponent<CharacterBody>();
                HealthComponent com = body.healthComponent;
                float increase = (healOrBarrier * 0.01f * 3) * (stacks * stackMult * 3);
                ProcChainMask mask = new();
                mask.AddProc(Main.HealingBonus);
                com.Heal(com.fullHealth * increase, mask, true);
            };

            /// generate maps
            onhitmap = new()
            {
                {fireProjectile, $"Gain a <style=cIsDamage>{chance}%</style> chance on hit to fire a {projectileName} for <style=cIsDamage>{damage }%</style> <style=cStack>(+{damage * stackMult}% per stack)</style> <style=cIsDamage>base damage</style>."},
                {applyBleed, $"Gain a <style=cIsDamage>{chance}%</style> chance on hit to <style=cDeath>bleed</style> a target for <style=cIsUtility>{speedOrBleed} seconds</style>."}
            };

            onKillEffectMap = new()
            {
                {projOnKill, $"On kill, fire a {projectileName} for <style=cIsDamage>{damage*2f}%</style> <style=cStack>(+{damage*stackMult*2f}% per stack)</style> <style=cIsDamage>base damage</style>."},
                {healOnkill, $"Receive <style=cIsHealing>healing</style> equal to <style=cIsHealing>{healOrBarrier * 0.1f * 3}%</style> <style=cStack>(+{healOrBarrier * stackMult * 0.1f * 3}% per stack)</style> of your maximum <style=cIsHealing>health</style> upon <style=cIsHealing>killing an enemy</style>"}
            };

            onHealMap = new()
            {
                {barrier, $"Receive <style=cIsHealing>{healOrBarrier * 0.8f}%</style> <style=cStack>(+{healOrBarrier * stackMult * 0.8f}% per stack)</style> of your maximum health as <style=cIsDamage>barrier</style> upon being <style=cIsHealing>healed</style>."},
                {bonus, $"Receive <style=cIsHealing>bonus healing</style> equal to <style=cIsHealing>{healOrBarrier * 0.1f}%</style> <style=cStack>(+{healOrBarrier * stackMult * 0.1f}% per stack)</style> of your maximum <style=cIsHealing>health</style> upon being <style=cIsHealing>healed</style>"}
            };

            statmap = new()
            {
                {healthBoost, $"Gain <style=cIsHealing>{statIncrease}%</style> <style=cStack>(+{statIncrease * stackMult}% per stack)</style> <style=cIsHealing>maximum health</style>."},
                {attackSpeedBoost, $"Increase <style=cIsDamage>attack speed</style> by <style=cIsDamage>{statIncrease}%</style> <style=cStack>(+{statIncrease * stackMult}% per stack)</style>."},
                {speedBoost, $"Gain <style=cIsUtility>{statIncrease}%</style> <style=cStack>(+{statIncrease * stackMult}% per stack)</style> <style=cIsUtility>movement speed</style>."},
                {damageBoost, $"Increase <style=cIsDamage>base damage</style> by <style=cIsDamage>{statIncrease}%</style> <style=cStack>(+{statIncrease * stackMult}% per stack)</style>."},
                {shieldBoost, $"Gain a <style=cIsHealing>shield</style> equal to <style=cIsHealing>{statIncrease}%</style> <style=cStack>(+{statIncrease * stackMult}% per stack)</style> of your maximum health."},
                {armorBoost, $"Gain <style=cIsHealing>{statIncrease * 0.8f}</style> <style=cStack>(+{statIncrease * 0.8f * stackMult} per stack)</style> <style=cIsHealing>armor</style>."},
                {regenBoost, $"Increase <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>{statIncrease * 11}%</style> <style=cStack>(+{statIncrease * 11 * stackMult} per stack)." },
                {critBoost, $"Gain <style=cIsDamage>{statIncrease}%</style> <style=cStack>(+{statIncrease * stackMult} per stack)</style> <style=cIsDamage>critical chance</style>." },
                {secondaryCdrBoost, $"<style=cIsUtility>Reduce secondary skill cooldown</style> by <style=cIsUtility>{statIncrease * 0.7f * stackMult}%</style> <style=cStack>(+{statIncrease * 0.7f * stackMult}% per stack)" },
                {utilityCdrBoost, $"<style=cIsUtility>Reduce utility skill cooldown</style> by <style=cIsUtility>{statIncrease * 0.7f * stackMult}%</style> <style=cStack>(+{statIncrease * 0.7f * stackMult}% per stack)" },
                {specialCdrBoost, $"<style=cIsUtility>Reduce special skill cooldown</style> by <style=cIsUtility>{statIncrease * 0.7f * stackMult}%</style> <style=cStack>(+{statIncrease * 0.7f * stackMult}% per stack)" },
                {allSkillCdrBoost, $"<style=cIsUtility>Reduce skill cooldowns</style> by <style=cIsUtility>{statIncrease * 0.4f * stackMult}%</style> <style=cStack>(+{statIncrease * 0.4f * stackMult}% per stack)" }
            };

            onEliteMap = new()
            {
                {barrierOnElite, $"On killing an <style=cIsDamage>elite</style>, gain <style=cIsDamage>{healOrBarrier * 3}%</style> <style=cStack>(+{healOrBarrier * 3 * stackMult}% per stack)</style> of your maximum health as barrier. Remove <style=cIsUtility>the maximum barrier cap</style> for 10 seconds."}
            };

            conditionmap = new()
            {
                {shieldMore, $"While you have a <style=cIsHealing>shield</style>, "},
                {moving, "While <style=cIsUtility>moving</style>, "},
                {notMoving, "After standing still for <style=cIsHealing>1</style> second, "},
                {ooc, "While <style=cIsUtility>out of combat</style>, "},
                {ood, "While <style=cIsUtility>out of danger</style>, "},
                {underHalfHp, "While below <style=cIsHealth>50% health</style>, "},
                {atFullHp, "While at <style=cIsHealth>full health</style>, "},
                {midair, "While <style=cIsUtility>midair</style>, " },
                {debuffed, "While <style=cIsHealth>debuffed</style>, " },
                {firstXSeconds, "For the first <style=cIsUtility>3 minutes</style> every stage, " },
                {tpEvent, "During the <style=cIsUtility>Teleporter Event</style>, " }
            };

            onHurtMap = new()
            {
              {retaliateProjectile, $"Upon <style=cDeath>taking damage</style>, fire a {projectileName} for <style=cIsDamage>{damage}%</style> <style=cStack>(+{damage*stackMult}% per stack)</style> damage."},
              {speedBonus, $"Upon <style=cDeath>taking damage</style>, gain a <style=cIsUtility>speed boost</style> for <style=cIsUtility>{speedOrBleed}</style> <style=cStack>(+{speedOrBleed * stackMult} per stack)</style> seconds."}
            };

            onSkillUseMap = new()
            {
                {fireProjSkill, $"Gain a <style=cIsDamage>{chance}%</style> chance on skill use to fire a {projectileName} for <style=cIsDamage>{damage*0.5f}%</style> <style=cStack>(+{damage*stackMult*0.5f}% per stack)</style> <style=cIsDamage>base damage</style>."}
            };

            healCallbackList = new()
            {
                barrier,
                bonus
            };

            onKillCallbackList = new()
            {
                healOnkill,
                projOnKill
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

            onSkillUseCallbackList = new()
            {
                fireProjSkill,
            };

            conditionCallbackList = new()
            {
                shieldMore,
                ooc,
                ood,
                moving,
                notMoving,
                underHalfHp,
                atFullHp,
                midair,
                debuffed,
                firstXSeconds,
                tpEvent
            };

            onHitCallbackList = new()
            {
                fireProjectile,
                applyBleed
            };

            onHurtCallbackList = new()
            {
                retaliateProjectile,
                speedBonus
            };

            onEliteCallbackList = new()
            {
                barrierOnElite
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