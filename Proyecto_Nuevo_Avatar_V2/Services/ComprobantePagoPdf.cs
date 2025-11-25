using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Proyecto_Nuevo_Avatar_V2.Services
{
    public class ComprobantePagoPdf
    {
        static ComprobantePagoPdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static byte[] GenerarComprobante(int? numeroPago, long numeroFactura, decimal montoPago, string periodo = null)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // Header
                    page.Header()
                        .AlignCenter()
                        .Column(col =>
                        {
                            col.Item().Text("COMPROBANTE DE PAGO").Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            col.Item().PaddingTop(5).Text($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        });

                    // Content
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            col.Spacing(20);

                            // Información del pago
                            col.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(column =>
                            {
                                column.Spacing(10);

                                if (numeroPago.HasValue)
                                {
                                    column.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Número de Pago:").Bold();
                                        row.RelativeItem().Text(numeroPago.Value.ToString()).FontColor(Colors.Blue.Darken1);
                                    });
                                }

                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Número de Factura:").Bold();
                                    row.RelativeItem().Text(numeroFactura.ToString()).FontColor(Colors.Blue.Darken1);
                                });

                                if (!string.IsNullOrEmpty(periodo))
                                {
                                    column.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Periodo:").Bold();
                                        row.RelativeItem().Text(periodo);
                                    });
                                }

                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Fecha de Pago:").Bold();
                                    row.RelativeItem().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                                });

                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Estado:").Bold();
                                    row.RelativeItem().Text("PAGADO").FontColor(Colors.Green.Medium).Bold();
                                });
                            });

                            // Monto
                            col.Item().Background(Colors.Green.Lighten4)
                                .Border(2)
                                .BorderColor(Colors.Green.Medium)
                                .Padding(20)
                                .Column(column =>
                                {
                                    column.Item().AlignCenter().Text("MONTO PAGADO").Bold().FontSize(14);
                                    column.Item().AlignCenter().Text($"₡{montoPago:N2}").Bold().FontSize(24).FontColor(Colors.Green.Darken2);
                                });

                            // Nota - CORREGIDO
                            col.Item().PaddingTop(20)
                                .DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken1))
                                .Text(text =>
                                {
                                    text.Span("Nota: ").Bold();
                                    text.Span("Este comprobante certifica que el pago ha sido registrado exitosamente en el sistema.");
                                });
                        });

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Documento generado automáticamente - ");
                            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
