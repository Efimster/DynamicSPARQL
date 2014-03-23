
namespace DynamicSPARQLSpace
{
    public class Using : ClauseBase
    {
        public override string Clause { get { return "USING"; } } 
        public Using(string iri) : base(iri) {}

    }
}
