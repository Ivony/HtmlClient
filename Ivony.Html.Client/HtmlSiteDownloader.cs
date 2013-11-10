using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ivony.Html;
using System.IO;

namespace Ivony.Html.Client
{
  public class HtmlSiteDownloader : IDisposable
  {

    public HtmlSiteDownloader() : this( new HtmlClient() ) { }

    public HtmlSiteDownloader( HtmlClient client )
    {
      Client = client;
    }

    public HtmlClient Client
    {
      get;
      private set;
    }



    private HashSet<string> siteLinks = new HashSet<string>();
    private HashSet<string> tracedLinks = new HashSet<string>();

    private object sync = new object();

    public Task<IHtmlDocument[]> RetrieveAllPages( string entryUri )
    {
      return RetrieveAllPagesCore( new Uri( entryUri ) );
    }


    public Task<IHtmlDocument[]> RetrieveAllPages( Uri entryUri )
    {
      return RetrieveAllPagesCore( entryUri );
    }


    protected async Task<IHtmlDocument[]> RetrieveAllPagesCore( Uri entryUri )
    {

      var document = await Client.Get( entryUri );
      if ( document == null )
        return new IHtmlDocument[0];

      lock ( sync )
      {
        siteLinks.UnionWith( FindLinks( document ) );
      }

      if ( IsObjective( document ) )
        Save( document );


      List<IHtmlDocument> result = new List<IHtmlDocument>();

      string[] allLinks;
      lock ( sync )
      {
        allLinks = siteLinks.Except( tracedLinks ).ToArray();
      }


      foreach ( var link in allLinks )
      {
        var traceUri = new Uri( entryUri, link );
        if ( Tracable( entryUri, traceUri ) )
        {
          lock ( sync )
          {
            var traceLink = traceUri.AbsoluteUri;
            if ( tracedLinks.Contains( traceLink ) )
              continue;

            tracedLinks.Add( traceLink );
          }

          result.AddRange( await RetrieveAllPagesCore( traceUri ) );
        }
      }

      return result.ToArray();

    }


    /// <summary>
    /// 判断指定文档是否为目标文档
    /// </summary>
    /// <param name="document">要判断的文档</param>
    /// <returns>是否为要保存的目标文档</returns>
    protected virtual bool IsObjective( IHtmlDocument document )
    {
      return true;
    }


    /// <summary>
    /// 保存指定文档
    /// </summary>
    /// <param name="document">要保存的文档</param>
    protected virtual void Save( IHtmlDocument document )
    {
      var filename = Guid.NewGuid().ToString( "N" ) + ".html";
      using ( var stream = File.OpenWrite( Path.Combine( @"C:\Temp", filename ) ) )
      {
        document.Render( stream, Encoding.UTF8 );
      }
    }


    /// <summary>
    /// 判断指定 URI 地址是否可以进行追踪
    /// </summary>
    /// <param name="refer">引用来源</param>
    /// <param name="traceUri">需要判断的 URI 地址</param>
    /// <returns>是否可以进行追踪</returns>
    protected virtual bool Tracable( Uri refer, Uri traceUri )
    {
      return refer.Host == traceUri.Host;
    }

    protected virtual IEnumerable<string> FindLinks( IHtmlDocument document )
    {

      document.ResolveUriToAbsoluate();

      foreach ( var href in document.Find( "a[href]" ).Select( element => element.Attribute( "href" ).Value() ) )
      {
        var uri = new UriBuilder( new Uri( document.DocumentUri, href ) );
        uri.Fragment = null;
        yield return uri.Uri.AbsoluteUri;
      }

    }




    public void Dispose()
    {
      Client.Dispose();
    }

  }
}
