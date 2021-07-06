using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SiamCross.Models.Tools
{
    public static class ClassPropertiesConverter
    {
        public static T CreateDelegate<T>(this DynamicMethod dm) where T : class
        {
            return dm.CreateDelegate(typeof(T)) as T;
        }

        static Dictionary<Type, Func<object, Dictionary<string, object>>> cache =
        new Dictionary<Type, Func<object, Dictionary<string, object>>>();

        public static PropertyInfo[] GetProperties(object o)
        {
            var dict = new Dictionary<string, object>();
            var t = o.GetType();
            PropertyInfo[] propertyInfos;
            propertyInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return propertyInfos;
        }
        /*
        public static Dictionary<string, object> GetProperties2(object o)
        {

            var t = o.GetType();

            Func<object, Dictionary<string, object>> getter;

            if (!cache.TryGetValue(t, out getter))
            {
                var rettype = typeof(Dictionary<string, object>);

                var dm = new DynamicMethod(t.Name + ":GetProperties", rettype,
                   new Type[] { typeof(object) }, t);

                var ilgen = dm.GetILGenerator();

                var instance = ilgen.DeclareLocal(t);
                var dict = ilgen.DeclareLocal(rettype);

                ilgen.Emit(OpCodes.Ldarg_0);
                ilgen.Emit(OpCodes.Castclass, t);
                ilgen.Emit(OpCodes.Stloc, instance);

                ilgen.Emit(OpCodes.Newobj, rettype.GetConstructor(Type.EmptyTypes));
                ilgen.Emit(OpCodes.Stloc, dict);

                var add = rettype.GetMethod("Add");

                foreach (var prop in t.GetProperties(
                  BindingFlags.Instance |
                  BindingFlags.Public))
                {
                    ilgen.Emit(OpCodes.Ldloc, dict);

                    ilgen.Emit(OpCodes.Ldstr, prop.Name);

                    ilgen.Emit(OpCodes.Ldloc, instance);
                    //ilgen.Emit(OpCodes.Ldfld, prop);
                    ilgen.Emit(OpCodes.Castclass, typeof(object));

                    ilgen.Emit(OpCodes.Callvirt, add);
                }

                ilgen.Emit(OpCodes.Ldloc, dict);
                ilgen.Emit(OpCodes.Ret);

                cache[t] = getter =
                  dm.CreateDelegate<Func<object, Dictionary<string, object>>>();
            }

            return getter(o);
        }
        */
    }

}
