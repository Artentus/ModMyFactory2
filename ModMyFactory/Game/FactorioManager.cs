using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ModMyFactory.Game
{
    /// <summary>
    /// Manages Factorio instances
    /// </summary>
    public class FactorioManager : IEnumerable<FactorioInstance>
    {
        readonly Dictionary<Guid, FactorioInstance> _instances;

        private FactorioManager(FileInfo configFile)
        {

        }

        public Guid Add(FactorioInstance instance)
        {
            
        }

        public IEnumerator<FactorioInstance> GetEnumerator() => _instances.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
