using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightstarDB.Client;
using DynamicSPARQLSpace;
using Xunit;
using Should.Fluent;

namespace DynamicSPARQLSpace.BrightstarDB.Tests
{
    public class ConnectoreFixture
    {
        private BrightstarDB.Connector brightstarConnector;

        [Fact(DisplayName = "Get Quering Function"), Xunit.Trait("BrightstarDB connector", "")]
        public void BrightstarConnectorTest()
        {
            string storeName = null;
            
            try
            {
                storeName = GenerateTestStore();
                brightstarConnector = new Connector("type=embedded;storesdirectory=brightstar;storename=" + storeName);
                var func = brightstarConnector.GetQueryingFunction();

                var dyno = DynamicSPARQL.CreateDyno(func, autoquotation: true);

                IEnumerable<dynamic> list = dyno.Select(
                        projection: "?product ?p ?o ?level",
                        where: SPARQL.Group(
                            SPARQL.Triple("?product ?p ?o"),
                            SPARQL.Optional(
                                @"?product ""http://www.brightstardb.com/schemas/product/level"" ?level"
                            )
                        )
                );

                var resultList = list.ToList();

                IList<string> result = resultList.Select(triple => (string)triple.o).ToList();

                result.Should().Contain.One("nosql");
                result.Should().Contain.One(".net");
                result.Should().Contain.One("rdf");

                ((object)resultList.First().level).Should().Be.Null();

                
            }
            catch(Exception exc)
            {
                throw exc;
            }
            finally
            {
                if (brightstarConnector.Client!=null)
                    brightstarConnector.Client.DeleteStore(brightstarConnector.StoreName);
            }
        }

        private string GenerateTestStore()
        {
            var client = BrightstarService.GetClient("type=embedded;storesdirectory=brightstar;");
            string storeName = "Store_" + Guid.NewGuid();
            client.CreateStore(storeName);
            
            var data = new StringBuilder();
            data.AppendLine("<http://www.brightstardb.com/products/brightstar> <http://www.brightstardb.com/schemas/product/name> \"BrightstarDB\" .");
            data.AppendLine("<http://www.brightstardb.com/products/brightstar> <http://www.brightstardb.com/schemas/product/category> <http://www.brightstardb.com/categories/nosql> .");
            data.AppendLine("<http://www.brightstardb.com/products/brightstar> <http://www.brightstardb.com/schemas/product/category> <http://www.brightstardb.com/categories/.net> .");
            data.AppendLine("<http://www.brightstardb.com/products/brightstar> <http://www.brightstardb.com/schemas/product/category> <http://www.brightstardb.com/categories/rdf> .");

            client.ExecuteTransaction(storeName, null, null, data.ToString());


            return storeName;
        }

        [Fact(DisplayName = "Get Update Function"), Xunit.Trait("BrightstarDB connector", "")]
        public void BrightstarGetUpdateFunction()
        {
            string storeName = null;

            try
            {
                storeName = "UpdStore_" + Guid.NewGuid();
                brightstarConnector = new Connector("type=embedded;storesdirectory=brightstar;storename=" + storeName);
                brightstarConnector.Client.CreateStore(storeName);

                var updFunc = brightstarConnector.GetUpdateFunction();
                var queryFunc = brightstarConnector.GetQueryingFunction();

                var dyno = DynamicSPARQL.CreateDyno(queryingFunc: queryFunc, updateFunc: updFunc, autoquotation: false);

                var updres = dyno.Insert(
                   prefixes: new[] {
                        SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/")
                    },
                   insert: SPARQL.Triple(s: "<http://example/book1>", p: new[] { @"dc:title ""David Copperfield""@fr",
                                                                                  @"dc:creator ""Edmund Wells""",
                                                                                  "dc:price 42"})
                );

                ((IJobInfo)updres).JobCompletedOk.Should().Be.True();


                IEnumerable<dynamic> res = dyno.Select(
                    projection: "?s ?p ?o",
                    where: SPARQL.Triple("?s ?p ?o")
                );

                var list = res.ToList();
                list.Count.Should().Equal(3);
                list.Where(x => x.p == "price" && x.o == 42).Count().Should().Equal(1);
                list.Where(x => x.p == "title" && x.o == "David Copperfield").Count().Should().Equal(1);
                list.Where(x => x.p == "creator" && x.o == "Edmund Wells").Count().Should().Equal(1);

                dyno.Delete(
                    where: SPARQL.Triple(s: "<http://example/book1>", p: "?property ?value" )
                );

                res = dyno.Select(
                   projection: "?s ?p ?o",
                   where: SPARQL.Triple("?s ?p ?o")
                );
                
                res.ToList().Count.Should().Equal(0);
            }
            finally
            {
                if (brightstarConnector.Client != null)
                    brightstarConnector.Client.DeleteStore(brightstarConnector.StoreName);
            }
        }
    }
}
