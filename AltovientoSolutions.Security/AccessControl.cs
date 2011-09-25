using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AltovientoSolutions.Security
{
    public class AccessControl
    {
        /// <summary>
        /// In the objects passed as arguments compares the properties that are decorated with the <see cref="ContextualSecurityAttribute"/> and if the values are the same or there are any intersections returns true.
        /// </summary>
        /// <param name="securableObject">The obj1.</param>
        /// <param name="subject">The obj2.</param>
        /// <returns></returns>
        public static bool CanAccess(object securableObject, object subject)
        {
            Type t = securableObject.GetType();
            Type g = subject.GetType();


            // Get the properties to compare.
            PropertyInfo propT = null;
            PropertyInfo propG = null;

            foreach (PropertyInfo p in t.GetProperties())
            {
                ContextualSecurityAttribute[] attr = (ContextualSecurityAttribute[])p.GetCustomAttributes(typeof(ContextualSecurityAttribute), true);
                if (attr.Length > 0)
                {
                    //found a property decorated with the ContextualSecurityAttribute.
                    propT = p;
                    break;
                }
            }

            // now for G
            foreach (PropertyInfo p in g.GetProperties())
            {
                ContextualSecurityAttribute[] attr = (ContextualSecurityAttribute[])p.GetCustomAttributes(typeof(ContextualSecurityAttribute), true);
                if (attr.Length > 0)
                {
                    //found a property decorated with the ContextualSecurityAttribute.
                    propG = p;
                    break;
                }
            }


            if (propT == null || propG == null)
                return false;  // can not compare them.




            // if both properties are of the same primitive type, one being an enumerable, the other being a simple type, they can be compared.

            Type baseTypeT = GetPrimitiveType(propT.PropertyType);
            Type baseTypeG = GetPrimitiveType(propG.PropertyType);
           
            
            //if the two properties are not of the same type, they cannot be compared.
            if (!baseTypeT.Equals(baseTypeG))
                return false;

            // Do the actual comparison now.

            //Does any of the values belong 

            Type genericList = typeof(List<>);


            Type typeListT, typeListG;

            if (!propT.PropertyType.IsGenericType)
            {
                typeListT = genericList.MakeGenericType(new Type[] { propT.PropertyType });
            }
            else
            {
                typeListT = propT.PropertyType;
            }

            if (!propG.PropertyType.IsGenericType)
            {
                typeListG = genericList.MakeGenericType(new Type[] { propG.PropertyType });
            }
            else
            {
                typeListG = propG.PropertyType;
            }



            object listT = Activator.CreateInstance(typeListT);
            object listG = Activator.CreateInstance(typeListG);


            if (propT.PropertyType.IsPrimitive || propT.PropertyType == typeof(string))
            {
                typeListT.InvokeMember("Add", BindingFlags.InvokeMethod, null, listT, new object[] { propT.GetValue(securableObject, null) });
            }
            else
            {
                typeListT.InvokeMember("AddRange", BindingFlags.InvokeMethod, null, listT, new object[] { propT.GetValue(securableObject, null) });
            }

            if (propG.PropertyType.IsPrimitive || propG.PropertyType == typeof(string))
            {
                typeListG.InvokeMember("Add", BindingFlags.InvokeMethod, null, listG, new object[] { propG.GetValue(subject, null) });
            }
            else
            {
                typeListG.InvokeMember("AddRange", BindingFlags.InvokeMethod, null, listG, new object[] { propG.GetValue(subject, null) });
            }

            // Check that there is an intersection of objects...

            foreach (object objT in (System.Collections.IEnumerable) listT)
            {
                foreach (object objG in (System.Collections.IEnumerable) listG)
                {
                    if (objT.Equals(objG))
                        return true;
                }
            }


            return false;
        }

        private static Type GetPrimitiveType(Type t)
        {
            Type baseTypeT;

            if (t.IsPrimitive || t.Equals(typeof(string)))
            {
                baseTypeT = t;
            }
            else
            {
                // complex
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type[] genericArguments = t.GetGenericArguments();
                    if (genericArguments.Length == 1) // only one generic argument like List<T>
                    {
                        baseTypeT = genericArguments[0];
                    }
                    else
                    {
                        baseTypeT = null;
                    }
                }
                else
                {
                    baseTypeT = null;
                }
            }

            return baseTypeT;
        }



    }
}
