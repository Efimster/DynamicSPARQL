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
            string path = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "TestTripleStores\\triple.Fixture.ttl");
            store.LoadFromFile(path);
            var graph = store.Graphs.First();

            Func<string, SparqlResultSet> sendSPARQLQuery = xquery => graph.ExecuteQuery(xquery) as SparqlResultSet;
            dyno = DynamicSPARQL.CreateDyno(sendSPARQLQuery, autoquotation: true);

            dyno.Prefixes = new[]{
                        SPARQL.Prefix(Prefix:"rdf:", IRI: "http://www.w3.org/1999/02/22-rdf-syntax-ns#" ),
                        SPARQL.Prefix(Prefix:"rdf:", IRI: "http://www.w3.org/1999/02/22-rdf-syntax-ns#" ),
                        SPARQL.Prefix(Prefix:"cat:", IRI: "http://my.web/catalogues#" ),
                        SPARQL.Prefix(Prefix:"cp:", IRI: "http://my.web/catalogues/predicates/" )
                        };
        }

        [Fact(DisplayName = "TripleWithInteger")]
        public void TestTripleWithInteger()
        {
            IEnumerable<dynamic> list = dyno.Select(
                    projection: "?s ?p ?o",
                    where: SPARQL.Group(
                        SPARQL.Tripple(S: "?s", P: "cp:integer", O: "?o"))
            );

            dynamic x = list.First();

            Assert.IsType<int>(x.o);
            int i = x.o;
            i.Should().Equals(1);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Group(
                        SPARQL.Tripple(S: "?s", P: "cp:int", O: "?o"))
            );

            x = list.First();

            Assert.IsType<int>(x.o);
            i = x.o;
            i.Should().Equals(-65000);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Group(
                        SPARQL.Tripple(S: "?s", P: "cp:nonPositiveInteger", O: "?o"))
            );

            x = list.First();

            Assert.IsType<int>(x.o);
            i = x.o;
            i.Should().Equals(-5);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Group(
                        SPARQL.Tripple(S: "?s", P: "cp:negativeInteger", O: "?o"))
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
                       SPARQL.Tripple(S: "?s", P: "cp:datetime", O: "?o")));

            dynamic x = list.First();

            Assert.IsType<DateTime>(x.o);
            ((DateTime)x.o).Should().Equals(new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            list = dyno.Select(
               projection: "?s ?o",
               where: SPARQL.Group(
                   SPARQL.Tripple(S: "?s", P: "cp:date", O: "?o")));

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
                       SPARQL.Tripple(S: "?s", P: "cp:boolean", O: "?o")));

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
                        SPARQL.Tripple(S: "?s", P: "cp:long", O: "?o"))
            );

            dynamic x = list.First();

            Assert.IsType<long>(x.o);
            long i = x.o;
            i.Should().Equals(-1999999);

            list = dyno.Select(
                    projection: "?s ?o",
                    where: SPARQL.Group(
                        SPARQL.Tripple(S: "?s", P: "cp:unsignedLong", O: "?o"))
            );

            x = list.First();

            Assert.IsType<ulong>(x.o);
            ((ulong)x.o).Should().Equals(3000000);
        }
    }
}
