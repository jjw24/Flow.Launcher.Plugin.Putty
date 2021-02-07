using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin;
using NUnit.Framework;

namespace Flow.Launcher.Plugin.Putty.Test
{
    [TestFixture]
    public class PluginTest
    {
        private const string Keyword = "pt";
        private readonly Result _defaultPuttyResultItem = new Result { Title = "putty.exe", IcoPath = "logo.png", SubTitle = "Launch Clean Putty" };

        [Test]
        public void Query_returns_only_Putty_as_result_with_empty_searchquery()
        {
            // Arrange
            var plugin = new PuttyPlugin();
            var query = new Query(Keyword, Keyword, new string[0]);

            var fakeSessions = new List<PuttySession>();
            plugin.PuttySessionService = new FakePuttySessionService { FakeResult = fakeSessions };

            var expectedResult = new List<Result> { _defaultPuttyResultItem };

            // Act
            var result = plugin.Query(query);

            // Assert
            Assert.AreEqual(result.Count, expectedResult.Count);

            foreach (var sessionResult in result)
            {
                Assert.IsNotNull(
                    expectedResult.Where(x => x.Title == sessionResult.Title && x.SubTitle == sessionResult.SubTitle && x.IcoPath == sessionResult.IcoPath));
            }
        }

        [Test]
        public void Query_search_will_only_find_default_Putty_result()
        {
            // Arrange
            var plugin = new PuttyPlugin();
            var query = new Query(Keyword + " asdf", "asdf", new string[0]);

            var fakeSessions = new List<PuttySession>
            {
                new PuttySession { Identifier = "foo@foobar.com", Protocol = "ssh", Username = "foo", Hostname = "foobar.com" },
                new PuttySession { Identifier = "bar@foobar.com", Protocol = "ssh", Username = "bar", Hostname = "foobar.com" },
                new PuttySession { Identifier = "poop@muh.com", Protocol = "ssh", Username = "poop", Hostname = "muh.com" },
            };
            plugin.PuttySessionService = new FakePuttySessionService { FakeResult = fakeSessions };

            var expectedResult = new List<Result> { _defaultPuttyResultItem };

            // Act
            var result = plugin.Query(query);

            // Assert
            Assert.AreEqual(result.Count, expectedResult.Count);

            foreach (var sessionResult in result)
            {
                Assert.IsNotNull(
                    expectedResult.Where(x => x.Title == sessionResult.Title && x.SubTitle == sessionResult.SubTitle && x.IcoPath == sessionResult.IcoPath));
            }
        }

        [Test]
        public void Query_returns_three_results_with_searchquery_given()
        {
            // Arrange
            var plugin = new PuttyPlugin();
            var query = new Query(Keyword + " foo", "foo", new string[0]);

            var fakeSessions = new List<PuttySession>
            {
                new PuttySession { Identifier = "foo@foobar.com", Protocol = "ssh", Username = "foo", Hostname = "foobar.com" },
                new PuttySession { Identifier = "bar@foobar.com", Protocol = "ssh", Username = "bar", Hostname = "foobar.com" },
                new PuttySession { Identifier = "poop@muh.com", Protocol = "ssh", Username = "poop", Hostname = "muh.com" },
            };
            plugin.PuttySessionService = new FakePuttySessionService { FakeResult = fakeSessions };

            var expectedResult = new List<Result>
            {
                _defaultPuttyResultItem,
                new Result{ Title = "foo@foobar.com", IcoPath = "icon.png", SubTitle = "ssh://foo@foobar.com" },
                new Result{ Title = "bar@foobar.com", IcoPath = "icon.png", SubTitle = "ssh://bar@foobar.com" },
            };

            // Act
            var result = plugin.Query(query);

            // Assert
            Assert.AreEqual(result.Count, expectedResult.Count);

            foreach (var sessionResult in result)
            {
                Assert.IsNotNull(
                    expectedResult.Where(x => x.Title == sessionResult.Title && x.SubTitle == sessionResult.SubTitle && x.IcoPath == sessionResult.IcoPath));
            }
        }
    }
}