using RestSharp;


var smsResponse = await SendSmsAsync("48509980548", "test wiadomosc");
Console.WriteLine(smsResponse.Content);

var counter = 0;
var max = args.Length != 0 ? Convert.ToInt32(args[0]) : -1;
while (max == -1 || counter < max)
{
    Console.WriteLine($"Counter: {++counter}");
    await Task.Delay(TimeSpan.FromMilliseconds(1_000));
}

async Task<RestResponse> SendSmsAsync(string to, string message)
{
    var client = new RestClient("https://api.smsapi.pl/sms.do");

    var request = new RestRequest();
    request.AddHeader("Authorization", "Bearer zRi17DqtZUOx3xVdG9ehdeD0bPkon8ze7lCwxcTe");
    request.AddQueryParameter("to", to);
    request.AddQueryParameter("message", message);
    request.AddQueryParameter("test", "1");
    request.AddQueryParameter("format", "json");

    var response = await client.GetAsync(request);

    return response;
}