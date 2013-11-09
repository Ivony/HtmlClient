using Ivony.Html.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDownload
{
  class Program
  {
    static void Main( string[] args )
    {

      var client = new HtmlSiteDownloader();

      client.RetrieveAllPages( "http://www.cnblogs.com/" ).Wait();

    }


  }
}
