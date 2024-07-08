using BepInEx;
using HarmonyLib;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static WeaponSystem;

namespace TestPlugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("dyingsun.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static new BepInEx.Logging.ManualLogSource Log;

        public const string pluginGuid = "x17.hotds.changed_target_zero";
        public const string pluginName = "ChangedTargetZero";
        public const string pluginVersion = "1.0.0.1";

        void Awake()
        {
            Log = base.Logger;
            InitHarmony();
            Logger.LogInfo(pluginName + " has fully loaded and is ready to go!");
        }

        private void InitHarmony()
        {
            Harmony harmony = new Harmony(pluginGuid);
            Logger.LogInfo("Patching with harmony...");
            harmony.PatchAll();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;  // Unsubscribe since we only want this to happen once
            StartCoroutine(InitAfterFrame(scene));    // Coroutine required to wait for the game to actually finish loading
        }

        private IEnumerator InitAfterFrame(Scene scene)
        {
            yield return null;  // Wait 1 frame so that the scene can load fully

            Modify_CampaignGenerationSettings();
        }

        private WeaponBlueprint findWeaponBlueprint(List<WeaponBlueprint> weapons, string nameMatch)
        {
            Logger.LogInfo($"nameMatch: {nameMatch}");
            foreach (WeaponBlueprint bp in weapons)
            {
                Logger.LogInfo(bp.name);
                if (bp.name.Equals(nameMatch)) return bp;
            }
            return null;
        }

        private void Modify_CampaignGenerationSettings()
        {
            Logger.LogInfo("Modify_CampaignGenerationSettings started");
            var exists = CampaignGenerationSettings.Instance != null;
            Logger.LogInfo($"exists: {exists}");
            var blueprints = CampaignGenerationSettings.Instance.allAvailableWeapons;
            Logger.LogInfo($"blueprints Count {blueprints.Count}");

            //WeaponBlueprint furyMissiles = findWeaponBlueprint(blueprints, "wpn_imp_fighter_lightmissile");
            //if (furyMissiles != null)
            //{
            //    //WeaponBlueprint furyMissiles = FindElement(CampaignGenerationSettings.Instance.allAvailableWeapons, "wpn_imp_fighter_lightmissile");

            //    furyMissiles.displayName = "6BM-VII FURY MISSILES";
            //    furyMissiles.perceivedRange = 0.4f;
            //    furyMissiles.projectile.maxRange = 2800f;
            //    furyMissiles.projectile.muzzleVelocity = 250f;
            //    furyMissiles.projectile.steeringDelay = 0.1f;
            //    furyMissiles.projectile.steeringForce = 3000f;
            //    furyMissiles.projectile.rangeDeterminesDetonation = true;

            //    furyMissiles.weapon.lockAngle = 30f;
            //    furyMissiles.weapon.fireMode = FireMode.Burst;
            //    furyMissiles.weapon.rateOfFire = 0.09f;
            //    furyMissiles.weapon.roundsPerBurst = 5;
            //    furyMissiles.weapon.burstCooldown = 1.2f;
            //    Logger.LogInfo("Patched Fury Missiles");
            //}

            WeaponBlueprint scarabTorp = findWeaponBlueprint(blueprints, "wpn_imp_fighter_torpedo");
            if(scarabTorp != null)
            {
                scarabTorp.weapon.maxAmmo = 2;
                scarabTorp.weapon.roundsOnCollect = 2;
            }

            //todo unerf scarabs
        }
    }

    [HarmonyPatch(typeof(CUICockpitFPVIndicator), "OnPostRenderCockpitLines")]
    public class CUICockpitFPVIndicator_OnPostRenderCockpitLines
    {
        static void Postfix(Material m, Camera cam, CUICockpitFPVIndicator __instance)
        {
            if (Fighter.player != null && Game.Instance.combatState == CombatState.Cockpit && Game.cameras.activeCam == Game.cameras.cockpitCam)
            {
                var projectionDistance = 2000f;

                var VRCam = Game.cameras.cockpitCam.steamVRCameras[0];
                var VRCamForward = VRCam.transform.forward;
                var VRCamRight = VRCam.transform.right;
                var newVector = Quaternion.AngleAxis(10, VRCamRight) * VRCamForward;

                Color hudColor = new(0.941f, 0.392f, 0f, 1f);
                var radius = 42f;

                var VRCamPosition = VRCam.transform.position;
                var VRCamUp = VRCam.transform.up;

                GLRenderer.DrawCircle(VRCamPosition + newVector * projectionDistance, newVector, VRCamUp, radius, radius, hudColor, 32);


                var player = Fighter.player;
                if (player == null) return;

                var target = player.target;
                if (target == null) return;
                if (target.team == player.team) return;

                var activeWeaponGroup = player.weaponSystem.activeGroup;
                if (!activeWeaponGroup.weaponBlueprint.name.Equals("wpn_imp_fighter_lightmissile")) return;
                float lockAngle = activeWeaponGroup.weaponBlueprint.weapon.lockAngle;
                float wpnMaxRange = activeWeaponGroup.maxRange;

                Color lockingCircleColor = new(0.392f, 0.941f, 0f, 1f);

                double thetaInDegrees = lockAngle;
                float slantHeight = wpnMaxRange;

                double thetaInRadians = thetaInDegrees * (Math.PI / 180.0);
                double alpha = thetaInRadians / 2.0;
                float lockingCircleRadius = (float)(slantHeight * Math.Sin(alpha));

                GLRenderer.DrawCircle(VRCamPosition + newVector * slantHeight, newVector, VRCamUp, lockingCircleRadius, lockingCircleRadius, lockingCircleColor, 32);
            }
        }
    }

    //[HarmonyPatch(typeof(WeaponSystem))]
    //class WeaponSystem_Patches
    //{
    //    [HarmonyPrefix]
    //    [HarmonyPatch("Update")]
    //    static bool Prefix1()
    //    {
    //        //todo make this into a Prefix method where it'll return false if all of these conditions are met, otherwise it will use the default method, so you don't have to reimplement it
    //        var player = Fighter.player;
    //        if (player == null) return true;

    //        var VRCam = Game.cameras.cockpitCam.steamVRCameras[0];
    //        if (VRCam == null) return true;

    //        var target = player.target;
    //        if (target == null) return true;
    //        if (target.team == player.team) return true;

    //        var activeWeaponGroup = player.weaponSystem.activeGroup;
    //        if (activeWeaponGroup == null) return true;
    //        if (activeWeaponGroup.overheatRemaining != 0f) return true;
    //        if (activeWeaponGroup.ammo == 0) return true;
    //        if (activeWeaponGroup.weaponBlueprint == null) return true;
    //        if (!activeWeaponGroup.weaponBlueprint.name.Equals("wpn_imp_fighter_lightmissile")) return true;

    //        float lockTime = activeWeaponGroup.weaponBlueprint.weapon.lockTime;
    //        if (lockTime <= 0) return true;

    //        float lockAngle = activeWeaponGroup.weaponBlueprint.weapon.lockAngle / 2;
    //        if (lockAngle <= 0) return true;


    //        float wpnMaxRange = activeWeaponGroup.maxRange;
    //        float targetDistance = Vector3.Distance(player.position, target.position);
    //        var isWithinRange = targetDistance < wpnMaxRange;
    //        if (!isWithinRange) return true;

    //        var VRCamPosition = VRCam.transform.position;
    //        var VRCamForward = VRCam.transform.forward;
    //        var VRCamRight = VRCam.transform.right;
    //        Vector3 lookDirectionOffset = Quaternion.AngleAxis(10, VRCamRight) * VRCamForward;
    //        Vector3 targetingVector = VRCamPosition + lookDirectionOffset * wpnMaxRange - Game.cameras.cockpitCam.position;

    //        Vector3 targetVectorRelative = target.position - Game.cameras.cockpitCam.position;

    //        float targetAngleFromPLV = Vector3.Angle(targetVectorRelative, targetingVector);

    //        //Vector3 zero = Vector3.zero;
    //        //zero = ((!_cockpitCamera) 
    //        //    ? (base.transform.forward * activeGroup.zeroDistance) 
    //        //    : (base.transform.position + base.transform.forward * activeGroup.zeroDistance - cockpitCameraPosition));
    //        //Vector3 zero2 = Vector3.zero;
    //        //zero2 = ((!_cockpitCamera) 
    //        //    ? (base.unit.target.position - base.transform.position) 
    //        //    : (base.unit.target.position - cockpitCameraPosition));
    //        //float num2 = Vector3.Angle(zero2, zero);

    //        if (targetAngleFromPLV < lockAngle)
    //        {
    //            player.weaponSystem.currentLockTime += Time.deltaTime;
    //            //Plugin.Log.LogInfo("");
    //            //Plugin.Log.LogInfo("target within angle");
    //            //Plugin.Log.LogInfo($"targetAngleFromPLV: {targetAngleFromPLV}");
    //            //Plugin.Log.LogInfo($"lockAngle: {lockAngle}");
    //            //Plugin.Log.LogInfo($"currentLockTime: {player.weaponSystem.currentLockTime}");
    //        }
    //        else
    //        {
    //            player.weaponSystem.currentLockTime = 0f;
    //            //Plugin.Log.LogInfo("");
    //            //Plugin.Log.LogInfo("target out of angle");
    //            //Plugin.Log.LogInfo($"targetAngleFromPLV: {targetAngleFromPLV}");
    //            //Plugin.Log.LogInfo($"lockAngle: {lockAngle}");
    //        }

    //        if (player.weaponSystem.currentLockTime >= lockTime)
    //        {
    //            activeWeaponGroup.lockState = LockState.Locked;
    //            //Plugin.Log.LogInfo("");
    //            //Plugin.Log.LogInfo("locked!");
    //        }
    //        else if (player.weaponSystem.currentLockTime > 0f)
    //        {
    //            activeWeaponGroup.lockState = LockState.Locking;
    //            //Plugin.Log.LogInfo("");
    //            //Plugin.Log.LogInfo($"locking: {player.weaponSystem.currentLockTime}");
    //        }
    //        else
    //        {
    //            activeWeaponGroup.lockState = LockState.NotLocking;
    //            //Plugin.Log.LogInfo("");
    //            //Plugin.Log.LogInfo("stopped locking");
    //        }
    //        return false;
    //    }

    //    [HarmonyPostfix]
    //    [HarmonyPatch("OnEnable")]
    //    static void Postfix1() 
    //    {
    //        GLLineRenderer.OnLinePostRender += DrawModHud;
    //    }

    //    [HarmonyPostfix]
    //    [HarmonyPatch("OnDisable")]
    //    static void Postfix2() 
    //    {
    //        GLLineRenderer.OnLinePostRender -= DrawModHud;
    //    }

    //    static void DrawModHud(Material m, Camera cam) 
    //    {
    //        if (Fighter.player != null && Game.Instance.combatState == CombatState.Cockpit && Game.cameras.activeCam == Game.cameras.cockpitCam)
    //        {
    //            var projectionDistance = 2000f;

    //            var VRCam = Game.cameras.cockpitCam.steamVRCameras[0];
    //            var VRCamForward = VRCam.transform.forward;
    //            var VRCamRight = VRCam.transform.right;
    //            var newVector = Quaternion.AngleAxis(10, VRCamRight) * VRCamForward;

    //            Color hudColor = new(0.941f, 0.392f, 0f, 1f);
    //            var radius = 42f;

    //            var VRCamPosition = VRCam.transform.position;
    //            var VRCamUp = VRCam.transform.up;

    //            GLRenderer.DrawCircle(VRCamPosition + newVector * projectionDistance, newVector, VRCamUp, radius, radius, hudColor, 32);


    //            var player = Fighter.player;
    //            if (player == null) return;

    //            var target = player.target;
    //            if (target == null) return;
    //            if (target.team == player.team) return;

    //            var activeWeaponGroup = player.weaponSystem.activeGroup;
    //            if (!activeWeaponGroup.weaponBlueprint.name.Equals("wpn_imp_fighter_lightmissile")) return;
    //            float lockAngle = activeWeaponGroup.weaponBlueprint.weapon.lockAngle;
    //            float wpnMaxRange = activeWeaponGroup.maxRange;

    //            Color lockingCircleColor = new(0.392f, 0.941f, 0f, 1f);

    //            double thetaInDegrees = lockAngle;
    //            float slantHeight = wpnMaxRange;

    //            double thetaInRadians = thetaInDegrees * (Math.PI / 180.0);
    //            double alpha = thetaInRadians / 2.0;
    //            float lockingCircleRadius = (float)(slantHeight * Math.Sin(alpha));

    //            GLRenderer.DrawCircle(VRCamPosition + newVector * slantHeight, newVector, VRCamUp, lockingCircleRadius, lockingCircleRadius, lockingCircleColor, 32);
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(WeaponSystem), "Update")]
    public class WeaponSystem_Update
    {
        //static bool Prefix()
        //{
        //    __result = newLogic();

        //    return false;
        //}

        static bool Prefix()
        {
            //todo make this into a Prefix method where it'll return false if all of these conditions are met, otherwise it will use the default method, so you don't have to reimplement it
            var player = Fighter.player;
            if (player == null) return true;

            var VRCam = Game.cameras.cockpitCam.steamVRCameras[0];
            if (VRCam == null) return true;

            var target = player.target;
            if (target == null) return true;
            if (target.team == player.team) return true;

            var activeWeaponGroup = player.weaponSystem.activeGroup;
            if (activeWeaponGroup == null) return true;
            if (activeWeaponGroup.overheatRemaining != 0f) return true;
            if (activeWeaponGroup.ammo == 0) return true;
            if (activeWeaponGroup.weaponBlueprint == null) return true;
            if (!activeWeaponGroup.weaponBlueprint.name.Equals("wpn_imp_fighter_lightmissile")) return true;

            float lockTime = activeWeaponGroup.weaponBlueprint.weapon.lockTime;
            if (lockTime <= 0) return true;

            float lockAngle = activeWeaponGroup.weaponBlueprint.weapon.lockAngle / 2;
            if (lockAngle <= 0) return true;


            float wpnMaxRange = activeWeaponGroup.maxRange;
            float targetDistance = Vector3.Distance(player.position, target.position);
            var isWithinRange = targetDistance < wpnMaxRange;
            if (!isWithinRange) return true;

            var VRCamPosition = VRCam.transform.position;
            var VRCamForward = VRCam.transform.forward;
            var VRCamRight = VRCam.transform.right;
            Vector3 lookDirectionOffset = Quaternion.AngleAxis(10, VRCamRight) * VRCamForward;
            Vector3 targetingVector = VRCamPosition + lookDirectionOffset * wpnMaxRange - Game.cameras.cockpitCam.position;

            Vector3 targetVectorRelative = target.position - Game.cameras.cockpitCam.position;

            float targetAngleFromPLV = Vector3.Angle(targetVectorRelative, targetingVector);

            //Vector3 zero = Vector3.zero;
            //zero = ((!_cockpitCamera) 
            //    ? (base.transform.forward * activeGroup.zeroDistance) 
            //    : (base.transform.position + base.transform.forward * activeGroup.zeroDistance - cockpitCameraPosition));
            //Vector3 zero2 = Vector3.zero;
            //zero2 = ((!_cockpitCamera) 
            //    ? (base.unit.target.position - base.transform.position) 
            //    : (base.unit.target.position - cockpitCameraPosition));
            //float num2 = Vector3.Angle(zero2, zero);

            if (targetAngleFromPLV < lockAngle)
            {
                player.weaponSystem.currentLockTime += Time.deltaTime;
                //Plugin.Log.LogInfo("");
                //Plugin.Log.LogInfo("target within angle");
                //Plugin.Log.LogInfo($"targetAngleFromPLV: {targetAngleFromPLV}");
                //Plugin.Log.LogInfo($"lockAngle: {lockAngle}");
                //Plugin.Log.LogInfo($"currentLockTime: {player.weaponSystem.currentLockTime}");
            }
            else
            {
                player.weaponSystem.currentLockTime = 0f;
                //Plugin.Log.LogInfo("");
                //Plugin.Log.LogInfo("target out of angle");
                //Plugin.Log.LogInfo($"targetAngleFromPLV: {targetAngleFromPLV}");
                //Plugin.Log.LogInfo($"lockAngle: {lockAngle}");
            }

            if (player.weaponSystem.currentLockTime >= lockTime)
            {
                activeWeaponGroup.lockState = LockState.Locked;
                //Plugin.Log.LogInfo("");
                //Plugin.Log.LogInfo("locked!");
            }
            else if (player.weaponSystem.currentLockTime > 0f)
            {
                activeWeaponGroup.lockState = LockState.Locking;
                //Plugin.Log.LogInfo("");
                //Plugin.Log.LogInfo($"locking: {player.weaponSystem.currentLockTime}");
            }
            else
            {
                activeWeaponGroup.lockState = LockState.NotLocking;
                //Plugin.Log.LogInfo("");
                //Plugin.Log.LogInfo("stopped locking");
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ControlSystem), "GetUnitUnderCrosshair")]
    public class ControlSystem_GetUnitUnderCrosshair
    {

        static bool Prefix(ref Unit __result)
        {
            __result = newLogic();
            return false;
        }

        static Unit newLogic()
        {
            var playerUnit = Fighter.player;
            var VRCam = Game.cameras.cockpitCam.steamVRCameras[0];

            var VRCamForward = VRCam.transform.forward;
            var VRCamRight = VRCam.transform.right;
            var newVector = Quaternion.AngleAxis(10, VRCamRight) * VRCamForward;

            float num = 0f;
            Unit result = null;
            LinkedListNode<Unit> linkedListNode = Unit.allUnits.First;
            while (linkedListNode != null)
            {
                Unit unit = linkedListNode.Value;
                if (!unit.isTargetable || unit == playerUnit)
                {
                    linkedListNode = linkedListNode.Next;
                    continue;
                }

                if (unit.unitType == UnitType.EscapePod)
                {
                    linkedListNode = linkedListNode.Next;
                    continue;
                }

                float num2 = Vector3.Dot(newVector.normalized, (unit.transform.position - Game.cameras.cockpitCam.transform.position).normalized);
                bool flag = true;
                Vector3 vector2 = unit.position - playerUnit.position;


                if ((bool)unit.parent && vector2.sqrMagnitude > unit.parent.maxSubsystemTargetingRange * unit.parent.maxSubsystemTargetingRange)
                {
                    flag = false;
                }

                if (unit.isTargetable && flag && num2 > num)
                {
                    num = num2;
                    result = unit;
                }

                linkedListNode = linkedListNode.Next;
            }
            return result;
        }
    }
}