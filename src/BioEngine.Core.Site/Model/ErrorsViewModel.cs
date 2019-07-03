namespace BioEngine.Core.Site.Model
{
    public class ErrorsViewModel : PageViewModel
    {
        public int ErrorCode { get; }

        public ErrorsViewModel(PageViewModelContext context, int errorCode) : base(context)
        {
            ErrorCode = errorCode;
        }
    }
}