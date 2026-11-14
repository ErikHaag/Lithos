using Lithos;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System;
using System.Linq;

[MonoModPatch("Sim")]
class patch_Sim
{
    [MonoModIgnore]
    public static readonly float collSizeEmergingAtom;
    
    [MonoModIgnore]
    [MonoModPublic]
    public extern void PlaySound(Sound sound);

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
    public void SpawnColliderRelative(Part part, HexIndex offset) => SpawnCollider(part.GlobalPosFromLocal(offset));

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
    public Atom SpawnAtomRelative(AtomType atomType, Part part, HexIndex offset) => SpawnAtom(atomType, part.GlobalPosFromLocal(offset));

    [MonoModILInject("layer_0_RunCycleGlyphs")]
    public static void PatchLayer0RunCycleGlyphs(MethodDefinition method, CustomAttribute attr)
    {
        Console.WriteLine("[Lithos] Patching RunCycleGlyphs.");
        ILCursor gremlin = new(new ILContext(method));
        if (!gremlin.TryGotoNext(MoveType.After,
            instr => instr.OpCode == OpCodes.Br,
            instr => instr.OpCode == OpCodes.Newobj && ((MethodReference)instr.Operand).DeclaringType.FullName == "Sim/ReferredPart",
            instr => instr.OpCode == OpCodes.Stloc_S
        )) {
            Console.WriteLine("[Lithos] Could not find ReferredPart creation.");
            throw new Exception();
        }
        Instruction partLoopEnd = (Instruction)gremlin.Prev.Previous.Previous.Operand;
        VariableDefinition refPartVar = (VariableDefinition)gremlin.Prev.Operand;
        if (!gremlin.TryGotoNext(MoveType.After,
            instr => instr.OpCode == OpCodes.Callvirt && ((MethodReference)instr.Operand).Name == "get_Item",
            instr => instr.OpCode == OpCodes.Stloc_S && ((VariableDefinition)instr.Operand).VariableType.Name == "PartSimState"
        ))
        {
            Console.WriteLine("[Lithos] Could not find Part sim state.");
            throw new Exception();
        }
        VariableDefinition pssVar = (VariableDefinition)gremlin.Prev.Operand;
        Instruction temp = gremlin.Next; // return here later
        gremlin.Goto(partLoopEnd);

        TypeDefinition simDef = method.DeclaringType.Resolve();
        MethodDefinition AfterRecipesEventDef = simDef.Methods.First(m => m.Name == nameof(AfterRecipesEvent) && m.IsPrivate && !m.IsStatic);
        
        // Detour to new code.
        gremlin.Emit(OpCodes.Ldarg_0);
        Instruction newTarget = gremlin.Prev;
        gremlin.Emit(OpCodes.Ldloc, refPartVar);
        gremlin.Emit(OpCodes.Ldloc, pssVar);
        gremlin.Emit(OpCodes.Ldarg_1);
        gremlin.Emit(OpCodes.Callvirt, AfterRecipesEventDef);
        
        gremlin.Goto(temp); // woah, deja vu.
        if (!gremlin.TryGotoNext(MoveType.After,
            instr => instr.OpCode == OpCodes.Callvirt && ((MethodReference)instr.Operand).Name == "TryGetValue",
            instr => instr.OpCode == OpCodes.Brfalse && ((Instruction)instr.Operand) == partLoopEnd
        ))
        {
            Console.WriteLine("[Lithos] Couldn't find recipeless branch.");
            throw new Exception();
        }
        gremlin.Prev.Operand = newTarget;

        if (!gremlin.TryGotoNext(MoveType.After,
            instr => instr.OpCode == OpCodes.Ldfld && ((FieldReference)instr.Operand).Name == "wasActivated",
            instr => instr.OpCode == OpCodes.Brtrue && ((Instruction)instr.Operand) == partLoopEnd
        ))
        {
            Console.WriteLine("[Lithos] Couldn't find wasActivated break.");
            throw new Exception();
        }
        gremlin.Prev.Operand = newTarget;

        Console.WriteLine("[Lithos] Ok.");
    }

    private void AfterRecipesEvent(Sim.ReferredPart referred, PartSimState simState, bool isCycleStart)
    {
        Part part = referred.part;
        if (LApi.AfterNativeRecipesHandlers.TryGetValue(part.GetType().Id, out var handlers))
        {
            foreach (var handler in handlers.Values)
            {
                handler((Sim)(object)this, part, simState, isCycleStart);
            }
        }
    }
}