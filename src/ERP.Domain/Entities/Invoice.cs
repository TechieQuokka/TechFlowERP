using System.ComponentModel.DataAnnotations;
using System.Linq;

using ERP.Domain.Enums;
using ERP.Domain.ValueObjects;
using ERP.SharedKernel.Abstractions;
using ERP.SharedKernel.Utilities;

namespace ERP.Domain.Entities
{
    public class Invoice : BaseEntity
    {
        private readonly List<InvoiceLineItem> _lineItems = new();

        private Invoice() { } // EF Core

        public Invoice(string tenantId, string invoiceNumber, Guid projectId, Guid clientId,
            DateTime dueDate) : base(tenantId)
        {
            Guard.AgainstNullOrEmpty(invoiceNumber, nameof(invoiceNumber));
            Guard.AgainstInvalidGuid(projectId, nameof(projectId));
            Guard.AgainstInvalidGuid(clientId, nameof(clientId));
            Guard.Against(dueDate.Date <= DateTime.Today, "Due date must be in the future");

            InvoiceNumber = invoiceNumber;
            ProjectId = projectId;
            ClientId = clientId;
            DueDate = dueDate.Date;
            Status = InvoiceStatus.Draft;
            Currency = "USD";
            TaxRate = 0;
            InvoiceDate = DateTime.Today;
        }

        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; private set; } = default!;

        public Guid ProjectId { get; private set; }
        public Guid ClientId { get; private set; }

        public DateTime InvoiceDate { get; private set; }
        public DateTime DueDate { get; private set; }
        public DateTime? PaymentDate { get; private set; }

        public InvoiceStatus Status { get; private set; }

        [MaxLength(3)]
        public string Currency { get; private set; } = default!;

        [Range(0, 100)]
        public decimal TaxRate { get; private set; }

        public string? Notes { get; private set; }
        public string? PaymentMethod { get; private set; }

        public IReadOnlyCollection<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

        // Navigation properties
        public Project Project { get; private set; } = default!;
        public Client Client { get; private set; } = default!;

        public Money SubTotal => new(_lineItems.Sum(item => item.Amount), Currency);
        public Money TaxAmount => new(SubTotal.Amount * TaxRate / 100, Currency);
        public Money TotalAmount => SubTotal.Add(TaxAmount);

        public bool IsOverdue => Status == InvoiceStatus.Sent && DateTime.Today > DueDate;

        public void AddLineItem(string description, decimal quantity, decimal unitPrice)
        {
            Guard.Against(Status != InvoiceStatus.Draft, "Cannot modify non-draft invoice");
            Guard.AgainstNullOrEmpty(description, nameof(description));
            Guard.Against(quantity <= 0, "Quantity must be positive");
            Guard.AgainstNegative(unitPrice, nameof(unitPrice));

            var lineItem = new InvoiceLineItem(Id, description, quantity, unitPrice);
            _lineItems.Add(lineItem);
            UpdateTimestamp();
        }

        public void RemoveLineItem(Guid lineItemId)
        {
            Guard.Against(Status != InvoiceStatus.Draft, "Cannot modify non-draft invoice");

            var item = _lineItems.FirstOrDefault(li => li.Id == lineItemId);
            if (item != null)
            {
                _lineItems.Remove(item);
                UpdateTimestamp();
            }
        }

        public void Send()
        {
            Guard.Against(Status != InvoiceStatus.Draft, "Only draft invoices can be sent");
            Guard.Against(!_lineItems.Any(), "Cannot send invoice without line items");

            Status = InvoiceStatus.Sent;
            UpdateTimestamp();
        }

        public void MarkAsPaid(DateTime paymentDate, string? paymentMethod = null)
        {
            Guard.Against(Status != InvoiceStatus.Sent, "Only sent invoices can be marked as paid");
            Guard.Against(paymentDate.Date < InvoiceDate, "Payment date cannot be before invoice date");

            Status = InvoiceStatus.Paid;
            PaymentDate = paymentDate.Date;
            PaymentMethod = paymentMethod;
            UpdateTimestamp();
        }

        public void Cancel()
        {
            Guard.Against(Status == InvoiceStatus.Paid, "Cannot cancel paid invoice");

            Status = InvoiceStatus.Cancelled;
            UpdateTimestamp();
        }

        public void UpdateNotes(string? notes)
        {
            Notes = notes;
            UpdateTimestamp();
        }

        public void UpdateTaxRate(decimal taxRate)
        {
            Guard.Against(Status != InvoiceStatus.Draft, "Cannot modify non-draft invoice");
            Guard.Against(taxRate < 0 || taxRate > 100, "Tax rate must be between 0 and 100");

            TaxRate = taxRate;
            UpdateTimestamp();
        }
    }
}