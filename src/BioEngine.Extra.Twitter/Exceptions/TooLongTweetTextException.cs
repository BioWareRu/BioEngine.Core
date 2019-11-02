using System.Diagnostics.CodeAnalysis;

namespace BioEngine.Extra.Twitter.Exceptions
{
    [SuppressMessage("Readability", "RCS1194", Justification = "Reviewed")]
    public class TooLongTweetTextException : TwitterException
    {
        public TooLongTweetTextException() : base("Текст твита больше 140 символов")
        {
        }
    }
}