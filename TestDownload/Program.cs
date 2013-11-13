using Ivony.Html.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TestDownload
{
  class Program
  {
    static void Main( string[] args )
    {


      System.Net.ServicePointManager.MaxServicePoints = 1024;


      var client = new HtmlSiteDownloader( new HtmlClient( new HttpClientHandler() { AllowAutoRedirect = false } ) );

      var task = client.RetrieveAllPages( "http://www.cnblogs.com/" );
      task.Wait();

    }


  }
}
