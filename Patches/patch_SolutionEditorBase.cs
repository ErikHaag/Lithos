using MonoMod;

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
}
