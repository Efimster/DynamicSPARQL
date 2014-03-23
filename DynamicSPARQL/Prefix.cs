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

        public override string ToString()
        {
            return string.Concat("PREFIX ", PREFIX, " <", IRI, ">");
        }

        public static string Collection2String(IEnumerable<Prefix> list)
        {
            if (list == null)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var prefix in list)
            {
                var str = prefix.ToString();
                if (!string.IsNullOrEmpty(str))
                    sb.AppendLine(str);
            }

            return sb.ToString();
        }
    }
}
