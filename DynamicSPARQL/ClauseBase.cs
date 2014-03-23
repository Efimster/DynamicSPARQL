using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public abstract class ClauseBase
    {
        public abstract string Clause { get; }
        public string IRI { get; protected set; }

        public ClauseBase(string iri)
        {
            IRI = iri.GetIRIType() == Utilities.IRIType.FullUnbracketed ? string.Concat("<", iri, ">") : iri;
        }

        

        public override string ToString()
        {
            if (string.IsNullOrEmpty(IRI))
                return string.Empty;

            return string.Concat(Clause, " ", IRI, Environment.NewLine);
        }

        public static string Collection2String(IEnumerable<ClauseBase> list)
        {
            if (list == null)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var item in list)
            {
                var str = item.ToString();
                if (!string.IsNullOrEmpty(str))
                    sb.Append(str);
            }

            return sb.ToString();
        }
    }
}
