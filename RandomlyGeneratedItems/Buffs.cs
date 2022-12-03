using UnityEngine;
using RoR2;
using R2API;
using System.Collections.Generic;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Networking;

namespace RandomlyGeneratedItems
{
    public class Buffs
    {
        public static void Awake()
        {
            NoDecay.Create();
        }

        public class NoDecay
        {
            public static BuffDef buffDef;

            public static void Create()
            {
                buffDef = ScriptableObject.CreateInstance<BuffDef>();
                buffDef.name = "NoDecay";
                buffDef.isHidden = true;
                buffDef.isDebuff = false;

                ContentAddition.AddBuffDef(buffDef);

                RecalculateStatsAPI.GetStatCoefficients += (body, args) =>
                {
                    if (NetworkServer.active)
                    {
                        if (body.HasBuff(buffDef))
                        {
                            body.maxBarrier = int.MaxValue;
                        }
                    }
                };
            }
        }
    }
}