using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Should.Fluent;

namespace DynamicSPARQLSpace.Tests
{
    public class Path
    {
        [Theory(DisplayName = "Property Paths and Equivalent Patterns "),
         Xunit.Trait("SPARQL Query", ""),
         InlineData(@"@prefix :       <http://example/> .
            :order  :item :z1 .
            :order  :item :z2 .

            :z1 :name ""Small"" .
            :z1 :price 5 .

            :z2 :name ""Large"" .
            :z2 :price 5 .")]
        public void TestPath1(string data)
        {
            var dyno = TestDataProvider.GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("", "http://example/")
                },
                projection: "sum(?x) AS ?total",
                where: SPARQL.Triple("?s :item/:price ?x")
            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            ((int)list.First().total).Should().Equal(10);

        }

        [Theory(DisplayName = "Property Paths with triple chain "),
         Xunit.Trait("SPARQL Query", ""),
         InlineData(@"@prefix :       <http://example/> .
                    :order  :item :z1 .
                    :order  :item :z2 .

                    :z1 :name ""Small"" .
                    :z1 :price 5 .

                    :z2 :name ""Large"" .
                    :z2 :price 5 .")]
        public void TestPath2(string data)
        {
            var dyno = TestDataProvider.GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("", "http://example/")
                },
                projection: "*",
                where: SPARQL.TripleChain("?order :item  ?item :price ?price")
            );

            var list = res.ToList();
            list.Count.Should().Equal(2);

            list.Where(x => x.item == "z1" && x.price == 5).Count().Should().Equal(1);
            list.Where(x => x.item == "z2" && x.price == 5).Count().Should().Equal(1);
        }
    }
}
