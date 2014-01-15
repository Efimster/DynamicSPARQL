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
        public Func<string, SparqlResultSet> GetQueringFunction()
        {
            Func<string, SparqlResultSet> queringFunction = xquery =>
            {
                var settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Auto;
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                XmlDocument doc = new XmlDocument();
                var reader = XmlReader.Create(Client.ExecuteQuery(StoreName, xquery, resultsFormat: SparqlResultsFormat.Xml), settings);
                var root = XElement.Load(reader, LoadOptions.None);
                foreach (XElement XE in root.DescendantsAndSelf())
                {
                    XE.Name = XE.Name.LocalName;
                    XE.ReplaceAttributes((from xattrib in XE.Attributes().Where(xa => !xa.IsNamespaceDeclaration) select new XAttribute(xattrib.Name.LocalName, xattrib.Value)));
                }
                var sp = root.Elements().Where(x => x.Name.LocalName == "head").First();
                foreach (var el in sp.Elements())
                {
                    var name = el.Attribute("name").Value;
                    el.RemoveAttributes();
                    el.Value = name;
                }

                sp.ReplaceWith(new XElement("variables", sp.Elements()));

                var settings2 = new XmlWriterSettings();
                settings2.OmitXmlDeclaration = true;
                settings2.NamespaceHandling = NamespaceHandling.OmitDuplicates;
                settings2.ConformanceLevel = ConformanceLevel.Fragment;

                var mem = new MemoryStream();
                var writer = XmlWriter.Create(mem, settings2);

                foreach (var el in root.Elements())
                    el.WriteTo(writer);

                writer.Dispose();
                mem.Seek(0, SeekOrigin.Begin);

                var result = new SparqlResultSet();
                result.ReadXml(XmlReader.Create(mem, settings));

                return result;
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
