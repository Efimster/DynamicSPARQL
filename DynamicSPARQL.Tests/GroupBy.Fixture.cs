using System.Collections.Generic;
using System.Linq;
using Should.Fluent;
using Xunit.Extensions;

namespace DynamicSPARQLSpace.Tests
{
    public class GroupByFixture
    {

        [Theory(DisplayName = "Group By & Having"),
         Xunit.Trait("SPARQL Query", ""),
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
            var dyno = TestDataProvider.GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] {
                        SPARQL.Prefix(":", "http://books.example/"),
                },
                projection: "(SUM(?lprice) AS ?totalPrice)",
                where: SPARQL.Group(
                    SPARQL.Triple("?org :affiliates ?auth"),
                    SPARQL.Triple("?auth :writesBook ?book"),
                    SPARQL.Triple("?book :price ?lprice")
                ),
                groupBy:"?org",
                having: "SUM(?lprice) > 10"

            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            ((int)list.First().totalPrice).Should().Equal(21);
        }

    }
}
