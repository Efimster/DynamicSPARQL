using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class Optional : Group, IWhereItem
    {
        public Optional() : base() { }
        public Optional(params IWhereItem[] items):base(items){}

        public new WhreItemType ItemType { get { return WhreItemType.Optional; } }

        public override StringBuilder AppendToString(StringBuilder sb,bool autoQuotation = false)
        {
            sb.Append("OPTIONAL ");
            return base.AppendToString(sb, autoQuotation);
        }
    }
}
