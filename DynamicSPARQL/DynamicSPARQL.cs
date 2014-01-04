using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using HelperExtensionsLibrary.ReflectionExtentions;
using HelperExtensionsLibrary.Strings;
using VDS.RDF;
using VDS.RDF.Query;

namespace DynamicSPARQLSpace
{
    public class DynamicSPARQL : DynamicObject
    {
        /// <summary>
        /// true: automatically adds missed quotes
        /// </summary>
        public bool AutoQuotation { get; private set; }
        /// <summary>
        /// true: result uri will be treated (fragment or last segment)
        /// </summary>
        public bool TreatUri { get; private set; }
        /// <summary>
        /// Quering function
        /// </summary>
        public Func<string, SparqlResultSet> QueryingFunc { get; private set; }
        /// <summary>
        /// Predefined prefixes
        /// </summary>
        public IList<Prefix> Prefixes { get; set; }
        /// <summary>
        /// Update query function
        /// </summary>
        public Action<string> UpdateQueryingFunc { get; private set; }

        /// <summary>
        /// Creates dynamic object for SPARQL querying
        /// </summary>
        /// <param name="queryingFunc">Function for SPARQL querying to RDF source </param>
        /// <param name="updateQueringFunc">Function for SPARQL UPDATE querying to RDF source. </param>
        /// <param name="autoquotation">true: automatically adds missed quotes</param>
        /// <param name="treatUri">true: result uri will be treated (fragment or last segment)</param>
        /// <param name="prefixes">Predefined prefixes</param>
        /// <returns></returns>
        public static dynamic CreateDyno(
            Func<string, SparqlResultSet> queryingFunc, 
            Action<string> updateQueringFunc = null,
            bool autoquotation = false, 
            bool treatUri = true, 
            IEnumerable<Prefix> prefixes = null)
        {
            return (dynamic)(new DynamicSPARQL(queryingFunc, updateQueringFunc,autoquotation, treatUri, prefixes));
        }

        private DynamicSPARQL(
            Func<string, SparqlResultSet> queringFunc,
            Action<string> updateQueringFunc,
            bool autoquotation = false,
            bool treatUri = true,
            IEnumerable<Prefix> prefixes = null)
        {
            QueryingFunc = queringFunc;
            UpdateQueryingFunc = updateQueringFunc;
            AutoQuotation = autoquotation;
            TreatUri = treatUri;
            Prefixes = prefixes == null ? new List<Prefix>(5) : new List<Prefix>(prefixes);
        }

        /// <summary>
        /// Processes dynamic method invocation
        /// </summary>
        /// <param name="binder">binder</param>
        /// <param name="args">argument values</param>
        /// <param name="result">output result</param>
        /// <returns>result enumeration</returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var info = binder.CallInfo;

            // accepting named args only!
            if (info.ArgumentNames.Count != args.Length)
                throw new InvalidOperationException("Please use named arguments for this type of query - prefixes, orderby, projection, where, etc");

            result = null;

            //first should be "Select, Construct, ASK, Describe"
            var op = binder.Name.ToLower();

            //the "select" and "update" methods are supported only
            if (op != "select" && op!="update")
                return false;

            if (op == "update" && UpdateQueryingFunc == null)
                throw new ArgumentNullException("updateQueryingFunc", "Please define function for SPARQL Update while DynamicSPARQL.CreateDyno(...) call");
                        

            IEnumerable<Prefix> prefixes = null;
            string projection = string.Empty;
            Group where = null;
            string orderBy = string.Empty;
            string limit = string.Empty;
            string offset = string.Empty;
            string groupBy = string.Empty;
            string having = string.Empty;
            Group delete = null;
            Group insert = null;

            for (int i = 0; i < args.Length; i++)
            {
                var name = info.ArgumentNames[i].ToLower();
                switch (name)
                {
                    case "prefixes":
                        prefixes = (IEnumerable<Prefix>)args[i];
                        break;
                    case "orderby":
                        orderBy = args[i].ToString();
                        break;
                    case "projection":
                        projection = args[i].ToString();
                        break;
                    case "delete":
                        delete = ParseGroup(args[i]);
                        break;
                    case "insert":
                        insert = ParseGroup(args[i]);
                        break;
                    case "where":
                        where = ParseGroup(args[i]);
                        break;
                    case "limit":
                        limit = args[i].ToString();
                        break;
                    case "offset":
                        offset = args[i].ToString();
                        break;
                    case "groupby":
                        groupBy = args[i].ToString();
                        break;
                    case "having":
                        having = args[i].ToString();
                        break;
                }
            }

            
            if (op=="select")
                result = Select<dynamic>(prefixes, projection, where, orderBy, groupBy, having, limit, offset);
            if (op == "update")
            {
                Update(prefixes, where, delete, insert);
                result = 0;
            }

            return true;
        }

        private Group ParseGroup(object arg)
        {
            return (arg as Group != null) ? (Group)arg :
                (arg as IWhereItem != null) ? new Group((IWhereItem)arg) :
                arg as IWhereItem[] != null ?  new Group((IWhereItem[])arg) : null;
        }

        /// <summary>
        /// SPARQL Select method
        /// </summary>
        /// <typeparam name="T">type of elemets</typeparam>
        /// <param name="prefixes">SPARQL prefixes</param>
        /// <param name="projection">SPARQL projection ("Select" statement)</param>
        /// <param name="where">SPARQL "where" statement</param>
        /// <param name="orderBy">SPARQL "order by" statement</param>
        /// <param name="groupBy">SPARQL "group by" statement</param>
        /// <param name="having">SPARQL "having" statement</param>
        /// <param name="limit">SPARQL "limit" statement</param>
        /// <param name="offset">SPARQL "offset" statement</param>
        /// <returns>enumeration of result objects</returns>
        public IEnumerable<T> Select<T>(IEnumerable<Prefix> prefixes = null, 
            string projection = null, 
            Group where = null, 
            string orderBy = null, 
            string groupBy = null, 
            string having = null, 
            string limit = null, 
            string offset = null)
        {
            if (where == null)
                where = new Group();

            where.NoBrackets = false;
            
            projection = string.Concat("SELECT", " ", string.IsNullOrEmpty(projection) ? "*" : projection);
            orderBy = !string.IsNullOrEmpty(orderBy) ? "ORDER BY " + orderBy : string.Empty;
            var wherestr = "WHERE " + where.AppendToString(new StringBuilder(), autoQuotation: AutoQuotation).ToString();
            limit = !string.IsNullOrEmpty(limit) ? "LIMIT " + limit : string.Empty;
            offset = !string.IsNullOrEmpty(offset) ? "OFFSET " + offset : string.Empty;
            groupBy = !string.IsNullOrEmpty(groupBy) ? "GROUP BY " + groupBy : string.Empty;
            having = !string.IsNullOrEmpty(having) ? string.Concat("HAVING (",having,")") : string.Empty;
            prefixes = prefixes == null ? new Prefix[] {} : prefixes;
            var prefixesStr = (Prefixes.Concat(prefixes)).GetPrefixesString();

            string query = new[] { prefixesStr, projection, wherestr, groupBy, having, orderBy, limit, offset }.Join2String(Environment.NewLine);

            if (typeof(T).FullName == "System.Object")
            {
                foreach (var res in Query(query))
                    yield return res;

                yield break;
            }


            foreach (var res in Query<T>(query))
                yield return res;
        }


        /// <summary>
        /// Queries an RDF source
        /// </summary>
        /// <param name="SPARQLQuery">SPARQL query</param>
        /// <returns>enumeration of result objects</returns>
        private IEnumerable<dynamic> Query(string SPARQLQuery)
        {
            var resultSet = QueryingFunc(SPARQLQuery);

            foreach (var result in resultSet)
            {
                IDictionary<string, object> item = new ExpandoObject();
                foreach (var node in result)
                {
                    var val = GetValue(node.Value);
                    item[node.Key] = val;
                }

                foreach (var key in resultSet.Variables.Except(item.Keys))
                {
                    item[key] = null;
                }
                
                yield return item;
            }



        }

        /// <summary>
        /// Queries an RDF source
        /// </summary>
        /// <typeparam name="T">items type</typeparam>
        /// <param name="SPARQLQuery">SPARQL query</param>
        /// <returns>enumeration of result objects</returns>
        private IEnumerable<T> Query<T>(string SPARQLQuery)
        {
            var resultSet = QueryingFunc(SPARQLQuery);

            foreach (var result in resultSet)
            {
                T item = Activator.CreateInstance<T>();
                dynamic dyno = new DynamicPropertiesObject<T>(item);

                foreach (var node in result)
                {
                    var val = GetValue(node.Value);
                    dyno[node.Key] = val;
                }

                yield return item;
            }

        }

        /// <summary>
        /// Resolves result node value
        /// </summary>
        /// <param name="node"></param>
        /// <returns>result node value</returns>
        private dynamic GetValue(INode node)
        {
            if (node.NodeType == VDS.RDF.NodeType.Uri)
            {
                var uri = (node as IUriNode).Uri;

                if (!TreatUri || string.IsNullOrEmpty(uri.LocalPath))
                    return uri.ToString();
                

                return string.IsNullOrEmpty(uri.Fragment) ? uri.Segments[uri.Segments.Length - 1] : uri.Fragment.Substring(1);
            }
            else if (node.NodeType == VDS.RDF.NodeType.Literal)
            {
                var val = node as ILiteralNode;
                if (val.DataType == null)
                    return val.Value; 

                switch (val.DataType.Fragment)
                {
                    case "#nonPositiveInteger":
                    case "#negativeInteger":
                    case "#int":
                    case "#integer": return int.Parse(val.Value);
                    case "#boolean": return bool.Parse(val.Value);
                    case "#decimal": return decimal.Parse(val.Value);
                    case "#date": 
                    case "#dateTime": return DateTime.Parse(val.Value);
                    case "#double": return double.Parse(val.Value);
                    case "#float": return float.Parse(val.Value);
                    case "#positiveInteger":
                    case "#nonNegativeInteger":
                    case "#unsignedInt": return uint.Parse(val.Value);
                    case "#long": return long.Parse(val.Value);
                    case "#short": return short.Parse(val.Value);
                    case "#unsignedByte": 
                    case "#byte": return byte.Parse(val.Value);
                    case "#unsignedShort": return ushort.Parse(val.Value);
                    case "#unsignedLong": return ulong.Parse(val.Value);


                    default: return val.Value; 
                }

            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">type of elements</typeparam>
        /// <param name="prefixes">SPARQL prefixes</param>
        /// <param name="where">SPARQL "where" statement</param>
        /// <param name="delete">SPARQL "delete" statement</param>
        /// <param name="insert">SPARQL "insert" statement</param>
        /// <returns></returns>
        public void Update(
            IEnumerable<Prefix> prefixes = null,
            Group where = null,
            Group delete = null,
            Group insert = null)
        {

            var updateFlag = where!=null ? 0x3 : ((delete==null ? 0 : 1)) | ((insert==null ? 0 : 1) << 1);
            if (updateFlag == 0)
                return;

            prefixes = prefixes == null ? new Prefix[] { } : prefixes;
            var prefixesStr = (Prefixes.Concat(prefixes)).GetPrefixesString();

            string query = null;

            if (updateFlag == 1)
            {
                delete.NoBrackets = false;
                var deletestr = "DELETE DATA " + delete.AppendToString(new StringBuilder(), autoQuotation: AutoQuotation).ToString();
                query = new[] {prefixesStr, deletestr}.Join2String(Environment.NewLine);
            }
            else if (updateFlag == 2)
            {
                insert.NoBrackets = false;
                var insertstr = "INSERT DATA " + insert.AppendToString(new StringBuilder(), autoQuotation: AutoQuotation).ToString();
                query = new[] {prefixesStr, insertstr }.Join2String(Environment.NewLine);
            }
            else if (updateFlag == 3)
            {
                string deletestr = string.Empty, insertstr = string.Empty, wherestr = string.Empty;
                if (delete != null)
                {
                    delete.NoBrackets = false;
                    deletestr = "DELETE " + delete.AppendToString(new StringBuilder(), autoQuotation: AutoQuotation).ToString();
                }
                    
                if (insert != null) 
                {
                    insert.NoBrackets = false;
                    insertstr = "INSERT " + insert.AppendToString(new StringBuilder(), autoQuotation: AutoQuotation).ToString();
                }

                if (where != null)
                {
                    where.NoBrackets = false;
                    wherestr = "WHERE " + where.AppendToString(new StringBuilder(), autoQuotation: AutoQuotation).ToString();
                }
                
                query = new[] { prefixesStr, deletestr, insertstr, wherestr }.Join2String(string.Empty);
            }

            UpdateQueryingFunc(query);            
        }
    }
   
}
