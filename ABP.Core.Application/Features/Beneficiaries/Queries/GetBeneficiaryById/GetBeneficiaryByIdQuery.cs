using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Dtos.Beneficiaries;
using AutoMapper;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ABP.Core.Application.Features.Beneficiaries.Queries.GetBeneficiaryById
{
    /// <summary>
    /// Parameters required to get a beneficiary by id
    /// </summary>
    public class GetBeneficiaryByIdQuery : IRequest<BeneficiaryDto>
    {
        [SwaggerParameter(Description = "The Id of the beneficiary to retrieve")]
        public int Id { get; set; }
    }

    public class GetBeneficiaryByIdQueryHandler : IRequestHandler<GetBeneficiaryByIdQuery, BeneficiaryDto>
    {
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly IMapper _mapper;

        public GetBeneficiaryByIdQueryHandler(IBeneficiaryRepository beneficiaryRepository, IMapper mapper)
        {
            _beneficiaryRepository = beneficiaryRepository;
            _mapper = mapper;
        }

        public async Task<BeneficiaryDto> Handle(GetBeneficiaryByIdQuery request, CancellationToken cancellationToken)
        {
            var beneficiary = await _beneficiaryRepository.GetByIdAsync(request.Id);

            if (beneficiary == null)
            {
                throw new Exception("Beneficiary not found");
            }

            var beneficiaryDto = _mapper.Map<BeneficiaryDto>(beneficiary);

            return beneficiaryDto;
        }
    }
}