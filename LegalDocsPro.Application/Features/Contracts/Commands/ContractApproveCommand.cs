using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace LegalDocsPro.Application.Features.Contracts.Commands
{
    public record ContractApproveCommand(int Id) : IRequest<bool>;
}
