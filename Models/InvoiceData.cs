using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace QuickBooksETLService.Models
{
    public class InvoiceData
    {
        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("customer")]
        public CustomerInfo Customer { get; set; } = new();

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("lineItems")]
        public List<LineItem> LineItems { get; set; } = new();

        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("dueDate")]
        public DateTime? DueDate { get; set; }

        [JsonProperty("memo")]
        public string? Memo { get; set; }

        [JsonProperty("terms")]
        public string? Terms { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTime ModifiedDate { get; set; }
    }

    public class CustomerInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("phone")]
        public string? Phone { get; set; }

        [JsonProperty("address")]
        public AddressInfo? Address { get; set; }
    }

    public class AddressInfo
    {
        [JsonProperty("line1")]
        public string? Line1 { get; set; }

        [JsonProperty("line2")]
        public string? Line2 { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("state")]
        public string? State { get; set; }

        [JsonProperty("postalCode")]
        public string? PostalCode { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }
    }

    public class LineItem
    {
        [JsonProperty("itemName")]
        public string ItemName { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty("rate")]
        public decimal Rate { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("itemType")]
        public string? ItemType { get; set; }
    }
}
