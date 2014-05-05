using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    /// <summary>
    /// Result set of SPARQL query 
    /// </summary>
    public class SPARQLQueryResults : IEnumerable<SPARQLQueryResult>
    {
        private IList<SPARQLQueryResult> Results { get; set; }
        /// <summary>
        /// variables of result set
        /// </summary>
        public HashSet<string> Variables {get; private set;}
        public SPARQLQueryResults() 
        {
            Results = new List<SPARQLQueryResult>();
            Variables = new HashSet<string>();
        }
        /// <summary>
        /// Adds result row to result set
        /// </summary>
        /// <param name="result">result row</param>
        public void AddResult(SPARQLQueryResult result)
        {
            Results.Add(result);
            foreach (var name in result.Bindings.Select(bind => bind.Name))
                Variables.Add(name);
        }


        public IEnumerator<SPARQLQueryResult> GetEnumerator()
        {
            return Results.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    /// <summary>
    /// Result item of result set of SPARQL query
    /// </summary>
    public class SPARQLQueryResult : IEnumerable<ResultBinding>
    {
        public IList<ResultBinding> Bindings { get; private set; }

        public SPARQLQueryResult() 
        {
            Bindings = new List<ResultBinding>();
        }
        /// <summary>
        /// Adds result binding to result row
        /// </summary>
        /// <param name="binding">result binding</param>
        public void AddBinding(ResultBinding binding)
        {
            Bindings.Add(binding);
        }

        public IEnumerable<string> BindingNames { get { return Bindings.Select(bind => bind.Name); } }

        public IEnumerator<ResultBinding> GetEnumerator()
        {
            return Bindings.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Binding of result item of result set of SPARQL query
    /// </summary>
    public abstract class  ResultBinding
    {
        public BindingType Type { get; protected set; }
        /// <summary>
        /// Binding name
        /// </summary>
        public string Name { get; set; }
    }
    /// <summary>
    /// Literal binding of result item of result set of SPARQL query
    /// </summary>
    public class LiteralBinding : ResultBinding
    {
        public LiteralBinding() { Type = BindingType.Literal; }
        /// <summary>
        /// Iri of binding value type
        /// </summary>
        public Uri DataType { get; set; }
        /// <summary>
        /// Language of binding value type
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// Literal binding value
        /// </summary>
        public string Literal { get; set; }
    }
    /// <summary>
    /// Iri binding of result item of result set of SPARQL query
    /// </summary>
    public class IriBinding : ResultBinding
    {
        public IriBinding() { Type = BindingType.Iri; }
        /// <summary>
        /// IRI binding value
        /// </summary>
        public Uri Iri { get; set; }
    }


    /// <summary>
    /// Binding type of result item binding
    /// </summary>
    public enum BindingType
    {
        Literal,
        Iri
    }


}
