using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace LalaCoffee.Tests
{
    [TestClass]
    public class GlobalTestInit
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var requestedName = new AssemblyName(args.Name).Name;
                if (requestedName == "Microsoft.Owin.Security" || requestedName == "Microsoft.Owin")
                {
                    var loaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == requestedName);
                    if (loaded != null) return loaded;
                }
                return null;
            };
        }
    }
}
