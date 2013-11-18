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
    public class UnionFixture
    {
        private static dynamic GetDyno(string data, bool autoquotation = true)
        {
            var graph = new Graph();
            graph.LoadFromString(data);

            Func<string, SparqlResultSet> sendSPARQLQuery = xquery => graph.ExecuteQuery(xquery) as SparqlResultSet;
            dynamic dyno = DynamicSPARQL.CreateDyno(sendSPARQLQuery, autoquotation);

            return dyno;
        }


        [Theory(DisplayName = "Alternative Matching"),
        InlineData(@"@prefix dc10:  <http://purl.org/dc/elements/1.0/> .
            @prefix dc11:  <http://purl.org/dc/elements/1.1/> .

            _:a  dc10:title     ""SPARQL Query Language Tutorial"" .
            _:a  dc10:creator   ""Alice"" .

            _:b  dc11:title     ""SPARQL Protocol Tutorial"" .
            _:b  dc11:creator   ""Bob"" .

            _:c  dc10:title     ""SPARQL"" .
            _:c  dc11:title     ""SPARQL (updated)"" .")]
        public void TestUnion1(string data)
        {
            var dyno = GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] {
                    SPARQL.Prefix("dc10:", "http://purl.org/dc/elements/1.0/"),
                    SPARQL.Prefix("dc11:", "http://purl.org/dc/elements/1.1/")
                },
                projection: "?title",
                where: SPARQL.Group(
                    SPARQL.Union(left: SPARQL.Tripple("?book dc10:title  ?title"), right: SPARQL.Tripple("?book dc11:title  ?title"))
                )
            );

            var list = res.ToList();

            list.Count.Should().Equal(4);
            list.Any(x => x.title == "SPARQL Protocol Tutorial").Should().Be.True();
            list.Any(x => x.title == "SPARQL").Should().Be.True();
            list.Any(x => x.title == "SPARQL (updated)").Should().Be.True();
            list.Any(x => x.title == "SPARQL Query Language Tutorial").Should().Be.True();

            res = dyno.Select(
                prefixes: new[] {
                                SPARQL.Prefix("dc10:", "http://purl.org/dc/elements/1.0/"),
                                SPARQL.Prefix("dc11:", "http://purl.org/dc/elements/1.1/")
                            },
                projection: "?x ?y",
                where: SPARQL.Group(
                    SPARQL.Union(left: SPARQL.Tripple("?book dc10:title ?x"), right: SPARQL.Tripple("?book dc11:title  ?y"))
                )
            );

            list = res.ToList();
            list.Count.Should().Equal(4);
            list.Any(x => x.x==null && x.y == "SPARQL Protocol Tutorial").Should().Be.True();
            list.Any(x => x.x == null && x.y == "SPARQL (updated)").Should().Be.True();
            list.Any(x => x.x == "SPARQL" && x.y == null).Should().Be.True();
            list.Any(x => x.x == "SPARQL Query Language Tutorial" && x.y == null).Should().Be.True();

            res = dyno.Select(
                prefixes: new[] {
                                            SPARQL.Prefix("dc10:", "http://purl.org/dc/elements/1.0/"),
                                            SPARQL.Prefix("dc11:", "http://purl.org/dc/elements/1.1/")
                                        },
                projection: "?title ?author",
                where: SPARQL.Group(
                    SPARQL.Union(
                        left: SPARQL.Group(SPARQL.Tripple("?book dc10:title ?title"), SPARQL.Tripple("?book dc10:creator ?author")),
                        right: SPARQL.Group(SPARQL.Tripple("?book dc11:title ?title"), SPARQL.Tripple("?book dc11:creator ?author"))
                    )
                )
            );

            list = res.ToList();
            list.Count.Should().Equal(2);
            list.Any(x => x.author == "Alice" && x.title == "SPARQL Query Language Tutorial").Should().Be.True();
            list.Any(x => x.author == "Bob" && x.title == "SPARQL Protocol Tutorial").Should().Be.True();

        }
    }
}
