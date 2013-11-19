using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicSPARQLSpace
{
    public class DynamicPropertiesObject<T> : DynamicObject
        //where T : class, new()
    {
        private class GetSetDelegates
        {
            public Delegate GetDelegate { get; set; }
            public Delegate SetDelegate { get; set; }
        }


        public T Obj { get; private set; }

        private static Dictionary<string, GetSetDelegates> HoleProperties { get; set; }

        public static dynamic CreateDyno(T user)
        {
            return (dynamic)(new DynamicPropertiesObject<T>(user));
        }

        static DynamicPropertiesObject()
        {
            HoleProperties = new Dictionary<string, GetSetDelegates>(5);
            //ConstructAllPropertiesDelegates();

        }


        public DynamicPropertiesObject(T obj)
        {
            Obj = obj;
        }


        private bool TryGetGetPropertyDelegate(string propertyName, out Delegate get)
        {
            GetSetDelegates xprop;
            if (HoleProperties.TryGetValue(propertyName, out xprop))
            {
                get = xprop.GetDelegate;
                if (get != null)
                    return true;
            }

            get = null;
            return false;
        }

        private bool TryGetSetPropertyDelegate(string propertyName, out Delegate set)
        {
            GetSetDelegates xprop;
            if (HoleProperties.TryGetValue(propertyName, out xprop))
            {
                set = xprop.SetDelegate;
                if (set != null)
                    return true;
            }

            set = null;
            return false;
        }

        private static void AddSetPropertyDelegate(string propertyName, Delegate set)
        {
            GetSetDelegates xprop;
            if (!HoleProperties.TryGetValue(propertyName, out xprop))
            {
                xprop = new GetSetDelegates();
                HoleProperties.Add(propertyName, xprop);
            }

            xprop.SetDelegate = set;
        }

        private static void AddGetPropertyDelegate(string propertyName, Delegate get)
        {
            GetSetDelegates xprop;
            if (!HoleProperties.TryGetValue(propertyName, out xprop))
            {
                xprop = new GetSetDelegates();
                HoleProperties.Add(propertyName, xprop);
            }

            xprop.GetDelegate = get;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            string prop = indexes[0].ToString();
            Delegate xprop;
            if (!TryGetSetPropertyDelegate(prop, out xprop))
            {
                AddSetPropertyDelegate(prop, xprop = ConstructSetDelegate(prop).Compile());
            }
            
            xprop.DynamicInvoke(this.Obj, Convert.ChangeType(value,xprop.Method.ReturnType));

            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            string prop = indexes[0].ToString();
            Delegate xprop;
            if (!TryGetGetPropertyDelegate(prop, out xprop))
            {
                AddGetPropertyDelegate(prop, xprop = ConstructGetDelegate(prop).Compile());
            }

            result = xprop.DynamicInvoke(this.Obj);

            return true;
        }

        private static LambdaExpression ConstructGetDelegate(string propertyName)
        {
            ParameterExpression param1 = Expression.Parameter(typeof(T), "TObj1");
            MemberExpression property = Expression.Property(param1, propertyName);

            return Expression.Lambda(property, param1);
        }

        private static LambdaExpression ConstructSetDelegate(string propertyName)
        {
            ParameterExpression param1 = Expression.Parameter(typeof(T), "TObj1");
            MemberExpression property = Expression.Property(param1, propertyName);
            ParameterExpression param2 = Expression.Parameter(property.Type, "TSetValue1");

            BinaryExpression ass = Expression.Assign(property, param2);
            return Expression.Lambda(ass, param1, param2);
        }

        public static void ConstructAllPropertiesDelegates()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var prop in properties)
            {
                if (prop.CanWrite && prop.CanRead)
                {
                    AddSetPropertyDelegate(prop.Name, ConstructSetDelegate(prop.Name).Compile());
                    AddGetPropertyDelegate(prop.Name, ConstructGetDelegate(prop.Name).Compile());
                }

            }
        }

        public static object GetDefaultValue(Type t)
        {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }


    }

}
