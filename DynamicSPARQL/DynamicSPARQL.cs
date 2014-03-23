using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using HelperExtensionsLibrary.ReflectionExtentions;
using VDS.RDF;
using VDS.RDF.Query;

namespace DynamicSPARQLSpace
{
    public class DynamicSPARQL : DynamicObject
    {
        public  static readonly string[] OPERATIONS = {"select","update","delete","insert"};
        
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
        public Func<string, object> UpdateFunc { get; private set; }
        /// <summary>
        /// Print of last query
        /// </summary>
        public string LastQueryPrint { get; private set; }
        /// <summary>
        /// Defines whether to skip triples with empty object
        /// </summary>
        public bool SkipTriplesWithEmptyObject { get; private set; }
        /// <summary>
        ///  Determines whether to interpret '?*' as 'some variable' (don't care which one)
        /// </summary>
        public bool MindAsterisk { get; private set; }


        /// <summary>
        /// Creates dynamic object for SPARQL querying
        /// </summary>
        /// <param name="queryingFunc">Function for SPARQL querying to RDF source </param>
        /// <param name="updateFunc">Function for SPARQL UPDATE querying to RDF source. </param>
        /// <param name="autoquotation">true: automatically adds missed quotes</param>
        /// <param name="treatUri">true: result uri will be treated (fragment or last segment)</param>
        /// <param name="prefixes">Predefined prefixes</param>
        /// <param name="skipTriplesWithEmptyObject">Defines whether to skip triples with empty object</param>
        /// <param name="mindAsterisk">Determines whether to interpret '?*' as 'some variable' (don't care which one)</param>
        /// <returns>DynamicSPARQL object</returns>
        public static dynamic CreateDyno(
            Func<string, SparqlResultSet> queryingFunc, 
            Func<string,object> updateFunc = null,
            bool autoquotation = false, 
            bool treatUri = true, 
            IEnumerable<Prefix> prefixes = null,
            bool skipTriplesWithEmptyObject = false,
            bool mindAsterisk = false)
        {
            return (dynamic)(new DynamicSPARQL(queryingFunc, updateFunc,autoquotation, treatUri, prefixes, 
                skipTriplesWithEmptyObject, mindAsterisk));
        }

        private DynamicSPARQL(
            Func<string, SparqlResultSet> queringFunc,
            Func<string,object> updateQueringFunc,
            bool autoquotation = false,
            bool treatUri = true,
            IEnumerable<Prefix> prefixes = null,
            bool skipTriplesWithEmptyObject = false,
            bool mindAsterisk = false)
        {
            QueryingFunc = queringFunc;
            UpdateFunc = updateQueringFunc;
            AutoQuotation = autoquotation;
            TreatUri = treatUri;
            Prefixes = prefixes == null ? new List<Prefix>(5) : new List<Prefix>(prefixes);
            SkipTriplesWithEmptyObject = skipTriplesWithEmptyObject;
            MindAsterisk = mindAsterisk;
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

            //supported methods 
            if (!OPERATIONS.Contains(op))
                return false;

            if (op != "select" && UpdateFunc == null)
                throw new ArgumentNullException("updateQueryingFunc", "Please define function for SPARQL Update while DynamicSPARQL.CreateDyno(...) call");
                        

            IEnumerable<Prefix> prefixes = null;
            From from = null;
            Using usingClause = null;
            With with = null;
            IEnumerable<FromNamed> fromNamed = null;
            IEnumerable<UsingNamed> usingNamed = null;
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
                    case "from":
                        from = new From((string)args[i]);
                        break;
                    case "using":
                        usingClause = new Using((string)args[i]);
                        break;
                    case "with":
                        with = new With((string)args[i]);
                        break;
                    case "fromnamed":
                        fromNamed = FromNamed.Parse(args[i]);
                        break;
                    case "usingnamed":
                        usingNamed = UsingNamed.Parse(args[i]);
                        break;

                }
            }

            
            if (op=="select")
            {
                result = Select<dynamic>(prefixes, projection, where, orderBy, groupBy, having, limit, offset, 
                    from, fromNamed);
                return true;
            }


            if (op == "update")
                result = Update(prefixes, where, delete, insert, with:with, usingClause:usingClause, usingNamed:usingNamed);
            else if (op == "delete")
                result = Delete(prefixes, where, delete);
            else if (op == "insert")
                result = Insert(prefixes, where, insert);
            else
                result = 0;
            
            return true;
        }
        /// <summary>
        /// Parse group pattern
        /// </summary>
        /// <param name="arg">Named parameter value</param>
        /// <returns>Group pattern</returns>
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
            string offset = null,
            From from = null,
            IEnumerable<FromNamed> fromNamed = null)
        {
            if (where == null)
                where = new Group();

            where.NoBrackets = false;
            
            projection = string.Concat("SELECT", " ", string.IsNullOrEmpty(projection) ? "*" : projection);
            orderBy = !string.IsNullOrEmpty(orderBy) ? "ORDER BY " + orderBy : string.Empty;
            var wherestr = "WHERE " + where.ToString(AutoQuotation, SkipTriplesWithEmptyObject, MindAsterisk);
            limit = !string.IsNullOrEmpty(limit) ? "LIMIT " + limit : string.Empty;
            offset = !string.IsNullOrEmpty(offset) ? "OFFSET " + offset : string.Empty;
            groupBy = !string.IsNullOrEmpty(groupBy) ? "GROUP BY " + groupBy : string.Empty;
            having = !string.IsNullOrEmpty(having) ? string.Concat("HAVING (",having,")") : string.Empty;
            prefixes = prefixes == null ? new Prefix[] {} : prefixes;
            var prefixesStr = Prefix.Collection2String((Prefixes.Concat(prefixes)));
            var fromStr = from != null ? from.ToString() : string.Empty;
            var fromNamedStr = FromNamed.Collection2String(fromNamed);

            string query = string.Join(Environment.NewLine, new[] { prefixesStr, projection, fromStr, fromNamedStr, wherestr, groupBy, having, orderBy, limit, offset});


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
            LastQueryPrint = SPARQLQuery;

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
        /// <param name="node">Node</param>
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

        private string GetPrefixesString(IEnumerable<Prefix> prefixes)
        {
            prefixes = prefixes == null ? new Prefix[] { } : prefixes;
            return Prefix.Collection2String((Prefixes.Concat(prefixes)));
        }

        /// <summary>
        /// SPARQL Update (Delete/Insert)
        /// </summary>
        /// <typeparam name="T">type of elements</typeparam>
        /// <param name="prefixes">SPARQL prefixes</param>
        /// <param name="where">SPARQL "where" statement</param>
        /// <param name="delete">SPARQL "delete" statement</param>
        /// <param name="insert">SPARQL "insert" statement</param>
         public object Update(
            IEnumerable<Prefix> prefixes = null,
            Group where = null,
            Group delete = null,
            Group insert = null,
            With with = null,
            Using usingClause = null,
            IEnumerable<UsingNamed> usingNamed = null
             )
        {
            var prefixesStr = GetPrefixesString(prefixes);

            string query = null;


            string deletestr = string.Empty, insertstr = string.Empty, wherestr = string.Empty;
            if (delete != null)
            {
                delete.NoBrackets = false;
                deletestr = "DELETE " + delete.ToString(AutoQuotation, SkipTriplesWithEmptyObject, MindAsterisk);
            }
                    
            if (insert != null) 
            {
                insert.NoBrackets = false;
                insertstr = "INSERT " + insert.ToString(AutoQuotation, SkipTriplesWithEmptyObject, MindAsterisk);
            }

            if (where != null)
            {
                where.NoBrackets = false;
                wherestr = "WHERE " + where.ToString(AutoQuotation, SkipTriplesWithEmptyObject, MindAsterisk);
            }

            var withstr = with != null ? with.ToString() : string.Empty;
            var usingstr = usingClause != null ? usingClause.ToString() : string.Empty;
            var usingNamedStr = usingNamed != null ? UsingNamed.Collection2String(usingNamed) : string.Empty;
                
            LastQueryPrint = query = string.Join(string.Empty, new[] { prefixesStr, withstr, deletestr, insertstr, 
                usingstr, usingNamedStr, wherestr });

            return UpdateFunc(query);            
        }
        /// <summary>
        /// SPARQL Update (DELETE DATA & DELETE WHERE)
        /// </summary>
        /// <param name="prefixes">SPARQL prefixes</param>
        /// <param name="where">SPARQL "where" statement</param>
        /// <param name="delete">SPARQL "delete" statement</param>
        public object Delete(
            IEnumerable<Prefix> prefixes = null,
            Group where = null,
            Group delete = null)
        {

            if (where != null && delete != null)
                return Update(prefixes,where,delete,null);

            var prefixesStr = GetPrefixesString(prefixes);
            string query = string.Empty;

            if (delete != null)
            {
                delete.NoBrackets = false;
                var deletestr = "DELETE DATA " + delete.ToString(AutoQuotation, SkipTriplesWithEmptyObject, MindAsterisk);
                LastQueryPrint = query = string.Join(Environment.NewLine, new[] { prefixesStr, deletestr });
                return UpdateFunc(query);
            }

            if (where!=null)
            {
                where.NoBrackets = false;
                var deletestr = "DELETE WHERE " + where.ToString(AutoQuotation, SkipTriplesWithEmptyObject, MindAsterisk);
                LastQueryPrint = query = string.Join(Environment.NewLine, new[] { prefixesStr, deletestr });
                return UpdateFunc(query);
            }

            return 0;
        }
        /// <summary>
        /// SPARQL Update (INSERT DATA)
        /// </summary>
        /// <param name="prefixes">SPARQL prefixes</param>
        /// <param name="where">SPARQL "where" statement</param>
        /// <param name="insert">SPARQL "insert" statement</param>
        public object Insert(
            IEnumerable<Prefix> prefixes = null,
            Group where = null,
            Group insert = null)
        {

            if (where != null && insert != null)
                return Update(prefixes, where, null, insert);


            var prefixesStr = GetPrefixesString(prefixes);
            string query = string.Empty;

            if (insert != null)
            {
                insert.NoBrackets = false;
                var insertstr = "INSERT DATA " + insert.ToString(AutoQuotation, SkipTriplesWithEmptyObject, MindAsterisk);
                LastQueryPrint = query = string.Join(Environment.NewLine, new[] { prefixesStr, insertstr });
                return UpdateFunc(query);
            }

            return 0;
        }

        
    }
   
}
