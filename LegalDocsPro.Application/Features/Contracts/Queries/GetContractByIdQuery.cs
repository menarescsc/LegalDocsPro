using LegalDocsPro.Application.Dtos;
using MediatR;

namespace LegalDocsPro.Application.Features.Contracts.Queries
{
    // IRequest<ContractDto?> significa: "Alguien hará esta pregunta pasando un 'Id', 
    // y espera que se le responda con un ContractDto o null (si no existe)".
    public record GetContractByIdQuery(int Id) : IRequest<ContractDto?>;
}