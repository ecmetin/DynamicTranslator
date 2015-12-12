﻿namespace DynamicTranslator.Orchestrators.Finders
{
    #region using

    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Cache;
    using System.Text;
    using System.Threading.Tasks;
    using Core.Config;
    using Core.Orchestrators.Finder;
    using Core.Orchestrators.Model;
    using Core.Orchestrators.Organizer;
    using Core.ViewModel.Constants;

    #endregion

    public class TurengFinder : IMeanFinder
    {
        private readonly IStartupConfiguration configuration;
        private readonly IMeanOrganizerFactory meanOrganizerFactory;

        public TurengFinder(IMeanOrganizerFactory meanOrganizerFactory, IStartupConfiguration configuration)
        {
            this.meanOrganizerFactory = meanOrganizerFactory;
            this.configuration = configuration;
        }

        private bool CanBeTranslated => configuration.IsToLanguageTurkish;

        public async Task<TranslateResult> Find(TranslateRequest translateRequest)
        {
            if (!CanBeTranslated)
            {
                return new TranslateResult();
            }

            var address = configuration.TurengUrl;
            var uri = new Uri(address + translateRequest.CurrentText);

            var turenClient = new WebClient();
            turenClient.Encoding = Encoding.UTF8;
            turenClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36");
            turenClient.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,tr;q=0.6");
            turenClient.CachePolicy = new HttpRequestCachePolicy(HttpCacheAgeControl.MaxAge, TimeSpan.FromHours(1));

            var compositeMean = await turenClient.DownloadStringTaskAsync(uri);
            var organizer = meanOrganizerFactory.GetMeanOrganizers().First(x => x.TranslatorType == TranslatorType.TURENG);
            var mean = await organizer.OrganizeMean(compositeMean);

            return new TranslateResult(true, mean);
        }
    }
}