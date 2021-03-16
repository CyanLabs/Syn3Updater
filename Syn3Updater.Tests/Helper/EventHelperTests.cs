namespace Syn3Updater.Tests.Helper
{
    using Cyanlabs.Syn3Updater.Helper;
    using T = System.String;
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class EventArgs_1Tests
    {
        private EventArgs<T> _testClass;
        private T _value;

        [SetUp]
        public void SetUp()
        {
            _value = "TestValue1845229743";
            _testClass = new EventArgs<T>(_value);
        }

        [Test]
        public void CanConstruct()
        {
            var instance = new EventArgs<T>(_value);
            Assert.That(instance, Is.Not.Null);
        }

        [Test]
        public void ValueIsInitializedCorrectly()
        {
            Assert.That(_testClass.Value, Is.EqualTo(_value));
        }
    }

    [TestFixture]
    public static class EventRaiserTests
    {
        [Test]
        public static void CanCallRaise()
        {
            var handler = default(EventHandler<EventArgs<T>>);
            var sender = new object();
            var value = "TestValue1822516516";
            handler.Raise<T>(sender, value);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CannotCallRaiseWithNullHandler()
        {
            Assert.Throws<ArgumentNullException>(() => default(EventHandler<EventArgs<T>>).Raise<T>(new object(), "TestValue806317714"));
        }

        [Test]
        public static void CannotCallRaiseWithNullSender()
        {
            Assert.Throws<ArgumentNullException>(() => default(EventHandler<EventArgs<T>>).Raise<T>(default(object), "TestValue1165280271"));
        }
    }
}