using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class Group : IWhereItem, IEnumerable<IWhereItem>, IList<IWhereItem>, ICollection<IWhereItem>
    {
        public IList<IWhereItem> Items { get; set; }
        public bool NoBrackets { get; set; }

        public Group(bool noBrackets = false)
        {
            NoBrackets = noBrackets;
        }

        public Group(params IWhereItem[] items)
        {
            NoBrackets = false;
            Items = new List<IWhereItem>(items);
        }

        #region IEnumerable<>
        public IEnumerator<IWhereItem> GetEnumerator()
        {
            return (IEnumerator<IWhereItem>)Items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        #endregion

        public WhreItemType ItemType { get { return WhreItemType.Group; } }


        public virtual StringBuilder AppendToString(StringBuilder sb, bool autoQuotation = false)
        {
            if (!NoBrackets)
                sb.AppendLine("{");
            foreach (IWhereItem item in Items)
            {
                sb = item.AppendToString(sb, autoQuotation);
            }

            if (!NoBrackets)
                sb.AppendLine("}");

            return sb;
        }

        public override string ToString()
        {
            return AppendToString(new StringBuilder()).ToString();
        }

        #region IList<>

        public int IndexOf(IWhereItem item)
        {
            return Items.IndexOf(item);
        }

        public virtual void Insert(int index, IWhereItem item)
        {
            Items.Insert(index, item);
        }

        public virtual void RemoveAt(int index)
        {
            Items.RemoveAt(index);
        }

        public IWhereItem this[int index]
        {
            get
            {
                return Items[index];
            }
            set
            {
                Items[index] = value;
            }
        }

        public virtual void Add(IWhereItem item)
        {
            Items.Add(item);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool Contains(IWhereItem item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(IWhereItem[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return Items!=null ? Items.Count : 0; }
        }

        public bool IsReadOnly
        {
            get { return Items.IsReadOnly; }
        }

        public virtual bool Remove(IWhereItem item)
        {
            return Items.Remove(item);
        }

        #endregion 
    }
}
