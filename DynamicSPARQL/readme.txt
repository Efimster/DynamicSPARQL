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
            SPARQL.Triple("?org :affiliates ?auth"),
            SPARQL.Triple("?auth :writesBook ?book"),
            SPARQL.Triple("?book :price ?lprice")
        ),
        groupBy:"?org",
        having: "(SUM(?lprice) > 10)"

    );
4) Enumerate results
		foreach(var obj in results)
			Console.WriteLine(obj.totalPrice);

Select function parameters (ignorecase) :
	prefixes: array of SPARQL.Prefix
	projection:string
	where:   SPARQL.Group with a combination of SPARQL.Triple, SPARQL.Optional, SPARQL.Group, SPARQL.Union, SPARQL.Filter, SPARQL.Exists, SPARQL.NotExists, SPARQL.Minus, SPARQL.Bind, SPARQL.TripleChain
	groupBy: string
	having: string
	orderBy": string
	limit: integer
	offset: integer

Examples:

    IEnumerable<dynamic> list = dyno.Select(
            projection: "?s ?o",
            where: SPARQL.Triple(s: "?s", p: "cp:boolean", o: "?o")
	);


        dyno.Select(
            prefixes: new[] { 
                SPARQL.Prefix("dc:", "http://purl.org/dc/elements/1.1/"),//prefix could be used with colon
                SPARQL.Prefix("ns", "http://example.org/ns#")//or without colon
            },
            projection: "?title ?price",
            where: SPARQL.Group(
                SPARQL.Triple("?x dc:title ?title"),
                SPARQL.Optional(
                    SPARQL.Triple("?x ns:price ?price"),
                    SPARQL.Filter("?price < 30"))
            )
        );

        dyno.Select(
            prefixes: new[] { 
                SPARQL.Prefix("foaf:", "http://xmlns.com/foaf/0.1/"),
                SPARQL.Prefix("rdf:","http://www.w3.org/1999/02/22-rdf-syntax-ns#")
            },
            projection: "?person",
            where: SPARQL.Group(
                SPARQL.Triple("?person rdf:type  foaf:Person"),
                SPARQL.Exists("?person foaf:name ?name")
            )
        );
            
		dyno.Select(
            prefixes: new[] {
                SPARQL.Prefix("dc10", "http://purl.org/dc/elements/1.0/"),
                SPARQL.Prefix("dc11", "http://purl.org/dc/elements/1.1/")
            },
            projection: "?title",
            where: SPARQL.Group(
                SPARQL.Union(left: SPARQL.Triple("?book dc10:title  ?title"), right: SPARQL.Triple("?book dc11:title  ?title"))
            )
        );

        dyno.Select(
            prefixes: new[] {
                                        SPARQL.Prefix("dc10", "http://purl.org/dc/elements/1.0/"),
                                        SPARQL.Prefix("dc11", "http://purl.org/dc/elements/1.1/")
                                    },
            projection: "?title ?author",
            where: SPARQL.Group(
                SPARQL.Union(
                    left: SPARQL.Group(SPARQL.Triple("?book dc10:title ?title"), SPARQL.Triple("?book dc10:creator ?author")),
                    right: SPARQL.Group(SPARQL.Triple("?book dc11:title ?title"), SPARQL.Triple("?book dc11:creator ?author"))
                )
            )
        );


see other examles within unit tests.

CreateDyno parameters:
 
 queringFunc - Function for SPARQL querying of RDF source 
 autoquotation - true: automatically adds missed quotes. Avoid use with regular expression filtering.
 treatUri - true: result uri will be treated (fragment or last segment)
 prefixes - predefined prefixes


 Typed projection:

  DynamicSPARQL dyno = DynamicSPARQL.CreateDyno(QueringFunc);

	Book book = dyno.Select<Book>(
		prefixes: new[] { 
			SPARQL.Prefix("dc", "http://purl.org/dc/elements/1.1/"),
			SPARQL.Prefix("ns", "http://example.org/ns#"),
		},
		projection: "?title ?price",
		where: SPARQL.Group(
			SPARQL.Triple("?x ns:price ?price"),
			SPARQL.Triple("?x dc:title ?title")
		),
		orderBy: "desc(?price)"
	)
	.First();
