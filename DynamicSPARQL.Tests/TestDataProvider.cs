using System;
using DynamicSPARQLSpace.dotNetRDF;
using VDS.RDF;
using VDS.RDF.Query.Datasets;

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
            DynamicSPARQLSpace.dotNetRDF.Connector connector = null;
           
           
            if (useStore)
            {
                var store = new VDS.RDF.TripleStore();
                store.LoadFromString(data);
                connector =  new Connector(new InMemoryDataset(store, new Uri(defaultGraphUri)));
            }
            else
            {
                var graph = new VDS.RDF.Graph();
                graph.LoadFromString(data);
                connector =  new Connector(new InMemoryDataset(graph));
            }

            dynamic dyno = DynamicSPARQL.CreateDyno(connector.GetQueryingFunction(), 
                updateFunc: connector.GetUpdateFunction(),
                autoquotation: autoquotation, 
                treatUri: treatUri,
                skipTriplesWithEmptyObject:skipTriplesWithEmptyObject,
                mindAsterisk:mindAsterisk);

            return dyno;
        }
    }
}
