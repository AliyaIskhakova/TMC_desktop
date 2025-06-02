using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMC.Model
{
    public class MastersListView
    {
        public int Id {  get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Telephone { get; set; }
        public int OpenRequests { get; set; }

    }
}
