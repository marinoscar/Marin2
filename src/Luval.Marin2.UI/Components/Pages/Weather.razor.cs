using FluentGridToolkit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Net.Http;
using System.Security.Principal;
using System.Text.Json;

namespace Luval.Marin2.UI.Components.Pages
{
    public partial class Weather : ComponentBase
    {
        private IQueryable<WeatherForecast>? forecasts = (new List<WeatherForecast>()).AsQueryable();
        private FluentGridFilterManager<WeatherForecast> filterManager = default;
        private readonly HttpClient _httpClient;

        protected override async Task OnInitializedAsync()
        {
            filterManager = new FluentGridFilterManager<WeatherForecast>(forecasts);
            await _httpClient.GetAsync("WeatherForecast").ContinueWith(async response =>
            {
                if (response.IsCompletedSuccessfully)
                {
                    var content = await response.Result.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var value = JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(content, options)!.ToList();

                    filterManager = new FluentGridFilterManager<WeatherForecast>(value.AsQueryable());
                    StateHasChanged();
                }
            });
        }

        private void OnFilterChange()
        {
            // Apply all filters when a change occurs
            filterManager.ApplyFilters();
        }

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
