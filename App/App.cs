﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Websilk.Services
{
    public class App: Service
    {
        public App(Core WebsilkCore, string[] paths):base(WebsilkCore, paths)
        {
        }

        public PageRequest LoadPage(string title)
        {
            if (S.isSessionLost() == true) { return lostPageRequest(); } //check session

            S.Page.Url.path = title.Replace("-", " ");
            S.Page.pageTitle = S.Util.Str.GetWebsiteTitle(S.Page.pageTitle) + " - " + title;
            S.Page.GetPageId();
            S.Page.LoadPageFromId(S.Page.pageId);
            S.Page.Render();
            return S.Page.PageRequest;
        }

        public PageRequest Url(string url)
        {
            if(S.isSessionLost() == true) { return lostPageRequest(); } //check session
            Console.WriteLine("Parse Url: " + url);
            return ParseUrl(url);
        }

        private PageRequest ParseUrl(string url, bool again = false)
        {
             if (string.IsNullOrEmpty(url) | url.ToLower().IndexOf("dashboard") == 0)
            {
                //load current page with no url
                string pageName = "Home";
                if (url.ToLower().IndexOf("dashboard") == 0 & S.User.userId < 1) { pageName = "Login"; }
                S.Page.PageRequest = new PageRequest();
                S.Page.Url.path = "";
                if ((S.Page.isEditorLoaded == false & url.ToLower().IndexOf("dashboard") == 0) | url.ToLower().IndexOf("dashboard") < 0)
                {
                    S.Page.Url.path = pageName.ToLower().Replace("-", " ");
                    S.Page.pageTitle = S.Page.pageTitle.Split(new char[] { '-', ' ', '\"' })[0] + " - " + pageName.Replace("-", " ");
                    S.Page.GetPageId();
                    S.Page.LoadPageFromId(S.Page.pageId);
                }
                S.Page.PageRequest.url = url;
                if (url.ToLower().IndexOf("dashboard") == 0)
                {
                    S.Page.RegisterJS("dashload", "setTimeout(function(){if(S.editor.dashboard){S.editor.dashboard.show('" + url + "');}},1000);");
                    S.Page.PageRequest.pageTitle = S.Page.websiteTitle + " - Dashboard";
                }
                S.Page.Render();
                Console.WriteLine("Load page from no url");
                return S.Page.PageRequest;
            }

            string[] arrUrl = url.Split('\"');
            int oldPageId = S.Page.pageId;

            if (arrUrl[0].IndexOf("+") < 0)
            {
                //found page with no query in url
                S.Page.Url.path = arrUrl[0].Replace("-", " ");
                S.Page.pageTitle = S.Page.pageTitle.Split(new char[] { '-', ' ', '\"' })[0] + " - " + arrUrl[0].Replace("-", " ");
                S.Page.GetPageId();
                S.Page.LoadPageFromId(S.Page.pageId);
                S.Page.Render();
                Console.WriteLine("Load page: " + S.Page.pageTitle);
                if(S.Page.PageRequest == null)
                {
                    S.Page.PageRequest = new PageRequest();
                    S.Page.PageRequest.components = new List<PageComponent>();
                    S.Page.PageRequest.css = "";
                    S.Page.PageRequest.editor = "";
                    S.Page.PageRequest.js = "";
                    S.Page.PageRequest.pageTitle = "";
                    S.Page.PageRequest.url = "";

                }
                S.Page.PageRequest.url = url;
                return S.Page.PageRequest;
            }

            return new PageRequest();
        }

        public string KeepAlive(string save = "")
        {
            if (S.isSessionLost() == true) { return "lost"; } //check session

            if (!string.IsNullOrEmpty(save))
            { 
                SaveWebPage(save);
            }
            return "";
        }

        public void SaveWebPage(string save = "")
        {
            Console.WriteLine("Save Web Page: " + save);
            JArray data = JsonConvert.DeserializeObject<JArray>(save);
            if(data != null)
            {
                string id = "";
                string type = "";
                int index = 0;
                bool matched = false;

                //process each change
                foreach(JObject item in data)
                {
                    //get component info
                    id = (string)item["id"];
                    type = (string)item["type"];
                    //find componentView match
                    for (int x = 0; x < S.Page.ComponentViews.Count; x++)
                    {
                        if (S.Page.ComponentViews[x].id == id) { index = x; break; }
                    }
                    switch (type)
                    {
                        case "position":
                            //update position data for a component
                            S.Page.ComponentViews[index].positionField = (string)item["data"];
                            break;
                        case "data":
                            //update data field for a component
                            S.Page.ComponentViews[index].dataField = (string)item["data"];
                            break;
                        case "arrangement":
                            //rearrange components within a panel
                            List<ComponentView> views = new List<ComponentView>();
                            List<string> comps = new List<string>();
                            foreach(string comp in item["data"])
                            {
                                comps.Add(comp);
                            }
                            int step = 0;
                            for(var i = 0; i < S.Page.ComponentViews.Count; i++)
                            {
                                if (step == 0)
                                { 
                                    //find first matching component
                                    for (var e = 0; e < comps.Count; e++)
                                    {
                                        if (comps[e] == S.Page.ComponentViews[i].id)
                                        {
                                            step = 1;
                                            break;
                                        }
                                    }
                                    if (step == 0)
                                    {
                                        //add view to new array
                                        views.Add(S.Page.ComponentViews[i]);
                                    }
                                }

                                if (step == 1)
                                {  
                                    //find matching component views and add to new array
                                    for (var e = 0; e < comps.Count; e++)
                                    {
                                        for (var u = 0; u < S.Page.ComponentViews.Count; u++)
                                        {
                                            if (S.Page.ComponentViews[u].id == comps[e])
                                            {
                                                views.Add(S.Page.ComponentViews[u]);
                                                break;
                                            }
                                        }
                                    }
                                    step = 2;
                                }

                                if(step == 2)
                                {
                                    //add rest of component views to new array
                                    matched = false;
                                    for (var e = 0; e < comps.Count; e++)
                                    {
                                        if (comps[e] == S.Page.ComponentViews[i].id)
                                        {
                                            matched = true;
                                            break;
                                        }
                                    }
                                    if (matched == false)
                                    {
                                        views.Add(S.Page.ComponentViews[i]);
                                    }
                                }
                                
                            }

                            S.Page.ComponentViews = views;
                            break;
                    }
                }

                //save page
                S.Page.Save(true);
            }
        }
    }
}
