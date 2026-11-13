using MonoMod.Utils;
using Quintessential;
using Quintessential.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lithos;

public class MutableWheel
{

    private class PermissionChecker(Identifier identifier)
    {
        private readonly Identifier checkedId = identifier;

        public bool IsPermitted(HashSet<Identifier> ids) => ids.Contains(checkedId);
    }

    public static readonly List<MutableWheel> instances = [];
    public readonly PartType wheel;

    private readonly PermissionChecker checker;
    private readonly AtomType[] startingAtoms;

    public MutableWheel(QuintessentialMod mod, string id, Texture iconTexture, Texture iconHoverTexture, AtomType[] startingAtoms, int cost = 30, string permissionID = "")
    {
        ArgumentNullException.ThrowIfNull(mod);
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(iconTexture);
        ArgumentNullException.ThrowIfNull(iconHoverTexture);
        ArgumentNullException.ThrowIfNull(startingAtoms);
        if (startingAtoms.Length != 6)
        {
            throw new ArgumentException($"You must have 6 starting atoms present, got {startingAtoms.Length} atoms instead.");
        }
        ArgumentNullException.ThrowIfNull(permissionID);
        wheel = new PartType()
        {
            Id = mod.GetIdentifier(id),
            iconTexture = iconTexture,
            iconHoverTexture = iconHoverTexture,
            cost = cost,
            isUnique = true,
            layer = PartTypeLayer.Top,
            isProgrammable = true,
            attachedAtoms = new() {
                {new(0, 1), LithosMod.Invisitite },
                {new(1, 0), LithosMod.Invisitite },
                {new(1, -1), LithosMod.Invisitite },
                {new(0, -1), LithosMod.Invisitite },
                {new(-1, 0), LithosMod.Invisitite },
                {new(-1, 1), LithosMod.Invisitite }
            },
            canRotateInSimulation = true
        };

        // I don't know why you wouldn't want a custom permission check, but I wont judge.
        if (permissionID.Length > 0)
        {
            checker = new(mod.GetIdentifier(permissionID));
            wheel.CustomPermissionCheck = checker.IsPermitted;
        }

        this.startingAtoms = startingAtoms;
        instances.Add(this);

        QApi.AddPartTypeToPanel(wheel, PartTypes.ravarisWheel);
        QApi.AddPartType(mod, wheel, id);
    }

    public Molecule GetDefaultMolecule()
    {
        Molecule m = new();
        m.AddAtom(new(startingAtoms[0]), new(0, 1));
        m.AddAtom(new(startingAtoms[1]), new(1, 0));
        m.AddAtom(new(startingAtoms[2]), new(1, -1));
        m.AddAtom(new(startingAtoms[3]), new(0, -1));
        m.AddAtom(new(startingAtoms[4]), new(-1, 0));
        m.AddAtom(new(startingAtoms[5]), new(-1, 1));
        return m;
    }
    
    public static bool MaybeGetWheel(PartType potentialWheelType, out MutableWheel wheel)
    {
        try
        {
            wheel = instances.First(w => w.wheel == potentialWheelType);
            return true;
        }catch (InvalidOperationException) {
            wheel = default;
            return false;
        }
    }

    public Molecule LoadMolecule(PartSimState pss)
    {
        if (pss.TryGetComponent(MutableWheelComponent.identifier, out ISimStateComponent component))
        {
            return ((MutableWheelComponent)component).heldMolecule;
        }
        MutableWheelComponent mwc = new(GetDefaultMolecule());
        pss.AddComponent(mwc);
        return mwc.heldMolecule;

    }

    public bool TryFindWheelAtomAt(ISim sim, List<Part> parts, HexIndex hex, out AtomReference reference)
    {
        foreach (var part in parts.Where(part => part.GetType() == wheel))
        {
            PartSimState pss = sim.GetSimState(part);
            HexIndex relHex = hex - pss.newPosition;
            if (relHex.Length() != 1)
            {
                // not adjacent
                continue;
            }
            relHex = relHex.Rotated(pss.rotation.Negative());
            Molecule m = LoadMolecule(pss);
            Atom atom = m.GetAtoms()[relHex];
            reference = new AtomReference(m, relHex, atom.atomType, atom, true);
            return true;
        }
        // no wheel here :(
        reference = default;
        return false;
    }

    public bool TryFindWheelAtomAt(Sim sim, HexIndex hex, out AtomReference reference) => TryFindWheelAtomAt(sim, sim.GetSolution().parts, hex, out reference);

    public bool TryFindWheelAtomAt(SolutionEditorBase seb, HexIndex hex, out AtomReference reference) => TryFindWheelAtomAt(seb.GetSimulation(), seb.GetSolution().parts, hex, out reference);

    public bool TryFindWheelAtRelative(ISim sim, List<Part> parts, Part originPart, HexIndex offset, out AtomReference reference) => TryFindWheelAtomAt(sim, parts, originPart.GlobalPosFromLocal(offset), out reference);

    public bool TryFindWheelAtRelative(Sim sim, Part originPart, HexIndex offset, out AtomReference reference) => TryFindWheelAtomAt(sim, originPart.GlobalPosFromLocal(offset), out reference);

    public bool TryFindWheelAtRelative(SolutionEditorBase seb, Part originPart, HexIndex offset, out AtomReference reference) => TryFindWheelAtomAt(seb, originPart.GlobalPosFromLocal(offset), out reference);

    
}
