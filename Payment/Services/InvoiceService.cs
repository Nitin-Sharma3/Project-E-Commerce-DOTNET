using iTextSharp.text;
using iTextSharp.text.pdf;
using RazorpayApi.Models;
using System.Text;
using System.Xml.Linq;

namespace RazorpayApi.Services;
public class InvoiceService : IInvoiceService
{
    private readonly ILogger<InvoiceService> _logger;
    private readonly string _invoiceDirectory;

    // Currency symbols
    private static readonly Dictionary<string, string> CurrencySymbols = new()
    {
        { "INR", "Rs." },
        { "USD", "$" },
        { "EUR", "EUR" },
        { "GBP", "GBP" }
    };

    public InvoiceService(ILogger<InvoiceService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _invoiceDirectory = Path.Combine(env.WebRootPath, "invoices");

        // Create invoices directory if it doesn't exist
        if (!Directory.Exists(_invoiceDirectory))
        {
            Directory.CreateDirectory(_invoiceDirectory);
        }
    }

    public string GenerateInvoiceFileName(string orderId, string paymentId)
    {
        return $"Invoice_{orderId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
    }

    public InvoiceResponse GenerateInvoicePdf(InvoiceRequest request)
    {
        try
        {
            var fileName = GenerateInvoiceFileName(request.OrderId, request.PaymentId);
            var filePath = Path.Combine(_invoiceDirectory, fileName);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                Document doc = new Document(PageSize.A4, 20, 20, 20, 20);
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                // Add content
                AddHeader(doc, request);
                AddSellerInfo(doc, request);
                AddCustomerInfo(doc, request);
                doc.Add(new Paragraph(" ")); // spacing
                AddInvoiceDetails(doc, request);
                doc.Add(new Paragraph(" ")); // spacing
                AddItemsTable(doc, request);
                AddTotals(doc, request);
                doc.Add(new Paragraph(" ")); // spacing
                AddTermsAndConditions(doc, request);
                AddFooter(doc, request);

                doc.Close();
                writer.Close();
            }

            // Also return PDF as bytes for download
            byte[] pdfBytes = File.ReadAllBytes(filePath);

            _logger.LogInformation("Invoice generated: {FileName}", fileName);

            return new InvoiceResponse
            {
                Success = true,
                FilePath = filePath,
                FileName = fileName,
                PdfBytes = pdfBytes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate invoice for order {OrderId}", request.OrderId);
            return new InvoiceResponse
            {
                Success = false,
                FileName = string.Empty,
                FilePath = string.Empty
            };
        }
    }

    private void AddHeader(Document doc, InvoiceRequest request)
    {
        // Title
        var title = new Paragraph("INVOICE", new Font(Font.FontFamily.HELVETICA, 24, Font.BOLD));
        title.Alignment = Element.ALIGN_CENTER;
        doc.Add(title);

        var invoiceNum = new Paragraph($"Invoice #: {request.OrderId}",
            new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL));
        invoiceNum.Alignment = Element.ALIGN_CENTER;
        doc.Add(invoiceNum);

        doc.Add(new Paragraph(" "));
    }

    private void AddSellerInfo(Document doc, InvoiceRequest request)
    {
        var seller = request.SellerDetails;
        if (seller == null) return;

        var sellerTable = new PdfPTable(1) { WidthPercentage = 100f };
        sellerTable.DefaultCell.Border = Rectangle.BOX;
        sellerTable.DefaultCell.Padding = 8f;

        var sellerCell = new PdfPCell(new Paragraph(
            $"{seller.CompanyName}\n" +
            $"{seller.Address}\n" +
            $"{seller.City}, {seller.State} {seller.PostalCode}\n" +
            $"Phone: {seller.Phone}\n" +
            $"Email: {seller.Email}" +
            (string.IsNullOrEmpty(seller.GstNumber) ? "" : $"\nGST: {seller.GstNumber}"),
            new Font(Font.FontFamily.HELVETICA, 10)))
        {
            Border = Rectangle.BOX,
            Padding = 10f
        };

        sellerTable.AddCell(sellerCell);
        doc.Add(sellerTable);
        doc.Add(new Paragraph(" "));
    }

    private void AddCustomerInfo(Document doc, InvoiceRequest request)
    {
        var customer = request.CustomerDetails;
        if (customer == null) return;

        var customerLabel = new Paragraph("Bill To:", new Font(Font.FontFamily.HELVETICA, 11, Font.BOLD));
        doc.Add(customerLabel);

        var customerInfo = new Paragraph(
            $"{customer.Name}\n" +
            $"{customer.Address}\n" +
            $"{customer.City}, {customer.State} {customer.PostalCode}\n" +
            $"Phone: {customer.Phone}\n" +
            $"Email: {customer.Email}",
            new Font(Font.FontFamily.HELVETICA, 10));
        doc.Add(customerInfo);
        doc.Add(new Paragraph(" "));
    }

    private void AddInvoiceDetails(Document doc, InvoiceRequest request)
    {
        var detailsTable = new PdfPTable(2) { WidthPercentage = 100f };
        detailsTable.SetWidths(new[] { 0.5f, 0.5f });

        // Left column
        var invoiceDate = request.TransactionDate;
        try
        {
            var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            var istZone = TimeZoneInfo.FindSystemTimeZoneById(isWindows ? "India Standard Time" : "Asia/Kolkata");
            invoiceDate = TimeZoneInfo.ConvertTime(invoiceDate.ToUniversalTime(), istZone);
        }
        catch (Exception)
        {
            invoiceDate = invoiceDate.ToLocalTime();
        }
        AddDetailRow(detailsTable, "Invoice Date:", invoiceDate.ToString("dd-MMM-yyyy"));
        AddDetailRow(detailsTable, "Order ID:", request.OrderId);
        AddDetailRow(detailsTable, "Payment Method:", request.PaymentMethod);

        // Right column (empty for alignment)
        AddDetailRow(detailsTable, "Payment ID:", request.PaymentId);
        AddDetailRow(detailsTable, "Currency:", request.Currency);

        doc.Add(detailsTable);
    }

    private void AddDetailRow(PdfPTable table, string label, string value)
    {
        var labelCell = new PdfPCell(new Paragraph(label, new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD)))
        {
            Border = Rectangle.NO_BORDER,
            Padding = 5f
        };

        var valueCell = new PdfPCell(new Paragraph(value, new Font(Font.FontFamily.HELVETICA, 10)))
        {
            Border = Rectangle.NO_BORDER,
            Padding = 5f
        };

        table.AddCell(labelCell);
        table.AddCell(valueCell);
    }

    private void AddItemsTable(Document doc, InvoiceRequest request)
    {
        var itemsTable = new PdfPTable(4) { WidthPercentage = 100f };
        itemsTable.SetWidths(new[] { 0.4f, 0.2f, 0.2f, 0.2f });

        // Headers
        var currencySymbol = CurrencySymbols.ContainsKey(request.Currency)
            ? CurrencySymbols[request.Currency]
            : request.Currency;

        AddHeaderCell(itemsTable, "Item Description");
        AddHeaderCell(itemsTable, "Qty");
        AddHeaderCell(itemsTable, $"Unit Price ({currencySymbol})");
        AddHeaderCell(itemsTable, $"Amount ({currencySymbol})");

        // Items
        foreach (var item in request.Items)
        {
            itemsTable.AddCell(new PdfPCell(new Paragraph(item.ItemName, new Font(Font.FontFamily.HELVETICA, 10)))
            {
                Padding = 8f,
                Border = Rectangle.BOTTOM_BORDER
            });
            itemsTable.AddCell(new PdfPCell(new Paragraph(item.Quantity.ToString(), new Font(Font.FontFamily.HELVETICA, 10)))
            {
                Padding = 8f,
                Border = Rectangle.BOTTOM_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            itemsTable.AddCell(new PdfPCell(new Paragraph(item.UnitPrice.ToString("N2"), new Font(Font.FontFamily.HELVETICA, 10)))
            {
                Padding = 8f,
                Border = Rectangle.BOTTOM_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT
            });
            itemsTable.AddCell(new PdfPCell(new Paragraph(item.TotalPrice.ToString("N2"), new Font(Font.FontFamily.HELVETICA, 10)))
            {
                Padding = 8f,
                Border = Rectangle.BOTTOM_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT
            });
        }

        doc.Add(itemsTable);
    }

    private void AddHeaderCell(PdfPTable table, string headerText)
    {
        var cell = new PdfPCell(new Paragraph(headerText, new Font(Font.FontFamily.HELVETICA, 11, Font.BOLD)))
        {
            BackgroundColor = new BaseColor(200, 200, 200),
            Padding = 10f,
            HorizontalAlignment = Element.ALIGN_CENTER
        };
        table.AddCell(cell);
    }

    private void AddTotals(Document doc, InvoiceRequest request)
    {
        var currencySymbol = CurrencySymbols.ContainsKey(request.Currency)
            ? CurrencySymbols[request.Currency]
            : request.Currency;

        var totalsTable = new PdfPTable(2) { WidthPercentage = 50f };
        totalsTable.HorizontalAlignment = Element.ALIGN_RIGHT;
        totalsTable.SetWidths(new[] { 0.6f, 0.4f });

        // Subtotal
        AddTotalRow(totalsTable, "Subtotal:", request.Subtotal, currencySymbol);

        // Discount
        if (request.DiscountAmount > 0)
        {
            AddTotalRow(totalsTable, "Discount:", -request.DiscountAmount, currencySymbol);
        }

        // Tax
        if (request.TaxAmount > 0)
        {
            AddTotalRow(totalsTable, "Tax:", request.TaxAmount, currencySymbol);
        }

        // Grand Total
        var totalLabelCell = new PdfPCell(new Paragraph("Total Due:", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD)))
        {
            Border = Rectangle.TOP_BORDER,
            Padding = 10f,
            HorizontalAlignment = Element.ALIGN_RIGHT
        };

        var totalValueCell = new PdfPCell(new Paragraph($"{currencySymbol} {request.Total:N2}",
            new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD)))
        {
            Border = Rectangle.TOP_BORDER,
            Padding = 10f,
            HorizontalAlignment = Element.ALIGN_RIGHT,
            BackgroundColor = new BaseColor(240, 240, 240)
        };

        totalsTable.AddCell(totalLabelCell);
        totalsTable.AddCell(totalValueCell);

        doc.Add(totalsTable);
    }

    private void AddTotalRow(PdfPTable table, string label, decimal amount, string currencySymbol)
    {
        var labelCell = new PdfPCell(new Paragraph(label, new Font(Font.FontFamily.HELVETICA, 10)))
        {
            Border = Rectangle.NO_BORDER,
            Padding = 8f,
            HorizontalAlignment = Element.ALIGN_RIGHT
        };

        var amountCell = new PdfPCell(new Paragraph($"{currencySymbol} {Math.Abs(amount):N2}", new Font(Font.FontFamily.HELVETICA, 10)))
        {
            Border = Rectangle.NO_BORDER,
            Padding = 8f,
            HorizontalAlignment = Element.ALIGN_RIGHT
        };

        table.AddCell(labelCell);
        table.AddCell(amountCell);
    }

    private void AddTermsAndConditions(Document doc, InvoiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TermsAndConditions))
            return;

        var termsLabel = new Paragraph("Terms & Conditions:", new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD));
        doc.Add(termsLabel);

        var terms = new Paragraph(request.TermsAndConditions, new Font(Font.FontFamily.HELVETICA, 9));
        doc.Add(terms);
    }

    private void AddFooter(Document doc, InvoiceRequest request)
    {
        doc.Add(new Paragraph(" "));
        var footer = new Paragraph("Thank you for your business!",
            new Font(Font.FontFamily.HELVETICA, 10, Font.ITALIC));
        footer.Alignment = Element.ALIGN_CENTER;
        doc.Add(footer);

        var timestamp = new Paragraph($"Generated on {DateTime.Now:dd-MMM-yyyy HH:mm:ss}",
            new Font(Font.FontFamily.HELVETICA, 8));
        timestamp.Alignment = Element.ALIGN_CENTER;
        doc.Add(timestamp);
    }
}
