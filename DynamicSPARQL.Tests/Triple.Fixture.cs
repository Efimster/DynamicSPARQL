using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Ordering;
using Xunit;
using Xunit.Extensions;
using Should.Fluent;

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

            Func<string, SparqlResultSet> sendSPARQLQuery = xquery => graph.ExecuteQuery(xquery) as SparqlResultSet;
            dyno = DynamicSPARQL.CreateDyno(sendSPARQLQuery, autoquotation: true);

            dyno.Prefixes = new[]{
                        SPARQL.Prefix(prefix:"rdf:", iri: "http://www.w3.org/1999/02/22-rdf-syntax-ns#" ),
                        SPARQL.Prefix(prefix:"rdf:", iri: "http://www.w3.org/1999/02/22-rdf-syntax-ns#" ),
                        SPARQL.Prefix(prefix:"cat:", iri: "http://my.web/catalogues#" ),
                        SPARQL.Prefix(prefix:"cp:", iri: "http://my.web/catalogues/predicates/" )
                        };
        }

        [Fact(DisplayName = "TripleWithInteger")]
        public void TestTripleWithInteger()
        {
            IEnumerable<dynamic> list = dyno.Select(
                    projection: "?s ?p ?o",
                    where: SPARQL.Group(
                        SPARQL.Triple(s: "?s", p: "cp:integer", o: "?o"))
            );

            dynamic x = list.First();

            Assert.IsType<int>(x.o);
            int i = x.o;
            i.Should().Equals(1);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Group(
                        SPARQL.Triple(s: "?s", p: "cp:int", o: "?o"))
            );

            x = list.First();

            Assert.IsType<int>(x.o);
            i = x.o;
            i.Should().Equals(-65000);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Group(
                        SPARQL.Triple(s: "?s", p: "cp:nonPositiveInteger", o: "?o"))
            );

            x = list.First();

            Assert.IsType<int>(x.o);
            i = x.o;
            i.Should().Equals(-5);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Group(
                        SPARQL.Triple(s: "?s", p: "cp:negativeInteger", o: "?o"))
            );

            x = list.First();

            Assert.IsType<int>(x.o);
            i = x.o;
            i.Should().Equals(-100);
        }

        [Fact(DisplayName = "TripleWithDateTime")]
        public void TestTripleWithDateTime()
        {
            IEnumerable<dynamic> list = dyno.Select(
                   projection: "?s ?p ?o",
                   where: SPARQL.Group(
                       SPARQL.Triple(s: "?s", p: "cp:datetime", o: "?o")));

            dynamic x = list.First();

            Assert.IsType<DateTime>(x.o);
            ((DateTime)x.o).Should().Equals(new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            list = dyno.Select(
               projection: "?s ?o",
               where: SPARQL.Group(
                   SPARQL.Triple(s: "?s", p: "cp:date", o: "?o")));

            x = list.First();

            Assert.IsType<DateTime>(x.o);
            ((DateTime)x.o).Should().Equals(new DateTime(2004, 12, 31));

        }

        [Fact(DisplayName = "TripleWithBoolean")]
        public void TestTripleWithBoolean()
        {
            IEnumerable<dynamic> list = dyno.Select(
                   projection: "?s ?o",
                   where: SPARQL.Group(
                       SPARQL.Triple(s: "?s", p: "cp:boolean", o: "?o")));

            dynamic x = list.First();

            Assert.IsType<bool>(x.o);
            ((bool)x.o).Should().Equals(true);


        }
        [Fact(DisplayName = "TripleWithLong")]
        public void TestTripleWithLong()
        {
            IEnumerable<dynamic> list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Group(
                        SPARQL.Triple(s: "?s", p: "cp:long", o: "?o"))
            );

            dynamic x = list.First();

            Assert.IsType<long>(x.o);
            long i = x.o;
            i.Should().Equals(-1999999);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Group(
                        SPARQL.Triple(s: "?s", p: "cp:unsignedLong", o: "?o"))
            );

            x = list.First();

            Assert.IsType<ulong>(x.o);
            ((ulong)x.o).Should().Equals(3000000);
        }
    }
}
