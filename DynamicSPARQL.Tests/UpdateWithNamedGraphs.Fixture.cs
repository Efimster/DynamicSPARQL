using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions;
using Should.Fluent;

namespace DynamicSPARQLSpace.Tests
{
    public class UpdateWithNamedGraphsFixture
    {
        [Theory(DisplayName = "DELETE DATA"),
        Xunit.Trait("SPARQL Update", "Named Graphs"),
        InlineData(@"
            @prefix dc: <http://purl.org/dc/elements/1.1/> .
            @prefix ns: <http://example.org/ns#> .
                        
            ns:g1{
                <http://example/book2> ns:price 42 .
                <http://example/book2> dc:title ""David Copperfield"" .}
            ns:g2{
                <http://example/book2> dc:creator ""Edmund Wells"" .}
            
        ")]
        public void TestDeleteData1(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, autoquotation: false, useStore:true, defaultGraphUri: "http://example.org/ns#g2");

            dyno.Delete(
                prefixes: new[] {
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/"),
                    SPARQL.Prefix("ns", "http://example.org/ns#")
                },
                delete:SPARQL.Graph("ns:g1",
                                    SPARQL.Triple(s: "<http://example/book2>", 
                                        p: new[] { @"dc:title ""David Copperfield""",
                                                   @"dc:creator ""Edmund Wells"""})
                                  )
            );

            IEnumerable<dynamic> res = dyno.Select(
                    prefixes: new[] {
                        SPARQL.Prefix("ns", "http://example.org/ns#")
                    },
                    projection: "?s ?p ?o",
                    where: SPARQL.Triple("?s ?p ?o")
            );
            var list = res.ToList();
            list.Count.Should().Equal(1);
            list.Any(x => x.p == "creator").Should().Be.True();
        }

        [Theory(DisplayName = "INSERT DATA"),
         Xunit.Trait("SPARQL Update", "Named Graphs"),
         InlineData(@"
                @prefix dc: <http://purl.org/dc/elements/1.1/> .
                @prefix ns: <http://example.org/ns#> .
                ns:g1 {
                <http://example/book1> ns:price 42 .}")]
        public void TestInsertData(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, autoquotation: false, useStore: true, defaultGraphUri: "http://example.org/ns#g1");

            dyno.Insert(
                prefixes: new[] {
                    SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/"),
                     SPARQL.Prefix("ns", "http://example.org/ns#")
                },
                insert: SPARQL.Graph("ns:g2", 
                        SPARQL.Triple(s: "<http://example/book1>", p: new[] { @"dc:title ""David Copperfield""",
                                                                              @"dc:creator ""Edmund Wells""" })
                        )
            );


            IEnumerable<dynamic> res = dyno.Select(
                    projection: "?s ?p ?o",
                    from: "http://example.org/ns#g2",
                    where: SPARQL.Triple("?s ?p ?o")
            );

            var list = res.ToList();
            list.Count.Should().Equal(2);
            list.Where(x => x.p == "title" && x.o == "David Copperfield").Count().Should().Equal(1);
            list.Where(x => x.p == "creator" && x.o == "Edmund Wells").Count().Should().Equal(1);
        }

        [Theory(DisplayName = "DELETE/INSERT"),
        Xunit.Trait("SPARQL Update", "Named Graphs"),
        InlineData(@"
            @prefix foaf:  <http://xmlns.com/foaf/0.1/> .
            @prefix ns: <http://example.org/ns#> .
            ns:g1{
            <http://example/president25> foaf:givenName ""Bill"" .
            <http://example/president25> foaf:familyName ""McKinley"" .
            <http://example/president27> foaf:givenName ""Bill"" .
            <http://example/president27> foaf:familyName ""Taft"" .}
            ns:g2{
            <http://example/president42> foaf:givenName ""Bill"" .
            <http://example/president42> foaf:familyName ""Clinton"" .}")]
        public void TestDeleteInsert(string data)
        {
            var dyno = TestDataProvider.GetDyno(data, autoquotation: true, useStore: true, defaultGraphUri: "http://example.org/ns#g1");

            dyno.Update(
                prefixes: new[] {
                    SPARQL.Prefix("foaf", "http://xmlns.com/foaf/0.1/"),
                    SPARQL.Prefix("ns", "http://example.org/ns#")
                },
                with: "ns:g1",
                delete: SPARQL.Triple("?person foaf:givenName Bill"),
                insert: SPARQL.Triple("?person foaf:givenName William"),
                Using: "ns:g1",
                where: SPARQL.Triple("?person foaf:givenName Bill")
            );

            dyno.Update(
               prefixes: new[] {
                                SPARQL.Prefix("foaf", "http://xmlns.com/foaf/0.1/"),
                                SPARQL.Prefix("ns", "http://example.org/ns#")
                            },
               with: "ns:g2",
               delete: SPARQL.Triple("?person foaf:givenName Bill"),
               insert: SPARQL.Triple("?person foaf:givenName Ben"),
               UsingNamed: new[] { "ns:g2" },
               where: SPARQL.Graph("?g",
                   SPARQL.Triple("?person foaf:givenName Bill"))
           );


            IEnumerable<dynamic> res = dyno.Select(
                    projection: "?s ?p ?o",
                    where: SPARQL.Triple("?s ?p ?o")
            );

            var list = res.ToList();
            list.Count.Should().Equal(4);
            list.Where(x => x.p == "givenName" && x.o == "William").Count().Should().Equal(2);
            list.Where(x => x.p == "givenName" && x.o == "Bill").Count().Should().Equal(0);
                       

            res = dyno.Select(
                    prefixes: new[] {
                        SPARQL.Prefix("ns", "http://example.org/ns#")
                    },
                    projection: "?s ?p ?o",
                    from:"ns:g2",
                    where: SPARQL.Triple("?s ?p ?o")
            );

            list = res.ToList();
            list.Count.Should().Equal(2);
            list.Where(x => x.p == "givenName" && x.o == "Ben").Count().Should().Equal(1);
            list.Where(x => x.p == "givenName" && x.o == "Bill").Count().Should().Equal(0);


        }
  


    }
}
