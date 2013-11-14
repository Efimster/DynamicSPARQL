using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class Filter : IWhereItem
    {

        public string FILTER { get; set; }

        public WhreItemType ItemType
        {
            get { return WhreItemType.Filter; }
        }

        public StringBuilder AppendToString(StringBuilder sb, bool autoQuotation = false)
        {
            string str = autoQuotation ? FILTER.AutoquoteSPARQL() : FILTER;
            return sb.AppendLine(Regex.IsMatch(FILTER, @"\(([^)]*)\)$") ? string.Concat("FILTER ", str, " .") : string.Concat("FILTER (", str, ") ."));
        }
    }
}
