using System.ComponentModel.DataAnnotations;

using ERP.SharedKernel.Utilities;

namespace ERP.Domain.Entities
{
    public class InvoiceLineItem
    {
        private InvoiceLineItem() { } // EF Core

        public InvoiceLineItem(Guid invoiceId, string description, decimal quantity, decimal unitPrice)
        {
            Guard.AgainstInvalidGuid(invoiceId, nameof(invoiceId));
            Guard.AgainstNullOrEmpty(description, nameof(description));
            Guard.Against(quantity <= 0, "Quantity must be positive");
            Guard.AgainstNegative(unitPrice, nameof(unitPrice));

            Id = Guid.NewGuid();
            InvoiceId = invoiceId;
            Description = description;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public Guid Id { get; private set; }
        public Guid InvoiceId { get; private set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; private set; } = default!;

        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; private set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; private set; }

        public decimal Amount => Quantity * UnitPrice;

        // Navigation property
        public Invoice Invoice { get; private set; } = default!;

        public void UpdateQuantity(decimal newQuantity)
        {
            Guard.Against(newQuantity <= 0, "Quantity must be positive");
            Quantity = newQuantity;
        }

        public void UpdateUnitPrice(decimal newUnitPrice)
        {
            Guard.AgainstNegative(newUnitPrice, nameof(newUnitPrice));
            UnitPrice = newUnitPrice;
        }

        public void UpdateDescription(string newDescription)
        {
            Guard.AgainstNullOrEmpty(newDescription, nameof(newDescription));
            Description = newDescription;
        }
    }
}