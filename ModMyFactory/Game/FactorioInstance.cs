namespace ModMyFactory.Game
{
    public static class FactorioInstance
    {
        /// <summary>
        /// Converts this Factorio instance into a managed instance.
        /// If the instance is already managed it will be returned unaltered.
        /// </summary>
        public static ManagedFactorioInstance ToManaged(this IFactorioInstance instance) => ManagedFactorioInstance.FromInstance(instance);
    }
}
