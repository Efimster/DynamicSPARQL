using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class Union :  Group, IWhereItem
    {
        //public IList<Group> Items { get; set; }

        public Union() : base() { }
        public Union(params IWhereItem[] items) : base(items) { }

        
        public Group Left { get { return Items[0] as Group; } set { Items[0] = value; } }
        public Group Right { get { return Items[1] as Group; } set { Items[1] = value; } }

        public Triple LeftTriple { get { return Left.Items[0] as Triple; } set { Left.Items[0] = value; } }
        public Triple RightTriple { get { return Right.Items[0] as Triple; } set { Right.Items[0] = value; } }

        public new WhreItemType ItemType { get { return WhreItemType.Union; } }

        public override StringBuilder AppendToString(StringBuilder sb, bool autoQuotation = false,
            bool skipTriplesWithEmptyObject = false, bool mindAsterisk = false)
        {
            if (Right == null)
                throw new MissingMemberException("Union right part is missing");
            
            sb = Left.AppendToString(sb, autoQuotation, skipTriplesWithEmptyObject, mindAsterisk);
            sb.AppendLine("UNION");
            Right.AppendToString(sb, autoQuotation, skipTriplesWithEmptyObject, mindAsterisk);

            return sb;
        }

        public override void Add(IWhereItem item)
        {
            if (item as Group ==null)
                item = new Group(item);

            if (Left.Count == 0)
                Items[0] = item;
            else
                Items[1] = item;
        }

        public override bool Remove(IWhereItem item)
        {
            throw new NotSupportedException("Use Left and Right properties instead");
        }

        public override void RemoveAt(int index)
        {
            throw new NotSupportedException("Use Left and Right properties instead");
        }

        public override void Insert(int index, IWhereItem item)
        {
            throw new NotSupportedException("Use Left and Right properties instead");
        }

        //#region IEnumerable<>
        //public IEnumerator<Group> GetEnumerator()
        //{
        //    return (IEnumerator<Group>)Items.GetEnumerator();
        //}

        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //{
        //    return Items.GetEnumerator();
        //}

        //#endregion

        //#region IList<>

        //public int IndexOf(Group item)
        //{
        //    return Items.IndexOf(item);
        //}

        //public void Insert(int index, Group item)
        //{
        //    Items.Insert(index, item);
        //}

        //public void RemoveAt(int index)
        //{
        //    Items.RemoveAt(index);
        //}

        //public Group this[int index]
        //{
        //    get
        //    {
        //        return Items[index];
        //    }
        //    set
        //    {
        //        Items[index] = value;
        //    }
        //}

        //public void Add(Group item)
        //{
        //    Items.Add(item);
        //}

        //public void Clear()
        //{
        //    Items.Clear();
        //}

        //public bool Contains(Group item)
        //{
        //    return Items.Contains(item);
        //}

        //public void CopyTo(Group[] array, int arrayIndex)
        //{
        //    throw new NotImplementedException();
        //}

        //public int Count
        //{
        //    get { return Items.Count; }
        //}

        //public bool IsReadOnly
        //{
        //    get { return Items.IsReadOnly; }
        //}

        //public bool Remove(Group item)
        //{
        //    return Items.Remove(item);
        //}

        //#endregion 


    }
}
