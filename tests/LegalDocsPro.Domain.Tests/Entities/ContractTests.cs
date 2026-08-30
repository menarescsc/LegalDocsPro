using FluentAssertions;
using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Enums;
using LegalDocsPro.Domain.Exceptions;

namespace LegalDocsPro.Domain.Tests.Entities;

public class ContractTests
{
    [Fact]
    public void Constructor_ShouldInitializeContractInDraftState()
    {
        var expirationDate = new DateTime(2030, 12, 31);

        var contract = new Contract(
            "Master Services Agreement",
            "Agreement description",
            "Client Name",
            "/documents/msa.pdf",
            expirationDate);

        contract.Title.Should().Be("Master Services Agreement");
        contract.Description.Should().Be("Agreement description");
        contract.DocumentUrl.Should().Be("/documents/msa.pdf");
        contract.ExpirationDate.Should().Be(expirationDate);
        contract.ClientName.Should().Be("Client Name");
        contract.Status.Should().Be(ContractStatus.Draft);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_ShouldRejectBlankTitle(string? title)
    {
        var act = () => new Contract(title!, "Description", "Client", "document.pdf", null);

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Contract title is required.");
    }

    [Fact]
    public void SendToReview_ShouldMoveDraftToInReview()
    {
        var contract = CreateContract();

        contract.SendToReview();

        contract.Status.Should().Be(ContractStatus.InReview);
    }

    [Fact]
    public void SendToReview_ShouldRejectContractAlreadyInReview()
    {
        var contract = CreateInReviewContract();

        var act = () => contract.SendToReview();

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Only draft contracts can be sent to review.");
        contract.Status.Should().Be(ContractStatus.InReview);
    }

    [Fact]
    public void SendToReview_ShouldRejectApprovedContract()
    {
        var contract = CreateApprovedContract();

        var act = () => contract.SendToReview();

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Only draft contracts can be sent to review.");
        contract.Status.Should().Be(ContractStatus.Approved);
    }

    [Fact]
    public void SendToReview_ShouldRejectActiveContract()
    {
        var contract = CreateActiveContract();

        var act = () => contract.SendToReview();

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Only draft contracts can be sent to review.");
        contract.Status.Should().Be(ContractStatus.Active);
    }

    [Fact]
    public void Approve_ShouldMoveInReviewToApproved()
    {
        var contract = CreateInReviewContract();

        contract.Approve();

        contract.Status.Should().Be(ContractStatus.Approved);
    }

    [Fact]
    public void Approve_ShouldRejectDraft()
    {
        var contract = CreateContract();

        var act = () => contract.Approve();

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Only contracts in review can be approved.");
        contract.Status.Should().Be(ContractStatus.Draft);
    }

    [Fact]
    public void Approve_ShouldRejectApprovedContract()
    {
        var contract = CreateApprovedContract();

        var act = () => contract.Approve();

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Only contracts in review can be approved.");
        contract.Status.Should().Be(ContractStatus.Approved);
    }

    [Fact]
    public void Approve_ShouldRejectActiveContract()
    {
        var contract = CreateActiveContract();

        var act = () => contract.Approve();

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Only contracts in review can be approved.");
        contract.Status.Should().Be(ContractStatus.Active);
    }

    [Fact]
    public void Activate_ShouldMoveApprovedToActive()
    {
        var contract = CreateApprovedContract();

        contract.Activate();

        contract.Status.Should().Be(ContractStatus.Active);
    }

    [Fact]
    public void Activate_ShouldRejectDraft()
    {
        var contract = CreateContract();

        var act = () => contract.Activate();

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Contract must be approved before activation.");
        contract.Status.Should().Be(ContractStatus.Draft);
    }

    [Fact]
    public void Activate_ShouldRejectInReviewContract()
    {
        var contract = CreateInReviewContract();

        var act = () => contract.Activate();

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Contract must be approved before activation.");
        contract.Status.Should().Be(ContractStatus.InReview);
    }

    [Fact]
    public void Activate_ShouldRejectActiveContract()
    {
        var contract = CreateActiveContract();

        var act = () => contract.Activate();

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Contract must be approved before activation.");
        contract.Status.Should().Be(ContractStatus.Active);
    }

    [Fact]
    public void UpdateDetails_ShouldChangeDetailsInDraft()
    {
        var contract = CreateContract();

        contract.UpdateDetails("Updated title", "Updated description");

        contract.Title.Should().Be("Updated title");
        contract.Description.Should().Be("Updated description");
    }

    [Fact]
    public void UpdateDetails_ShouldChangeDetailsInReview()
    {
        var contract = CreateInReviewContract();

        contract.UpdateDetails("Updated title", "Updated description");

        contract.Title.Should().Be("Updated title");
        contract.Description.Should().Be("Updated description");
    }

    [Fact]
    public void UpdateDetails_ShouldChangeDetailsWhenApproved()
    {
        var contract = CreateApprovedContract();

        contract.UpdateDetails("Updated title", "Updated description");

        contract.Title.Should().Be("Updated title");
        contract.Description.Should().Be("Updated description");
    }

    [Fact]
    public void UpdateDetails_ShouldRejectActiveContractAndPreserveDetails()
    {
        var contract = CreateActiveContract();

        var act = () => contract.UpdateDetails("Updated title", "Updated description");

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Cannot modify details of an active or expired contract.");
        contract.Title.Should().Be("Title");
        contract.Description.Should().Be("Description");
    }

    [Fact]
    public void AttachDocument_ShouldSetDocumentUrlInDraft()
    {
        var contract = CreateContract();

        contract.AttachDocument("/documents/updated.pdf");

        contract.DocumentUrl.Should().Be("/documents/updated.pdf");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AttachDocument_ShouldRejectBlankUrlInDraft(string? documentUrl)
    {
        var contract = CreateContract();

        var act = () => contract.AttachDocument(documentUrl!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Document URL is required.");
    }

    [Fact]
    public void AttachDocument_ShouldRejectInReviewContract()
    {
        var contract = CreateInReviewContract();

        var act = () => contract.AttachDocument("/documents/updated.pdf");

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Documents can only be attached to draft contracts.");
    }

    [Fact]
    public void AttachDocument_ShouldRejectApprovedContract()
    {
        var contract = CreateApprovedContract();

        var act = () => contract.AttachDocument("/documents/updated.pdf");

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Documents can only be attached to draft contracts.");
    }

    [Fact]
    public void AttachDocument_ShouldRejectActiveContract()
    {
        var contract = CreateActiveContract();

        var act = () => contract.AttachDocument("/documents/updated.pdf");

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Documents can only be attached to draft contracts.");
    }

    private static Contract CreateContract() =>
        new("Title", "Description", "Client", "document.pdf", null);

    private static Contract CreateInReviewContract()
    {
        var contract = CreateContract();
        contract.SendToReview();
        return contract;
    }

    private static Contract CreateApprovedContract()
    {
        var contract = CreateInReviewContract();
        contract.Approve();
        return contract;
    }

    private static Contract CreateActiveContract()
    {
        var contract = CreateApprovedContract();
        contract.Activate();
        return contract;
    }
}
