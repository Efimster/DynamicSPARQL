using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class NotExists : Group, IWhereItem
    {
        public NotExists() : base() { }
        public NotExists(params IWhereItem[] items) : base(items) { }

        public new WhreItemType ItemType { get { return WhreItemType.NotExists; } }

        public override StringBuilder AppendToString(StringBuilder sb, bool autoQuotation = false)
        {
            sb.Append("FILTER NOT EXISTS ");
            return base.AppendToString(sb, autoQuotation);
        }
    }
}
