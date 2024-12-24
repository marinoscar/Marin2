using Luval.AuthMate.Core.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Luval.AuthMate.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.Marin2.UI.Components.Auth
{
    public partial class UserProfile : ComponentBase
    {
        private AppUser? _user;

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        [Inject]
        private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

        private HttpContext? Context => HttpContextAccessor?.HttpContext;

        #region Parameters

        /// <summary>
        /// Gets or sets the header label for the profile menu.
        /// </summary>
        [Parameter]
        public string HeaderLabel { get; set; } = "Account Information";

        /// <summary>
        /// Gets or sets the initials displayed in the profile menu.
        /// </summary>
        [Parameter]
        public string Initials { get; set; } = "JD";

        /// <summary>
        /// Gets or sets the full name displayed in the profile menu.
        /// </summary>
        [Parameter]
        public string? FullName { get; set; } = "John Doe";

        /// <summary>
        /// Gets or sets the email displayed in the profile menu.
        /// </summary>
        [Parameter]
        public string Email { get; set; } = "john@example.com";

        /// <summary>
        /// Gets or sets the footer link text in the profile menu.
        /// </summary>
        [Parameter]
        public string FooterLink { get; set; } = "View Account";

        /// <summary>
        /// Gets or sets the header button text in the profile menu.
        /// </summary>
        [Parameter]
        public string HeaderButton { get; set; } = "Signout";

        /// <summary>
        /// Gets or sets the popover style for the profile menu.
        /// </summary>
        [Parameter]
        public string? PopoverStyle { get; set; } = "min-width: 330px;";

        /// <summary>
        /// Gets or sets the image URL for the profile menu.
        /// </summary>
        [Parameter]
        public string? Image { get; set; }

        /// <summary>
        /// Gets or sets a value to indicate if the profile menu should load the user information from the context.
        /// Default is true.
        /// </summary>
        public bool LoadFromContext { get; set; } = true;

        /// <summary>
        /// The url that will trigger the session to logout
        /// </summary>
        [Parameter]
        public string SignoutNavigateTo { get; set; } = "/auth/logout";
        /// <summary>
        /// The page where the user profile information will be shown
        /// </summary>
        [Parameter]
        public string AccountNavigateTo { get; set; } = "/profile";

        #endregion

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (!LoadFromContext) return;
            if (_user != null) return;
            if(Context == null || Context.User == null) return;

            _user = Context.User.ToUser();

            if (_user != null)
            {
                Initials = _user.GetDisplayNameInitials();
                FullName = _user.DisplayName;
                Email = _user.Email;
                Image = _user.ProfilePictureUrl;
            }
        }

        private void DoSingout()
        {
            Navigation.NavigateTo(SignoutNavigateTo);
        }

        private void GoToAccount()
        {
            Navigation.NavigateTo(AccountNavigateTo);
        }



    }
}
