using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AedenthornUtils
{
    public static List<Container> containerList = new List<Container>();

    public static List<Container> GetNearbyContainers(Vector3 center, float range)
    {
        List<Container> containers = new List<Container>();
        foreach (Container container in containerList)
        {
            if (container != null
                && container.GetComponentInParent<Piece>() != null
                && Player.m_localPlayer != null
                && container?.transform != null
                && container.GetInventory() != null
                && (range <= 0 || Vector3.Distance(center, container.transform.position) < range)
                && Traverse.Create(container).Method("CheckAccess", new object[] { Player.m_localPlayer.GetPlayerID() }).GetValue<bool>()
                && !container.IsInUse())
            {
                Traverse.Create(container).Method("Load").GetValue();
                containers.Add(container);
            }
        }
        return containers;
    }

    [HarmonyPatch(typeof(Container), "Awake")]
    static class Container_Awake_Patch
    {
        static void Postfix(Container __instance, ZNetView ___m_nview)
        {
            __instance.StartCoroutine(AddContainer(__instance, ___m_nview));
        }
    }

    public static IEnumerator AddContainer(Container container, ZNetView nview)
    {
        yield return null;
        try
        {
            //Dbgl($"Checking {container.name} {nview != null} {nview?.GetZDO() != null} {nview?.GetZDO()?.GetLong("creator".GetStableHashCode(), 0L)}");
            if (container.GetInventory() != null
                && nview?.GetZDO() != null
                && (container.name.StartsWith("piece_")
                    || container.name.StartsWith("Container")
                    || nview.GetZDO().GetLong("creator".GetStableHashCode(), 0L) != 0))
            {
                //Dbgl($"Adding {container.name}");
                containerList.Add(container);
            }
        }
        catch
        {

        }
        yield break;
    }

    [HarmonyPatch(typeof(Container), "OnDestroyed")]
    static class Container_OnDestroyed_Patch
    {
        static void Prefix(Container __instance)
        {
            containerList.Remove(__instance);
        }
    }


    public static bool IgnoreKeyPresses(bool extra = false)
    {
        if (!extra)
            return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() || Console.IsVisible() || TextInput.IsVisible() || ZNet.instance.InPasswordDialog() || Chat.instance?.HasFocus() == true;
        return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() || Console.IsVisible() || TextInput.IsVisible() || ZNet.instance.InPasswordDialog() || Chat.instance?.HasFocus() == true || StoreGui.IsVisible() || InventoryGui.IsVisible() || Menu.IsVisible() || TextViewer.instance?.IsVisible() == true;
    }
    public static bool CheckKeyDown(string value)
    {
        try
        {
            return Input.GetKeyDown(value.ToLower());
        }
        catch
        {
            return false;
        }
    }
    public static bool CheckKeyHeld(string value, bool req = true)
    {
        try
        {
            return Input.GetKey(value.ToLower());
        }
        catch
        {
            return !req;
        }
    }
}
