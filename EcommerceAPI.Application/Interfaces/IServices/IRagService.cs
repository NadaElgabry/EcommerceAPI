using EcommerceAPI.Application.DTOs.Rag;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IRagService
    {
        Task<AnswerResponse> AskAsync(string question,CancellationToken cancellationToken);
        Task<TerminationResult> TerminateAsync(CancellationToken cancellationToken);
    }
}
