using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Code_Game.Domain;
using Code_Game.Scripts.Core;
using Code_Game.Scripts.Contracts.Api;

namespace Code_Game.Scripts.Services.Api;

public class PlayerProfileApi : IPlayerProfileApi
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri(AppConfig.ApiBaseUrl)
    };

    public async Task<bool> SendProfileAsync(PlayerProfile profile)
    {
        try
        {
            var payload = JsonSerializer.Serialize(profile);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(AppConfig.PlayerProfileEndpoint, content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
