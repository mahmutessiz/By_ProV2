using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

// --- Data Models (to hold your report data) ---

public class ReportItem
{
    public string Tarih { get; set; }
    public double Miktar { get; set; }
    public double Kesinti { get; set; }
    public double Yag { get; set; }
    public double Protein { get; set; }
}

/// <summary>
/// Represents the summary section at the bottom of the report.
/// </summary>
public class ReportSummary
{
    public double NetMiktar { get; set; }
    public double Kesinti { get; set; }
    public double OrtYag { get; set; }
    public double OrtProtein { get; set; }
    public int ToplamKayit { get; set; }
}

/// <summary>
/// Represents all data needed for the entire report.
/// </summary>
public class ReportData
{
    public string Title { get; set; }
    public string DateRange { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public List<string> ColumnHeaders { get; set; } = new List<string>();
    public List<ReportItem> Items { get; set; } = new List<ReportItem>();
    public ReportSummary Summary { get; set; }
}

/// <summary>
/// Main class responsible for generating the PDF report.
/// </summary>
public static class ReportGenerator
{
    /// <summary>
    /// Generates the PDF report from the provided data and saves it to the specified file path.
    /// </summary>
    /// <param name="data">The report data to render.</param>
    /// <param name="filePath">The full path where the PDF file will be saved.</param>
    public static void GenerateReport(ReportData data, string filePath)
    {
        using (PdfDocument document = new PdfDocument())
        {
            PdfPage page = document.AddPage();
            page.Orientation = PdfSharp.PageOrientation.Portrait;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // --- Fonts ---
            XFont fontTitle = new XFont("Arial", 16, XFontStyleEx.Bold);
            XFont fontSubtitle = new XFont("Arial", 12, XFontStyleEx.Regular);
            XFont fontHeader = new XFont("Arial", 10, XFontStyleEx.Bold);
            XFont fontBody = new XFont("Arial", 9, XFontStyleEx.Regular);
            XFont fontBodyBold = new XFont("Arial", 9, XFontStyleEx.Bold);

            XBrush brushBlack = XBrushes.Black;

            // --- Page layout ---
            double pageWidth = page.Width.Point;
            double pageHeight = page.Height.Point;
            double topMargin = 50;
            double leftMargin = 40;
            double rightMargin = 40;
            double bottomMargin = 50;
            double currentY = topMargin;

            // --- String formats ---
            XStringFormat formatCenter = new XStringFormat { Alignment = XStringAlignment.Center };
            XStringFormat formatRight = new XStringFormat { Alignment = XStringAlignment.Far };
            XStringFormat formatLeft = new XStringFormat { Alignment = XStringAlignment.Near };

            // === Title ===
            gfx.DrawString(data.Title, fontTitle, brushBlack, new XRect(0, currentY, pageWidth, 0), formatCenter);
            currentY += 25;
            gfx.DrawString(data.DateRange, fontSubtitle, brushBlack, new XRect(0, currentY, pageWidth, 0), formatCenter);
            currentY += 40;

            // === Customer Info ===
            double infoX = leftMargin + 70;
            XSize cariKoduSize = gfx.MeasureString("Cari Kodu:", fontHeader);
            XSize cariAdiSize = gfx.MeasureString("Cari Adı:", fontHeader);
            double labelWidth = Math.Max(cariKoduSize.Width, cariAdiSize.Width) + 10;
            
            gfx.DrawString("Cari Kodu:", fontHeader, brushBlack, infoX, currentY, formatLeft);
            gfx.DrawString(data.CustomerCode, fontBody, brushBlack, infoX + labelWidth, currentY, formatLeft);
            currentY += cariKoduSize.Height + 5;
            gfx.DrawString("Cari Adı:", fontHeader, brushBlack, infoX, currentY, formatLeft);
            gfx.DrawString(data.CustomerName, fontBody, brushBlack, infoX + labelWidth, currentY, formatLeft);
            currentY += cariAdiSize.Height + 20;

            // === Table setup with dynamic column widths ===
            // Measure header text to determine minimum column widths
            XSize tarihHeaderSize = gfx.MeasureString(data.ColumnHeaders.Count > 0 ? data.ColumnHeaders[0] : "Tarih", fontHeader);
            XSize miktarHeaderSize = gfx.MeasureString(data.ColumnHeaders.Count > 1 ? data.ColumnHeaders[1] : "Miktar", fontHeader);
            XSize kesintiHeaderSize = gfx.MeasureString(data.ColumnHeaders.Count > 2 ? data.ColumnHeaders[2] : "Kesinti", fontHeader);
            XSize yagHeaderSize = gfx.MeasureString(data.ColumnHeaders.Count > 3 ? data.ColumnHeaders[3] : "Yağ (%)", fontHeader);
            XSize proteinHeaderSize = gfx.MeasureString(data.ColumnHeaders.Count > 4 ? data.ColumnHeaders[4] : "Protein (%)", fontHeader);
            
            // Set column widths with padding
            double columnPadding = 20;
            double colTarihWidth = tarihHeaderSize.Width + columnPadding;
            double colMiktarWidth = Math.Max(miktarHeaderSize.Width + columnPadding, 90);
            double colKesintiWidth = Math.Max(kesintiHeaderSize.Width + columnPadding, 80);
            double colYagWidth = Math.Max(yagHeaderSize.Width + columnPadding, 80);
            double colProteinWidth = Math.Max(proteinHeaderSize.Width + columnPadding, 80);

            double totalTableWidth = colTarihWidth + colMiktarWidth + colKesintiWidth + colYagWidth + colProteinWidth;
            double usableWidth = pageWidth - leftMargin - rightMargin;
            
            // Center table if it's smaller than usable width, otherwise start from left margin
            double tableStartX = totalTableWidth < usableWidth ? 
                leftMargin + (usableWidth - totalTableWidth) / 2 : leftMargin;

            double colTarihX = tableStartX;
            double colMiktarX = colTarihX + colTarihWidth;
            double colKesintiX = colMiktarX + colMiktarWidth;
            double colYagX = colKesintiX + colKesintiWidth;
            double colProteinX = colYagX + colYagWidth;
            double tableEndX = colProteinX + colProteinWidth;

            double cellPadding = 5;
            double rowHeight = gfx.MeasureString("Test", fontBody).Height + 4;
            double tableHeaderY = currentY;

            // === Table Headers ===
            gfx.DrawString(data.ColumnHeaders.Count > 0 ? data.ColumnHeaders[0] : "Tarih", fontHeader, brushBlack, colTarihX + cellPadding, tableHeaderY, formatLeft);
            gfx.DrawString(data.ColumnHeaders.Count > 1 ? data.ColumnHeaders[1] : "Miktar", fontHeader, brushBlack, new XRect(colMiktarX, tableHeaderY, colMiktarWidth - cellPadding, 20), formatRight);
            gfx.DrawString(data.ColumnHeaders.Count > 2 ? data.ColumnHeaders[2] : "Kesinti", fontHeader, brushBlack, new XRect(colKesintiX, tableHeaderY, colKesintiWidth - cellPadding, 20), formatRight);
            gfx.DrawString(data.ColumnHeaders.Count > 3 ? data.ColumnHeaders[3] : "Yağ (%)", fontHeader, brushBlack, new XRect(colYagX, tableHeaderY, colYagWidth - cellPadding, 20), formatRight);
            gfx.DrawString(data.ColumnHeaders.Count > 4 ? data.ColumnHeaders[4] : "Protein (%)", fontHeader, brushBlack, new XRect(colProteinX, tableHeaderY, colProteinWidth - cellPadding, 20), formatRight);

            // Draw line below headers
            XSize headerTextSize = gfx.MeasureString("Tarih", fontHeader);
            double headerLineY = tableHeaderY + headerTextSize.Height + 3;
           // gfx.DrawLine(XPens.Black, tableStartX, headerLineY, tableEndX, headerLineY);

            currentY = headerLineY + 10;

            // === Table Rows ===
            foreach (var item in data.Items)
            {
                gfx.DrawString(item.Tarih, fontBody, brushBlack, colTarihX + cellPadding, currentY, formatLeft);
                gfx.DrawString(item.Miktar.ToString("N2"), fontBody, brushBlack, 
                    new XRect(colMiktarX, currentY, colMiktarWidth - cellPadding, 20), formatRight);
                gfx.DrawString(item.Kesinti.ToString("N2"), fontBody, brushBlack, 
                    new XRect(colKesintiX, currentY, colKesintiWidth - cellPadding, 20), formatRight);
                gfx.DrawString(item.Yag.ToString("N2"), fontBody, brushBlack, 
                    new XRect(colYagX, currentY, colYagWidth - cellPadding, 20), formatRight);
                gfx.DrawString(item.Protein.ToString("N2"), fontBody, brushBlack, 
                    new XRect(colProteinX, currentY, colProteinWidth - cellPadding, 20), formatRight);

                currentY += rowHeight;

                // Simple pagination
                if (currentY > pageHeight - bottomMargin - 80)
                {
                    page = document.AddPage();
                    page.Orientation = PdfSharp.PageOrientation.Portrait;
                    gfx = XGraphics.FromPdfPage(page);
                    currentY = topMargin;
                    
                    // Redraw headers on new page
                    gfx.DrawString(data.ColumnHeaders.Count > 0 ? data.ColumnHeaders[0] : "Tarih", fontHeader, brushBlack, colTarihX + cellPadding, currentY, formatLeft);
                    gfx.DrawString(data.ColumnHeaders.Count > 1 ? data.ColumnHeaders[1] : "Miktar", fontHeader, brushBlack, new XRect(colMiktarX, currentY, colMiktarWidth - cellPadding, 20), formatRight);
                    gfx.DrawString(data.ColumnHeaders.Count > 2 ? data.ColumnHeaders[2] : "Kesinti", fontHeader, brushBlack, new XRect(colKesintiX, currentY, colKesintiWidth - cellPadding, 20), formatRight);
                    gfx.DrawString(data.ColumnHeaders.Count > 3 ? data.ColumnHeaders[3] : "Yağ (%)", fontHeader, brushBlack, new XRect(colYagX, currentY, colYagWidth - cellPadding, 20), formatRight);
                    gfx.DrawString(data.ColumnHeaders.Count > 4 ? data.ColumnHeaders[4] : "Protein (%)", fontHeader, brushBlack, new XRect(colProteinX, currentY, colProteinWidth - cellPadding, 20), formatRight);
                    
                    double newHeaderLineY = currentY + headerTextSize.Height + 3;
                    gfx.DrawLine(XPens.Black, tableStartX, newHeaderLineY, tableEndX, newHeaderLineY);
                    currentY = newHeaderLineY + 10;
                }
            }

            // Table bottom line
            currentY += 3;
            gfx.DrawLine(XPens.Black, tableStartX, currentY, tableEndX, currentY);
            currentY += 25;

            // === Summary Section - Dynamic Layout ===
            double summaryStartX = tableStartX;
            
            // Measure summary labels to calculate proper spacing
            XSize netMiktarSize = gfx.MeasureString("Net Miktar:", fontHeader);
            XSize kesintiLabelSize = gfx.MeasureString("Kesinti:", fontHeader);
            XSize ortYagSize = gfx.MeasureString("Ort. Yağ:", fontHeader);
            XSize ortProteinSize = gfx.MeasureString("Ort. Protein:", fontHeader);
            XSize toplamKayitSize = gfx.MeasureString("Toplam Kayıt:", fontHeader);
            
            double col1LabelWidth = Math.Max(netMiktarSize.Width, kesintiLabelSize.Width) + 10;
            double col1ValueGap = 20;
            double col2Offset = col1LabelWidth + col1ValueGap + 120;
            double col2LabelWidth = Math.Max(ortYagSize.Width, ortProteinSize.Width) + 10;
            double col2ValueGap = 20;
            
            // Row 1: Net Miktar and Ort. Yağ
            gfx.DrawString("Net Miktar:", fontHeader, brushBlack, summaryStartX, currentY, formatLeft);
            gfx.DrawString(data.Summary.NetMiktar.ToString("N2"), fontBodyBold, brushBlack, 
                summaryStartX + col1LabelWidth, currentY, formatLeft);
            
            gfx.DrawString("Ort. Yağ:", fontHeader, brushBlack, 
                summaryStartX + col2Offset, currentY, formatLeft);
            gfx.DrawString(data.Summary.OrtYag.ToString("N2"), fontBodyBold, brushBlack, 
                summaryStartX + col2Offset + col2LabelWidth, currentY, formatLeft);

            currentY += netMiktarSize.Height + 5;

            // Row 2: Kesinti and Ort. Protein
            gfx.DrawString("Kesinti:", fontHeader, brushBlack, summaryStartX, currentY, formatLeft);
            gfx.DrawString(data.Summary.Kesinti.ToString("N2"), fontBodyBold, brushBlack, 
                summaryStartX + col1LabelWidth, currentY, formatLeft);
            
            gfx.DrawString("Ort. Protein:", fontHeader, brushBlack, 
                summaryStartX + col2Offset, currentY, formatLeft);
            gfx.DrawString(data.Summary.OrtProtein.ToString("N2"), fontBodyBold, brushBlack, 
                summaryStartX + col2Offset + col2LabelWidth, currentY, formatLeft);

            currentY += kesintiLabelSize.Height + 15;

            // Row 3: Toplam Kayıt (right-aligned)
            double toplamKayitLabelX = tableEndX - toplamKayitSize.Width - 60;
            gfx.DrawString("Toplam Kayıt:", fontHeader, brushBlack, toplamKayitLabelX, currentY, formatLeft);
            gfx.DrawString(data.Summary.ToplamKayit.ToString(), fontBodyBold, brushBlack, 
                toplamKayitLabelX + toplamKayitSize.Width + 10, currentY, formatLeft);

            // === Save ===
            document.Save(filePath);
        }
    }


}