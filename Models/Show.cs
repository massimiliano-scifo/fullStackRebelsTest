using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FullStackRebelsTestCSharp.Models
{
    public class Show
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Cast> Cast { get; set; }
    }
}
