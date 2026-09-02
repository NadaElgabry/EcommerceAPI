using System.Net.Http.Json;
using EcommerceAPI.Application.DTOs.Rag;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.ExternalServices.Rag;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Infrastructure.ExternalServices.Rag
{
    public class RagClient : IRagClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<RagClient> _logger;

        public RagClient(HttpClient http, ILogger<RagClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<AnswerResponse> AskAsync(QuestionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/chat", request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<AnswerResponse>(cancellationToken: cancellationToken);
                return result ?? throw new ExternalServiceException("AI service returned an empty response.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "RAG service unreachable while calling /chat for user {UserId}", request.UserId);
                throw new ExternalServiceException("The assistant is temporarily unavailable.");
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "RAG service timed out while calling /chat for user {UserId}", request.UserId);
                throw new ExternalServiceException("The assistant took too long to respond.");
            }
        }

        public async Task<TerminationResponse> TerminateAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                var request = new TerminationRequest { UserId = userId };
                var response = await _http.PostAsJsonAsync("/terminate", request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<TerminationResponse>(cancellationToken: cancellationToken);
                return result ?? new TerminationResponse { UserId = userId, SuggestedProducts = new() };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "RAG service unreachable while calling /terminate for user {UserId}", userId);
                throw new ExternalServiceException("The assistant is temporarily unavailable.");
            }
        }
    }
}