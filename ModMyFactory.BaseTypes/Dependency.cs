//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace ModMyFactory.BaseTypes
{
    /// <summary>
    /// Defines a dependency between mods
    /// </summary>
    [JsonConverter(typeof(DependencyConverter))]
    public class Dependency : IEquatable<Dependency>
    {
        /// <summary>
        /// The type of the dependency
        /// </summary>
        public DependencyType Type { get; }

        /// <summary>
        /// The name of the mod the dependency is referencing
        /// </summary>
        public string ModName { get; }

        /// <summary>
        /// The comparison function of this dependency
        /// </summary>
        public DependencyComparison Comparison { get; }

        /// <summary>
        /// The version to compare the mod against using the comparison function
        /// </summary>
        public AccurateVersion CompareVersion { get; }

        public Dependency(DependencyType type, string modName, DependencyComparison comparison, AccurateVersion compareVersion)
            => (Type, ModName, Comparison, CompareVersion) = (type, modName, comparison, compareVersion);

        /// <summary>
        /// Determines if a given mod matches this dependency
        /// </summary>
        protected virtual bool Matches(string modName, AccurateVersion modVersion)
        {
            if (string.Equals(modName, ModName, StringComparison.InvariantCulture)) return false;
            return Comparison.TestFor(modVersion, CompareVersion);
        }

        public static bool TryParse(string? value, [NotNullWhen(true)] out Dependency? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(value)) return false;

            value = value.Trim();
            var type = DependencyType.Normal;
            if (value[0] == '!')
            {
                type = DependencyType.Inverted;
                value = value[1..].TrimStart();
            }
            else if (value[0] == '?')
            {
                type = DependencyType.Optional;
                value = value[1..].TrimStart();
            }
            else if (value.StartsWith("(?)"))
            {
                type = DependencyType.Hidden;
                value = value[3..].TrimStart();
            }
            if (string.IsNullOrWhiteSpace(value)) return false;

            const string pattern = @"\A(?<name>[a-zA-Z0-9_\-\s\.]+)(?:\s*(?<op><=|>=|<|>|=)\s*(?<ver>\d+(?:\.\d+){0,3}))?\Z";
            var match = Regex.Match(value, pattern);
            if (!match.Success) return false;

            string name = match.Groups["name"].Value.TrimEnd();
            DependencyComparison comp;
            AccurateVersion version = default;

            var opG = match.Groups["op"];
            if (opG.Success)
            {
                comp = new DependencyComparison(opG.Value);
                version = AccurateVersion.Parse(match.Groups["ver"].Value);
            }
            else comp = new DependencyComparison(DependencyOperator.None);

            result = new Dependency(type, name, comp, version);
            return true;
        }

        public static Dependency Parse(string? value)
        {
            if (TryParse(value, out var result)) return result;
            else throw new FormatException();
        }

        /// <summary>
        /// Checks if a given mod satisfies this dependency<br/>
        /// If the dependency is inverted, a mod is assumed to satisfy it if it does NOT match the dependency
        /// </summary>
        /// <param name="modName">The name of the mod</param>
        /// <param name="modVersion">The version of the mod</param>
        public bool IsSatisfiedBy(string modName, AccurateVersion modVersion)
        {
            if (Type == DependencyType.Inverted) return !Matches(modName, modVersion);
            else return Matches(modName, modVersion);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            switch (Type)
            {
                case DependencyType.Inverted:
                    sb.Append('!');
                    break;

                case DependencyType.Optional:
                    sb.Append("? ");
                    break;

                case DependencyType.Hidden:
                    sb.Append("(?) ");
                    break;
            }

            sb.Append(ModName);

            if (Comparison.Operator != DependencyOperator.None)
                sb.AppendFormat(" {0} {1}", Comparison.Operator, CompareVersion);

            return sb.ToString();
        }

        public bool Equals(Dependency? other)
        {
            if (other is null) return false;

            string opA = this.Comparison.Operator;
            string opB = other.Comparison.Operator;
            return (this.Type, this.ModName, opA) == (other.Type, other.ModName, opB)
                && ((opA == DependencyOperator.None) || (this.CompareVersion == other.CompareVersion)); // If no comparison, version doesn't matter
        }

        public override bool Equals(object? obj) => Equals(obj as Dependency);

        public override int GetHashCode()
        {
            string op = Comparison.Operator;
            int hash = Type.GetHashCode() ^ ModName.GetHashCode() ^ op.GetHashCode();
            if (op != DependencyOperator.None) hash ^= CompareVersion.GetHashCode();
            return hash;
        }
    }
}
