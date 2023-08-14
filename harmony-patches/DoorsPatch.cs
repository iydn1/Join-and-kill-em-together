namespace Jaket.HarmonyPatches;

using HarmonyLib;

using Jaket.Net;

[HarmonyPatch(typeof(Door), nameof(Door.Unlock))]
public class DoorUnlockPatch
{
    static void Postfix(Door __instance)
    {
        // doors are unlocked only at the host, because only he has original enemies
        if (LobbyController.Lobby != null && LobbyController.IsOwner) World.Instance.SendDoorOpen(__instance.gameObject);
    }
}

[HarmonyPatch(typeof(Door), nameof(Door.Lock))]
public class DoorLockPatch
{
    static void Postfix(Door __instance)
    {
        // after restart, the doors close again, which can lead to getting stuck in the room
        if (LobbyController.Lobby != null && !LobbyController.IsOwner) World.Instance.CheckDoorOpen(__instance.gameObject);
    }
}

[HarmonyPatch(typeof(FinalDoor), nameof(FinalDoor.Open))]
public class FinalDoorPatch
{
    static void Postfix(FinalDoor __instance)
    {
        // final door can also open on the client, but in the case of boss fight, it only opens on the host
        if (LobbyController.Lobby != null && LobbyController.IsOwner) World.Instance.SendDoorOpen(__instance.gameObject);
    }
}

[HarmonyPatch(typeof(BigDoorOpener), "OnEnable")]
public class BigDoorPatch
{
    static void Postfix(BigDoorOpener __instance)
    {
        // level 0-5 has a unique door that for some reason does not want to open itself
        if (LobbyController.Lobby != null && LobbyController.IsOwner) World.Instance.SendDoorOpen(__instance.gameObject);
    }
}

[HarmonyPatch(typeof(DoorOpener), "OnEnable")]
public class AcidLevelAndSkullCasePatch
{
    static void Postfix(DoorOpener __instance)
    {
        // no need to do anything if the player is playing alone
        if (LobbyController.Lobby == null || !LobbyController.IsOwner) return;

        // level 3-1 has acid that comes down in layers
        if (World.AcidLevelsNames.Contains(__instance.name)) World.Instance.SendDoorOpen(__instance.gameObject);

        // level 5-1 has cases with skulls inside them
        if (World.SkullCasesNames.Contains(__instance.transform.parent.gameObject.name)) World.Instance.SendDoorOpen(__instance.gameObject);
    }
}

[HarmonyPatch(typeof(CheckPoint), nameof(CheckPoint.ActivateCheckPoint))]
public class CheckPointPatch
{
    // some objects in the level do not appear immediately
    static void Postfix() => World.Instance.Invoke("Recache", .1f);
}
