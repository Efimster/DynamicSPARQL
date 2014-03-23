
namespace DynamicSPARQLSpace
{
    public class From : ClauseBase
    {
        public override string Clause { get { return "FROM"; } } 

        public From(string iri):base(iri)
        {}
    }
}
