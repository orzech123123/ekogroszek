using Ekogroszek.Emails;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using Serilog;

const string pggShopBaseUrl = "https://sklep.pgg.pl/";

var adminNumbers = new List<string>
{
    //"48509980548"
    "michalorzechowski123@gmail.com"
};
var commonNumbers = new List<string>
{
    //"48509338549"
};
var allNumbers = adminNumbers.Concat(commonNumbers);

const int loopDelayInSeconds = 60;

using var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ekogroszek.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true);

var configuration = builder.Build();

var serviceProvider = new ServiceCollection()
           .AddTransient<IEmailService, EmailService>()
           .Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)))
           .BuildServiceProvider();

var emailService = serviceProvider.GetService<IEmailService>();




await MainLoopAsync();




async Task HandleFatalErrorAsync(Exception ex)
{
    logger.Error(ex, "Fatal error");

    foreach (var number in adminNumbers)
    {
        var response = await SendSmsAsync(number, "Ekogroszek - ERROR!", "Sth went wrong! Search in log file.");
    }

    await MainLoopAsync();
}

async Task MainLoopAsync()
{
    try
    {
        while (true)
        {
            var productsNodes = await LoadProductsNodesAsync();

            var availableProductsNodes = productsNodes.Where(node => !node.InnerHtml.Contains("Brak towaru"));

            var availableProductsUrls = GetAvailableProductsUrls(availableProductsNodes);

            //availableProductsUrls = availableProductsUrls.Where(url => url.Contains("karolin")); //TODO

            if (!availableProductsUrls.Any())
            {
                logger.Warning("No available products.");
                await Delay();
                continue;
            }

            var finalUrls = availableProductsUrls.Select(url => $"{url}"); //TODO {pggShopBaseUrl}/

            logger.Information("<<< ACTIVATING >>>");
            logger.Information($"{string.Join(Environment.NewLine, finalUrls)}");

            foreach (var number in allNumbers)
            {
                var response = await SendSmsAsync(number, "Ekogroszek - NA STANIE!", "Pojawil sie niezerowy stan w sklepie!");
                logger.Information(response.Content);
            }

            await Delay();
        }
    }
    catch (Exception ex)
    {
        await HandleFatalErrorAsync(ex);
    }
}

async Task Delay() => await Task.Delay(TimeSpan.FromSeconds(loopDelayInSeconds));

async Task<IEnumerable<HtmlNode>> LoadProductsNodesAsync()
{
    logger.Information("Downloading data from sklep.pgg.pl");

    var client = new RestClient("https://sklep.pgg.pl/");
    var request = await client.GetAsync(new RestRequest());
    var document = new HtmlDocument();
    document.LoadHtml(request.Content);
    var productsNodes = document.DocumentNode
        .SelectNodes("//div[@class='row mt-4 justify-content-center']")
        .ToList();

    return productsNodes;
}

IEnumerable<string> GetAvailableProductsUrls(IEnumerable<HtmlNode> availableProductsNodes)
{
    logger.Information("Getting available products...");

    var availableProductsUrls = availableProductsNodes
        .Select(node => node.SelectNodes(".//a"))
        .Select(nodes => nodes.FirstOrDefault())
        .Where(node => node is not null)
        .Select(node => node.Attributes["href"].Value)
        .ToList();

    return availableProductsUrls;
}

async Task<RestResponse> SendSmsAsync(string to, string subject, string message)
{
    //logger.Information($"Sending sms to {to}: {message}");

    //var client = new RestClient("https://api.smsapi.pl/sms.do");

    //var request = new RestRequest();
    //request.AddHeader("Authorization", "Bearer zRi17DqtZUOx3xVdG9ehdeD0bPkon8ze7lCwxcTe");
    //request.AddQueryParameter("to", to);
    //request.AddQueryParameter("message", message);
    //request.AddQueryParameter("test", "0");
    //request.AddQueryParameter("format", "json");

    //var response = await client.GetAsync(request);

    //return response;

    await emailService.SendAsync(new Microsoft.AspNet.Identity.IdentityMessage
    {
        Body = message,
        Subject = subject,
        Destination = to
    });

    return new RestResponse
    {
        Content = "OK"
    };
}
