using System;
using UnityEngine;
using RoR2;
using R2API;
using Unity;
using System.Collections.Generic;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;

namespace RandomlyGeneratedItems {
    public class Effect {
        #region VALUES
        public string description;
        public float num1;
        public float num2;
        public float stat1;
        public float stat2;
        public float stat3;
        public float chance;
        // public static float staticChance;
        
        #endregion
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

        
        #endregion
        
        #region CHOSEN_CALLBACKs
        public ConditionCallback condition;
        public StatEffectCallback statEffect;
        public OnHitCallback onHitEffect;
        
        #endregion
        
        #region PREFABS
        public GameObject missilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileProjectile.prefab").WaitForCompletion();
        public GameObject flowerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/TreebotFlowerSeed.prefab").WaitForCompletion();
        public GameObject voidPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierDeathBombProjectile.prefab").WaitForCompletion();
        public GameObject sawPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Saw/Sawmerang.prefab").WaitForCompletion();
        
        #endregion

        #region CHOSEN_PREFABS
        public BuffDef buff;
        public BuffDef debuff;
        public GameObject chosenPrefab;
        public string projectileName;
        
        #endregion
        public EffectType effectType = EffectType.Passive;

        public enum EffectType {
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

        #endregion

        #region DESC_DICTIONARIES

        public Dictionary<string, GameObject> projNameMap;

        public List<StatEffectCallback> statCallbackList;

        #endregion
        
        public void Generate(Xoroshiro128Plus rng, float mult) {
            conditions = rng.nextBool;
            num1 = Mathf.Ceil(rng.RangeFloat(1f, 100f));
            num2 = Mathf.Ceil(rng.RangeFloat(1f, 1000f));
            stat1 = Mathf.Ceil(rng.RangeFloat(1f, 20f)) * mult;
            chance = Mathf.Ceil(rng.RangeFloat(1f, 100f));
            // staticChance = chance;

            // generate maps
            GenerateMapsAndCallbacks();
            
            string[] prefabs = { // TODO: this is partially broken - the projectile name doesnt exist for some reason
                "<style=cIsDamage>Missile</style>", 
                "<style=cIsVoid>Void Implosion</style>",
                "<style=cIsHealth>Healing Flower</style>",
                "<style=cDeath>Rotating Sawblade</style>", 
            };
 
            projectileName = prefabs[rng.RangeInt(0, 4)];
            projNameMap.TryGetValue(projectileName, out chosenPrefab);

            effectType = (EffectType)rng.RangeInt(0, 2);

            string condesc = "";
            if (conditions) {
                condition = conditionCallbackList[rng.RangeInt(0, conditionCallbackList.Count)];
                conditionmap.TryGetValue(condition, out condesc);
            }

            string statdesc = "";
            string onKillDesc = "";
            string onHealDesc = "";
            string onEliteDesc = "";
            string onHitDesc = "";
            string onHurtDesc = "";


            switch (effectType) {
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

        public bool ConditionsMet(CharacterBody body) {
            if (!conditions) {
                return true;
            }
            else {
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
        // stateffect callbacks
        public StatEffectCallback jumpBoost;

        public StatEffectCallback speedBoost;

        public StatEffectCallback healthBoost;
        public StatEffectCallback damageBoost;
        public StatEffectCallback shieldBoost;

        // on hit callbacks
        public static OnHitCallback fireProjectile;

        public void GenerateMapsAndCallbacks() {
            // callbacks
            // condition callbacks

            shieldMore = (body) => {
                return body.healthComponent.shield > 0;
            };

            ood = (body) => {
                return body.outOfDanger;
            };

            ooc = (body) => {
                return body.outOfCombat;
            };

            moving = (body) => {
                return body.characterMotor.velocity.magnitude > 0;
            };

            notMoving = (body) => {
                return body.characterMotor.velocity.magnitude <= 0;
            };

            // stateffect callbacks
            jumpBoost = (args, stacks, body) => {
                args.jumpPowerMultAdd += ((stat1*stacks) * 0.01f);
            };

            speedBoost = (args, stacks, body) => {
                args.moveSpeedMultAdd += ((stat1*stacks) * 0.01f);
            };

            healthBoost = (args, stacks, body) => {
                args.healthMultAdd += ((stat1*stacks) * 0.01f);
            };
            damageBoost = (args, stacks, body) => {
                args.damageMultAdd += ((stat1*stacks) * 0.01f);
            };
            shieldBoost = (args, stacks, body) => {
                float amount = body.healthComponent.fullHealth * ((stat1*stacks) * 0.01f);
                args.baseShieldAdd += amount;
            };

            // on hit callbacks
            OnHitCallback fireProjectile = (DamageInfo info) => {
                    FireProjectileInfo proj = new();
                    proj.damage = info.damage * num2;
                    proj.speedOverride = 2000;
                    proj.owner = info.attacker;
                    proj.rotation = Util.QuaternionSafeLookRotation(info.attacker.GetComponent<CharacterBody>().equipmentSlot.GetAimRay().direction);
                    proj.position = info.attacker.GetComponent<CharacterBody>().corePosition + new Vector3(0, 1, 0);
                    proj.damageColorIndex = DamageColorIndex.Item;
                    proj.projectilePrefab = chosenPrefab;

                    ProjectileManager.instance.FireProjectile(proj);
            };


            /// generate maps
            onhitmap = new() {
                {fireProjectile, $" gain a {chance}% chance on hit to fire a {projectileName} for <style=cIsDamage>{num2}%</style> damage"}
            };

            statmap = new() {
                {healthBoost, $" gain {stat1}% <style=cStack>(+{stat1}% per stack)</style> more health"},
                {jumpBoost, $" gain {stat1}% <style=cStack>(+{stat1}% per stack)</style> jump height"},
                {speedBoost, $" gain {stat1}% <style=cStack>(+{stat1}% per stack)</style> movement speed"},
                {damageBoost, $" gain {stat1}% <style=cStack>(+{stat1}% per stack)</style> more damage"},
                {shieldBoost, $" gain {stat1}% <style=cStack>(+{stat1}% per stack)</style> more shields"},
            };

            conditionmap = new() {
                {shieldMore, $"If your <style=cIsUtility>shield</style> is up,"},
                {moving, "When <style=cIsUtility>moving</style>,"},
                {notMoving, "When <style=cIsUtility>not moving</style>,"},
                {ooc, "When <style=cIsUtility>out of combat</style>,"},
                {ood, "When <style=cIsUtility>out of danger</style>,"}
            };

            statCallbackList = new() {
                speedBoost,
                healthBoost,
                jumpBoost,
                damageBoost,
                shieldBoost
            };

            conditionCallbackList = new() {
                shieldMore,
                ooc,
                ood,
                moving,
                notMoving
            };

            onHitCallbackList = new() {
                fireProjectile
            };

            projNameMap = new() {
                {"<style=cIsDamage>Missile</style>", missilePrefab},
                {"<style=cIsVoid>Void Implosion</style>", voidPrefab},
                {"<style=cIsHealth>Healing Flower</style>", flowerPrefab},
                {"<style=cDeath>Rotating Sawblade</style>", sawPrefab}
            };
        }
    }
}