
using AnaderiaDemo.Helpers;
namespace Ganaderia.Helpers
{
#if ANDROID

    using Android.Content;
    using Android.Print;
    using Java.Interop;
    public class AndroidHelper
    {

        public static void PrintHtml(string html)
        {
            Context context = Platform.CurrentActivity;

            PrintManager printManager = context.GetSystemService(Context.PrintService).JavaCast<PrintManager>();

            PrintAttributes printAttribute = new PrintAttributes.Builder()
                                                .SetMediaSize(PrintAttributes.MediaSize.NaLetter)
                                                .Build();

            Android.Webkit.WebView webView = new Android.Webkit.WebView(context);
            webView.LoadData(html, null, null);

            printManager.Print(Constants.AppName, webView.CreatePrintDocumentAdapter(Constants.AppName), printAttribute);
        }
    }

#endif
}


