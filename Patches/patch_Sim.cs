using MonoMod;
using System.Collections.Generic;

[MonoModPatch("Sim")]
class patch_Sim
{
    [MonoModIgnore]
    private static readonly float collSizeEmergingAtom;

    /// <summary>
    /// Turns two molecules into a single molecule.<br/>
    /// From Lithos.
    /// </summary>
    /// <param name="molA">A molecule to merge.</param>
    /// <param name="molB">Another molecule to merge.</param>
    /// <returns>The combined molecule.</returns>
    public Molecule CombineMolecules(Molecule molA, Molecule molB)
    {
        if (molA != molB)
        {
            ((Sim)(object)this).molecules.Remove(molA);
            ((Sim)(object)this).molecules.Remove(molB);
            molA = molA.MergeWith(molB);
            ((Sim)(object)this).molecules.Add(molA);
        }
        return molA;
    }

    public void SpawnCollider(HexIndex hex)
    {
        ((Sim)(object)this).additionalCollisions.Add(new Sim.Collider()
        {
            type = Sim.ColliderType.Atom,
            center = HexGrid.standardGrid.ToPixelCoords(hex),
            radius = collSizeEmergingAtom
        });
    }

    public void SpawnColliderRelative(Part part, HexIndex offset) => SpawnCollider(part.InFrontBy(offset));

    public Atom SpawnAtom(AtomType atomType, HexIndex hex)
    {
        Molecule molecule = new();
        Atom atom = new(atomType);
        molecule.AddAtom(atom, hex);
        ((Sim)(object)this).molecules.Add(molecule);
        return atom;
    }

    public Atom SpawnAtomRelative(AtomType atomType, Part part, HexIndex offset) => SpawnAtom(atomType, part.InFrontBy(offset));
}
