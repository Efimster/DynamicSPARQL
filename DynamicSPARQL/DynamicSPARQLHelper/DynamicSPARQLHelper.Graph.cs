using System;
using System.Collections.Generic;


namespace DynamicSPARQLSpace
{
    /// <summary>
    /// Helps creating of SPARQL
    /// </summary>
    public static partial class SPARQL
    {
        /// <summary>
        /// Makes an NAMED GRAPH pattern
        /// </summary>
        /// <param name="IRI">NAMED GRAPH IRI</param>
        /// <param name="items">GRAPH pattern items</param>
        /// <returns>GRAPH graph pattern</returns>
        public static Graph Graph(string IRI, params IWhereItem[] items)
        {
            return new Graph (IRI,  items );
        }
    }
}
