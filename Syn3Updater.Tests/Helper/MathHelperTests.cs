using Cyanlabs.Syn3Updater.Helper;
using System;
using NUnit.Framework;

namespace Syn3Updater.Tests.Helper
{
    [TestFixture]
    [Ignore("FIXME")]
    public static class MathHelperTests
    {
        [Test]
        public static void CanCallGetDouble()
        {
            var value = "TestValue6769535";
            var defaultValue = 381389917.59;
            var result = value.GetDouble(defaultValue);
            Assert.Fail("Create or modify test");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public static void CannotCallGetDoubleWithInvalidValue(string value)
        {
            Assert.Throws<ArgumentNullException>(() => value.GetDouble(1896834304.53));
        }

        [Test]
        public static void CanCallClampWithDoubleAndDoubleAndDouble()
        {
            var value = 2107456715.43;
            var min = 1948516905.06;
            var max = 1565912407.95;
            var result = MathHelper.Clamp(value, min, max);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CanCallClampWithFloatAndFloatAndFloat()
        {
            var value = 20907.52F;
            var min = 11824.6748F;
            var max = 19632.2988F;
            var result = MathHelper.Clamp(value, min, max);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CanCallClampWithIntAndIntAndInt()
        {
            var value = 1139644205;
            var min = 1483504046;
            var max = 1185586685;
            var result = MathHelper.Clamp(value, min, max);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CanCallBytesToString()
        {
            var byteCount = 1563554813L;
            var result = MathHelper.BytesToString(byteCount);
            Assert.Fail("Create or modify test");
        }
    }
}