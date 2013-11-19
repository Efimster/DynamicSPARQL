using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using BrightstarDB;
using BrightstarDB.Client;
using VDS.RDF.Query;
using System.Text.RegularExpressions;
using HelperExtensionsLibrary.IEnumerable;

namespace DynamicSPARQLSpace.BrightstarDB
{
    public static class Connector
    {
        public static Func<string, SparqlResultSet> GetQueringFunction(string connectionString)
        {
            var client = BrightstarService.GetClient(connectionString);
            string storeName = Regex.Match(connectionString, @"storename=[\w-]+").Value.Split('=')[1];

            Func<string, SparqlResultSet> queringFunction = xquery =>
            {
                var settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Auto;
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                XmlDocument doc = new XmlDocument();
                var reader = XmlReader.Create(client.ExecuteQuery(storeName, xquery, resultsFormat: SparqlResultsFormat.Xml), settings);
                var root = XElement.Load(reader, LoadOptions.None);
                foreach (XElement XE in root.DescendantsAndSelf())
                {
                    XE.Name = XE.Name.LocalName;
                    XE.ReplaceAttributes((from xattrib in XE.Attributes().Where(xa => !xa.IsNamespaceDeclaration) select new XAttribute(xattrib.Name.LocalName, xattrib.Value)));
                }
                var sp = root.Elements().Where(x => x.Name.LocalName == "head").First();
                sp.Elements().ForEach(el =>
                {
                    var name = el.Attribute("name").Value;
                    el.RemoveAttributes();
                    el.Value = name;
                });

                sp.ReplaceWith(new XElement("variables", sp.Elements()));

                var settings2 = new XmlWriterSettings();
                settings2.OmitXmlDeclaration = true;
                settings2.NamespaceHandling = NamespaceHandling.OmitDuplicates;
                settings2.ConformanceLevel = ConformanceLevel.Fragment;

                var mem = new MemoryStream();
                var writer = XmlWriter.Create(mem, settings2);

                root.Elements().ForEach(el => el.WriteTo(writer));
                writer.Dispose();
                mem.Seek(0, SeekOrigin.Begin);

                var result = new SparqlResultSet();
                result.ReadXml(XmlReader.Create(mem, settings));

                return result;
            };

            return queringFunction;

        }

    }
}
