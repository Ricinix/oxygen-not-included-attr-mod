using Database;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TUNING;
using UnityEngine;

namespace oni_attr_mod
{
    public class Patches
    {

        // 传送门无限打印修改(Unlimited printing and modification of the portal)
        [HarmonyPatch(typeof(Immigration))]
        [HarmonyPatch("Sim200ms")]
        public class Immigration_Sim200ms_Patch
        {
            public static void Postfix(object __instance)
            {
                var field = AccessTools.Field(__instance.GetType(), "bImmigrantAvailable");
                field.SetValue(__instance, true);
            }
        }

        // 修改小人兴趣数量和数值属性(Modify the number and value attributes of the little people's interests)
        [HarmonyPatch(typeof(MinionStartingStats))]
        [HarmonyPatch("GenerateAptitudes")]
        public class MinionStartingStats_GenerateAptitudes_Patch
        {
            public static bool Prefix(ref MinionStartingStats __instance, string guaranteedAptitudeID = null)
            {
                if (__instance.personality.model == BionicMinionConfig.MODEL)
                {
                    return false;
                }
                int num = UnityEngine.Random.Range(3, 4);
                List<SkillGroup> list = new List<SkillGroup>(Db.Get().SkillGroups.resources);
                list.RemoveAll((SkillGroup match) => !match.allowAsAptitude);
                list.Shuffle<SkillGroup>();
                if (guaranteedAptitudeID != null)
                {
                    __instance.skillAptitudes.Add(Db.Get().SkillGroups.Get(guaranteedAptitudeID), (float)DUPLICANTSTATS.APTITUDE_BONUS);
                    list.Remove(Db.Get().SkillGroups.Get(guaranteedAptitudeID));
                    num--;
                }
                for (int i = 0; i < num; i++)
                {
                    __instance.skillAptitudes.Add(list[i], (float)DUPLICANTSTATS.APTITUDE_BONUS);
                }
                return false;
            }
        }

        // 修改小人兴趣数量和数值属性(Modify the number and value attributes of the little people's interests)
        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                TUNING.DUPLICANTSTATS.APTITUDE_ATTRIBUTE_BONUSES = new int[16] {
                    1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000,
                    1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000
                };
                // 没有坏习惯(No bad habits)
                TUNING.DUPLICANTSTATS.BADTRAITS = TUNING.DUPLICANTSTATS.GOODTRAITS;
                // 经验获取加成(Experience gain bonus)
                TUNING.SKILLS.PASSIVE_EXPERIENCE_PORTION = 500f;
                // 学一个技能给的士气奖励修改(Modify the morale reward for learning a skill)
                TUNING.DUPLICANTSTATS.APTITUDE_BONUS = 1000;
                // 航天运动服行走速度提升(Atmosuit walking speed boost)
                TUNING.EQUIPMENT.SUITS.ATMOSUIT_ATHLETICS = 6;
                // 铅服行走速度提升(Lead suit walking speed boost)
                TUNING.EQUIPMENT.SUITS.LEADSUIT_ATHLETICS = 8;
                // 氧气面罩行走速度提升(Oxygen mask walking speed boost)
                TUNING.EQUIPMENT.SUITS.OXYGEN_MASK_ATHLETICS = 2;
                // 游戏原始储存上限(Original game storage limit)
                PrimaryElement.MAX_MASS = 1000000000f;

                Debug.Log($"[ONI_ATTR_MOD] APTITUDE_ATTRIBUTE_BONUSES patched in Db.Initialize. Length: {TUNING.DUPLICANTSTATS.APTITUDE_ATTRIBUTE_BONUSES.Length}");
            }
        }

        // 储存箱100倍修改(Storage box 100x modification)
        [HarmonyPatch(typeof(Storage))]
        [HarmonyPatch(MethodType.Constructor)]
        public class Storage_Constructor_Patch
        {
            public static void Postfix(ref Storage __instance)
            {
                __instance.capacityKg = 200000000f;
            }
        }

        // 储气罐100倍修改(Gas reservoir 100x modification)
        [HarmonyPatch(typeof(GasReservoirConfig))]
        [HarmonyPatch("ConfigureBuildingTemplate")]
        public class GasReservoirConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                Storage storage = go.AddOrGet<Storage>();
                storage.capacityKg = 10000000f;
                ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
                conduitConsumer.capacityKG = storage.capacityKg;
            }
        }

        // 储气罐100倍修改(Liquid reservoir 100x modification)
        [HarmonyPatch(typeof(LiquidReservoirConfig))]
        [HarmonyPatch("ConfigureBuildingTemplate")]
        public class LiquidReservoirConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                Storage storage = go.AddOrGet<Storage>();
                storage.capacityKg = 50000000f;
                ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
                conduitConsumer.capacityKG = storage.capacityKg;
            }
        }

        // 食物盒100倍修改(Ration box 100x modification)
        [HarmonyPatch(typeof(RationBoxConfig))]
        [HarmonyPatch("ConfigureBuildingTemplate")]
        public class RationBoxConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                Storage storage = go.AddOrGet<Storage>();
                storage.capacityKg = 1500000f;
            }
        }

        // 冰箱1000倍修改(Refrigerator 1000x modification)
        [HarmonyPatch(typeof(RefrigeratorConfig))]
        [HarmonyPatch("DoPostConfigureComplete")]
        public class RefrigeratorConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                Storage storage = go.AddOrGet<Storage>();
                storage.capacityKg = 1000000f;
            }
        }

        // 电池电量100倍 and 耗电量修改(Battery capacity 100x and power consumption modification)
        [HarmonyPatch(typeof(BatteryConfig))]
        [HarmonyPatch("DoPostConfigureComplete")]
        public class BatteryConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                Battery battery = go.AddOrGet<Battery>();
                battery.capacity = 100000000f;
                battery.joulesLostPerSecond = 0.6666666f;
            }
        }

        // 电线负载修改(Wire load modification)
        [HarmonyPatch(typeof(Wire))]
        [HarmonyPatch("GetMaxWattageAsFloat")]
        public class Wire_GetMaxWattageAsFloat_Patch
        {
            public static void Postfix(ref float __result)
            {
                __result = 50000000f;
            }
        }

        // 发电机发电100倍(Generator 100x modification)
        [HarmonyPatch(typeof(GeneratorConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public class GeneratorConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.GeneratorWattageRating = 6000000f;
                __result.GeneratorBaseCapacity = __result.GeneratorWattageRating;
            }
        }

        // 人力发电机发电100倍(Manual generator 100x modification)
        [HarmonyPatch(typeof(ManualGeneratorConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public class ManualGeneratorConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.GeneratorWattageRating = 4000000f;
                __result.GeneratorBaseCapacity = __result.GeneratorWattageRating;
            }
        }

        // 天燃气发电机发电100倍(Methane generator 100x modification)
        [HarmonyPatch(typeof(MethaneGeneratorConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public class MethaneGeneratorConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.GeneratorWattageRating = 8000000f;
                __result.GeneratorBaseCapacity = __result.GeneratorWattageRating;
            }
        }

        // 氢气发电机发电100倍(Hydrogen generator 100x modification)
        [HarmonyPatch(typeof(HydrogenGeneratorConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public class HydrogenGeneratorConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.GeneratorWattageRating = 8000000f;
                __result.GeneratorBaseCapacity = __result.GeneratorWattageRating;
            }
        }

        // 石油发电机发电100倍(Petroleum generator 100x modification)
        [HarmonyPatch(typeof(PetroleumGeneratorConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public class PetroleumGeneratorConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.GeneratorWattageRating = 20000000f;
                __result.GeneratorBaseCapacity = __result.GeneratorWattageRating;
            }
        }

        // 木燃发电机发电100倍(Wood gas generator 100x modification)
        [HarmonyPatch(typeof(WoodGasGeneratorConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public class WoodGasGeneratorConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.GeneratorWattageRating = 3000000f;
                __result.GeneratorBaseCapacity = __result.GeneratorWattageRating;
            }
        }

        // 蒸汽发电机发电100倍(Steam generator 100x modification)
        [HarmonyPatch(typeof(StaterpillarGeneratorConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public class StaterpillarGeneratorConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.GeneratorWattageRating = 16000000f;
                __result.GeneratorBaseCapacity = __result.GeneratorWattageRating;
            }
        }
    }
}
