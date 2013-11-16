DynamicSPARQL helps making SPARQ queries to RDF sources


General usage:

1)Create function for quering specific source that takes query string and returns SparqlResultSet collection.
   Func<string, SparqlResultSet> QueringFunc;
2) Create a dynamic object 
    dynamic dyno = DynamicSPARQL.CreateDyno(QueringFunc);
3) Make a query
            IEnumerable<dynamic> results = dyno.Select(
                prefixes: new[] {
                        SPARQL.Prefix(":", "http://books.example/"),
                },
                projection: "(SUM(?lprice) AS ?totalPrice)",
                where: SPARQL.Group(
                    SPARQL.Tripple("?org :affiliates ?auth"),
                    SPARQL.Tripple("?auth :writesBook ?book"),
                    SPARQL.Tripple("?book :price ?lprice")
                ),
                groupBy:"?org",
                having: "(SUM(?lprice) > 10)"

            );
4) Enumerate results

				foreach(var obj in results)
					Console.WriteLine(obj.totalPrice);