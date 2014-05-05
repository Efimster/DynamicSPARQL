using System.Collections.Generic;
using System.Linq;
using Should.Fluent;
using Xunit.Extensions;

namespace DynamicSPARQLSpace.Tests
{
    public class GraphFixture
    {


        [Theory(DisplayName = "Graph Pattern"),
         Xunit.Trait("SPARQL Query", "Named Graphs"),
         InlineData(@"
                    @prefix foaf:       <http://xmlns.com/foaf/0.1/> .
                    @prefix rdf:        <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
                    @prefix graph:      <http://example.org/foaf/> .
                    graph:def  {
                        _:b  rdf:type        foaf:Person . }
                    graph:aliceFoaf  {   
                        _:a  rdf:type        foaf:Person .
                        _:a  foaf:name       ""Alice"" 
                    }
                    graph:bobFoaf  {   
                        _:a  rdf:type        foaf:Person .
                        _:a  foaf:name       ""Bob"" 
                    }")]
        public void TestGraph1(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, useStore: true, defaultGraphUri: "http://example.org/foaf/def");

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { SPARQL.Prefix("foaf:", "http://xmlns.com/foaf/0.1/") },
                from: "http://example.org/foaf/aliceFoaf",
                projection: "?s ?p ?o",
                where: SPARQL.Group(
                    SPARQL.Triple("?s ?p ?o")
                )
            );

            var list = res.ToList();
            list.Count.Should().Equal(2);

            res = dyno.Select(
                prefixes: new[] { SPARQL.Prefix("foaf:", "http://xmlns.com/foaf/0.1/"),
                    SPARQL.Prefix("graph", "http://example.org/foaf/")},
                from: "<http://example.org/foaf/def>",
                fromNamed: "<http://example.org/foaf/aliceFoaf>",
                projection: "?s ?p ?o",
                where: SPARQL.Graph("graph:aliceFoaf",
                    SPARQL.Triple("?s ?p ?o")
                )
            );

            res.ToList().Count.Should().Equal(2);

            res = dyno.Select(
                prefixes: new[] { SPARQL.Prefix("foaf:", "http://xmlns.com/foaf/0.1/"),
                                SPARQL.Prefix("graph", "http://example.org/foaf/")},
                from: "<http://example.org/foaf/def>",
                fromNamed: new[] {"http://example.org/foaf/aliceFoaf", "http://example.org/foaf/bobFoaf"},
                projection: "?g ?s ?p ?o",
                where: SPARQL.Group(
                    SPARQL.Graph("?g",
                        SPARQL.Triple("?s ?p ?o")
                    )
                )
            );

            res.ToList().Count.Should().Equal(4);
        }

       
    }
}
