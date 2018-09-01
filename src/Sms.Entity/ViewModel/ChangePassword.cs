using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Entity.ViewModel
{
    public class ChangePassword
    {
        public string OldPwd { get; set; }

        public string NewPwd { get; set; }

        public string ConfirmNewPwd { get; set; }
    }
}
