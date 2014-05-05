using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

namespace DynamicSPARQLSpace.dotNetRDF
{
    public class Connector
    {
        public ISparqlDataset Dataset { get; private set; }

        public Connector(ISparqlDataset dataset)
        {
            Dataset = dataset;
        }

        /// <summary>
        /// Constructs querying function for BrightstarDB
        /// </summary>
        /// <returns>Querying function</returns>
        public Func<string, SPARQLQueryResults> GetQueryingFunction()
        {
            Func<string, SPARQLQueryResults> queringFunction = xquery =>
            {
                var queryProcessor = new LeviathanQueryProcessor(Dataset);
                var dnRdfResultSet = queryProcessor.ProcessQuery(new SparqlQueryParser().ParseFromString(xquery)) as SparqlResultSet;
                var results = new SPARQLQueryResults();

                foreach (var dnRdfResult in dnRdfResultSet)
                {
                    var result = new SPARQLQueryResult();

                    foreach (var node in dnRdfResult)
                    {
                        ResultBinding binding = null;

                        if (node.Value.NodeType == VDS.RDF.NodeType.Uri)
                        {
                            var uriNode = node.Value as IUriNode;
                            var iriBinding = new IriBinding();
                            iriBinding.Iri = uriNode.Uri;
                            binding = iriBinding;
                        }
                        else if (node.Value.NodeType == VDS.RDF.NodeType.Literal)
                        {
                            var literalNode = node.Value as ILiteralNode;
                            var litBinding = new LiteralBinding();
                            litBinding.DataType = literalNode.DataType;
                            litBinding.Language = literalNode.Language;
                            litBinding.Literal = literalNode.Value;
                            binding = litBinding;
                        }
                        else
                            binding = new LiteralBinding();

                        binding.Name = node.Key;
                        result.AddBinding(binding);
                    }

                    results.AddResult(result);
                }

                return results;
            };

            return queringFunction;

        }
        /// <summary>
        /// Constructs Update function for dotNetRDF
        /// </summary>
        /// <returns>Update function</returns>
        public Func<string, object> GetUpdateFunction()
        {
            
            Func<string, object> func = update =>
            {
                var updProcessor = new LeviathanUpdateProcessor(Dataset);
                updProcessor.ProcessCommandSet(new SparqlUpdateParser().ParseFromString(update));
                return 0;
            };

            return func;

        }
    }

    
}
