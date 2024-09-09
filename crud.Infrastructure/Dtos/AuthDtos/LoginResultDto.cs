using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crud.Infrastructure.Dtos.AuthDtos
{
    public class LoginResultDto
    {
        [DefaultValueAttribute("")]

        public string AccessToken { get; set; }
        [DefaultValueAttribute("")]

        public string RefreshToken { get; set; }
    }
}
