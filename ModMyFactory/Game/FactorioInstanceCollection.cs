using System.Collections.ObjectModel;

namespace ModMyFactory.Game
{
    public sealed class FactorioInstanceCollection : ReadOnlyObservableCollection<ManagedFactorioInstance>
    {
        public FactorioInstanceCollection(ObservableCollection<ManagedFactorioInstance> list)
            : base(list)
        { }
    }
}
