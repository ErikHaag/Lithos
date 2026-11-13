using Quintessential;
using Quintessential.Components;
using System;
using System.Linq;

namespace Lithos;

internal class MutableWheelComponent(Molecule held) : ISimStateComponent
{
    public Identifier Id => identifier;

    public CycleEvent.CycleEventExecutionType CallbackType => CycleEvent.CycleEventExecutionType.None;

    internal static Identifier identifier = LithosMod.Instance.GetIdentifier("MutableWheelComponent");
    public Molecule heldMolecule = held;

    public void OnReset()
    {}

    public void OnBind(PartSimState self, Part part, Sim sim)
    {}

    public void OnUnbind(PartSimState self, Part arg1, Sim arg2)
    {
        heldMolecule = null;
    }

    public void OnCycleCallback(Sim sim, CycleEvent.CycleEventExecutionType executionType) {}
}
