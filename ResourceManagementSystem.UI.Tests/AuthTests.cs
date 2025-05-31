
using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;
using ResourceManagementSystem.UI.Tests.PageObjects; 
using FluentAssertions;

namespace ResourceManagementSystem.UI.Tests
{

    public class AuthTests : IAsyncLifetime
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private IPage? _page;
        private LoginPage? _loginPage;
        private ResourcesPage? _resourcesPage; 

       
        private const string BaseUiUrl = "http://localhost:5115";

        private const string TestUserEmail = "testui@example.com";
        private const string TestUserPassword = "Password123!";

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
           
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false, 
                SlowMo = 500 
            });
            _page = await _browser.NewPageAsync();
            _loginPage = new LoginPage(_page, BaseUiUrl);
            _resourcesPage = new ResourcesPage(_page, BaseUiUrl);
        }

        public async Task DisposeAsync()
        {
            if (_browser != null) await _browser.CloseAsync();
            _playwright?.Dispose();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldNavigateToResourcesPage()
        {
            Assert.NotNull(_loginPage); 
            Assert.NotNull(_resourcesPage);

            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(TestUserEmail, TestUserPassword);

            await _page!.WaitForURLAsync($"**/resources", new PageWaitForURLOptions { Timeout = 5000 });
            await _resourcesPage.AssertIsOnResourcesPage();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldShowErrorMessage()
        {
            Assert.NotNull(_loginPage);

            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync("invalid@example.com", "wrongpassword");

            var errorMessage = await _loginPage.GetErrorMessageAsync();
            errorMessage.Should().NotBeNullOrEmpty();
            errorMessage.Should().Contain("Invalid email");
            await _loginPage.AssertIsOnLoginPage(); 
        }
    }
}