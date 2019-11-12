using System;
using System.Reflection;
using Assembly_Browser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AssemblyBrowserLib.Tests
{
    [TestClass]
    public class AssemblyBrowserTest
    {
        private readonly AssemblyBrowser _assemblyBrowser = new AssemblyBrowser();

        private string _testLocation = (Assembly.Load("TestableLibrary")).Location;

        [TestMethod]
        public void NamespaceTest()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var namespaces = _assemblyBrowser.GetAssemblyInfo(location);
            var _namespace = namespaces[0];
            var currentNamespace = Assembly.GetExecutingAssembly().GetTypes()[0].Namespace;
            Assert.AreEqual(_namespace.Name, currentNamespace);
        }

        [TestMethod]
        public void TypesTest()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var namespaces = _assemblyBrowser.GetAssemblyInfo(location);
            var _namespace = namespaces[0];
            var types = _namespace.Members;
            Assert.AreEqual(Assembly.GetExecutingAssembly().GetTypes().Length, types.Count);
        }

        [TestMethod]
        public void TypesNameTest()
        {
            var namespaces = _assemblyBrowser.GetAssemblyInfo(_testLocation);
            foreach (var _namespace in namespaces)
            {
                if (_namespace.Name != "TestableLibrary" && _namespace.Name != "TestableLibrary.Another")
                {
                    Assert.Fail($"Error in namespace name {_namespace.Name}");
                }
            }
        }

        [TestMethod]
        public void TypeNamesTest()
        {
            var namespaces = _assemblyBrowser.GetAssemblyInfo(_testLocation);
            var types = namespaces[0].Members;
            var flag = false;
            foreach (var type in types)
            {
                if (type.Name == "MyClass")
                {
                    flag = true;
                    continue;
                }

            }
            if (!flag)
            {
                Assert.Fail();
            }
        }
    }
}