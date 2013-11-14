using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class Triple : IWhereItem
    {
        public string Subject { get; set; }
        public dynamic Property { get; set; }
        public dynamic Object { get; set; }


        public override string ToString()
        {
            return ToStringBuilder().ToString();
        }

        public virtual StringBuilder AppendToString(StringBuilder sb, bool autoQuotation = false)
        {
            return sb.Append(autoQuotation ? ToStringBuilder().ToString().AutoquoteSPARQL() : ToStringBuilder().ToString());        
        }

        private StringBuilder ToStringBuilder()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(Property as string))
            {
                if ((Object as IList) == null)
                    return sb.AppendLine(string.Concat(Subject, " ", Property, " ", Object, " ."));

                if (Object.Length > 0)
                {
                    sb.Append(string.Concat(Subject, " ", Property, " "));

                    foreach (var obj in Object)
                        sb.Append(obj + ", ");

                    sb.Remove(sb.Length - ", ".Length, ", ".Length);
                    sb.AppendLine(" .");
                }

                return sb;
            }

            IList prop = Property as IList;
            if (prop != null && prop.Count > 0)
            {
                sb.Append(Subject + " ");
                foreach (var propObj in Property)
                    sb.Append(propObj + "; ");

                sb.Remove(sb.Length - "; ".Length, "; ".Length);
                sb.AppendLine(" .");
            }

            return sb;
        }


        public WhreItemType ItemType { get { return WhreItemType.Triple; } }
    }
}
