using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ivony.Html;
using System.IO;
using Ivony.Fluent;

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

    public Task RetrieveAllPages( string entryUri )
    {
      return RetrieveAllPages( new Uri( entryUri ) );
    }


    public Task RetrieveAllPages( Uri entryUri )
    {
      return RetrieveAllPagesCore( entryUri );
    }


    protected async Task RetrieveAllPagesCore( Uri entryUri )
    {

      var document = await Client.Get( entryUri );
      if ( document == null )
        return;

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



      var tasks = new List<Task>();

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

          tasks.Add( RetrieveAllPagesCore( traceUri ) );
        }
      }

      await Task.WhenAll( tasks.ToArray() );
    }


    /// <summary>
    /// 判断指定文档是否为目标文档
    /// </summary>
    /// <param name="document">要判断的文档</param>
    /// <returns>是否为要保存的目标文档</returns>
    protected virtual bool IsObjective( IHtmlDocument document )
    {
      return document.DocumentUri.Host.EqualsIgnoreCase( "www.cnblogs.com" );
    }


    /// <summary>
    /// 保存指定文档
    /// </summary>
    /// <param name="document">要保存的文档</param>
    protected virtual void Save( IHtmlDocument document )
    {

      var path = document.DocumentUri.AbsolutePath;
      Path.GetInvalidFileNameChars().ForAll( c => path = path.Replace( c, '_' ) );

      path = Path.Combine( @"C:\Temp", Path.GetFileNameWithoutExtension( path ) + ".html" );

      lock ( sync )
      {
        if ( File.Exists( path ) )
        {
          for ( int i = 0; i < int.MaxValue; i++ )
          {
            var _path = Path.GetFileNameWithoutExtension( path ) + "_" + i + ".html";
            _path = Path.Combine( @"C:\Temp", _path );
            if ( !File.Exists( _path ) )
            {
              path = _path;
              break;
            }
          }

        }

      }

      using ( var stream = File.OpenWrite( path ) )
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
