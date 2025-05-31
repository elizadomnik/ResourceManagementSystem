using Microsoft.Playwright;
using System.Threading.Tasks;
using FluentAssertions;

namespace ResourceManagementSystem.UI.Tests.PageObjects
{
    public class LoginPage : BasePage
    {
        private ILocator EmailInput => Page.Locator("#email");
        private ILocator PasswordInput => Page.Locator("#password");
        private ILocator LoginButton => Page.Locator("button[type='submit']");
        private ILocator ErrorMessage => Page.Locator(".alert-danger");

        public LoginPage(IPage page, string baseUiUrl) : base(page, GetAbsoluteUrl(baseUiUrl, "/login")) 
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
            await Page.GotoAsync(GetAbsoluteUrl(BaseUiUrl, "/login"));
            await Page.WaitForURLAsync($"**/login"); 
        }

        public async Task FillEmailAsync(string email)
        {
            await EmailInput.FillAsync(email);
        }

        public async Task FillPasswordAsync(string password)
        {
            await PasswordInput.FillAsync(password);
        }

        public async Task ClickLoginButtonAsync()
        {
            await LoginButton.ClickAsync();
        }

        public async Task LoginAsync(string email, string password)
        {
            await FillEmailAsync(email);
            await FillPasswordAsync(password);
            await ClickLoginButtonAsync();
        }

        public async Task<string?> GetErrorMessageAsync()
        {
            if (await ErrorMessage.IsVisibleAsync())
            {
                return await ErrorMessage.InnerTextAsync();
            }
            return null;
        }

        public async Task AssertIsOnLoginPage()
        {
            Page.Url.Should().Contain("/login");
            (await LoginButton.IsVisibleAsync()).Should().BeTrue();
        }
    }
}