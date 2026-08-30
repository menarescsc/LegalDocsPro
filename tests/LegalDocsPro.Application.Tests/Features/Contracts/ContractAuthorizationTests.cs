using FluentAssertions;
using LegalDocsPro.Application.Common.Interfaces;
using LegalDocsPro.Application.Features.Contracts.Commands;
using LegalDocsPro.Application.Features.Contracts.Queries;
using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Interfaces;
using Moq;

namespace LegalDocsPro.Application.Tests.Features.Contracts;

public class ContractAuthorizationTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IContractRepository> _repositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDomainEventDispatcher> _eventDispatcherMock;

    public ContractAuthorizationTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IContractRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _eventDispatcherMock = new Mock<IDomainEventDispatcher>();

        _unitOfWorkMock.Setup(u => u.Contracts).Returns(_repositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // ── GetContractByIdQueryHandler ──────────────────────────────────────

    [Fact]
    public async Task GetContractById_OwnerCanAccessTheirContract()
    {
        var contract = CreateContract(createdBy: "user-alice");
        _repositoryMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(contract);
        _currentUserServiceMock.Setup(s => s.UserId).Returns("user-alice");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Standard");

        var handler = new GetContractByIdQueryHandler(_repositoryMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetContractByIdQuery(42), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
    }

    [Fact]
    public async Task GetContractById_NonOwnerGetsNull()
    {
        var contract = CreateContract(createdBy: "user-alice");
        _repositoryMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(contract);
        _currentUserServiceMock.Setup(s => s.UserId).Returns("user-bob");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Standard");

        var handler = new GetContractByIdQueryHandler(_repositoryMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetContractByIdQuery(42), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetContractById_AdminCanAccessAnyContract()
    {
        var contract = CreateContract(createdBy: "user-alice");
        _repositoryMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(contract);
        _currentUserServiceMock.Setup(s => s.UserId).Returns("admin-1");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");

        var handler = new GetContractByIdQueryHandler(_repositoryMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetContractByIdQuery(42), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
    }

    [Fact]
    public async Task GetContractById_ReturnsNullWhenContractNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Contract?)null);
        _currentUserServiceMock.Setup(s => s.UserId).Returns("user-alice");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Standard");

        var handler = new GetContractByIdQueryHandler(_repositoryMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetContractByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    // ── SendContractToReviewCommandHandler ───────────────────────────────

    [Fact]
    public async Task SendToReview_OwnerCanMutateTheirContract()
    {
        var contract = CreateContract(createdBy: "user-alice");
        _repositoryMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(contract);
        _currentUserServiceMock.Setup(s => s.UserId).Returns("user-alice");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Standard");

        var handler = new SendContractToReviewCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object, _eventDispatcherMock.Object);
        var result = await handler.Handle(new SendContractToReviewCommand(42), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendToReview_NonOwnerGetsFailure()
    {
        var contract = CreateContract(createdBy: "user-alice");
        _repositoryMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(contract);
        _currentUserServiceMock.Setup(s => s.UserId).Returns("user-bob");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Standard");

        var handler = new SendContractToReviewCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object, _eventDispatcherMock.Object);
        var result = await handler.Handle(new SendContractToReviewCommand(42), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task SendToReview_AdminCanMutateAnyContract()
    {
        var contract = CreateContract(createdBy: "user-alice");
        _repositoryMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(contract);
        _currentUserServiceMock.Setup(s => s.UserId).Returns("admin-1");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");

        var handler = new SendContractToReviewCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object, _eventDispatcherMock.Object);
        var result = await handler.Handle(new SendContractToReviewCommand(42), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // ── AttachContractDocumentCommandHandler ─────────────────────────────

    [Fact]
    public async Task AttachDocument_OwnerCanAttachToTheirContract()
    {
        var contract = CreateContract(createdBy: "user-alice");
        _repositoryMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(contract);
        _currentUserServiceMock.Setup(s => s.UserId).Returns("user-alice");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Standard");

        var handler = new AttachContractDocumentCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object, _eventDispatcherMock.Object);
        var result = await handler.Handle(new AttachContractDocumentCommand(42, "/docs/new.pdf"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AttachDocument_NonOwnerGetsFailure()
    {
        var contract = CreateContract(createdBy: "user-alice");
        _repositoryMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(contract);
        _currentUserServiceMock.Setup(s => s.UserId).Returns("user-bob");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Standard");

        var handler = new AttachContractDocumentCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object, _eventDispatcherMock.Object);
        var result = await handler.Handle(new AttachContractDocumentCommand(42, "/docs/new.pdf"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task AttachDocument_AdminCanAttachToAnyContract()
    {
        var contract = CreateContract(createdBy: "user-alice");
        _repositoryMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync(contract);
        _currentUserServiceMock.Setup(s => s.UserId).Returns("admin-1");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");

        var handler = new AttachContractDocumentCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object, _eventDispatcherMock.Object);
        var result = await handler.Handle(new AttachContractDocumentCommand(42, "/docs/new.pdf"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // ── GetContractsWithPaginationQueryHandler ───────────────────────────

    [Fact]
    public async Task GetPaged_NonAdminSeesOnlyOwnContracts()
    {
        var aliceContracts = new List<Contract> { CreateContract(createdBy: "user-alice") };
        _repositoryMock.Setup(r => r.GetPagedAsync(1, 10, null, "user-alice"))
            .ReturnsAsync((aliceContracts, 1));
        _currentUserServiceMock.Setup(s => s.UserId).Returns("user-alice");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Standard");

        var handler = new GetContractsWithPaginationQueryHandler(_repositoryMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetContractsWithPaginationQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        _repositoryMock.Verify(r => r.GetPagedAsync(1, 10, null, "user-alice"), Times.Once);
    }

    [Fact]
    public async Task GetPaged_AdminSeesAllContracts()
    {
        var allContracts = new List<Contract>
        {
            CreateContract(createdBy: "user-alice"),
            CreateContract(createdBy: "user-bob")
        };
        _repositoryMock.Setup(r => r.GetPagedAsync(1, 10, null, null))
            .ReturnsAsync((allContracts, 2));
        _currentUserServiceMock.Setup(s => s.UserId).Returns("admin-1");
        _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");

        var handler = new GetContractsWithPaginationQueryHandler(_repositoryMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetContractsWithPaginationQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        _repositoryMock.Verify(r => r.GetPagedAsync(1, 10, null, null), Times.Once);
    }

    // ── Helper ───────────────────────────────────────────────────────────

    private static Contract CreateContract(string createdBy, int id = 42)
    {
        var contract = new Contract("Title", "Description", "Client", "document.pdf", null);
        contract.Id = id;
        contract.CreatedBy = createdBy;
        return contract;
    }
}
