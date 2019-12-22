using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace ModMyFactoryGUI.Localization
{
    class LocalizedResourceExtension : BindingExtension
    {
        readonly LocalizedResource _resource;

        [ConstructorArgument("key")]
        public string Key
        {
            get => _resource.Key;
            set => _resource.Key = value;
        }

        public LocalizedResourceExtension(string key)
            : base("Value")
        {
            _resource = new LocalizedResource(key);
            Source = _resource;
        }
    }
}
