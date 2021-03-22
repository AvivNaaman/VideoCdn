using System;
using System.Linq;

namespace VideoCdn.Web.Client
{
    public static class UrlEncodingHelper
    {
        public static string DataToQueryString<TModel>(this TModel model) where TModel : class
        {
            return string.Join('&', model.GetType().GetProperties().Where(p => p.GetValue(model, null) != null)
            .Select(p => $"{p.Name}={System.Web.HttpUtility.UrlEncode(p.GetValue(model, null).ToString())}"));
        }
    }
}
