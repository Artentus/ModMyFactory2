using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ModMyFactory
{
    /// <summary>
    /// ModMyFactory version implementation.
    /// </summary>
    public sealed class CustomVersion : IComparable<CustomVersion>, IComparable, IEquatable<CustomVersion>
    {
        /// <summary>
        /// Main product version.
        /// Different main versions indicate broken compatibility.
        /// </summary>
        public int MainVersion { get; }

        /// <summary>
        /// Major product version.
        /// Indicates milestones.
        /// </summary>
        public int MajorVersion { get; }

        /// <summary>
        /// Minor product version.
        /// Indicates feature additions.
        /// </summary>
        public int MinorVersion { get; }

        /// <summary>
        /// Product revision.
        /// Indicates bugfixes, small feature changes and optimizations.
        /// </summary>
        public int Revision { get; }

        /// <summary>
        /// Build number within the revision.
        /// </summary>
        public int Build { get; }

        /// <summary>
        /// The build cycle of the product.
        /// Indicates the stage of development the product is in.
        /// </summary>
        public VersionCycle Cycle { get; }

        /// <summary>
        /// The current build branch.
        /// Indicates the builds sutability for end users.
        /// This value is purely informational and doesn't affect comparison.
        /// </summary>
        public VersionBranch Branch { get; }

        public CustomVersion(int main, int major, int minor, int revision, int build, VersionCycle cycle = VersionCycle.Release, VersionBranch branch = VersionBranch.Stable)
        {
            MainVersion = main;
            MajorVersion = major;
            MinorVersion = minor;
            Revision = revision;
            Build = build;
            Cycle = cycle;
            Branch = branch;
        }

        public int CompareTo(CustomVersion other)
        {
            if (other is null) return int.MaxValue;

            int result = this.MainVersion.CompareTo(other.MainVersion);
            if (result == 0)
            {
                result = ((int)this.Cycle).CompareTo((int)other.Cycle);
                if (result == 0)
                {
                    result = this.MajorVersion.CompareTo(other.MajorVersion);
                    if (result == 0)
                    {
                        result = this.MinorVersion.CompareTo(other.MinorVersion);
                        if (result == 0)
                        {
                            result = this.Revision.CompareTo(other.Revision);
                            if (result == 0) result = this.Build.CompareTo(other.Build);
                        }
                    }
                }
            }
            return result;
        }

        public int CompareTo(object obj)
        {
            if (obj is CustomVersion other) return CompareTo(other);
            else if (obj is null) return int.MaxValue;
            else throw new ArgumentException("Value of type CustomVersion expected.", nameof(obj));
        }

        public bool Equals(CustomVersion other) => CompareTo(other) == 0;

        public override bool Equals(object obj)
        {
            if (obj is CustomVersion other) return Equals(other);
            else  return false;
        }

        public override int GetHashCode()
            => MainVersion.GetHashCode() ^ MajorVersion.GetHashCode() ^ MinorVersion.GetHashCode()
                ^ Revision.GetHashCode() ^ Build.GetHashCode() ^ Cycle.GetHashCode();

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Cycle != VersionCycle.Release)
                sb.AppendFormat("{0} ", Cycle);

            sb.Append(MainVersion);
            sb.Append('.');
            sb.Append(MajorVersion);
            sb.Append('.');
            sb.Append(MinorVersion);

            if (Revision != 0)
            {
                sb.Append('.');
                sb.Append(Revision);
            }

            if (Build != 0)
                sb.AppendFormat(" Build {0}", Build);

            if (Branch != VersionBranch.Stable)
                sb.AppendFormat(" ({0})", Branch.ToString().ToLower());

            return sb.ToString();
        }

        public static bool TryParse(string value, out CustomVersion result)
        {
            result = null;
            value = value.Trim();

            var cycle = VersionCycle.Release;
            foreach (var c in Enum.GetNames(typeof(VersionCycle)))
            {
                if (value.StartsWith(c, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        cycle = (VersionCycle)Enum.Parse(typeof(VersionCycle), c, true);
                        value = value.Substring(c.Length).TrimStart();
                        break;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            var branch = VersionBranch.Stable;
            foreach (var b in Enum.GetNames(typeof(VersionBranch)))
            {
                if (value.EndsWith("(" + b + ")", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        branch = (VersionBranch)Enum.Parse(typeof(VersionBranch), b, true);
                        value = value.Substring(0, value.Length - b.Length - 2).TrimEnd();
                        break;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            int build = 0;
            var parts = Regex.Split(value, "build", RegexOptions.IgnoreCase);
            if (parts.Length > 2) return false;
            if ((parts.Length == 2) && !int.TryParse(parts[1].TrimStart(), out build)) return false;

            if (!Version.TryParse(parts[0].TrimEnd(), out var v)) return false;
            else
            {
                result = new CustomVersion(v.Major, v.Minor, v.Build, v.Revision, build, cycle, branch);
                return true;
            }
        }

        public static CustomVersion Parse(string value)
        {
            if (TryParse(value, out var result)) return result;
            else throw new FormatException();
        }
    }
}
