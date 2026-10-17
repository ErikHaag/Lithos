using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;

[MonoModPatch("SolutionEditorBase")]
class patch_SolutionEditorBase
{
    [MonoModIgnore]
    extern public static void method_2450(PartRenderer param_8686, Texture param_8687, Texture param_8688, Vector2 param_8689);

    //public static void DrawFallingAtom()
}
