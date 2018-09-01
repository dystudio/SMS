using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sms.Entity.ViewModel
{
    public class RoleModuleRight
    {
        public RoleModuleRight()
        {
            this.RightList = new List<ModuleRight>();
        }


        public string Name { get; set; }

        public int Value { get; set; }

        public int Parent { get; set; }

        public int Sort { get; set; }

        public List<ModuleRight> RightList { get; set; }
    }


    public class ModuleRight
    {
        public string Text { get; set; }

        public int Value { get; set; }

        public bool Checked { get; set; }
    }
}
