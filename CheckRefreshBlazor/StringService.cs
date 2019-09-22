using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckRefreshBlazor
{
    public class StringService
    {
        public List<string> Names { get; set; } = new List<string>();
       public void Add()
        {
            this.Names.Add("another name");
        }
    }
}
