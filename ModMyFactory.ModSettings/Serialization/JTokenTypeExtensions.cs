using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace ModMyFactory.ModSettings.Serialization
{
    static class JTokenTypeExtensions
    {
        public static PropertyTreeType ToTreeType(this JTokenType type)
        {
            switch (type)
            {
                case JTokenType.Object: return PropertyTreeType.Dictionary;
                case JTokenType.Array: return PropertyTreeType.List;
                case JTokenType.Integer: return PropertyTreeType.Number;
                case JTokenType.Float: return PropertyTreeType.Number;
                case JTokenType.String: return PropertyTreeType.String;
                case JTokenType.Boolean: return PropertyTreeType.Bool;
                default: throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(JTokenType));
            }
        }
    }
}
