using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class Minus : Group, IWhereItem
    {
        public Minus() : base() { }
        public Minus(params IWhereItem[] items) : base(items) { }

        public new WhreItemType ItemType { get { return WhreItemType.Minus; } }

        public override StringBuilder AppendToString(StringBuilder sb,bool autoQuotation = false)
        {
            sb.Append("MINUS ");
            return base.AppendToString(sb, autoQuotation);
        }
    }
}
