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
        public static dynamic GetDyno(string data, bool autoquotation = true, 
            bool treatUri = true,
            bool skipTriplesWithEmptyObject = false,
            bool mindAsterisk = false,
            bool useStore = false,
            string defaultGraphUri = "http://test.org/defaultgraph")
        {
            InMemoryDataset dataset;
            Func<string, SparqlResultSet> sendSPARQLQuery;
            
            if (useStore)
            {
                var store = new VDS.RDF.TripleStore();
                dataset = new InMemoryDataset(store, new Uri(defaultGraphUri));

                store.LoadFromString(data);
                var queryProcessor = new LeviathanQueryProcessor(dataset);
                sendSPARQLQuery =  xquery => queryProcessor.ProcessQuery(new SparqlQueryParser().ParseFromString(xquery)) as SparqlResultSet;
            }
            else
            {
                var graph = new VDS.RDF.Graph();
                graph.LoadFromString(data);
                dataset = new InMemoryDataset(graph);
                sendSPARQLQuery = xquery => graph.ExecuteQuery(new SparqlQueryParser().ParseFromString(xquery)) as SparqlResultSet;
            }
            
            var updProcessor = new LeviathanUpdateProcessor(dataset);

            Func<string,object> updateSPARQL = uquery => {
                updProcessor.ProcessCommandSet(new SparqlUpdateParser().ParseFromString(uquery));
                return 0;
            };

            dynamic dyno = DynamicSPARQL.CreateDyno(sendSPARQLQuery, 
                updateFunc: updateSPARQL,
                autoquotation: autoquotation, 
                treatUri: treatUri,
                skipTriplesWithEmptyObject:skipTriplesWithEmptyObject,
                mindAsterisk:mindAsterisk);

            return dyno;
        }
    }
}
