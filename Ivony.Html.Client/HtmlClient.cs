using Ivony.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Html.Client
{
  public class HtmlClient : IDisposable
  {

    private HttpClient client = new HttpClient();


    public virtual IHtmlParser GetParser()
    {
      return new JumonyParser();
    }


    public async Task<IHtmlDocument> Get( Uri requestUri )
    {
      var result = await client.GetAsync( requestUri );
      if ( !result.IsSuccessStatusCode )
        throw new HttpException( result );

      var content = await LoadText( result.Content );

      return GetParser().Parse( content, requestUri );
    }

    protected virtual async Task<string> LoadText( HttpContent httpContent )
    {
      return await httpContent.ReadAsStringAsync();
    }



    void IDisposable.Dispose()
    {
      client.Dispose();
    }
  }
}
