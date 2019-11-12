using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly_Browser
{
    public abstract class Container : TypeMember
    {
        public Container()
        {
            Members = new List<TypeMember>();
        }
        public List<TypeMember> Members { get; set; }
    }
}
