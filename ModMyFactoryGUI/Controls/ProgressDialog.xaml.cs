using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModMyFactoryGUI.Controls
{
    internal class ProgressDialog : DialogBase
    {
        private double _minimum, _maximum, _value;
        private ICommand _cancelCommand;

        public static readonly DirectProperty<ProgressDialog, double> MinimumProperty
            = RangeBase.MinimumProperty.AddOwner<ProgressDialog>(d => d.Minimum, (d, v) => d.Minimum = v);

        public static readonly DirectProperty<ProgressDialog, double> MaximumProperty
            = RangeBase.MaximumProperty.AddOwner<ProgressDialog>(d => d.Maximum, (d, v) => d.Maximum = v);

        public static readonly DirectProperty<ProgressDialog, double> ValueProperty
            = RangeBase.ValueProperty.AddOwner<ProgressDialog>(d => d.Value, (d, v) => d.Value = v);

        public static readonly StyledProperty<bool> IsIndeterminateProperty
            = ProgressBar.IsIndeterminateProperty.AddOwner<ProgressDialog>();

        public static readonly DirectProperty<ProgressDialog, ICommand> CancelCommandProperty
            = AvaloniaProperty.RegisterDirect<ProgressDialog, ICommand>(nameof(CancelCommand), d => d.CancelCommand, (d, v) => d.CancelCommand = v);

        public double Minimum
        {
            get => _minimum;
            set => SetAndRaise(MinimumProperty, ref _minimum, Math.Min(value, _maximum));
        }

        public double Maximum
        {
            get => _maximum;
            set => SetAndRaise(MaximumProperty, ref _maximum, Math.Max(value, _minimum));
        }

        public double Value
        {
            get => _value;
            set => SetAndRaise(ValueProperty, ref _value, Math.Min(Math.Max(value, Minimum), Maximum));
        }

        public bool IsIndeterminate
        {
            get => GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public ICommand CancelCommand
        {
            get => _cancelCommand;
            set => SetAndRaise(CancelCommandProperty, ref _cancelCommand, value);
        }

        public ProgressDialog()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Helper functions to make displaying the dialog for some operation a little easier
        public static async Task<DialogResult> Show(
            Task operation, Progress<double> progress, double min, double max,
            CancellationTokenSource cancellationSource, Window owner)
        {
            // Set up the dialog
            var dialog = new ProgressDialog { Minimum = min, Maximum = max };
            dialog.CancelCommand = ReactiveCommand.Create(cancellationSource.Cancel);

            // Use inner function instead of lambda so we can unsubscribe
            void OnProgressChanged(object _, double p)
                => dialog.Value = p;
            progress.ProgressChanged += OnProgressChanged;

            // Show dialog but don't wait yet
            var dialogTask = dialog.ShowDialog(owner);

            // Await the operation
            await operation;

            // Unsubscribe
            progress.ProgressChanged -= OnProgressChanged;

            // Close dialog and wait for it to finish
            dialog.Close();
            await dialogTask;

            if (cancellationSource.IsCancellationRequested) return DialogResult.Cancel;
            else return DialogResult.None;
        }

        public static async Task<DialogResult> Show(
            Task operation, Progress<double> progress, double min, double max, Window owner)
        {
            // Set up the dialog
            var dialog = new ProgressDialog { Minimum = min, Maximum = max };

            // Use inner function instead of lambda so we can unsubscribe
            void OnProgressChanged(object _, double p)
                => dialog.Value = p;
            progress.ProgressChanged += OnProgressChanged;

            // Show dialog but don't wait yet
            var dialogTask = dialog.ShowDialog(owner);

            // Await the operation
            await operation;

            // Unsubscribe
            progress.ProgressChanged -= OnProgressChanged;

            // Close dialog and wait for it to finish
            dialog.Close();
            await dialogTask;

            return DialogResult.None;
        }

        public static async Task<DialogResult> Show(
            Task operation, CancellationTokenSource cancellationSource, Window owner)
        {
            // Set up the dialog
            var dialog = new ProgressDialog { IsIndeterminate = true };
            dialog.CancelCommand = ReactiveCommand.Create(cancellationSource.Cancel);

            // Show dialog but don't wait yet
            var dialogTask = dialog.ShowDialog(owner);

            // Await the operation
            await operation;

            // Close dialog and wait for it to finish
            dialog.Close();
            await dialogTask;

            if (cancellationSource.IsCancellationRequested) return DialogResult.Cancel;
            else return DialogResult.None;
        }

        public static async Task<DialogResult> Show(
            Task operation, Window owner)
        {
            // Set up the dialog
            var dialog = new ProgressDialog { IsIndeterminate = true };

            // Show dialog but don't wait yet
            var dialogTask = dialog.ShowDialog(owner);

            // Await the operation
            await operation;

            // Close dialog and wait for it to finish
            dialog.Close();
            await dialogTask;

            return DialogResult.None;
        }
    }
}
