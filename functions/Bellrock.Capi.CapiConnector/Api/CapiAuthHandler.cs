namespace Bellrock.Capi.CapiConnector.Api;

/// <summary>Delegating handler for adding shepherd API authentication requirements to each request.</summary>
public class CapiAuthHandler : DelegatingHandler
{
    private readonly ICapiAuthenticationService authenticationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapiAuthHandler"/> class.
    /// </summary>
    /// <param name="authenticationService">Capi authentication service for retrieving an auth token.</param>
    public CapiAuthHandler(ICapiAuthenticationService authenticationService)
    {
        this.authenticationService = authenticationService;
    }

    /// <inheritdoc/>
    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await authenticationService.GetTokenAsync();
        request.Headers.Add(token.TokenScheme, token.Token);

        var response =  await base.SendAsync(request, cancellationToken);

        if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            token = await authenticationService.RefreshTokenAsync();
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token.TokenScheme, token.Token);
            response = await base.SendAsync(request, cancellationToken);
        }

        return response;
    }
}
