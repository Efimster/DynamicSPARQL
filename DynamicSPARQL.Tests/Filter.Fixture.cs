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
    public class FilterFixture
    {
        private static dynamic GetDyno(string data, bool autoquotation = true)
        {
            var graph = new Graph();
            graph.LoadFromString(data);

            Func<string, SparqlResultSet> sendSPARQLQuery = xquery => graph.ExecuteQuery(xquery) as SparqlResultSet;
            dynamic dyno = DynamicSPARQL.CreateDyno(sendSPARQLQuery, autoquotation);

            return dyno;
        }

        [Theory(DisplayName = "Filtering(Restricting the Value of Strings) "),
            InlineData(@"@prefix dc:   <http://purl.org/dc/elements/1.1/> .
                @prefix :     <http://example.org/book/> .
                @prefix ns:   <http://example.org/ns#> .

                :book1  dc:title  ""SPARQL Tutorial"" .
                :book1  ns:price  42 .
                :book2  dc:title  ""The Semantic Web"" .
                :book2  ns:price  23 .")]
        public void TestFilter1(string data)
        {
            var dyno = GetDyno(data, autoquotation:false);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("dc:", "http://purl.org/dc/elements/1.1/"),
                },
                projection: "?title",
                where: SPARQL.Group(
                    SPARQL.Tripple("?x dc:title ?title"),
                    SPARQL.Filter("regex(?title, \"^SPARQL\")")
                )
            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            list.Any(x => x.title == "SPARQL Tutorial").Should().Be.True();

            dyno = GetDyno(data, autoquotation: true);
            
            res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("dc:", "http://purl.org/dc/elements/1.1/"),
                },
                projection: "?title",
                where: SPARQL.Group(
                    SPARQL.Tripple("?x dc:title ?title"),
                    SPARQL.Filter("regex(?title, web, i ) ")
                )
            );

            list = res.ToList();
            list.Count.Should().Equal(1);
            list.Any(x => x.title == "The Semantic Web").Should().Be.True();

        }

        [Theory(DisplayName = "Filtering(Restricting Numeric Values) "),
            InlineData(@"@prefix dc:   <http://purl.org/dc/elements/1.1/> .
                @prefix :     <http://example.org/book/> .
                @prefix ns:   <http://example.org/ns#> .

                :book1  dc:title  ""SPARQL Tutorial"" .
                :book1  ns:price  42 .
                :book2  dc:title  ""The Semantic Web"" .
                :book2  ns:price  23 .")]
        public void TestFilter2(string data)
        {
            var dyno = GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("dc:", "http://purl.org/dc/elements/1.1/"),
                    SPARQL.Prefix("ns:", "http://example.org/ns#"),
                },
                projection: "?title ?price",
                where: SPARQL.Group(
                    SPARQL.Tripple("?x ns:price ?price"),
                    SPARQL.Filter("?price < 30.5"),
                    SPARQL.Tripple("?x dc:title ?title")
                )
            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            list.Any(x => x.title == "The Semantic Web" && x.price == 23).Should().Be.True();
        }
    }
}
