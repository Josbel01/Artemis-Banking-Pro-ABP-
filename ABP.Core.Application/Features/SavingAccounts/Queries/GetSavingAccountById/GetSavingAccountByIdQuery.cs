using ABP.Core.Application.ViewModels.SavingAccounts;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.SavingAccounts.Queries.GetSavingAccountById
{
    public class GetSavingAccountByIdQuery : IRequest<SavingAccountViewModel>
    {
        public int Id { get; set; }
    }

    public class GetSavingAccountByIdQueryHandler : IRequestHandler<GetSavingAccountByIdQuery, SavingAccountViewModel>
    {
        private readonly IGenericRepository<ABP.Core.Domain.Entities.SavingAccount> _repository;
        private readonly IMapper _mapper;

        public GetSavingAccountByIdQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.SavingAccount> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<SavingAccountViewModel> Handle(GetSavingAccountByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(query.Id);
            if (entity == null) throw new Exception("SavingAccount not found with this id");

            return _mapper.Map<SavingAccountViewModel>(entity);
        }
    }
}
