using Quintessential;

namespace Lithos;

internal class LithosMod : QuintessentialMod, IDataMod {
    public static LithosMod Instance { get; }
    public override string ModId => "lithos";

    public override void Load() {
        Logger.Log("Lithos: The world is shaped to your will, make it comfortable.");
    }

    public override void LoadContent() {}
    public override void LoadCompatContent() {
    }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
	public override void Unload() { }
}
