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
        private IBrightstarService client;

        [Fact(DisplayName = "Brightstar Connector Test")]
        public void BrightstarConnectorTest()
        {
            string storeName = null;
            
            try
            {
                storeName = GenerateTestStore();
                var func = Connector.GetQueringFunction("type=embedded;storesdirectory=brightstar;storename=" + storeName);

                var dyno = DynamicSPARQL.CreateDyno(func, autoquotation: true);

                IEnumerable<dynamic> list = dyno.Select(
                        projection: "?s ?p ?o",
                        where: SPARQL.Group(SPARQL.Tripple("?s ?p ?o"))
                );

                IList<string> result = list.Select(triple => (string)triple.o).ToList();

                result.Should().Contain.One("nosql");
                result.Should().Contain.One(".net");
                result.Should().Contain.One("rdf");
            }
            finally
            {
                if (client!=null && storeName!=null)
                    client.DeleteStore(storeName);
            }
        }

        private string GenerateTestStore()
        {
            client = BrightstarService.GetClient("type=embedded;storesdirectory=brightstar;");
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
    }
}
