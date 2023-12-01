using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HeritageApi.Data.Entities;
using HeritageApi.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


namespace Heritage.Services;

public class ExhibitService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly SessionService _sessionService;
    private string _apiBaseUrl;
    public ExhibitService(IHttpClientFactory clientFactory, SessionService sessionService)
    {
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _apiBaseUrl = "https://localhost:7210";
    }

    private async Task<HttpClient> GetClientAsync()
    {   
        var client = _clientFactory.CreateClient();
        var token = await _sessionService.GetTokenAsync();

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    public async Task<IEnumerable<Exhibit>> GetExhibitsAsync()
    {
        var client = _clientFactory.CreateClient();

        var response = await client.GetFromJsonAsync<IEnumerable<Exhibit>>($"{_apiBaseUrl}/api/Exhibit");

        return response ?? Array.Empty<Exhibit>();
    }

    public async Task<Exhibit> GetExhibitByIdAsync(long id)
    {
        var client = _clientFactory.CreateClient();

        var response = await client.GetFromJsonAsync<Exhibit>($"{_apiBaseUrl}/api/Exhibit/{id}");

        return response;
    }

    public async Task<long> CreateExhibitAsync(Exhibit exhibit)
    {
        var client = await GetClientAsync();
        var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/Exhibit", exhibit);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadFromJsonAsync<ExhibitCreationResponse>();
            return responseBody.ExhibitId;
        }

        return 0;
    }   

    public async Task<bool> UpdateExhibitAsync(long id, Exhibit exhibit)
    {
        var client = await GetClientAsync();

        var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/Exhibit/{id}", exhibit);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteExhibitAsync(long id)
    {
        var client = await GetClientAsync();

        var response = await client.DeleteAsync($"{_apiBaseUrl}/api/Exhibit/{id}");

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UploadImageAsync(long exhibitId, IFormFile imageFile)
    {
        var client = await GetClientAsync();

        var formData = new MultipartFormDataContent();
        formData.Add(new StreamContent(imageFile.OpenReadStream()), "file", imageFile.Name);

        var response = await client.PostAsync($"{_apiBaseUrl}/api/Exhibit/UploadImage/{exhibitId}", formData);

        return response.IsSuccessStatusCode;
    }

    public class ExhibitCreationResponse
    {
        public long ExhibitId { get; set; }
        public string Message { get; set; }
    }
}
