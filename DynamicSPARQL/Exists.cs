using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class Exists : Group, IWhereItem
    {
        public Exists() : base() { }
        public Exists(params IWhereItem[] items) : base(items) { }

        public new WhreItemType ItemType { get { return WhreItemType.Exists; } }

        public override StringBuilder AppendToString(StringBuilder sb, bool autoQuotation = false,
            bool skipTriplesWithEmptyObject = false, bool mindAsterisk = false)
        {
            sb.Append("FILTER EXISTS ");
            return base.AppendToString(sb, autoQuotation, skipTriplesWithEmptyObject, mindAsterisk);
        }
    }
}
