using HarmonyLib;
using UnityEngine;
using static UIPopupList;

namespace TestPlugin
{
    //public class MyHarmonyPatch
    //{
    //    private static BepInEx.Logging.ManualLogSource _logger;

    //    public static void SetLogger(BepInEx.Logging.ManualLogSource logger)
    //    {
    //        _logger = logger;
    //        _logger.LogInfo("Logger was set!");
    //    }

    //    public static void WeaponVisiblityTestDelegate(Plane[] frustumPlanes, SpherePack pack, ViewState viewState)
    //    {
    //        var position = CockpitCamera.Instance.position;

    //        _logger.LogInfo($"WeaponVisiblityTestDelegate position: {position}");

    //        Unit unit = pack.userData as Unit;
    //        if (unit != null && unit != Fighter.player)
    //        {
    //            Vector3 to = unit.position - position;
    //            var _toZeroPoint = (Fighter.player.weaponSystem.cockpitCameraPosition - position).normalized;
    //            _logger.LogInfo($"WeaponVisiblityTestDelegate _toZeroPoint: {_toZeroPoint}");

    //            float num = Vector3.Angle(_toZeroPoint, to);
    //            _logger.LogInfo($"WeaponVisiblityTestDelegate num: {num}");

    //            if (num < 5f)
    //            {
    //                CockpitCamera.unitClosestToWeaponZeroPointAngle = num;
    //                CockpitCamera.unitClosestToWeaponZeroPoint = unit;
    //            }
    //        }
    //    }

    //    public static bool Prefix()
    //    {
    //        _logger.LogInfo("Prefix called");
    //        var position = CockpitCamera.Instance.position;
    //        _logger.LogInfo($"Prefix position: {position}");

    //        CockpitCamera.unitClosestToWeaponZeroPoint = null;
    //        CockpitCamera.unitClosestToWeaponZeroPointAngle = 5f;

    //        var _toZeroPoint = (Fighter.player.weaponSystem.cockpitCameraPosition - position).normalized;
    //        _logger.LogInfo($"Prefix _toZeroPoint: {_toZeroPoint}");

    //        SphereTreeManager.GetFactory(SphereTreeIndex.Unit).VisibilityTest(position, _toZeroPoint, 5f, WeaponVisiblityTestDelegate);

    //        return false;
    //    }

    //    public static void ApplyPatch()
    //    {
    //        var harmony = new Harmony("x17.hotds.changed_target_zero");
    //        var original = AccessTools.Method(typeof(CockpitCamera), "GetClosestUnitToWeaponZeroPoint");
    //        var prefix = new HarmonyMethod(typeof(MyHarmonyPatch).GetMethod("Prefix"));
    //        harmony.Patch(original, prefix);
    //        _logger.LogInfo("MyHarmonyPatch.ApplyPatch() done");
    //    }
    //}
}
