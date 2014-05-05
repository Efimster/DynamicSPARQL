using System.Collections.Generic;
using System.Linq;
using Should.Fluent;
using Xunit.Extensions;

namespace DynamicSPARQLSpace.Tests
{
    public class FilterFixture
    {
        [Theory(DisplayName = "Filtering(Restricting the Value of Strings) "),
         Xunit.Trait("SPARQL Query", "Filtering"),
         InlineData(@"@prefix dc:   <http://purl.org/dc/elements/1.1/> .
                @prefix :     <http://example.org/book/> .
                @prefix ns:   <http://example.org/ns#> .

                :book1  dc:title  ""SPARQL Tutorial"" .
                :book1  ns:price  42 .
                :book2  dc:title  ""The Semantic Web"" .
                :book2  ns:price  23 .")]
        public void TestFilter1(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, autoquotation: false);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/"),
                },
                projection: "?title",
                where: SPARQL.Group(
                    SPARQL.Triple("?x dc:title ?title"),
                    SPARQL.Filter("regex(?title, \"^SPARQL\")")
                )
            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            list.Any(x => x.title == "SPARQL Tutorial").Should().Be.True();

            dyno = TestDataProvider.GetDyno(data, autoquotation: true);
            
            res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/"),
                },
                projection: "?title",
                where: SPARQL.Group(
                    SPARQL.Triple("?x dc:title ?title"),
                    SPARQL.Filter("regex(?title, web, i ) ")
                )
            );

            list = res.ToList();
            list.Count.Should().Equal(1);
            list.Any(x => x.title == "The Semantic Web").Should().Be.True();

        }

        [Theory(DisplayName = "Filtering(Restricting Numeric Values) "),
         Xunit.Trait("SPARQL Query", "Filtering"),
         InlineData(@"@prefix dc:   <http://purl.org/dc/elements/1.1/> .
                @prefix :     <http://example.org/book/> .
                @prefix ns:   <http://example.org/ns#> .

                :book1  dc:title  ""SPARQL Tutorial"" .
                :book1  ns:price  42 .
                :book2  dc:title  ""The Semantic Web"" .
                :book2  ns:price  23 .")]
        public void TestFilter2(string data)
        {
            var dyno = TestDataProvider.GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/"),
                    SPARQL.Prefix("ns", "http://example.org/ns#"),
                },
                projection: "?title ?price",
                where: SPARQL.Group(
                    SPARQL.Triple("?x ns:price ?price"),
                    SPARQL.Filter("?price < 30.5"),
                    SPARQL.Triple("?x dc:title ?title")
                )
            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            list.Any(x => x.title == "The Semantic Web" && x.price == 23).Should().Be.True();
        }

        [Theory(DisplayName = "Filtering - variable equels to IRI "),
 Xunit.Trait("SPARQL Query", "Filtering"),
 InlineData(@"@prefix dc:   <http://purl.org/dc/elements/1.1/> .
                @prefix :     <http://example.org/book/> .
                @prefix ns:   <http://example.org/ns#> .

                :book1  dc:title  ""SPARQL Tutorial"" .
                :book1  ns:price  42 .
                :book2  dc:title  ""The Semantic Web"" .
                :book2  ns:price  23 .")]
        public void TestFilter3(string data)
        {
            var dyno = TestDataProvider.GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/"),
                    SPARQL.Prefix("ns", "http://example.org/ns#"),
                    SPARQL.Prefix("book", "http://example.org/book/"),
                },
                projection: "?price",
                where: SPARQL.Group(
                    SPARQL.Triple("?x ns:price ?price"),
                    SPARQL.Filter("?x IN (book:book1)")
                )
            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            list.Any(x => x.price == 42).Should().Be.True();
        }
    }
}
