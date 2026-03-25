using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;



namespace ScintillaTextEditor
{
    class TextPrinter : System.Drawing.Printing.PrintDocument
    {
        private TextView sciTextView;

        private int mPosition;
        private int mPrintEnd;
        private int mCurrentPage;



        public TextPrinter(TextView ScintillaView)
        {
            sciTextView = ScintillaView;
            this.DefaultPageSettings = new PageSettings();
            this.DefaultPageSettings.Margins = new Margins(59, 59, 59, 59);
        }


        protected override void OnBeginPrint(PrintEventArgs e)
        {
            base.OnBeginPrint(e);

            mPosition = 0;
            mPrintEnd = sciTextView.GetTextLength();
            mCurrentPage = 1;
        }

        protected override void OnEndPrint(PrintEventArgs e)
        {
            base.OnEndPrint(e);
        }

        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            base.OnPrintPage(e);

            //PageSettings oPageSettings = null;
            //HeaderInformation oHeader = ((PageSettings)DefaultPageSettings).Header;
            //FooterInformation oFooter = ((PageSettings)DefaultPageSettings).Footer;
            Rectangle mPrintBounds = e.MarginBounds;
            bool bIsPreview = this.PrintController.IsPreview;

            //// When not in preview mode, adjust graphics to account for hard margin of the printer
            //if (!bIsPreview)
            //{
            //    e.Graphics.TranslateTransform(-e.PageSettings.HardMarginX, -e.PageSettings.HardMarginY);
            //}

            ////// Get the header and footer provided if using Scintilla.Printing.PageSettings
            ////if (e.PageSettings is PageSettings)
            ////{
            ////    oPageSettings = (PageSettings)e.PageSettings;

            ////    oHeader = oPageSettings.Header;
            ////    oFooter = oPageSettings.Footer;

            //sci_TextView.SetPrintMagnification(0);
            //sci_TextView.SetPrintColourMode((0));
            ////}

            ////// Draw the header and footer and get remainder of page bounds
            ////oPrintBounds = DrawHeader(e.Graphics, oPrintBounds, oHeader);
            ////oPrintBounds = DrawFooter(e.Graphics, oPrintBounds, oFooter);

            //// When not in preview mode, adjust page bounds to account for hard margin of the printer
            //if (!bIsPreview)
            //{
            //    oPrintBounds.Offset((int)-e.PageSettings.HardMarginX, (int)-e.PageSettings.HardMarginY);
            //}
            DrawCurrentPage(e.Graphics, mPrintBounds);

            // Increment the page count and determine if there are more pages to be printed
            mCurrentPage++;
            e.HasMorePages = (mPosition < mPrintEnd);

        }

        private void DrawCurrentPage(Graphics mGraphics, Rectangle mBounds)
        {
            Point[] mPoints = {
                new Point(mBounds.Left, mBounds.Top),
                new Point(mBounds.Right, mBounds.Bottom)
                };
            mGraphics.TransformPoints(CoordinateSpace.Device, CoordinateSpace.Page, mPoints);

            TextView.PrintRectangle oPrintRectangle =
                new TextView.PrintRectangle(mPoints[0].X, mPoints[0].Y,
                                            mPoints[1].X, mPoints[1].Y);

            TextView.RangeToFormat mRangeToFormat = new TextView.RangeToFormat();
            mRangeToFormat.hdc = mRangeToFormat.hdcTarget = mGraphics.GetHdc();
            mRangeToFormat.rc = mRangeToFormat.rcPage = oPrintRectangle;
            mRangeToFormat.chrg.cpMin = mPosition;
            mRangeToFormat.chrg.cpMax = mPrintEnd;

            mPosition = sciTextView.FormatRange(true, mRangeToFormat);
            //System.Media.SystemSounds.Beep.Play();
        }






    }
}
