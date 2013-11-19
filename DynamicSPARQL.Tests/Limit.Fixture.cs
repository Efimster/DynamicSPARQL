using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;
using Xunit.Extensions;
using Should.Fluent;

namespace DynamicSPARQLSpace.Tests
{
    public class LimitFixture
    {
        [Theory(DisplayName = "Limit & offset "),
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
                    SPARQL.Tripple("?x ns:price ?price"),
                    SPARQL.Tripple("?x dc:title ?title")
                ),
                orderBy: "desc(?price)",
                limit:1,
                offset:1
            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            ((int)list.First().price).Should().Equals(23);

        }
    
    }
}
