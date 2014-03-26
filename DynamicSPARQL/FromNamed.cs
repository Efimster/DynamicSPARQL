using System.Collections.Generic;

namespace DynamicSPARQLSpace
{
    public class FromNamed : From
    {
        public override string Clause { get { return "FROM NAMED"; } } 
        public FromNamed(string iri) : base(iri)
        {}

        public static IEnumerable<FromNamed> Parse(object fromNamed)
        {
            var str = fromNamed as string;
            if (str!=null)
            {
                return new List<FromNamed>(1) { new FromNamed(str) };
            }
            
            var list = fromNamed as IEnumerable<string>;
            if (list!= null)
            {
                var result = new List<FromNamed>();
                foreach (var item in list)
                {
                    result.Add(new FromNamed(item));
                }
                result.TrimExcess();
                return result;
            }

            return null;
        }


  
    }


}
