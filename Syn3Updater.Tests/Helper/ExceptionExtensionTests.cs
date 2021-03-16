namespace Syn3Updater.Tests.Helper
{
    using Cyanlabs.Syn3Updater.Helper;
    using System;
    using NUnit.Framework;

    [TestFixture]
    public static class ExceptionExtensionTests
    {
        [Test]
        public static void CanCallGetFullMessage()
        {
            var ex = new Exception();
            var message = "TestValue934286116";
            var result = ex.GetFullMessage(message);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CannotCallGetFullMessageWithNullEx()
        {
            Assert.Throws<ArgumentNullException>(() => default(Exception).GetFullMessage("TestValue360311510"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public static void CannotCallGetFullMessageWithInvalidMessage(string value)
        {
            Assert.Throws<ArgumentNullException>(() => new Exception().GetFullMessage(value));
        }
    }
}