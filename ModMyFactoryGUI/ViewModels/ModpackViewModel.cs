using ModMyFactory;
using ReactiveUI;
using System;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class ModpackViewModel : ReactiveObject, IDisposable
    {
        public Modpack Modpack { get; }

        public string DisplayName
        {
            get => Modpack.DisplayName;
            set
            {
                if (value != Modpack.DisplayName)
                {
                    Modpack.DisplayName = value;
                    this.RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }

        public bool? Enabled
        {
            get => Modpack.Enabled;
            set => Modpack.Enabled = value;
        }

        public ModpackViewModel(Modpack modpack)
        {
            Modpack = modpack;
            modpack.EnabledChanged += ModpackEnabledChangedHandler;
        }

        private void ModpackEnabledChangedHandler(object sender, EventArgs e)
            => this.RaisePropertyChanged(nameof(Enabled));

        #region IDisposable Support

        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    Modpack.EnabledChanged -= ModpackEnabledChangedHandler;

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}
