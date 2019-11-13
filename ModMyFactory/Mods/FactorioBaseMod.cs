using ModMyFactory.Game;

namespace ModMyFactory.Mods
{
    class FactorioBaseMod : Mod
    {
        public override bool CanDisable => false;

        public FactorioBaseMod(IFactorioInstance instance)
            : base(instance.BaseMod.Info.Name, instance.BaseMod.Info.DisplayName, instance.BaseMod.Info.Version, instance.BaseMod.Info.Version.ToMajor(),
                  instance.BaseMod.Info.Author, instance.BaseMod.Info.Description, instance.BaseMod.Info.Dependencies, instance.BaseMod.Thumbnail)
        { }
    }
}
