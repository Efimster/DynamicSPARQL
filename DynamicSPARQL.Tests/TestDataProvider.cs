using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

namespace DynamicSPARQLSpace.Tests
{
    public static class TestDataProvider
    {
        public static dynamic GetDyno(string data, bool autoquotation = true, bool treatUri = true)
        {
            var graph = new Graph();
            graph.LoadFromString(data);
            var processor = new LeviathanUpdateProcessor(new InMemoryDataset(graph));
            


            Func<string, SparqlResultSet> sendSPARQLQuery = 
                xquery => graph.ExecuteQuery(xquery) as SparqlResultSet;
            Action<string> updateSPARQL = uquery => {
                processor.ProcessCommandSet(new SparqlUpdateParser().ParseFromString(uquery));
            };

            dynamic dyno = DynamicSPARQL.CreateDyno(sendSPARQLQuery, 
                updateQueringFunc: updateSPARQL,
                autoquotation: autoquotation, 
                treatUri: treatUri);

            return dyno;
        }
    }
}
