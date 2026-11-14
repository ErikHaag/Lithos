using Quintessential;
using System;
using System.Collections.Generic;

namespace Lithos;

public static class LApi
{
    public static readonly OrderedDictionary<Identifier, Dictionary<Identifier, AfterNativeRecipesHandler>> AfterNativeRecipesHandlers = [];

    /// <summary>
    /// Use only as a last resort!
    /// </summary>
    public static void AddHandler(Identifier PartId, Identifier HandlerId, AfterNativeRecipesHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (!AfterNativeRecipesHandlers.TryGetValue(PartId, out Dictionary<Identifier, AfterNativeRecipesHandler> partHandlers))
        {
            partHandlers = [];
            AfterNativeRecipesHandlers.Add(PartId, partHandlers);
        }
        if (partHandlers.ContainsKey(HandlerId))
        {
            throw new Exception($"We already have a handler for \"{PartId}\" named \"{HandlerId}.");
        }
        partHandlers.Add(HandlerId, handler);
    }

    public delegate void AfterNativeRecipesHandler(Sim sim, Part part, PartSimState simState, bool isCycleStart);
}
