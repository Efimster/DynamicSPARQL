using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class Graph : Group, IWhereItem
    {
        public string IRI { get; set; }

        public Graph(string iri, params IWhereItem[] items) : base(items) 
        {
            IRI = iri.GetIRIType() == Utilities.IRIType.FullUnbracketed ? string.Concat("<", iri, ">") : iri;
        }
        

        public new WhreItemType ItemType { get { return WhreItemType.Graph; } }

        public override StringBuilder AppendToString(StringBuilder sb,bool autoQuotation = false,
            bool skipTriplesWithEmptyObject = false, bool mindAsterisk = false)
        {
            bool addBrackets = sb.Length == 0 ? true : false;
            if (addBrackets)
                sb.AppendLine("{");

            sb.Append("GRAPH ");
            sb.Append(IRI);
            
            base.AppendToString(sb, autoQuotation, skipTriplesWithEmptyObject, mindAsterisk);

            if (addBrackets)
                sb.AppendLine("}");

            return sb;


        }
    }
}
