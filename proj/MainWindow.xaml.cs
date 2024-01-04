using PuppeteerSharp;
using System.Windows;
using System.Windows.Threading;
using Page = PuppeteerSharp.Page;

namespace browserTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Browser? browser;
        private async Task<Browser> InitBrowser()
        {
            // Since PuppeteerSharp v11.0.0, BrowserFetcher.DefaultChromiumRevision is obsolete.
            // Use PuppeteerSharp.BrowserData.Chrome.DefaultBuildId instead.
            // DefaultBuildId may be updated with Nuget packge update of PuppeteerSharp.
            await new BrowserFetcher().DownloadAsync(PuppeteerSharp.BrowserData.Chrome.DefaultBuildId);
            var _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] {
                    "--js-flags=\"--max-old-space-size=128\"" // Limits memory usage to 128MB
                },
            });
            return (Browser)_browser; // Cast IBrowser to Browser
        }
        private Page? webPage;


        public MainWindow()
        {
            InitializeComponent();
            InitWebBrowser();
        }

        

        private void InitWebBrowser()
        {
            var browserTask = Task.Run(InitBrowser);
            browser = browserTask.GetAwaiter().GetResult();
            var pageTask = Task.Run(async () => await browser.NewPageAsync());
            webPage = (Page)pageTask.GetAwaiter().GetResult(); // Cast IPage to Page
            webPage.DefaultTimeout = 0;
        }

        private async Task<string> RequestNavigate(string url, Page page)
        {
            await page.GoToAsync(url, WaitUntilNavigation.Networkidle2);
            var content = await page.GetContentAsync();

            return content;
        }

        private void Navigate(string url, Page page)
        {
            string content = string.Empty;
            try
            {
                var navigateTask = Task.Run(async () => await RequestNavigate(url, page));
                content = navigateTask.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                content = ex.Message;
            }
            tbPageRaw.Text = content;
        }

        private void btNavigate_Click(object sender, RoutedEventArgs e)
        {
            Navigate(tbUrl.Text, webPage);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Close browser
            webPage?.Dispose();
            browser?.Dispose();
        }
    }
}