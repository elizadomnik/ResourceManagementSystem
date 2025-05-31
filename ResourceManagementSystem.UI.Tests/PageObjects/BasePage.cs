using Microsoft.Playwright;
using System.Threading.Tasks;

namespace ResourceManagementSystem.UI.Tests.PageObjects
{
    public abstract class BasePage
    {
        protected IPage Page { get; }
        protected string BaseUiUrl { get; }

        protected BasePage(IPage page, string baseUiUrl)
        {
            Page = page;
            BaseUiUrl = baseUiUrl;
        }

        protected string GetAbsoluteUrl(string relativePath)
        {
            var baseUri = new Uri(BaseUiUrl);
            var fullUri = new Uri(baseUri, relativePath.TrimStart('/'));
            return fullUri.ToString();
        }

        public abstract Task NavigateAsync();
    }
}