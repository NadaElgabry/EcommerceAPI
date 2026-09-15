using EcommerceAPI.Application.DTOs.Rag;

namespace EcommerceAPI.Application.Interfaces.ExternalServices.Rag
{
    public interface IRagClient
    {
        Task<AnswerResponse> AskAsync(QuestionRequest request, CancellationToken cancellationToken);
        Task<TerminationResponse> TerminateAsync(string userId, CancellationToken cancellationToken);
    }
}
