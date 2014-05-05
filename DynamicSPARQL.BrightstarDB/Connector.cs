using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using BrightstarDB;
using BrightstarDB.Client;
//using HelperExtensionsLibrary.IEnumerable;
using VDS.RDF.Query;

namespace DynamicSPARQLSpace.BrightstarDB
{
    public class Connector
    {

        public IBrightstarService Client { get; private set; }
        public string StoreName { get; private set; }

        public Connector(string connectionString)
        {
            Client = BrightstarService.GetClient(connectionString);
            StoreName = Regex.Match(connectionString, @"storename=[\w-]+").Value.Split('=')[1];
        }

        /// <summary>
        /// Constructs querying function for BrightstarDB
        /// </summary>
        /// <returns>Querying function</returns>
        public Func<string, SPARQLQueryResults> GetQueryingFunction()
        {
            Func<string, SPARQLQueryResults> queringFunction = xquery =>
            {
                var settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Auto;
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                XmlDocument doc = new XmlDocument();
                var reader = XmlReader.Create(Client.ExecuteQuery(StoreName, xquery, resultsFormat: SparqlResultsFormat.Xml), settings);
                var root = XElement.Load(reader, LoadOptions.None);
                var resultSet = new SPARQLQueryResults();
                root = root.Elements().First(x => x.Name.LocalName.ToLower() == "results");
                foreach (var resultElement in root.Elements())
                {
                    var result = new SPARQLQueryResult();
                    foreach (var bindingElement in resultElement.Elements())
                    {
                        var valueElement = bindingElement.Elements().First();
                        ResultBinding binding = null;
                        if (valueElement.Name.LocalName.ToLower() == "literal")
                        {
                            var literalBinding = new LiteralBinding();
                            literalBinding.Literal = valueElement.Value;
                            binding = literalBinding;
                            var attribute = valueElement.Attributes().FirstOrDefault(attr => attr.Name.LocalName.ToLower() == "datatype");
                            if (attribute != null)
                                literalBinding.DataType = new Uri(attribute.Value);
                            attribute = valueElement.Attributes().FirstOrDefault(attr => attr.Name.LocalName.ToLower() == "lang");
                            if (attribute != null)
                                literalBinding.Language = attribute.Value;
                            
                        }
                        else if (valueElement.Name.LocalName.ToLower() == "uri")
                        {
                            var iriBinding = new IriBinding();
                            iriBinding.Iri = new Uri(valueElement.Value);
                            binding = iriBinding;
                        }
                        binding.Name = bindingElement.Attributes().First(attr => attr.Name.LocalName.ToLower() == "name").Value;
                        result.AddBinding(binding);
                         
                    }
                    resultSet.AddResult(result);
                }

                return resultSet;
            };

            return queringFunction;

        }
        /// <summary>
        /// Constructs Update function for BrightstarDB
        /// </summary>
        /// <returns>Update function</returns>
        public Func<string, object> GetUpdateFunction()
        {
            Func<string, IJobInfo> func = query =>
            {
                return Client.ExecuteUpdate(StoreName, query);
            };

            return func;

        }
    }

    
}
