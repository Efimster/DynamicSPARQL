
using System.Collections.Generic;
namespace DynamicSPARQLSpace
{
    public class UsingNamed : ClauseBase
    {
        public override string Clause { get { return "USING NAMED"; } }
        public UsingNamed(string iri) : base(iri) { }
        public static IEnumerable<UsingNamed> Parse(object usingNamed)
        {
            var str = usingNamed as string;
            if (str != null)
            {
                return new List<UsingNamed>(1) { new UsingNamed(str) };
            }

            var list = usingNamed as IEnumerable<string>;
            if (list != null)
            {
                var result = new List<UsingNamed>();
                foreach (var item in list)
                {
                    result.Add(new UsingNamed(item));
                }
                result.TrimExcess();
                return result;
            }

            return null;
        }

    }
}
