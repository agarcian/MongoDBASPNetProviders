using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AltovientoSolutions.Common.Util;
namespace WebUI4.Tests
{
    [TestClass]
    public class CommonFunctionsTest
    {
        [TestMethod]
        public void TestMD5OfString()
        {
            // Expected md5 of "1234567890" is "e807f1fcf82d132f9bb018ca6738a19f"   http://www.liamdelahunty.com/tips/md5_string.php

            string md5Result = AltovientoSolutions.Common.Util.Password.EncodeString("1234567890", "md5");

            Assert.AreEqual("e807f1fcf82d132f9bb018ca6738a19f", md5Result);

        }
    }
}
