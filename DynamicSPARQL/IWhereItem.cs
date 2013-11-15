
using System.Text;
namespace DynamicSPARQLSpace
{
    public interface IWhereItem
    {
        WhreItemType ItemType { get; }
        StringBuilder AppendToString(StringBuilder sb, bool autoQuotation = false);
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
        None
    }
}