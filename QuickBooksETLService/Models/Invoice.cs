using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace QuickBooksETLService.Models
{
    public class Invoice
    {
        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("dueDate")]
        public DateTime? DueDate { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; } = new Customer();

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        [JsonProperty("memo")]
        public string Memo { get; set; } = string.Empty;

        [JsonProperty("lineItems")]
        public List<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();

        [JsonProperty("quickBooksId")]
        public string QuickBooksId { get; set; } = string.Empty;

        [JsonProperty("extractedAt")]
        public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
    }

    public class Customer
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("companyName")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonProperty("address")]
        public Address Address { get; set; } = new Address();
    }

    public class Address
    {
        [JsonProperty("line1")]
        public string Line1 { get; set; } = string.Empty;

        [JsonProperty("line2")]
        public string Line2 { get; set; } = string.Empty;

        [JsonProperty("city")]
        public string City { get; set; } = string.Empty;

        [JsonProperty("state")]
        public string State { get; set; } = string.Empty;

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; } = string.Empty;

        [JsonProperty("country")]
        public string Country { get; set; } = string.Empty;
    }

    public class InvoiceLineItem
    {
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("itemName")]
        public string ItemName { get; set; } = string.Empty;

        [JsonProperty("itemType")]
        public string ItemType { get; set; } = string.Empty;
    }
} 