using System.Collections.Generic;
using System.Net.Http;

namespace FullStackRebelsTestCSharp.Models
{
    public class ErrorResponse
    {
        public string status { get; set; }
    }

    public class ParseListResponse
    {
        public HttpResponseMessage exception { get; set; }
        public List<CastType> list { get; set; }
    }
}
