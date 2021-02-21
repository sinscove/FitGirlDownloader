using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FitGirlDownloader.Ui;

namespace FitGirlDownloader
{
    public static class FitGirlScraper
    {
        private const string BaseUrl = "https://fitgirl-repacks.site/";
        
        private static readonly HtmlParser HtmlParser;
        private static readonly HttpClient HttpClient;

        private static int _waitTime;

        static FitGirlScraper()
        {
            HtmlParser = new HtmlParser();
            HttpClient = new HttpClient();

            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
            HttpClient.DefaultRequestHeaders.Upgrade.ParseAdd("1");
        }

        public static async Task<Dictionary<string, GameInfo>> GetGameList()
        {
            IHtmlDocument firstPage = await GetAzPage(1);
            Dictionary<string, GameInfo> gameList = ListGamesInNode(firstPage);

            // Extracts the last page number from the nav bar at the bottom of the page
            string lastPageNumber = firstPage.QuerySelector(".lcp_paginator").Children[^2].Children[0].Text();

            for (int pageNum = 2; pageNum <= int.Parse(lastPageNumber); pageNum++)
            {
                IHtmlDocument page = await GetAzPage(pageNum);
                foreach ((string name, GameInfo gameInfo) in ListGamesInNode(page))
                {
                    if (!gameList.ContainsKey(name))
                    {
                        gameList.Add(name, gameInfo);
                    }
                }
            }

            return gameList;
        }

        public static async Task<GameInfo> GetGameInfo(string url)
        {
            string htmlString  = await HttpClient.GetStringWithRateLimitAsync(url);
            IHtmlDocument page = await HtmlParser.ParseDocumentAsync(htmlString);

            return new GameInfo 
            { 
                PageUrl     = url,
                Mirrors     = ListMirrorsInNode(page),
                Description = GetDescriptionFromNode(page)
            };
        }

        private static async Task<IHtmlDocument> GetAzPage(int pageNumber)
        {
            string htmlString = await HttpClient.GetStringWithRateLimitAsync(BaseUrl + $"all-my-repacks-a-z/?lcp_page0={pageNumber}#lcp_instance_0");
            return await HtmlParser.ParseDocumentAsync(htmlString);
        }

        private static Dictionary<string, GameInfo> ListGamesInNode(IParentNode parentNode) =>
            parentNode.QuerySelector("#lcp_instance_0") // Get unordered list containing all games on page
                .Children.Select(e => e.Children[0])    // Get the anchor first elements within the list item elements
                .Cast<IHtmlAnchorElement>()
                .ToDictionary(e => e.Text, e => new GameInfo { PageUrl = e.Href });

        private static List<Mirror> ListMirrorsInNode(IParentNode parentNode)
        {
            List<Mirror> mirrorList = new List<Mirror>();

            void AddMirrorsFromElement(IElement element)
            {
                if (element is IHtmlAnchorElement a && a.Text != "JDownloader2")
                {
                    mirrorList.Add(new Mirror
                    {
                        MirrorName = Regex.Replace(a.Text, "filehoster(s?): ", "", RegexOptions.IgnoreCase),
                        MirrorUrl  = a.Href
                    });
                }
            }

            IElement mirrorListElement = parentNode.QuerySelector(".entry-content")
                .Children.First(e => e.Text().Contains("download mirrors", StringComparison.CurrentCultureIgnoreCase))
                .NextElementSibling;

            foreach (IElement element in mirrorListElement.Children)
            {
                if (element.LocalName == "li")
                {
                    foreach (IElement child in element.Children)
                    {
                        AddMirrorsFromElement(child);
                    }
                }
                else
                {
                    AddMirrorsFromElement(element);
                } 
            }

            return mirrorList;
        }

        private static string GetDescriptionFromNode(IParentNode parentNode)
        {
            try
            {
                IElement descriptionElement = parentNode.QuerySelectorAll(".su-spoiler")
                    .First(e => e.InnerHtml.Contains("Game Description", StringComparison.InvariantCultureIgnoreCase))
                    .QuerySelector(".su-spoiler-content");

                return descriptionElement.Text().Replace("\n", "");
            }
            catch
            {
                return "";
            }
        }

        private static async Task<string> GetStringWithRateLimitAsync(this HttpClient client, string requestUri)
        {
            while (true)
            {
                try
                {
                    string result = await client.GetStringAsync(requestUri);
                    
                    RatelimitErrorDialog.End();

                    return result;
                }
                catch (HttpRequestException)
                {
                    _waitTime += 10;
                    RatelimitErrorDialog.Show(_waitTime);
                    await Task.Delay(TimeSpan.FromSeconds(_waitTime));
                }
            }
        }
    }
}