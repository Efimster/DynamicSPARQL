﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Should.Fluent;

namespace DynamicSPARQLSpace.Tests
{
    public class UpdateFixture
    {
        [Theory(DisplayName = "DELETE DATA"),
        Xunit.Trait("SPARQL Update",""),
        InlineData(@"@prefix dc: <http://purl.org/dc/elements/1.1/> .
            @prefix ns: <http://example.org/ns#> .

            <http://example/book2> ns:price 42 .
            <http://example/book2> dc:title ""David Copperfield"" .
            <http://example/book2> dc:creator ""Edmund Wells"" .")]
        public void TestDeleteData1(string data)
        {
            var dyno = TestDataProvider.GetDyno(data,autoquotation:false);

            dyno.Update(
                prefixes: new[] {
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/")
                },
                delete: SPARQL.Triple(s: "<http://example/book2>", p: new[] { @"dc:title ""David Copperfield""",
                                                                              @"dc:creator ""Edmund Wells""" })
            );


            IEnumerable<dynamic> res = dyno.Select(
                    projection: "?s ?p ?o",
                    where: SPARQL.Triple("?s ?p ?o")
            );

            var list = res.ToList();
            list.Count.Should().Equal(1);
            list.Any(x => x.p == "price" && x.o == 42).Should().Be.True();


        }

        [Theory(DisplayName = "INSERT DATA"),
         Xunit.Trait("SPARQL Update", ""),
         InlineData(@"@prefix dc: <http://purl.org/dc/elements/1.1/> .
                @prefix ns: <http://example.org/ns#> .

                <http://example/book1> ns:price 42 .")]
        public void TestInsertData(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, autoquotation: false);

            dyno.Update(
                prefixes: new[] {
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/")
                },
                insert: SPARQL.Triple(s: "<http://example/book1>", p: new[] { @"dc:title ""David Copperfield""",
                                                                              @"dc:creator ""Edmund Wells""" })
            );


            IEnumerable<dynamic> res = dyno.Select(
                    projection: "?s ?p ?o",
                    where: SPARQL.Triple("?s ?p ?o")
            );

            var list = res.ToList();
            list.Count.Should().Equal(3);
            list.Where(x => x.p == "price" && x.o == 42).Count().Should().Equal(1);
            list.Where(x => x.p == "title" && x.o == "David Copperfield").Count().Should().Equal(1);
            list.Where(x => x.p == "creator" && x.o == "Edmund Wells").Count().Should().Equal(1);


        }

        [Theory(DisplayName = "DELETE/INSERT"),
         Xunit.Trait("SPARQL Update", ""),
         InlineData(@"@prefix foaf:  <http://xmlns.com/foaf/0.1/> .
            <http://example/president25> foaf:givenName ""Bill"" .
            <http://example/president25> foaf:familyName ""McKinley"" .
            <http://example/president27> foaf:givenName ""Bill"" .
            <http://example/president27> foaf:familyName ""Taft"" .
            <http://example/president42> foaf:givenName ""Bill"" .
            <http://example/president42> foaf:familyName ""Clinton"" .")]
        public void TestDeleteInsert(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, autoquotation: true);

            dyno.Update(
                prefixes: new[] {
                    SPARQL.Prefix("foaf", "http://xmlns.com/foaf/0.1/")
                },
                delete: SPARQL.Triple("?person foaf:givenName Bill"),
                insert: SPARQL.Triple("?person foaf:givenName William"),
                where: SPARQL.Triple("?person foaf:givenName Bill")

            );


            IEnumerable<dynamic> res = dyno.Select(
                    projection: "?s ?p ?o",
                    where: SPARQL.Triple("?s ?p ?o")
            );

            var list = res.ToList();
            list.Count.Should().Equal(6);
            list.Where(x => x.p == "givenName" && x.o == "William").Count().Should().Equal(3);
            list.Where(x => x.p == "givenName" && x.o == "Bill").Count().Should().Equal(0);
        }

        [Theory(DisplayName = "DELETE(Informative)"),
         Xunit.Trait("SPARQL Update", ""),
         InlineData(@"@prefix dc: <http://purl.org/dc/elements/1.1/> .
            @prefix xsd: <http://www.w3.org/2001/XMLSchema#> .
            @prefix ns: <http://example.org/ns#> .

            <http://example/book1> dc:title ""Principles of Compiler Design"" .
            <http://example/book1> dc:date ""1977-01-01T00:00:00-02:00""^^xsd:dateTime .

            <http://example/book2> ns:price 42 .
            <http://example/book2> dc:title ""David Copperfield"" .
            <http://example/book2> dc:creator ""Edmund Wells"" .
            <http://example/book2> dc:date ""1948-01-01T00:00:00-02:00""^^xsd:dateTime .

            <http://example/book3> dc:title ""SPARQL 1.1 Tutorial"" .")]
        public void TestDeleteInformative(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, autoquotation: false);

            dyno.Update(
                prefixes: new[] {
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/"),
                    SPARQL.Prefix("xsd","http://www.w3.org/2001/XMLSchema#")
                },
                delete: SPARQL.Triple("?book ?p ?v"),
                where: SPARQL.Group( 
                    SPARQL.Triple("?book dc:date ?date"),
                    SPARQL.Filter(@"?date > ""1970-01-01T00:00:00-02:00""^^xsd:dateTime"),
                    SPARQL.Triple("?book ?p ?v")
                )

            );


            IEnumerable<dynamic> res = dyno.Select(
                    projection: "?s ?p ?o",
                    where: SPARQL.Triple("?s ?p ?o")
            );

            var list = res.ToList();
            list.Count.Should().Equal(5);
            list.Where(x => x.s == "book2").Count().Should().Equal(4);
            list.Where(x => x.s == "book3").Count().Should().Equal(1);
            list.Where(x => x.s == "book1").Count().Should().Equal(0);

        }

        [Theory(DisplayName = "INSERT(Informative)"),
         Xunit.Trait("SPARQL Update", ""),
         InlineData(@"@prefix foaf: <http://xmlns.com/foaf/0.1/> .
            @prefix rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .

            _:a  rdf:type        foaf:Person .
            _:a  foaf:name       ""Alice"" .
            _:a  foaf:mbox       <mailto:alice@example.com> .

            _:b  rdf:type        foaf:Person .
            _:b  foaf:name       ""Bob"" .")]
        public void TestInsertInformative(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, autoquotation: false);

            dyno.Update(
                prefixes: new[] {
                    SPARQL.Prefix("foaf","http://xmlns.com/foaf/0.1/")
                },
                insert: SPARQL.Group( 
                    SPARQL.Triple("?person  foaf:name2  ?name"),
                    SPARQL.Triple("?person  foaf:mbox2  ?email")
                ),
                where: SPARQL.Group(
                    SPARQL.Triple("?person  foaf:name  ?name"),
                    SPARQL.Optional("?person  foaf:mbox  ?email")
                )

            );


            IEnumerable<dynamic> res = dyno.Select(
                    projection: "?s ?p ?o",
                    where: SPARQL.Triple("?s ?p ?o")
            );

            var list = res.ToList();
            list.Count.Should().Equal(8);
            list.Where(x => x.p == "mbox2").Count().Should().Equal(1);
            list.Where(x => x.p == "name2").Count().Should().Equal(2);


        }
    }
}
