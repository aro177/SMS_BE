using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Student_Management_System.Config
{
    public class SupabaseAuthHandler : DelegatingHandler
    {
        private readonly SupabaseOptions _options;

        public SupabaseAuthHandler(IOptions<SupabaseOptions> options)
        {
            _options = options.Value;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.ApiSecretKey);

            request.Headers.Add("apikey", _options.ApiSecretKey);

            if (request.Content != null)
            {
                request.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
