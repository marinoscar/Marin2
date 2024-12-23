using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;

namespace Luval.Marin2.UI.Components.Pages
{
    public partial class Weather : ComponentBase
    {
        private IQueryable<WeatherForecast>? forecasts;

        protected override async Task OnInitializedAsync()
        {
            // Simulate asynchronous loading to demonstrate streaming rendering
            await Task.Delay(500);

            await _httpClient.GetAsync("WeatherForecast").ContinueWith(async response =>
            {
                if (response.IsCompletedSuccessfully)
                {
                    var content = await response.Result.Content.ReadAsStringAsync();
                    forecasts = JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(content)!.AsQueryable();
                    StateHasChanged();
                }
            });

        }

        private readonly HttpClient _httpClient;

        public Weather(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient.CreateClient();
            _httpClient.BaseAddress = new Uri("https+http://Marin2-Services");
        }

        private class WeatherForecast
        {
            public DateOnly Date { get; set; }
            public int TemperatureC { get; set; }
            public string? Summary { get; set; }
            public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        }
    }
}
