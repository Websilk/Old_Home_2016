﻿using System.Collections.Generic;

namespace Websilk.Services.Dashboard
{
    public class Users : Service
    {
        public Users(Core WebsilkCore) : base(WebsilkCore)
        {
        }

        public Inject Load()
        {
            if (S.isSessionLost()) { return lostInject(); }
            Inject response = new Inject();

            //check security
            if (S.User.Website(S.Page.websiteId).getWebsiteSecurityItem("dashboard/users", 0) == false) { return response; }

            //setup response
            response.element = ".winDashboardUsers > .content";

            //setup scaffolding variables
            //setup scaffolding variables
            Scaffold scaffold = new Scaffold(S, "/app/dashboard/users.html");
            scaffold.Data["test"] = S.Page.websiteTitle;

            //finally, scaffold Websilk platform HTML
            response.html = scaffold.Render();
            response.js = CompileJs();

            return response;
        }
    }
}
