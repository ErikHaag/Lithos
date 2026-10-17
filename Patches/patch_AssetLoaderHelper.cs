using System.IO;
using MonoMod;

[MonoModPatch("AssetLoaderHelper")]
class patch_AssetLoaderHelper
{
    /// <summary>
    /// Loads a sequence of frames from a folder, and packages them into an array.<br/>
    /// From Lithos 
    /// </summary>
    /// <param name="frameFolder">The folder containing the frames. By convention, these folders have names ending in <em>.array</em>.</param>
    /// <param name="frameName">The prefix name of individual frames.</param>
    /// <param name="count">The amount of frames in the animation.</param>
    /// <param name="padding">The amount of characters devoted to the frame number.</param>
    /// <returns>A <see cref="Texture"/> array of the animation.</returns>
    public static Texture[] LoadAnimation(string frameFolder, string frameName, int count, int padding = 4)
    {
        Texture[] animation = new Texture[count];
        for (int i = 0; i < count; i++)
        {
            animation[i] = AssetLoaderHelper.LoadTexture(Path.Combine(frameFolder, frameName + "_" + (i + 1).ToString().PadLeft(padding)));
        }
        return animation;
    }

    /// <summary>
    /// Loads a set of lighting <see cref="Texture"/>s from a folder.<br/>
    /// From Lithos
    /// </summary>
    /// <param name="path">The folder name containing the left, right, top, and bottom lit textures. By convention, these folders have names ending in <em>.lighting</em>.</param>
    /// <returns>A <see cref="LightDirTextures"/> from the given folder.</returns>
    public static LightDirTextures LoadLightDir(string path) => new(AssetLoaderHelper.LoadTexture(Path.Combine(path, "left")), AssetLoaderHelper.LoadTexture(Path.Combine(path, "right")), AssetLoaderHelper.LoadTexture(Path.Combine(path, "bottom")), AssetLoaderHelper.LoadTexture(Path.Combine(path, "top")));
}
