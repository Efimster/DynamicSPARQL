using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;

namespace DynamicSPARQLSpace.Tests
{
    public static class TestDataProvider
    {
        public static dynamic GetDyno(string data, bool autoquotation = true, bool treatUri = true)
        {
            var graph = new Graph();
            graph.LoadFromString(data);

            Func<string, SparqlResultSet> sendSPARQLQuery = xquery => graph.ExecuteQuery(xquery) as SparqlResultSet;
            dynamic dyno = DynamicSPARQL.CreateDyno(sendSPARQLQuery, autoquotation, treatUri);

            return dyno;
        }
    }
}
