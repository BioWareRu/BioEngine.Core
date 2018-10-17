using System.Globalization;
using BioEngine.Core.Helpers;
using Xunit;

namespace BioEngine.Core.Tests
{
    public class PluralizeTest
    {
        [Fact]
        public void Russian()
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru");
            var message = @"{n, plural,
                    =0 {Обсудить на форуме}
                    one {# комментарий} 
                    few {# комментария} 
                    many {# комментариев} 
                    other {# комментария} 
                }";

            Assert.Equal("Обсудить на форуме", message.Pluralize(0));
            Assert.Equal("1 комментарий", message.Pluralize(1));
            Assert.Equal("2 комментария", message.Pluralize(2));
            Assert.Equal("10 комментариев", message.Pluralize(10));
            Assert.Equal("11 комментариев", message.Pluralize(11));
            Assert.Equal($"{13.5.ToString(CultureInfo.CurrentCulture)} комментариев", message.Pluralize(13.5));
        }
    }
}