namespace ModMyFactory.Mods
{
    public static class ModFamilyExtensions
    {
        /// <summary>
        /// Gets the default mod in this family.
        /// The default mod is the mod that will be selected by Factorio if no version is specified.
        /// </summary>
        public static Mod GetDefaultMod(this ModFamily family)
        {
            Mod max = null;
            foreach (var mod in family)
                if ((max is null) || (mod.Version > max.Version)) max = mod;
            return max;
        }
    }
}
