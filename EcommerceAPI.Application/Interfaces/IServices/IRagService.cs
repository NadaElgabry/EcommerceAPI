using EcommerceAPI.Application.DTOs.Rag;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IRagService
    {
        public Task<AnswerResponse> AskAsync(string question,CancellationToken cancellationToken);
        public Task<bool> TerminateAsync(CancellationToken cancellationToken);
    }
}
