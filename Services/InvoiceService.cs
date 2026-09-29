using AutoSphere.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Threading.Tasks;

namespace AutoSphere.Services
{
    public class InvoiceService : IInvoiceService
    {
        public InvoiceService()
        {
            // Set QuestPDF License
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(Booking booking)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Inch);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("AUTOSPHERE").FontSize(24).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("Premium Vehicle Care").FontSize(10).Italic();
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text("INVOICE").FontSize(20).Bold();
                            col.Item().AlignRight().Text($"Invoice #: INV-{booking.BookingReference}");
                            col.Item().AlignRight().Text($"Date: {System.DateTime.Now:dd MMM yyyy}");
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        // Client and Company Info
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Billed To:").Bold();
                                c.Item().Text(booking.User?.FullName ?? "N/A");
                                c.Item().Text(booking.User?.Phone ?? "N/A");
                                c.Item().Text(booking.User?.Email ?? "N/A");
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("Service Station:").Bold();
                                c.Item().Text("AutoSphere Main HQ");
                                c.Item().Text("Plot 42, Tech Park");
                                c.Item().Text("Delhi, India");
                            });
                        });

                        col.Item().PaddingTop(1, Unit.Centimetre).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("#");
                                header.Cell().Element(CellStyle).Text("Service Description");
                                header.Cell().Element(CellStyle).AlignRight().Text("Qty");
                                header.Cell().Element(CellStyle).AlignRight().Text("Price");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.Bold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            int i = 1;
                            foreach (var detail in booking.BookingDetails)
                            {
                                table.Cell().Element(ContentStyle).Text(i++.ToString());
                                table.Cell().Element(ContentStyle).Text($"{detail.Service?.Name ?? "General Maintenance"} (Service)");
                                table.Cell().Element(ContentStyle).AlignRight().Text("1");
                                table.Cell().Element(ContentStyle).AlignRight().Text($"₹{detail.PriceAtBooking:N2}");

                                static IContainer ContentStyle(IContainer container)
                                {
                                    return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                }
                            }

                            // --- List Individual Parts ---
                            foreach (var partItem in booking.BookingParts)
                            {
                                table.Cell().Element(ContentStyle).Text(i++.ToString());
                                table.Cell().Element(ContentStyle).Text($"{partItem.Part?.Name ?? "Spare Part"} (Inventory)");
                                table.Cell().Element(ContentStyle).AlignRight().Text(partItem.Quantity.ToString());
                                table.Cell().Element(ContentStyle).AlignRight().Text($"₹{(partItem.UnitPriceAtBooking * partItem.Quantity):N2}");

                                static IContainer ContentStyle(IContainer container)
                                {
                                    return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                }
                            }

                            // Additional Charges (Labor/Misc) if any
                            if (booking.AdditionalCharges > 0)
                            {
                                table.Cell().Element(ContentStyle).Text(i.ToString());
                                table.Cell().Element(ContentStyle).Text("Additional Labor/Misc Charges");
                                table.Cell().Element(ContentStyle).AlignRight().Text("1");
                                table.Cell().Element(ContentStyle).AlignRight().Text($"₹{booking.AdditionalCharges:N2}");

                                static IContainer ContentStyle(IContainer container)
                                {
                                    return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                }
                            }
                        });

                        // Summary
                        col.Item().AlignRight().PaddingTop(20).Column(c =>
                        {
                            var totalBeforeGst = booking.TotalAmount + booking.AdditionalCharges - booking.DiscountAmount;
                            var gst = totalBeforeGst * 0.18m;
                            var final = totalBeforeGst + gst;

                            c.Item().Text($"Subtotal: ₹{totalBeforeGst:N2}");
                            c.Item().Text($"GST (18%): ₹{gst:N2}");
                            c.Item().Text($"Grand Total: ₹{final:N2}").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                        });

                        // Status Watermark-like Badge
                        col.Item().PaddingTop(30).Row(row => {
                            row.RelativeItem().Column(c => {
                                c.Item().Text("Payment Status:").Bold();
                                if (booking.Status == "Completed" || booking.Status == "Confirmed")
                                {
                                    c.Item().Text("PAID").FontColor(Colors.Green.Medium).FontSize(16).Bold();
                                }
                                else
                                {
                                    c.Item().Text("UNPAID").FontColor(Colors.Red.Medium).FontSize(16).Bold();
                                }
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return await Task.FromResult(document.GeneratePdf());
        }
    }
}
