using System.Collections.Generic;
using UnityEngine;

public static class StateDefinitionRegistry
{
    private static Dictionary<string, StateDefinition> byId;

    private static void EnsureLoaded()
    {
        if (byId != null)
            return;

        byId = new Dictionary<string, StateDefinition>();

        // Loads ALL StateDefinition assets in your project.
        // You can refine this later, but this works immediately.
        var all = Resources.LoadAll<StateDefinition>("");

        foreach (var def in all)
        {
            if (def != null && !string.IsNullOrWhiteSpace(def.Id))
                byId[def.Id] = def;
        }
    }

    public static bool TryGet(string id, out StateDefinition def)
    {
        EnsureLoaded();
        return byId.TryGetValue(id, out def);
    }
}
