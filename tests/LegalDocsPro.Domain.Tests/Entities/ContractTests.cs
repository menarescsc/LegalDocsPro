using FluentAssertions;
using LegalDocsPro.Domain.Entities;
using LegalDocsPro.Domain.Enums;

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
            "/documents/msa.pdf",
            expirationDate);

        contract.Title.Should().Be("Master Services Agreement");
        contract.Description.Should().Be("Agreement description");
        contract.DocumentUrl.Should().Be("/documents/msa.pdf");
        contract.ExpirationDate.Should().Be(expirationDate);
        contract.ClientName.Should().BeEmpty();
        contract.Status.Should().Be(ContractStatus.Draft);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_ShouldRejectBlankTitle(string? title)
    {
        var act = () => new Contract(title!, "Description", "document.pdf", null);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("El título del contrato es obligatorio.");
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
            .Throw<InvalidOperationException>()
            .WithMessage("Solo los borradores pueden enviarse a revisión.");
        contract.Status.Should().Be(ContractStatus.InReview);
    }

    [Fact]
    public void SendToReview_ShouldRejectApprovedContract()
    {
        var contract = CreateApprovedContract();

        var act = () => contract.SendToReview();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Solo los borradores pueden enviarse a revisión.");
        contract.Status.Should().Be(ContractStatus.Approved);
    }

    [Fact]
    public void SendToReview_ShouldRejectActiveContract()
    {
        var contract = CreateActiveContract();

        var act = () => contract.SendToReview();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Solo los borradores pueden enviarse a revisión.");
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
            .Throw<InvalidOperationException>()
            .WithMessage("Solo los contratos en revisión pueden ser aprobados.");
        contract.Status.Should().Be(ContractStatus.Draft);
    }

    [Fact]
    public void Approve_ShouldRejectApprovedContract()
    {
        var contract = CreateApprovedContract();

        var act = () => contract.Approve();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Solo los contratos en revisión pueden ser aprobados.");
        contract.Status.Should().Be(ContractStatus.Approved);
    }

    [Fact]
    public void Approve_ShouldRejectActiveContract()
    {
        var contract = CreateActiveContract();

        var act = () => contract.Approve();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Solo los contratos en revisión pueden ser aprobados.");
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
            .Throw<InvalidOperationException>()
            .WithMessage("El contrato debe estar aprobado antes de activarse.");
        contract.Status.Should().Be(ContractStatus.Draft);
    }

    [Fact]
    public void Activate_ShouldRejectInReviewContract()
    {
        var contract = CreateInReviewContract();

        var act = () => contract.Activate();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("El contrato debe estar aprobado antes de activarse.");
        contract.Status.Should().Be(ContractStatus.InReview);
    }

    [Fact]
    public void Activate_ShouldRejectActiveContract()
    {
        var contract = CreateActiveContract();

        var act = () => contract.Activate();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("El contrato debe estar aprobado antes de activarse.");
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
            .Throw<InvalidOperationException>()
            .WithMessage("No se pueden modificar los detalles de un contrato activo o expirado.");
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
            .Throw<ArgumentException>()
            .WithMessage("La URL del documento es obligatoria.");
    }

    [Fact]
    public void AttachDocument_ShouldRejectInReviewContract()
    {
        var contract = CreateInReviewContract();

        var act = () => contract.AttachDocument("/documents/updated.pdf");

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Solo se pueden adjuntar documentos a contratos en estado Borrador.");
    }

    [Fact]
    public void AttachDocument_ShouldRejectApprovedContract()
    {
        var contract = CreateApprovedContract();

        var act = () => contract.AttachDocument("/documents/updated.pdf");

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Solo se pueden adjuntar documentos a contratos en estado Borrador.");
    }

    [Fact]
    public void AttachDocument_ShouldRejectActiveContract()
    {
        var contract = CreateActiveContract();

        var act = () => contract.AttachDocument("/documents/updated.pdf");

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Solo se pueden adjuntar documentos a contratos en estado Borrador.");
    }

    private static Contract CreateContract() =>
        new("Title", "Description", "document.pdf", null);

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
