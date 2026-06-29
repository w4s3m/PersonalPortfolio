using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalProtfolioDataTier
{
    public class LoginRequest
    {
        [DefaultValue("")]
        public string UserName { get; set; } = string.Empty;
        [DefaultValue("")]
        public string Password { get; set; } = string.Empty;
    }
}
