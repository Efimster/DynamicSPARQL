using System.Collections.Generic;
using System.Linq;
using Should.Fluent;
using Xunit.Extensions;

namespace DynamicSPARQLSpace.Tests
{
    public class BindFixture
    {

        [Theory(DisplayName = "Binding"), Xunit.Trait("SPARQL Query", ""),
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
                    SPARQL.Triple("?P foaf:givenName ?G"),
                    SPARQL.Triple("?P foaf:surname ?S"),
                    SPARQL.Bind("CONCAT(?G, \" \", ?S) AS ?name")
                )
            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            list.Any(x => x.name == "John Doe").Should().Be.True();
        }
    }
}
