using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RubyistHotlinksReports.Core.Services;

namespace RubyistHotlinksReports.Core.Tests
{
    /// <summary>
    /// <seealso cref="AnalyzeService"/>をテストします。
    /// </summary>
    [TestFixture]
    public class AnalyzeServiceTest
    {
        /// <summary>
        /// <seealso cref="AnalyzeService.CacheAllTalk"/>をテストします。
        /// </summary>
        [Test]
        public async Task CacheAllTalk()
        {
            var service = new AnalyzeService();
            await service.CacheAllTalk();
        }
    }
}
