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
    public class NegationFixture
    {

        [Theory(DisplayName = "Removing Possible Solutions(MINUS)"),
        InlineData(@"@prefix :       <http://example/> .
                @prefix foaf:   <http://xmlns.com/foaf/0.1/> .

                :alice  foaf:givenName ""Alice"" ;
                        foaf:familyName ""Smith"" .

                :bob    foaf:givenName ""Bob"" ;
                        foaf:familyName ""Jones"" .

                :carol  foaf:givenName ""Carol"" ;
                        foaf:familyName ""Smith"" .")]
        public void TestMinus1(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, treatUri: true);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { SPARQL.Prefix("foaf:", "http://xmlns.com/foaf/0.1/") },
                projection: "DISTINCT ?s",
                where: SPARQL.Group(
                    SPARQL.Tripple("?s ?p ?o"),
                    SPARQL.Minus("?s foaf:givenName Bob")
                )
            );

            var list = res.ToList();

            list.Count.Should().Equal(2);

            list.Where(x => x.s == "alice").Count().Should().Equal(1);
            list.Where(x => x.s == "carol").Count().Should().Equal(1);

        }

        [Theory(DisplayName = "Presence of a Pattern(FILTER EXISTS)"),
         InlineData(@"@prefix  :       <http://example/> .
                @prefix  rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
                @prefix  foaf:   <http://xmlns.com/foaf/0.1/> .

                :alice  rdf:type   foaf:Person .
                :alice  foaf:name  ""Alice"" .
                :bob    rdf:type   foaf:Person . ")]
        public void TestExists(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, treatUri: true);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("foaf:", "http://xmlns.com/foaf/0.1/"),
                    SPARQL.Prefix("rdf:","http://www.w3.org/1999/02/22-rdf-syntax-ns#")
                },
                projection: "?person",
                where: SPARQL.Group(
                    SPARQL.Tripple("?person rdf:type  foaf:Person"),
                    SPARQL.Exists("?person foaf:name ?name")
                )
            );

            var list = res.ToList();

            list.Count.Should().Equal(1);
            list.Where(x => x.person == "alice").Count().Should().Equal(1);
        }

        [Theory(DisplayName = "Absence of a Pattern(FILTER NOT EXISTS)"),
         InlineData(@"@prefix  :       <http://example/> .
                @prefix  rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
                @prefix  foaf:   <http://xmlns.com/foaf/0.1/> .

                :alice  rdf:type   foaf:Person .
                :alice  foaf:name  ""Alice"" .
                :bob    rdf:type   foaf:Person . ")]
        public void TestNotExists(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, treatUri: true);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("foaf:", "http://xmlns.com/foaf/0.1/"),
                    SPARQL.Prefix("rdf:","http://www.w3.org/1999/02/22-rdf-syntax-ns#")
                },
                projection: "?person",
                where: SPARQL.Group(
                    SPARQL.Tripple("?person rdf:type  foaf:Person"),
                    SPARQL.NotExists("?person foaf:name ?name")
                )
            );

            var list = res.ToList();

            list.Count.Should().Equal(1);
            list.Where(x => x.person == "bob").Count().Should().Equal(1);

        }
    }
}
