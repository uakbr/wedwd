using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using iControl.Server.Auth;
using iControl.Server.Services;
using iControl.Shared.Models;

namespace iControl.Server.Pages
{
    [ServiceFilter(typeof(RemoteControlFilterAttribute))]
    public class RemoteControlModel : PageModel
    {
        private readonly IDataService _dataService;
        public RemoteControlModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public iControlUser iControlUser { get; private set; }
        public void OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                iControlUser = _dataService.GetUserByNameWithOrg(base.User.Identity.Name);
            }
        }
    }
}
