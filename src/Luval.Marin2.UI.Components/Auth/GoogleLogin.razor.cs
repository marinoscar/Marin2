using Luval.AuthMate.Core;
using Luval.AuthMate.Core.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Luval.Marin2.UI.Components.Auth
{
    public partial class GoogleLogin : ComponentBase
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        private HttpClient HttpClient { get; set; } = default!;

        [Inject]
        private IHttpContextAccessor ContextAccessor { get; set; } = default!;


        private DeviceInfo DeviceInfo { get; set; } = DeviceInfo.CreateEmpty();

        [Parameter]
        public string ReturnUrl { get; set; } = "/";

        [Parameter]
        public string NavigateTo { get; set; } = "/api/auth/login";

        [Parameter]
        public Color IconColor { get; set; } = Color.Fill;

        [Parameter]
        public Appearance ButtonAppereance { get; set; } = Appearance.Accent;

        private void DoLogin()
        {
            var info = string.Empty;
            if (DeviceInfo != null) info = DeviceInfo.ToBase64();
            NavigationManager.NavigateTo($"{NavigateTo}?provider=Google&deviceInfo={info}&returnUrl={HttpUtility.HtmlEncode(ReturnUrl)}");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            await GetDeviceInfo();
        }

        private async Task GetDeviceInfo()
        {
            // Call the JavaScript function
            DeviceInfo deviceInfo = default;
            try
            {
                deviceInfo = await JSRuntime.InvokeAsync<DeviceInfo>("getDeviceInfo");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            if (deviceInfo != null)
                deviceInfo.IpAddress = ContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            DeviceInfo = deviceInfo ?? DeviceInfo.CreateEmpty();

        }
    }
}
