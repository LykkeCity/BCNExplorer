using System;
                var ai = new TelemetryClient();
                ai.TrackException(filterContext.Exception);

                string controller = filterContext.RouteData.Values["controller"].ToString();
                string action = filterContext.RouteData.Values["action"].ToString();
        {
            string documentContents;
            using (Stream receiveStream = Request.InputStream)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    documentContents = readStream.ReadToEnd();
                }
            }
            return documentContents;
        }