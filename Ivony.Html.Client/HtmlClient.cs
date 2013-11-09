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


    protected virtual IHtmlParser GetParser()
    {
      return new JumonyParser();
    }



    public Task<IHtmlDocument> Get( string requestUri )
    {
      return GetCore( new Uri( requestUri ) );
    }

    public Task<IHtmlDocument> Get( Uri requestUri )
    {
      return GetCore( requestUri );
    }

    private async Task<IHtmlDocument> GetCore( Uri requestUri )
    {
      var result = await client.GetAsync( requestUri );
      if ( !result.IsSuccessStatusCode )
        throw new HttpException( result );


      if ( result.Content.Headers.ContentType.MediaType != "text/html" )
        return null;

      var content = await LoadTextContent( result.Content );

      return GetParser().Parse( content, requestUri );
    }

    protected virtual async Task<string> LoadTextContent( HttpContent httpContent )
    {
      return await httpContent.ReadAsStringAsync();
    }



    public void Dispose()
    {
      client.Dispose();
    }

  }
}
