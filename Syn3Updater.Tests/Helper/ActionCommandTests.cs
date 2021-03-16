namespace Syn3Updater.Tests.Helper
{
    using Cyanlabs.Syn3Updater.Helper;
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class ActionCommandTests
    {
        private ActionCommand _testClass;
        private Action _command;
        private Func<bool> _canExecute;

        [SetUp]
        public void SetUp()
        {
            _command = default(Action);
            _canExecute = default(Func<bool>);
            _testClass = new ActionCommand(_command, _canExecute);
        }

        [Test]
        public void CanConstruct()
        {
            var instance = new ActionCommand(_command, _canExecute);
            Assert.That(instance, Is.Not.Null);
        }

        [Test]
        public void CannotConstructWithNullCommand()
        {
            Assert.Throws<ArgumentNullException>(() => new ActionCommand(default(Action), default(Func<bool>)));
        }

        [Test]
        public void CannotConstructWithNullCanExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new ActionCommand(default(Action), default(Func<bool>)));
        }

        [Test]
        public void CanCallCanExecute()
        {
            var parameter = new object();
            var result = _testClass.CanExecute(parameter);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public void CannotCallCanExecuteWithNullParameter()
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.CanExecute(default(object)));
        }

        [Test]
        public void CanCallExecute()
        {
            var parameter = new object();
            _testClass.Execute(parameter);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public void CannotCallExecuteWithNullParameter()
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.Execute(default(object)));
        }

        [Test]
        public void CanCallRaiseCanExecuteChanged()
        {
            _testClass.RaiseCanExecuteChanged();
            Assert.Fail("Create or modify test");
        }
    }
}