namespace PRNFE.MVC.Helper;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;
    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;        
        _logger.LogInformation("Sending request to {Url}", request.RequestUri);

        if (request.Content != null)
        {
            var requestBody = await request.Content.ReadAsStringAsync();
            _logger.LogInformation("Request body: {Body}", requestBody);
        }

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation("Received response: {StatusCode}", response.StatusCode);

        if (response.Content != null)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response body: {Body}", responseBody);
        }
        Console.ResetColor();

        return response;
    }
}

