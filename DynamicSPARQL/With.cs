
namespace DynamicSPARQLSpace
{
    public class With : ClauseBase
    {
        public override string Clause { get { return "WITH"; } }
        public With(string iri) : base(iri) { }

    }
}
