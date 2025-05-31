using Microsoft.Playwright;
using System.Threading.Tasks;
using FluentAssertions;

namespace ResourceManagementSystem.UI.Tests.PageObjects
{
    public class ResourcesPage : BasePage
    {
        private ILocator CreateNewResourceButton => Page.Locator("text=Create New Resource");
        private ILocator ResourcesTable => Page.Locator("table.table"); 
        private ILocator NoResourcesMessage => Page.Locator("em:has-text('No resources found.')");

        public ResourcesPage(IPage page, string baseUiUrl) : base(page, GetAbsoluteUrl(baseUiUrl, "/resources"))
        {
        }
        
        private static string GetAbsoluteUrl(string baseUiUrl, string relativePath)
        {
            var baseUri = new Uri(baseUiUrl);
            var fullUri = new Uri(baseUri, relativePath.TrimStart('/'));
            return fullUri.ToString();
        }

        public override async Task NavigateAsync()
        {
            await Page.GotoAsync(GetAbsoluteUrl(BaseUiUrl, "/resources"));
            await Page.WaitForURLAsync($"**/resources");
        }

        public async Task AssertIsOnResourcesPage()
        {
            Page.Url.Should().Contain("/resources");
            (await CreateNewResourceButton.IsVisibleAsync()).Should().BeTrue();
        }

        public async Task<bool> AreResourcesVisibleAsync()
        {
            return await ResourcesTable.IsVisibleAsync();
        }
        
        public async Task<bool> IsNoResourcesMessageVisibleAsync()
        {
            return await NoResourcesMessage.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 3000 }); 
        }

        public async Task ClickCreateNewResourceAsync()
        {
            await CreateNewResourceButton.ClickAsync();
        }


        public async Task<ILocator> GetResourceRowByNameAsync(string resourceName)
        {
            return Page.Locator($"//table/tbody/tr[td[contains(text(), '{resourceName}')]]");
        }
    }
}