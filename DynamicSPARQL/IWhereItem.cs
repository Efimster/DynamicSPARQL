
using System.Text;
namespace DynamicSPARQLSpace
{
    public interface IWhereItem
    {
        WhreItemType ItemType { get; }
        StringBuilder AppendToString(StringBuilder sb, 
            bool autoQuotation = false, 
            bool skipTriplesWithEmptyObject = false,
            bool mindAsterisk = false);
    }

    public enum WhreItemType
    {
        Triple,
        Optional,
        Group,
        Union,
        Filter,
        Bind,
        Minus,
        Exists,
        NotExists,
        Graph,
        None
    }
}