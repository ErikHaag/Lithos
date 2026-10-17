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

    /// <summary>
    /// Spawns a small collider at the specified hex.<br/>
    /// From Lithos.
    /// </summary>
    /// <param name="hex">The hex to spawn.</param>
    public void SpawnCollider(HexIndex hex)
    {
        ((Sim)(object)this).additionalCollisions.Add(new Sim.Collider()
        {
            type = Sim.ColliderType.Atom,
            center = HexGrid.standardGrid.ToPixelCoords(hex),
            radius = collSizeEmergingAtom
        });
    }

    /// <summary>
    /// Spawns a small collider relative to a part.<br/>
    /// From Lithos.
    /// </summary>
    /// <param name="part">The part <paramref name="offset"/> is relative to.</param>
    /// <param name="offset">The hex to spawn.</param>
    public void SpawnColliderRelative(Part part, HexIndex offset) => SpawnCollider(part.InFrontBy(offset));

    /// <summary>
    /// Spawns a new molecule on the board with a single atom.<br/>
    /// From Lithos.
    /// </summary>
    /// <param name="atomType">The atomtype to spawn</param>
    /// <param name="hex">The hex to spawn.</param>
    /// <returns>The atom object being spawned.</returns>
    public Atom SpawnAtom(AtomType atomType, HexIndex hex)
    {
        Molecule molecule = new();
        Atom atom = new(atomType);
        molecule.AddAtom(atom, hex);
        ((Sim)(object)this).molecules.Add(molecule);
        return atom;
    }

    /// <summary>
    /// Spawns an atom relative to a part.<br/>
    /// From Lithos.
    /// </summary>
    /// <param name="atomType">The atomtype to spawn</param>
    /// <param name="part">The part <paramref name="offset"/> is relative to.</param>
    /// <param name="offset">The hex to spawn.</param>
    /// <returns>The atom object being spawned.</returns>
    public Atom SpawnAtomRelative(AtomType atomType, Part part, HexIndex offset) => SpawnAtom(atomType, part.InFrontBy(offset));
}
