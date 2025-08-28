using Database;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TUNING;

namespace oni_attr_mod
{
    public class Patches
    {

        //[HarmonyPatch(typeof(ElectrolyzerConfig))]
        //[HarmonyPatch("CreateBuildingDef")]
        //public class ElectrolyzerConfig_CreateBuildingDef_Patch
        //{
        //    public static void Postfix(ref BuildingDef __result)
        //    {
        //        __result.Mass = new float[] { 200f, 50f };
        //        __result.ConstructionTime = 30f;
        //        __result.ExhaustKilowattsWhenActive = 0.5f;
        //        __result.SelfHeatKilowattsWhenActive = 1f;
        //    }
        //}

        // 传送门无限打印修改
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

        // 修改小人兴趣数量和数值属性
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

        // 修改小人兴趣数量和数值属性
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
                TUNING.DUPLICANTSTATS.BADTRAITS = TUNING.DUPLICANTSTATS.GOODTRAITS;
                Debug.Log($"[ONI_ATTR_MOD] APTITUDE_ATTRIBUTE_BONUSES patched in Db.Initialize. Length: {TUNING.DUPLICANTSTATS.APTITUDE_ATTRIBUTE_BONUSES.Length}");
            }
        }

        // 好特质数量
        [HarmonyPatch(typeof(MinionStartingStats))]
        [HarmonyPatch("GenerateTraits")]
        public class MinionStartingStats_GenerateTraits_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                bool patched = false;

                for (int i = 0; i < codes.Count; i++)
                {
                    // 寻找 ldc.i4.1 指令（加载常数 1）
                    if (codes[i].opcode == OpCodes.Ldc_I4_1)
                    {
                        // 检查下一条指令是否是 stloc（存储到本地变量），这通常表示 num = 1
                        if (i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Stloc_0)
                        {
                            // 将 ldc.i4.1 (加载 1) 替换为 ldc.i4.3 (加载 3)
                            codes[i] = new CodeInstruction(OpCodes.Ldc_I4_3);
                            patched = true;
                            Debug.Log("[ONI_ATTR_MOD] Transpiler: Successfully patched num = 1 to num = 3 in GenerateTraits");
                            break; // 只替换第一个找到的实例
                        }
                        // 也检查其他可能的本地变量存储指令
                        else if (i + 1 < codes.Count &&
                                (codes[i + 1].opcode == OpCodes.Stloc_1 ||
                                 codes[i + 1].opcode == OpCodes.Stloc_2 ||
                                 codes[i + 1].opcode == OpCodes.Stloc_3 ||
                                 codes[i + 1].opcode == OpCodes.Stloc_S))
                        {
                            // 将 ldc.i4.1 替换为 ldc.i4.3
                            codes[i] = new CodeInstruction(OpCodes.Ldc_I4_3);
                            patched = true;
                            Debug.Log("[ONI_ATTR_MOD] Transpiler: Successfully patched num = 1 to num = 3 in GenerateTraits");
                            break;
                        }
                    }
                }

                if (!patched)
                {
                    Debug.LogWarning("[ONI_ATTR_MOD] Transpiler: Could not find num = 1 assignment in GenerateTraits");
                }

                return codes.AsEnumerable();
            }
        }
    }
}
