using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly_Browser
{
    public abstract class TypeMember
    {
        public string Name { get; set; }

        public string AccessModifier { get; set; }
        public string FullName { get; set; }

    }
}
