using ModMyFactory.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace ModMyFactory
{
    /// <summary>
    /// Manages Factorio instances and their mods.
    /// </summary>
    public sealed class Manager : IEnumerable<ManagedFactorioInstance>
    {
        readonly List<ManagedFactorioInstance> _managedInstances;
        DirectoryInfo _modDirectory;

        /// <summary>
        /// The list of instances being managed.
        /// </summary>
        public IReadOnlyList<ManagedFactorioInstance> ManagedInstances { get; }

        /// <summary>
        /// The managed mod directory.
        /// </summary>
        public DirectoryInfo ModDirectory
        {
            get => _modDirectory;
            set
            {
                if (!string.Equals(_modDirectory.FullName, value.FullName, StringComparison.InvariantCultureIgnoreCase))
                {
                    _modDirectory = value;
                    // TODO: relink all instances
                }
            }
        }

        public Manager(DirectoryInfo modDirectory)
        {
            _managedInstances = new List<ManagedFactorioInstance>();
            ManagedInstances = new ReadOnlyCollection<ManagedFactorioInstance>(_managedInstances);
            _modDirectory = modDirectory;
        }

        /// <summary>
        /// Adds a Factorio instance to the manager.
        /// </summary>
        /// <param name="instance">The instance to add.</param>
        public void AddInstance(ManagedFactorioInstance instance)
        {
            instance.HasManagerAttached = true;
            _managedInstances.Add(instance);
            // TODO: link instance
        }

        /// <summary>
        /// Adds a Factorio instance to the manager.
        /// </summary>
        /// <param name="instance">The instance to add.</param>
        public ManagedFactorioInstance AddInstance(IFactorioInstance instance)
        {
            var managedInstance = instance.ToManaged();
            AddInstance(managedInstance);
            return managedInstance;
        }

        /// <summary>
        /// Removes a Factorio instance from the manager.
        /// </summary>
        /// <param name="instance">The instance to remove.</param>
        public void RemoveInstance(ManagedFactorioInstance instance)
        {
            _managedInstances.Remove(instance);
            instance.HasManagerAttached = false;
            // TODO: unlink instance
        }

        public IEnumerator<ManagedFactorioInstance> GetEnumerator() => _managedInstances.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
