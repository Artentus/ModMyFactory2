//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using System;
using System.Collections.Generic;
using System.IO;

namespace ModMyFactory.Export
{
    /// <summary>
    /// Sets up an Exporter
    /// </summary>
    public sealed class ExporterFactory
    {
        /// <summary>
        /// List of mod definitions to export
        /// </summary>
        public List<ModDefinition> ModDefinitions { get; }

        /// <summary>
        /// List of modpack definitions to export
        /// </summary>
        public List<ModpackDefinition> ModpackDefinitions { get; }

        /// <summary>
        /// Whether the Exporter should be forced to create an FMPA file even when not required
        /// </summary>
        public bool Forcepack { get; set; }

        /// <summary>
        /// List of files to pack into the resulting FMPA file
        /// Adding files to this list will cause the Exporter to always generate an FMPA file
        /// </summary>
        public List<FileInfo> FilesToPack { get; }

        public ExporterFactory()
        {
            ModDefinitions = new List<ModDefinition>();
            ModpackDefinitions = new List<ModpackDefinition>();
            FilesToPack = new List<FileInfo>();
        }

        private void AssertNoDuplicateIds()
        {
            // We only check mod IDs here since the modpack keys are covered by the topological search already
            var set = new HashSet<int>();
            foreach (var modDef in ModDefinitions)
            {
                int id = modDef.Uid;
                if (set.Contains(id))
                    throw new InvalidOperationException("Multiple mod definitions with same ID in the list");
                set.Add(id);
            }
        }

        /// <summary>
        /// Creates an Exporter based on the current state of the factory
        /// Can be called multiple times to generate different Exporters
        /// </summary>
        public Exporter CreateExporter()
        {
            AssertNoDuplicateIds();

            bool pack = Forcepack || (FilesToPack.Count > 0);
            var sorted = TopologicalSort(ModpackDefinitions);
            var package = new Package(2, ModDefinitions, sorted);
            return new Exporter(package, pack, FilesToPack);
        }

        #region Topological Sort

        private static readonly Dictionary<int, int> _idIndexMappings = new Dictionary<int, int>();

        private static void TopologicalSortRec(in IList<ModpackDefinition> modpacks, in int index, in bool[] visited, in List<ModpackDefinition> result)
        {
            if (!visited[index])
            {
                visited[index] = true;

                foreach (var id in modpacks[index].ModpackIds)
                {
                    int i = _idIndexMappings[id];
                    TopologicalSortRec(modpacks, i, visited, result);
                }

                result.Add(modpacks[index]);
            }
        }

        private static IReadOnlyList<ModpackDefinition> TopologicalSort(in IList<ModpackDefinition> modpacks)
        {
            // We build this mapping once at the beginning to reduce time complexity
            _idIndexMappings.Clear();
            for (int i = 0; i < modpacks.Count; i++)
            {
                int id = modpacks[i].Uid;
                if (_idIndexMappings.ContainsKey(id)) // We can also use this to check for duplicate IDs
                    throw new InvalidOperationException("Multiple modpack definitions with same ID in the list");
                _idIndexMappings.Add(id, i);
            }

            var result = new List<ModpackDefinition>(modpacks.Count);
            var visited = new bool[modpacks.Count];

            for (int i = 0; i < modpacks.Count; i++)
                TopologicalSortRec(modpacks, i, visited, result);

            return result;
        }

        #endregion Topological Sort
    }
}
