using HtmlAgilityPack;
using RestSharp;

const string pggShopBaseUrl = "https://sklep.pgg.pl/";

var adminNumbers = new List<string>
{
    "48509980548"
};
var commonNumbers = new List<string>
{
    //"48509338549"
};
var allNumbers = adminNumbers.Concat(commonNumbers);

try
{
    await MainLoopAsync();
}
catch (Exception)
{
    foreach (var number in adminNumbers)
    {
        var response = await SendSmsAsync(number, "Sth went wrong!");
        Console.WriteLine(response.Content);
    }
}

async Task MainLoopAsync()
{
    while (true)
    {
        var productsNodes = await LoadProductsNodesAsync();

        var availableProductsNodes = productsNodes.Where(node => !!node.InnerHtml.Contains("Brak towaru"));

        var availableProductsUrls = GetAvailableProductsUrls(availableProductsNodes);

        availableProductsUrls = availableProductsUrls.Where(url => url.Contains("karolin")); //TODO

        await Task.Delay(TimeSpan.FromSeconds(10));

        if (!availableProductsUrls.Any())
        {
            Console.WriteLine("No available products.");
            continue;
        }

        var finalUrls = availableProductsUrls.Select(url => $"/{url}"); //TODO {pggShopBaseUrl}

        var message = $"Pojawił się niezerowy stan w sklepie: {string.Join(", ", finalUrls)}";

        Console.WriteLine(message);

        foreach (var number in allNumbers)
        {
            var response = await SendSmsAsync(number, message);
            Console.WriteLine(response.Content);
        }
    }
}

async Task<IEnumerable<HtmlNode>> LoadProductsNodesAsync()
{
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
    var availableProductsUrls = availableProductsNodes
        .Select(node => node.SelectNodes(".//a"))
        .Select(nodes => nodes.FirstOrDefault())
        .Where(node => node is not null)
        .Select(node => node.Attributes["href"].Value)
        .ToList();

    return availableProductsUrls;
}

async Task<RestResponse> SendSmsAsync(string to, string message)
{
    var client = new RestClient("https://api.smsapi.pl/sms.do");

    var request = new RestRequest();
    request.AddHeader("Authorization", "Bearer zRi17DqtZUOx3xVdG9ehdeD0bPkon8ze7lCwxcTe");
    request.AddQueryParameter("to", to);
    request.AddQueryParameter("message", message);
    request.AddQueryParameter("test", "0");
    request.AddQueryParameter("format", "json");

    var response = await client.GetAsync(request);

    return response;
}
