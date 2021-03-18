using NUnit.Framework;
using Cyanlabs.Updater.Common;
using Cyanlabs.Syn3Updater;

namespace Syn3Updater.Tests.UI.Tabs
{
    [TestFixture]
    public class HomeViewModelServiceTests
    {
        [SetUp]
        public void SetUp()
        {
        }
        //Integration Test 
        [Test]
        public void WhenTheProperParametersArePassedADownloadIsInitiated()
        {
            //HomeViewModelService.Download();
        }

        //Unit test
        [Test]
        public void WhenTheProperParametersArePassedTheStateVariablesAreSetProperly()
        {
            //HomeViewModelService.Download();

            Assert.Equals(ApplicationManager.Instance.DownloadOnly, false);
        }
    }
}