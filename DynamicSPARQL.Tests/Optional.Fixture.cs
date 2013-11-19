using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicSPARQLSpace;
using VDS.RDF;
using VDS.RDF.Query;
using Xunit;
using Xunit.Extensions;
using Should.Fluent;

namespace DynamicSPARQLSpace.Tests
{
   public class OptionalFixture
   {


        [Theory(DisplayName = "Optional Pattern Matching"),
        InlineData(@"@prefix foaf:       <http://xmlns.com/foaf/0.1/> .
                    @prefix rdf:        <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .

                    _:a  rdf:type        foaf:Person .
                    _:a  foaf:name       ""Alice"" .
                    _:a  foaf:mbox       <mailto:alice@example.com> .
                    _:a  foaf:mbox       <mailto:alice@work.example> .

                    _:b  rdf:type        foaf:Person .
                    _:b  foaf:name       ""Bob"" .")]
        public void TestOptional1(string data)
        {
            var dyno = TestDataProvider.GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { SPARQL.Prefix("foaf:", "http://xmlns.com/foaf/0.1/") },
                projection: "?name ?mbox",
                where: SPARQL.Group(
                SPARQL.Tripple(s: "?x", p: "foaf:name", o: "?name"),
                        SPARQL.Optional(s: "?x", p: "foaf:mbox", o: "?mbox")
                )
            );

            var list = res.ToList();

            list.Count.Should().Equal(3);

            list.Where(x => x.name == "Alice" && x.mbox == "mailto:alice@example.com").Count().Should().Equal(1);
            list.Where(x => x.name == "Alice" && x.mbox == "mailto:alice@work.example").Count().Should().Equal(1);
            list.Where(x => x.name == "Bob" && x.mbox == null).Count().Should().Equal(1);

        }

        [Theory(DisplayName = "Constraints in Optional Pattern Matching"),
             InlineData(@"@prefix dc:   <http://purl.org/dc/elements/1.1/> .
            @prefix :     <http://example.org/book/> .
            @prefix ns:   <http://example.org/ns#> .

            :book1  dc:title  ""SPARQL Tutorial"" .
            :book1  ns:price  42 .
            :book2  dc:title  ""The Semantic Web"" .
            :book2  ns:price  23 .")]
        public void TestOptional2(string data)
        {
            var dyno = TestDataProvider.GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("dc:", "http://purl.org/dc/elements/1.1/"),
                    SPARQL.Prefix("ns:", "http://example.org/ns#")
                },
                projection: "?title ?price",
                where: SPARQL.Group(
                    SPARQL.Tripple("?x dc:title ?title"),
                    SPARQL.Optional(
                        SPARQL.Tripple("?x ns:price ?price"),
                        SPARQL.Filter("?price < 30"))
                )
            );

            var list = res.ToList();

            list.Count.Should().Equal(2);

            list.Where(x => x.title == "SPARQL Tutorial" && x.price == null).Count().Should().Equal(1);
            list.Where(x => x.title == "The Semantic Web" && x.price == 23).Count().Should().Equal(1);

        }

        [Theory(DisplayName = "Multiple Optional Graph Patterns"),
            InlineData(@"@prefix foaf:  <http://xmlns.com/foaf/0.1/> .
            _:a  foaf:name       ""Alice"" .
            _:a  foaf:homepage   ""http://work.example.org/alice/"" .

            _:b  foaf:name       ""Bob"" .
            _:b  foaf:mbox       <mailto:bob@work.example> .")]
        public void TestOptional3(string data)
        {
            var dyno = TestDataProvider.GetDyno(data);

            IEnumerable<dynamic> res = dyno.Select(
                prefixes: new[] { 
                    SPARQL.Prefix("foaf:", "http://xmlns.com/foaf/0.1/"),
                },
                projection: "?name ?mbox ?hpage",
                where: SPARQL.Group(
                    SPARQL.Tripple("?x foaf:name  ?name"),
                    SPARQL.Optional("?x foaf:mbox ?mbox"),
                    SPARQL.Optional("?x foaf:homepage ?hpage"))

            );

            var list = res.ToList();

            list.Count.Should().Equal(2);

            list.Where(x => x.name == "Alice" && x.mbox == null && x.hpage == "http://work.example.org/alice/" ).Count().Should().Equal(1);
            list.Where(x => x.name == "Bob" && x.mbox == "mailto:bob@work.example" && x.hpage == null ).Count().Should().Equal(1);

        }
   }
}
