using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Common;
using Core.Asset;
using Core.BalanceReport;
using Core.Block;
using Core.Settings;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Services.BalanceReport
{
    public class PdfReportRenderer:IReportRender
    {
        private readonly BaseSettings _baseSettings;

        public PdfReportRenderer(BaseSettings baseSettings)
        {
            _baseSettings = baseSettings;
            CultureInfo culture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private static Font GetBaseFont(float size)
        {
            var fontName = "ProximaNova-Regular";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = @".\Resources\Pdf\fonts\proximanovacond-regular-webfont.ttf";
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED, size);
        }

        private static Font GetBaseFontBold(float size)
        {
            var fontName = "ProximaNova-Semibold";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = @".\Resources\Pdf\fonts\proximanova-bold-webfont.ttf";
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, size, Font.BOLD);
        }

        private static Font GetLinkFont(float size)
        {
            var fontName = "ProximaNova-Regular";
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = @".\Resources\Pdf\fonts\proximanovacond-regular-webfont.ttf";
                FontFactory.Register(fontPath);
            }
            

            return FontFactory.GetFont(fontName, size, Font.UNDERLINE, BaseColor.BLUE);
        }

        public void RenderBalance(Stream outputStream, IClient client,
            IBlockHeader reportedAtBlock,
            IFiatPrices fiatPrices,
            IClientBalances balances,
            IDictionary<string, IAssetDefinition> assetDefinitions)
        {
            var headerFont = new Font(GetBaseFontBold(22));
            var mainFontBold = new Font(GetBaseFontBold(14));
            var mainFontRegular = new Font(GetBaseFont(14));
            var linkFont = new Font(GetLinkFont(14));


            var smallFontRegular = new Font(GetBaseFont(12));
            var smallFontBold = new Font(GetBaseFontBold(12));
            var smallLinkFont = new Font(GetLinkFont(12));

            const string fiatPriceFormat = "### ### ### ### ##0.00";


            var document = new Document();
            using (var writer = PdfWriter.GetInstance(document, outputStream))
            {
                writer.CloseStream = false;
                document.Open();

                #region Header/Logo
                
                document.Add(new Paragraph("Digital Asset Portfolio Report", headerFont) { SpacingAfter = 50 });

                Image logo;
                using (var fs = new FileStream(@"Resources\Pdf\lykke_logo.png", FileMode.Open))
                {
                    logo = Image.GetInstance(fs);
                }

                logo.SetAbsolutePosition(400, 765);
                writer.DirectContent.AddImage(logo);

                #endregion

                #region MainInfoTable

                var mainInfoTable = new PdfPTable(2);
                
                mainInfoTable.SetWidths(new [] { 1f, 2.5f });
                mainInfoTable.WidthPercentage = 100;
                
                mainInfoTable.HorizontalAlignment = Element.ALIGN_LEFT;

                mainInfoTable.DefaultCell.Border = Rectangle.NO_BORDER;
                mainInfoTable.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                mainInfoTable.DefaultCell.FixedHeight = 30;
                
                mainInfoTable.SpacingAfter = 30;

                #region ClientName

                if (!string.IsNullOrEmpty(client.Name))
                {
                    mainInfoTable.AddCell(new Phrase("Client Name", mainFontBold));
                    mainInfoTable.AddCell(new Phrase(client.Name, mainFontRegular));
                }

                #endregion

                #region ClientID 

                if (!string.IsNullOrEmpty(client.Email))
                {
                    mainInfoTable.AddCell(new Phrase("Client Id", mainFontBold));
                    mainInfoTable.AddCell(new Anchor(client.Email, linkFont) { Reference = "mailto: " + client.Email });
                }

                #endregion

                #region Reporting block #

                mainInfoTable.AddCell(new Phrase("Reporting block #", mainFontBold));
                mainInfoTable.AddCell(new Phrase(reportedAtBlock.Height.ToString(), mainFontRegular));

                #endregion

                #region Reporting datetime

                mainInfoTable.AddCell(new Phrase("Reporting datetime", mainFontBold));
                mainInfoTable.AddCell(new Phrase(reportedAtBlock.Time.ToUniversalTime().ToString("dd MMMM yyyy HH:mm:ss"), mainFontRegular));

                #endregion

                #region Reporting currency

                mainInfoTable.AddCell(new Phrase("Reporting currency", mainFontBold));
                mainInfoTable.AddCell(new Phrase(fiatPrices.CurrencyName, mainFontRegular));

                #endregion

                document.Add(mainInfoTable);

                #endregion

                #region Balances Table 

                //var balancetableBorderBoldWidth = 1f;
                //var balancetableBorderRegularWidth = 0.5f;

                var balancesTable = new PdfPTable(5);

                balancesTable.SetWidths(new[] { 1, 0.6f, 1, 1, 1 });
                balancesTable.WidthPercentage = 100;

                balancesTable.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                balancesTable.DefaultCell.VerticalAlignment = Element.ALIGN_BASELINE;
                balancesTable.DefaultCell.MinimumHeight = 30;

                #region Header 

                //balancesTable.DefaultCell.BorderWidthLeft = balancetableBorderBoldWidth;
                //balancesTable.DefaultCell.BorderWidthRight = balancetableBorderBoldWidth;
                //balancesTable.DefaultCell.BorderWidthTop = balancetableBorderBoldWidth;
                //balancesTable.DefaultCell.BorderWidthBottom = balancetableBorderBoldWidth;

                //balancesTable.DefaultCell.BorderWidth = balancetableBorderBoldWidth;

                balancesTable.AddCell(new Phrase("Asset", smallFontBold));
                balancesTable.AddCell(new Phrase("Definition", smallFontBold));
                balancesTable.AddCell(new Phrase("Position [1]", smallFontBold));
                balancesTable.AddCell(new Phrase("Market Price [2]", smallFontBold));
                balancesTable.AddCell(new Phrase("Market Value [3]", smallFontBold));

                //balancesTable.DefaultCell.BorderWidth = balancetableBorderRegularWidth;

                //balancesTable.DefaultCell.BorderWidthLeft = balancetableBorderRegularWidth;
                //balancesTable.DefaultCell.BorderWidthRight = balancetableBorderRegularWidth;
                //balancesTable.DefaultCell.BorderWidthTop = balancetableBorderRegularWidth;
                //balancesTable.DefaultCell.BorderWidthBottom = balancetableBorderRegularWidth;


                #endregion

                #region Rows

                decimal sum = 0;
                foreach (var address in balances.AddressBalances.Select(p=>p.Key))
                {
                    balancesTable.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    balancesTable.AddCell(new Phrase("WalletId", smallFontBold));

                    balancesTable.DefaultCell.Colspan = 4;
                    balancesTable.AddCell(new Anchor(address, smallLinkFont) { Reference = _baseSettings.ExplolerUrl + "address/" + address });
                    balancesTable.DefaultCell.Colspan = 1;

                    foreach (var assetBalance in balances.AddressBalances[address])
                    {
                        int divisibility;
                        string assetName;
                        string assetNameShort;
                        if (assetBalance.AssetId != ClientBalance.BitcoinAssetId)
                        {
                            var asset = assetDefinitions[assetBalance.AssetId];
                            divisibility = asset.Divisibility;
                            assetName = asset.Name;
                            assetNameShort = asset.NameShort;
                        }
                        else
                        {
                            divisibility = 0;
                            assetName = ClientBalance.BitcoinAssetName;
                            assetNameShort = ClientBalance.BitcoinAssetId;
                        }

                        var marketPrice =
                            fiatPrices.AssetMarketPrices?.FirstOrDefault(p => p.AssetId == assetBalance.AssetId)?.MarketPrice;

                        var coloredValue =
                            Convert.ToDecimal(
                                BitcoinUtils.CalculateColoredAssetQuantity(Convert.ToDouble(assetBalance.Quantity),
                                    divisibility));

                        decimal? marketValue = null;
                        if (marketPrice != null)
                        {
                            marketValue = marketPrice.Value * coloredValue;
                            marketValue = Math.Round(marketValue.Value, 2);
                            sum += marketValue.Value;
                        }

                        var marketPriceString = marketPrice != null ? marketPrice.Value.ToStringBtcFormat() : "";

                        string marketValueString = null;
                        if (marketValue != null)
                        {
                            marketValueString = string.Format("{0} {1}", marketValue.Value.ToStringBtcFormat(), marketValue != null ? fiatPrices.CurrencyName : "");
                        }


                        var assetUrl = _baseSettings.ExplolerUrl + "asset/" + assetBalance.AssetId;

                        balancesTable.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        balancesTable.AddCell(new Phrase(assetName, smallFontBold));
                        //balancesTable.DefaultCell.BorderWidthLeft = balancetableBorderRegularWidth;

                        balancesTable.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;

                        if (assetBalance.AssetId != "BTC")
                        {
                            balancesTable.AddCell(new Anchor("view", smallLinkFont) { Reference = assetUrl });
                        }
                        else
                        {
                            balancesTable.AddCell(new Phrase("", smallFontRegular));
                        }

                        balancesTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        balancesTable.AddCell(new Phrase($"{coloredValue.ToStringBtcFormat()} {assetNameShort}", smallFontRegular));

                        balancesTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        balancesTable.AddCell(new Phrase(marketPriceString, smallFontRegular));


                        balancesTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        //balancesTable.DefaultCell.BorderWidthRight = balancetableBorderBoldWidth;
                        balancesTable.AddCell(new Phrase(marketValueString, smallFontRegular));
                        //balancesTable.DefaultCell.BorderWidthRight = balancetableBorderRegularWidth;
                    }
                }


                #endregion

                #region Total

                //balancesTable.DefaultCell.BorderWidthLeft = balancetableBorderBoldWidth;
                //balancesTable.DefaultCell.BorderWidthRight = balancetableBorderBoldWidth;
                //balancesTable.DefaultCell.BorderWidthTop = balancetableBorderBoldWidth;
                //balancesTable.DefaultCell.BorderWidthBottom = balancetableBorderBoldWidth;
                //balancesTable.DefaultCell.BorderWidth = balancetableBorderBoldWidth;

                balancesTable.AddCell(new Phrase("", smallFontRegular));
                balancesTable.AddCell(new Phrase("", smallFontRegular));
                balancesTable.AddCell(new Phrase("", smallFontRegular));

                balancesTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                balancesTable.AddCell(new Phrase("Total", smallFontBold));

                balancesTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                balancesTable.AddCell(new Phrase(sum.ToStringBtcFormat() + " " + fiatPrices.CurrencyName, smallFontBold));

                #endregion

                balancesTable.SpacingAfter = 30;

                document.Add(balancesTable);

                #endregion

                #region Remarks

                document.Add(new Paragraph("Comments", smallFontBold) {SpacingAfter = 15});
                
                document.Add(
                new Paragraph(string.Format("[1] The positions value as of the block #{0} {1}", 
                        reportedAtBlock.Height,  
                        reportedAtBlock.Time.ToUniversalTime().ToString("dd MMMM yyyy HH:mm:ss")),
                    smallFontRegular));

                //var addressUrl = _baseSettings.ExplolerUrl + "address/" + client.Address;
                //var addressLink = new Anchor(addressUrl, smallLinkFont) {Reference = addressUrl};
                //document.Add(addressLink);
                document.Add(new Paragraph("[2] Market price is averaged second-by-second mid price trailing 24 hours before reporting datetime", smallFontRegular) { SpacingAfter = 15, SpacingBefore = 15});
                document.Add(new Paragraph("[3] Market value in reporting currency, [3]=[1]*[2]", smallFontRegular));

                #endregion

                document.Close();
            }
        }
    }
}
