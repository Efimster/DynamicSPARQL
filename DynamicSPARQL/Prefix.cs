using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class Prefix
    {
        public string PREFIX { get; set; }
        public string IRI { get; set; }

        public StringBuilder AppendToString(StringBuilder sb)
        {
            return sb.AppendLine(string.Concat("PREFIX ", PREFIX, " <", IRI, ">"));
        }
    }
}
