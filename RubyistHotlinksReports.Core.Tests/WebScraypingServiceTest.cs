using System;
using System.Linq;
using NUnit.Framework;

namespace RubyistHotlinksReports.Core.Tests
{
    /// <summary>
    /// <seealso cref="WebScraypingService"/>をテストします。
    /// </summary>
    [TestFixture]
    public class WebScraypingServiceTest
    {
        /// <summary>
        /// <seealso cref="WebScraypingService.EnumerableRubyistHotlinksUrl"/>をテストします。
        /// </summary>
        [Test]
        public void EnumerableRubyistHotlinksUrl()
        {
            var service = new WebScraypingService();
            var result = service.EnumerableRubyistHotlinksUrl().Result.ToList();
            Assert.IsTrue(result.Any());
            foreach (var item in result)
            {
                Console.WriteLine(item);
            }
        }
        /// <summary>
        /// <seealso cref="WebScraypingService.EnumerableTalks"/>をテストします。
        /// </summary>
        [Test]
        public void EnumerableTalks()
        {
            var service = new WebScraypingService();
            var url = service.EnumerableRubyistHotlinksUrl().Result.FirstOrDefault();
            var result = service.EnumerableTalks(url).Result.ToList();
            Assert.IsTrue(result.Any());
            foreach (var talk in result)
            {
                Console.WriteLine($"{talk.User}:{talk.Message}");
            }
        }
    }
}
