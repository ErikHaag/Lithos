using Quintessential;

namespace Lithos;

public class LithosMod : QuintessentialMod, IDataMod {
    public static LithosMod Instance { get; }
    public override string ModId => "lithos";
    internal static AtomType Invisitite;

    public override void Load() {
        Instance.Log.Info("Lithos: The world is shaped to your will, make it comfortable.");
    }

    public override void LoadContent() {
        Invisitite = new AtomType()
        {
            symbol = Assets.textures.transparent,
            matteTextures = new() {
                diffuse = Assets.textures.transparent,
                shade = Assets.textures.transparent
            },
            shadow = Assets.textures.transparent
        };
        QApi.AddAtomType(Instance, Invisitite, "invisitite");
        // remove from the list, to prevent an undesirable gap.
        QApi.ModAtomTypes.Remove(Invisitite);
    }
    public override void LoadCompatContent() {
    }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
	public override void Unload() { }
}
