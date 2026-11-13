using Lithos;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System;
using System.Linq;

[MonoModPatch("SolutionEditorBase")]
class patch_SolutionEditorBase
{
    [MonoModIgnore]
    [MonoModPublic]
    public static extern void method_2450(PartRenderer param_8686, Texture param_8687, Texture param_8688, Vector2 param_8689);
    
    /// <summary>
    /// Draws an atom falling into a hole.<br/>
    /// From Lithos.
    /// </summary>
    /// <param name="atomReference">The atom to start falling.</param>
    public void DrawFallingAtom(AtomReference atomReference) => ((SolutionEditorBase)(object)this).consumptionEffects.Add(new ConsumptionEffect((SolutionEditorBase)(object)this, atomReference));

    [MonoModILInject("layer_0_RenderPartBase")]
    public static void PatchRenderPartBase(MethodDefinition method, CustomAttribute attr)
    {
        Console.WriteLine("[Lithos] Patching RenderPartBase");

        ILCursor gremlin = new(new ILContext(method));
        if (!gremlin.TryGotoNext(MoveType.After,
            instr => instr.OpCode == OpCodes.Ldarg_1,
            instr => instr.OpCode == OpCodes.Callvirt && ((MethodReference)instr.Operand).Name == "HasAtoms",
            instr => instr.OpCode == OpCodes.Brfalse
        ))
        {
            Console.WriteLine("[Lithos] Could not find HasAtoms condition.");
            throw new Exception();
        }

        if (!gremlin.TryGotoNext(MoveType.After,
            instr => instr.OpCode == OpCodes.Call && ((MethodReference)instr.Operand).Name == "RenderArmLights"
        ))
        {
            Console.WriteLine("[Lithos] Could not find RAL call");
            throw new Exception();
        }
        Instruction insertionInstruction = gremlin.Prev;

        if (!gremlin.TryGotoNext(MoveType.After,
            instr => instr.OpCode == OpCodes.Stloc_S,
            instr => instr.OpCode == OpCodes.Br_S
        )) {
            Console.WriteLine("[Lithos] Could not find loop start.");
            throw new Exception();
        }
        gremlin.Goto((Instruction)gremlin.Prev.Operand);
        if (!gremlin.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Endfinally))
        {
            Console.WriteLine("[Lithos] Could not find loop exit");
            throw new Exception();
        }
        Instruction ifEnd = gremlin.Next;
        gremlin.Goto(insertionInstruction, MoveType.After);
        
        TypeDefinition SEBType = method.DeclaringType.Resolve();
        MethodDefinition DWA = SEBType.Methods.First(m => m.Name == nameof(DrawWheelAtoms));

        gremlin.Emit(OpCodes.Ldarg_0);
        gremlin.Emit(OpCodes.Ldarg_1);
        gremlin.Emit(OpCodes.Ldloc_0);
        gremlin.Emit(OpCodes.Callvirt, DWA);
        gremlin.Emit(OpCodes.Brtrue, ifEnd);

        Console.WriteLine("[Lithos] Ok.");
    }

    public bool DrawWheelAtoms( Part part, IntermediatePartState ips)
    {
        if (!MutableWheel.MaybeGetWheel(part.GetType(), out MutableWheel wheel))
        {
            return false;
        }
        Molecule m = wheel.LoadMolecule(((SolutionEditorBase)(object)this).GetSimulation().GetSimState(part));
        Editor.RenderMolecule(m, ips.pos, new(0, 0), ips.rotation, 1f, 1f, 1f, false, ((SolutionEditorBase)(object)this));
        return true;
    }
}
