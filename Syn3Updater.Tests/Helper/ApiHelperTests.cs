using Cyanlabs.Syn3Updater.Helper;
using System;
using NUnit.Framework;
using Cyanlabs.Syn3Updater.Model;
using System.Threading.Tasks;

namespace Syn3Updater.Tests.Helper
{
    [TestFixture]
    public static class ApiHelperTests
    {
        //TODO:FIXME
        [Test]
        public static void CanCallGetSpecialIvsu()
        {
            var url = "TestValue1041730658";
            var result = ApiHelper.GetSpecialIvsu(url);
            Assert.Fail("Create or modify test");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public static void CannotCallGetSpecialIvsuWithInvalidUrl(string value)
        {
            Assert.Throws<ArgumentNullException>(() => ApiHelper.GetSpecialIvsu(value));
        }

        [Test]
        public async static Task GetSpecialIvsuPerformsMapping()
        {
            var url = "TestValue615803767";
            var result = await ApiHelper.GetSpecialIvsu(url);
            Assert.That(result.Url, Is.EqualTo(url));
        }

        [Test]
        public static void CanCallConvertIvsu()
        {
            var ivsu = new Api.Ivsu { Id = 1987917491, Name = "TestValue507747437", Type = "TestValue391370599", Regions = new[] { "TestValue941055651", "TestValue745553851", "TestValue1110261990" }, Md5 = "TestValue424093519", Url = "TestValue809188826", Notes = "TestValue323075795", Version = "TestValue29422309" };
            var result = ApiHelper.ConvertIvsu(ivsu);
            Assert.Fail("Create or modify test");
        }

        [Test]
        public static void CannotCallConvertIvsuWithNullIvsu()
        {
            Assert.Throws<ArgumentNullException>(() => ApiHelper.ConvertIvsu(default));
        }

        [Test]
        public static void ConvertIvsuPerformsMapping()
        {
            var ivsu = new Api.Ivsu { Id = 1898385250, Name = "TestValue965838048", Type = "TestValue981252909", Regions = new[] { "TestValue1863182185", "TestValue545353684", "TestValue1411896392" }, Md5 = "TestValue1364557053", Url = "TestValue2131094873", Notes = "TestValue1284512460", Version = "TestValue1676088225" };
            var result = ApiHelper.ConvertIvsu(ivsu);
            Assert.That(result.Name, Is.EqualTo(ivsu.Name));
            Assert.That(result.Type, Is.EqualTo(ivsu.Type));
            Assert.That(result.Md5, Is.EqualTo(ivsu.Md5));
            Assert.That(result.Url, Is.EqualTo(ivsu.Url));
            Assert.That(result.Notes, Is.EqualTo(ivsu.Notes));
            Assert.That(result.Version, Is.EqualTo(ivsu.Version));
        }
    }
}