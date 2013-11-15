using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class Bind : IWhereItem
    {
        public string BIND { get; set; }

        public WhreItemType ItemType
        {
            get { return WhreItemType.Bind; }
        }

        public StringBuilder AppendToString(StringBuilder sb, bool autoQuotation = false)
        {
            string str = BIND;
            return sb.AppendLine(Regex.IsMatch(BIND, @"\(([^)]*)\)$") ? string.Concat("BIND ", str, " .") : string.Concat("BIND (", str, ") ."));
        }
    }
}
