using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AltovientoSolutions.Security;

namespace WebUI4.Tests
{
    [TestClass]
    public class AccessControlTestUnit
    {

        private class Class1
        {
            public string Attribute1 { get; set; }
            public int Attribute2 { get; set; }

            private List<String> listAttribute3 = new List<string>();

            [ContextualSecurity]
            public List<String> ListAttribute3
            {
                get { return listAttribute3; }
                set { listAttribute3 = value; }
            }
        }


        private class Class2
        {
            [ContextualSecurity]
            public string Attribute1 { get; set; }

            public int Attribute2 { get; set; }

            private List<String> listAttribute3 = new List<string>();


        }


        private class Class3
        {
            public string Attribute1 { get; set; }
            public int Attribute2 { get; set; }

            private List<Int32> listAttribute3 = new List<int>();

            [ContextualSecurity]
            public List<Int32> ListAttribute3
            {
                get { return listAttribute3; }
                set { listAttribute3 = value; }
            }
        }


        private class Class4
        {
           
            public string Attribute1 { get; set; }

             [ContextualSecurity]
            public int Attribute2 { get; set; }

            private List<String> listAttribute3 = new List<string>();


        }



        [TestMethod]
        public void AccessControlTest()
        {

            // contextual security based on attribute 3.
            Class1 obj1 = new Class1() { Attribute1 = "ABC123", Attribute2 = 20, ListAttribute3 = new List<string>() { "Georgia", "California", "Illinois" } };

            // contextual security based on attribute 1.
            Class2 obj2 = new Class2() { Attribute1 = "Georgia", Attribute2 = 40 };

            bool canaccess;


            canaccess = AccessControl.CanAccess(obj1, obj2);
            Assert.IsTrue(canaccess);



            // contextual security based on attribute 3.
            obj1 = new Class1() { Attribute1 = "ABC123", Attribute2 = 20, ListAttribute3 = new List<string>() { "Georgia", "California", "Illinois" } };

            // contextual security based on attribute 1.
            obj2 = new Class2() { Attribute1 = "Arkansas", Attribute2 = 40 };

            canaccess = AccessControl.CanAccess(obj1, obj2);
            Assert.IsFalse(canaccess);


            // contextual security based on attribute 3.
            Class3 obj3 = new Class3() { Attribute1 = "ABC123", Attribute2 = 20, ListAttribute3 = new List<int>() { 20, 30, 40, 50 } };

            // contextual security based on attribute 1.
            Class4 obj4 = new Class4() { Attribute1 = "Arkansas", Attribute2 = 40 };

            canaccess = AccessControl.CanAccess(obj3, obj4);
            Assert.IsTrue(canaccess);


            // contextual security based on attribute 3.
            obj3 = new Class3() { Attribute1 = "ABC123", Attribute2 = 20, ListAttribute3 = new List<int>() { 20, 30, 40, 50 } };

            // contextual security based on attribute 1.
            obj4 = new Class4() { Attribute1 = "Arkansas", Attribute2 = 100 };

            canaccess = AccessControl.CanAccess(obj3, obj4);
            Assert.IsFalse(canaccess);

        }
    }
}
