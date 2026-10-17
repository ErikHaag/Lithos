using MonoMod;

[MonoModPatch("PartRenderer")]
class patch_PartRenderer
{
    /// <summary>
    /// Draws a gloss on a part, dictated by a mask.<br/>
    /// From Lithos.
    /// </summary>
    /// <param name="gloss">The gloss texture to draw.</param>
    /// <param name="mask">The mask texture</param>
    /// <param name="pivot">The point where this texture rotates.</param>
    public void RenderGloss(Texture gloss, Texture mask, Vector2 pivot)
    {
        patch_SolutionEditorBase.method_2450((PartRenderer)(object)this, gloss, mask, pivot);
    }
    /// <summary>
    /// Draws an functional iris.<br/>
    /// From Lithos
    /// </summary>
    /// <param name="ips">The intermediate part state from this part.</param>
    /// <param name="offset">the hex this iris is drawn to.</param>
    /// <param name="time">the mid-cycle time</param>
    /// <param name="underIris">The texture under the iris</param>
    /// <param name="irisFrames">The animation composing the iris, the last frame should be fully closed.</param>
    /// <param name="aboveIris">The texture above the iris.</param>
    /// <param name="atom">The atom to eject, or null.</param>
    /// <param name="forceOpen">make this iris open, even if <paramref name="atom"/> is null.</param>
    public void RenderIris(IntermediatePartState ips, HexIndex offset, float time, Texture underIris, Texture[] irisFrames, Texture aboveIris, AtomType atom, bool forceOpen = false)
    {
        int frame = irisFrames.Length - 1;
        Molecule risingMolecule = null;
        Vector2 risingOffset = ips.pos + HexGrid.standardGrid.ToPixelCoords(offset).Rotated(ips.rotation);
        bool afterIrisOpens = false;
        bool ejecting = false;
        if (atom is not null)
        {
            ejecting = true;
            risingMolecule = Molecule.CreateMonoatomic(atom);
        }
        ((PartRenderer)(object)this).RenderRotating(underIris, offset, Vector2.Zero);
        if (forceOpen || ejecting)
        {
            frame = Utils.Clamp((int)(Utils.InterpolateLinear(1f, -1f, time) * irisFrames.Length), 0, irisFrames.Length - 1);
            if (ejecting)
            {
                afterIrisOpens = time > 0.5;
                if (!afterIrisOpens)
                {
                    Editor.RenderMolecule(risingMolecule, risingOffset, new(0, 0), 0f, 1f, time, 1f, false, null);
                }
            }
        }
        ((PartRenderer)(object)this).RenderUpright(irisFrames[frame], offset, Vector2.Zero);
        ((PartRenderer)(object)this).RenderRotating(aboveIris, offset, Vector2.Zero);
        if (afterIrisOpens)
        {
            Editor.RenderMolecule(risingMolecule, risingOffset, new(0, 0), 0f, 1f, time, 1f, false, null);
        }
    }

    /// <summary>
    /// Draws an functional iris, this version only requires the iris animation.<br/>
    /// From Lithos
    /// </summary>
    /// <param name="ips">The intermediate part state from this part.</param>
    /// <param name="offset">the hex this iris is drawn to.</param>
    /// <param name="time">the mid-cycle time</param>
    /// <param name="irisFrames">The animation composing the iris, the last frame should be fully closed.</param>
    /// <param name="atom">The atom to eject, or null.</param>
    /// <param name="forceOpen">make this iris open, even if <paramref name="atom"/> is null.</param>
    public void RenderIris(IntermediatePartState ips, HexIndex offset, float time, Texture[] irisFrames, AtomType atom, bool forceOpen = false) => RenderIris(ips, offset, time, Assets.textures.parts.animismus.output_under_iris, irisFrames, Assets.textures.parts.animismus.output_above_iris, atom, forceOpen);

    /// <summary>
    /// Draws an functional iris, this version draws a basic iris.<br/>
    /// From Lithos
    /// </summary>
    /// <param name="ips">The intermediate part state from this part.</param>
    /// <param name="offset">the hex this iris is drawn to.</param>
    /// <param name="time">the mid-cycle time</param>
    /// <param name="atom">The atom to eject, or null.</param>
    /// <param name="forceOpen">make this iris open, even if <paramref name="atom"/> is null.</param>
    public void RenderIris(IntermediatePartState ips, HexIndex offset, float time, AtomType atom, bool forceOpen = false) => RenderIris(ips, offset, time, Assets.textures.parts.iris_full, atom, forceOpen);
}
