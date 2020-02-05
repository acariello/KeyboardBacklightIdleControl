using System;
using System.Linq;
using System.Reflection;

namespace BackLightProg
{
   static class MethodClass
   {

      public static MethodInfo GetRuntimeMethodsExt(this Type type, string name, params Type[] types)
      {
         // Find potential methods with the correct name and the right number of parameters and parameter names
         var potentials = (from ele in type.GetMethods()
                           where ele.Name.Equals(name)
                           select ele);

         // check if we have more than 1? Or not?
         return potentials.FirstOrDefault();
      }


   }
}
