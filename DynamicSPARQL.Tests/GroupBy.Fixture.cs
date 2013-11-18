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
    public class GroupByFixture
    {
        private static dynamic GetDyno(string data, bool autoquotation = true)
        {
            var graph = new Graph();
            graph.LoadFromString(data);

            Func<string, SparqlResultSet> sendSPARQLQuery = xquery => graph.ExecuteQuery(xquery) as SparqlResultSet;
            dynamic dyno = DynamicSPARQL.CreateDyno(sendSPARQLQuery, autoquotation);

            return dyno;
        }


        [Theory(DisplayName = "Group By & Having"),
        InlineData(@"@prefix : <http://books.example/> .
            :org1 :affiliates :auth1, :auth2 .
            :auth1 :writesBook :book1, :book2 .
            :book1 :price 9 .
            :book2 :price 5 .
            :auth2 :writesBook :book3 .
            :book3 :price 7 .
            :org2 :affiliates :auth3 .
            :auth3 :writesBook :book4 .
            :book4 :price 7 .")]
        public void TestGroupBy1(string data)
        {
            var dyno = GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] {
                        SPARQL.Prefix(":", "http://books.example/"),
                },
                projection: "(SUM(?lprice) AS ?totalPrice)",
                where: SPARQL.Group(
                    SPARQL.Tripple("?org :affiliates ?auth"),
                    SPARQL.Tripple("?auth :writesBook ?book"),
                    SPARQL.Tripple("?book :price ?lprice")
                ),
                groupBy:"?org",
                having: "SUM(?lprice) > 10"

            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            ((int)list.First().totalPrice).Should().Equals(21);
        }

    }
}
