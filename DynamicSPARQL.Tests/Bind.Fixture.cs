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
    public class BindFixture
    {
        private static dynamic GetDyno(string data, bool autoquotation = true)
        {
            var graph = new Graph();
            graph.LoadFromString(data);

            Func<string, SparqlResultSet> sendSPARQLQuery = xquery => graph.ExecuteQuery(xquery) as SparqlResultSet;
            dynamic dyno = DynamicSPARQL.CreateDyno(sendSPARQLQuery, autoquotation);

            return dyno;
        }

        [Theory(DisplayName = "Binding"),
            InlineData(@"@prefix foaf:  <http://xmlns.com/foaf/0.1/> .
            _:a  foaf:givenName   ""John"" .
            _:a  foaf:surname  ""Doe"" .")]
        public void TestBind1(string data)
        {
            var dyno = TestDataProvider.GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("foaf:", "http://xmlns.com/foaf/0.1/"),
                },
                projection: "?name",
                where: SPARQL.Group(
                    SPARQL.Tripple("?P foaf:givenName ?G"),
                    SPARQL.Tripple("?P foaf:surname ?S"),
                    SPARQL.Bind("CONCAT(?G, \" \", ?S) AS ?name")
                )
            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            list.Any(x => x.name == "John Doe").Should().Be.True();
        }
    }
}
