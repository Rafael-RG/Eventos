using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventos.GoogleAuth
{
    public interface IGoogleAuthService
    {
        public Task<GoogleUserDTO> AuthenticateAsync();
        
        public Task<GoogleUserDTO> GetCurrrentUserAsync();

        public Task LogoutAsync();
    }
}
