using System.Collections.Generic;
using System.Linq;
using Should.Fluent;
using Xunit.Extensions;

namespace DynamicSPARQLSpace.Tests
{
    public class LimitFixture
    {
        [Theory(DisplayName = "Limit & offset "),
         Xunit.Trait("SPARQL Query", ""),
         InlineData(@"@prefix dc:   <http://purl.org/dc/elements/1.1/> .
                @prefix :     <http://example.org/book/> .
                @prefix ns:   <http://example.org/ns#> .

                :book1  dc:title  ""SPARQL Tutorial"" .
                :book1  ns:price  42 .
                :book2  dc:title  ""The Semantic Web"" .
                :book2  ns:price  23 .")]
        public void TestLimit1(string data)
        {
            var dyno = TestDataProvider.GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("dc:", "http://purl.org/dc/elements/1.1/"),
                    SPARQL.Prefix("ns:", "http://example.org/ns#"),
                },
                projection: "?title ?price",
                where: SPARQL.Group(
                    SPARQL.Triple("?x ns:price ?price"),
                    SPARQL.Triple("?x dc:title ?title")
                ),
                orderBy: "desc(?price)",
                limit:1,
                offset:1
            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            ((int)list.First().price).Should().Equal(23);

        }
    
    }
}
