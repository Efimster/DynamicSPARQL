using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Should.Fluent;
using VDS.RDF;
using VDS.RDF.Query.Datasets;
using Xunit;

namespace DynamicSPARQLSpace.Tests
{
    public class TripleFixture
    {
        dynamic dyno;
        
        public TripleFixture()
        {
            var store = new TripleStore();
            string path = System.IO.Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "TestTripleStores\\triple.Fixture.ttl");
            store.LoadFromFile(path);
            var graph = store.Graphs.First();

            var connector = new DynamicSPARQLSpace.dotNetRDF.Connector(new InMemoryDataset(graph));

            //Func<string, SparqlResultSet> sendSPARQLQuery = xquery => graph.ExecuteQuery(xquery) as SparqlResultSet;
            dyno = DynamicSPARQL.CreateDyno(connector.GetQueryingFunction(), autoquotation: true);

            dyno.Prefixes = new[]{
                        SPARQL.Prefix(prefix:"rdf:", iri: "http://www.w3.org/1999/02/22-rdf-syntax-ns#" ),
                        SPARQL.Prefix(prefix:"rdf:", iri: "http://www.w3.org/1999/02/22-rdf-syntax-ns#" ),
                        SPARQL.Prefix(prefix:"cat:", iri: "http://my.web/catalogues#" ),
                        SPARQL.Prefix(prefix:"cp:", iri: "http://my.web/catalogues/predicates/" )
                        };
        }

        [Fact(DisplayName = "TripleWithInteger"),Xunit.Trait("SPARQL Query", "typed")]
        public void TestTripleWithInteger()
        {
            IEnumerable<dynamic> list = dyno.Select(
                    projection: "?s ?p ?o",
                    where: SPARQL.Triple(s: "?s", p: "cp:integer", o: "?o")
            );

            dynamic x = list.First();

            Assert.IsType<int>(x.o);
            int i = x.o;
            i.Should().Equal(1);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Triple(s: "?s", p: "cp:int", o: "?o")
            );

            x = list.First();

            Assert.IsType<int>(x.o);
            i = x.o;
            i.Should().Equal(-65000);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Triple(s: "?s", p: "cp:nonPositiveInteger", o: "?o")
            );

            x = list.First();

            Assert.IsType<int>(x.o);
            i = x.o;
            i.Should().Equal(-5);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Triple(s: "?s", p: "cp:negativeInteger", o: "?o")
            );

            x = list.First();

            Assert.IsType<int>(x.o);
            i = x.o;
            i.Should().Equal(-100);
        }

        [Fact(DisplayName = "TripleWithDateTime"), Xunit.Trait("SPARQL Query", "typed"),]
        public void TestTripleWithDateTime()
        {
            IEnumerable<dynamic> list = dyno.Select(
                   projection: "?s ?p ?o",
                   where: SPARQL.Triple(s: "?s", p: "cp:datetime", o: "?o")
            );

            dynamic x = list.First();

            Assert.IsType<DateTime>(x.o);
            ((DateTime)x.o).Should().Equal(new DateTime(2005, 1, 1, 1, 0, 0, DateTimeKind.Utc));

            list = dyno.Select(
               projection: "?s ?o",
               where: SPARQL.Group(
                   SPARQL.Triple(s: "?s", p: "cp:date", o: "?o")));

            x = list.First();

            Assert.IsType<DateTime>(x.o);
            ((DateTime)x.o).Should().Equal(new DateTime(2004, 12, 31));

        }

        [Fact(DisplayName = "TripleWithBoolean"), Xunit.Trait("SPARQL Query", "typed")]
        public void TestTripleWithBoolean()
        {
            IEnumerable<dynamic> list = dyno.Select(
                   projection: "?s ?o",
                   where: SPARQL.Triple(s: "?s", p: "cp:boolean", o: "?o")
            );

            dynamic x = list.First();

            Assert.IsType<bool>(x.o);
            ((bool)x.o).Should().Equal(true);


        }
        [Fact(DisplayName = "TripleWithLong"), Xunit.Trait("SPARQL Query", "typed"),]
        public void TestTripleWithLong()
        {
            IEnumerable<dynamic> list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Triple(s: "?s", p: "cp:long", o: "?o")
            );

            dynamic x = list.First();

            Assert.IsType<long>(x.o);
            long i = x.o;
            i.Should().Equal(-1999999L);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Triple(s: "?s", p: "cp:unsignedLong", o: "?o")
            );

            x = list.First();

            Assert.IsType<ulong>(x.o);
            ((ulong)x.o).Should().Equal(3000000UL);
        }

        [Fact(DisplayName = "Triple ToString"), Xunit.Trait("Triple", "")]
        public void TestTripleToString()
        {
            var triple = SPARQL.Triple(" ?x    ?y    ?z  ");
            var str = triple.ToString(autoQuotation: false, skipTriplesWithEmptyObject: false, mindAsterisk: false);
            str.Should().Equal("?x ?y ?z .");

            triple = SPARQL.Triple(" ?x  ?y  literal");
            str = triple.ToString(autoQuotation: true, skipTriplesWithEmptyObject: false, mindAsterisk: false);
            str.Should().Equal("?x ?y \"literal\" .");

            triple = SPARQL.Triple(" <http://test.11.com/1/_/-/ddd/#>  ?y  next literal");
            str = triple.ToString(autoQuotation: true, skipTriplesWithEmptyObject: false, mindAsterisk: false);
            str.Should().Equal("<http://test.11.com/1/_/-/ddd/#> ?y \"next literal\" .");

            triple = SPARQL.Triple(" ff:dd  ?y  111");
            str = triple.ToString(autoQuotation: true, skipTriplesWithEmptyObject: false, mindAsterisk: false);
            str.Should().Equal("ff:dd ?y 111 .");

            triple = SPARQL.Triple(" 1^^xsd:integer  ?y  \"next literal\"");
            str = triple.ToString(autoQuotation: true, skipTriplesWithEmptyObject: false, mindAsterisk: false);
            str.Should().Equal("1^^xsd:integer ?y \"next literal\" .");

            triple = SPARQL.Triple(" 1^^xsd:integer  ?y  <next literal>");
            str = triple.ToString(autoQuotation: true, skipTriplesWithEmptyObject: false, mindAsterisk: false);
            str.Should().Equal("1^^xsd:integer ?y \"<next literal>\" .");

            triple = SPARQL.Triple(s:"ff:dd",p:new[] {"?y  \"literal\"", "?z literal next"});
            str = triple.ToString(autoQuotation: true, skipTriplesWithEmptyObject: false, mindAsterisk: false);
            str.Should().Equal("ff:dd ?y \"literal\"; ?z \"literal next\" .");

            triple = SPARQL.Triple("?*  ?y  literal");
            str = triple.ToString(autoQuotation: true, skipTriplesWithEmptyObject: false, mindAsterisk: true);
            str.Should().Not.StartWith("?*");

        }
    }
}
