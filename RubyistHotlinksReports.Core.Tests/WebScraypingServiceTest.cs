using System;
using System.Linq;
using NUnit.Framework;
using RubyistHotlinksReports.Core.Services;

namespace RubyistHotlinksReports.Core.Tests
{
    /// <summary>
    /// <seealso cref="WebScraypingService"/>をテストします。
    /// </summary>
    [TestFixture]
    public class WebScraypingServiceTest
    {
        /// <summary>
        /// <seealso cref="WebScraypingService.ToListRubyistHotlinksUrl"/>をテストします。
        /// </summary>
        [Test]
        public void ToListRubyistHotlinksUrl()
        {
            var service = new WebScraypingService();
            var result = service.ToListRubyistHotlinksUrl().Result;
            Assert.IsTrue(result.Any());
            foreach (var item in result)
            {
                Console.WriteLine(item);
            }
        }
        /// <summary>
        /// <seealso cref="WebScraypingService.ToListTalks"/>をテストします。
        /// </summary>
        [Test]
        public void ToListTalks()
        {
            var service = new WebScraypingService();
            var url = service.ToListRubyistHotlinksUrl().Result.FirstOrDefault();
            var result = service.ToListTalks(url).Result;
            Assert.IsTrue(result.Any());
            foreach (var talk in result)
            {
                Console.WriteLine($"{talk.User}:{talk.Message}");
            }
        }
    }
}
