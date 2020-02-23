using System;
using System.Collections.Generic;
using Polly;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Net.Http;
using Newtonsoft.Json;
using FullStackRebelsTestCSharp.Models;
using System.Net;

namespace FullStackRebelsTestCSharp.Services
{
    public class TVMazeClientService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly DatabaseService _databaseService;

        public TVMazeClientService(HttpClient httpClient, DatabaseService databaseService)
        {
            // I left the magic string here for readability, but of course it would be better having it coming from a static file
            httpClient.BaseAddress = new Uri("http://api.tvmaze.com/");
            _httpClient = httpClient;
            _databaseService = databaseService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartRoutine();
        }

        public async Task StartRoutine()
        {
            try
            {
                //get all data
                await GetDataFromApi();
            } catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task GetDataFromApi(int page = 0, string url = "/shows?page=")
        {
            // use polly for safely retrying until the end
            await Policy.HandleResult<HttpResponseMessage>(message => message.StatusCode != HttpStatusCode.NotFound)
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))) //  wait from 2 seconds esponentially per the number of failed request
                .ExecuteAsync(async () => {
                    page++;
                    var response = await _httpClient.GetAsync(url + page);
                    var content = await response.Content.ReadAsStringAsync();
                    List<Show> shows = JsonConvert.DeserializeObject<List<Show>>(content);
                    
                    foreach(Show show in shows)
                    {
                        await GetCastDataAndSaveShow(show);
                    }
                    
                    //than return the response for checking the statusCode
                    return response;
                });

        }

        private async Task<HttpResponseMessage> GetCastDataAndSaveShow(Show show)
        {
            return await Policy.HandleResult<HttpResponseMessage>(message => message.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                .ExecuteAsync(async () =>
                {
                    // in case the application should crash, restart without spamming useless queries
                    if (!await _databaseService.AlreadyInDatabase(show))
                    {
                        var responseCast = await _httpClient.GetAsync("/shows/" + show.Id + "/cast");
                        var contentCast = await responseCast.Content.ReadAsStringAsync();
                        if(contentCast != null && contentCast.Length > 2) // since the empty response is returned as "[]" the content lenght will be always at least 2
                        {
                            List<CastType> listRaw = new List<CastType>();
                            try
                            {
                                listRaw = JsonConvert.DeserializeObject<List<CastType>>(contentCast);
                            } catch (Exception e)
                            {
                                var exception = JsonConvert.DeserializeObject<ErrorResponse>(contentCast);
                                if(exception.status == "429")
                                {
                                    //too many requests
                                    return responseCast;
                                }

                                throw new Exception(e.Message);
                            }
                            await AddCastAndSaveData(show, listRaw);
                        }
                        return responseCast;
                    }
                    return new HttpResponseMessage(HttpStatusCode.Continue);
                });
        }

        private async Task AddCastAndSaveData(Show show, List<CastType> list)
        {
            List<Cast> listCast = new List<Cast>();
            foreach (var castMember in list)
            {
                // we don't need the cast member without birthday
                if (castMember.person.Birthday != null)
                {
                    listCast.Add(castMember.person);
                }
            }
            show.Cast = listCast;
            // save to db
            await _databaseService.AddShow(show);
        }
        
    }
}
