using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ivony.Html.Client
{
  public class HttpException : Exception
  {
    private System.Net.Http.HttpResponseMessage result;

    public HttpException( System.Net.Http.HttpResponseMessage result )
    {
      // TODO: Complete member initialization
      this.result = result;
    }
  }
}
