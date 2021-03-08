using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace Bewildered.Editor
{
    public static class TypeUtility
    {
        public static List<Type> GetVisibleTypesFromAssembly(Assembly assembly)
        {
            Type[] assemblyTypes = assembly.GetTypes();
            List<Type> visibleTypes = new List<Type>(assemblyTypes.Length);

            foreach (Type type in assemblyTypes)
            {
                if (type.IsVisible)
                    visibleTypes.Add(type);
            }

            return visibleTypes;
        }

        public static Assembly[] GetAssembilesTypeHasAccessTo<T>()
        {
            return GetAssembliesTypeHasAccessTo(typeof(T));
        }

        /// <summary>
        /// Collects the assemblies the specified <see cref="Type"/> has access to. The <see cref="Type"/>'s native assembly and all it's referenced assemblies.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to collect assembiles from.</param>
        /// <returns>A collection of assembiles <paramref name="type"/> has access to.</returns>
        public static Assembly[] GetAssembliesTypeHasAccessTo(Type type)
        {
            Assembly typeAssembly;

            try
            {
                typeAssembly = type == null ? Assembly.Load("Assembly-CSharp") : type.Assembly;
            }
            catch (System.IO.FileNotFoundException)
            {
                throw new System.IO.FileNotFoundException("Assembly-CSharp.dll was not found. Please create any script in the Assets folder so that the assembly is generated.");
            }

            AssemblyName[] referencedAssemblies = typeAssembly.GetReferencedAssemblies();
            Assembly[] assemblies = new Assembly[referencedAssemblies.Length + 1];

            for (int i = 0; i < referencedAssemblies.Length; i++)
            {
                assemblies[i] = Assembly.Load(referencedAssemblies[i]);
            }

            assemblies[referencedAssemblies.Length] = typeAssembly;

            return assemblies;
        }
    } 
}
