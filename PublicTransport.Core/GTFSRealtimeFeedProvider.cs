using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TransitRealtime;

namespace PublicTransport.Core
{
    public class GTFSRealtimeFeedProvider
    {

        public IObservable<FeedMessage> GetFeed(Uri url)
        {
            return Observable.Create<FeedMessage>(async x =>
            {
                try
                {
                   var feed = await ConvertToFeedAsync(url);
                    x.OnNext(feed);
                }
                catch (Exception ex)
                {
                    x.OnError(ex);
                }

                return Disposable.Empty;
            });
        }


        private async Task<FeedMessage> ConvertToFeedAsync(Uri url)
        {
           var request = HttpWebRequest.Create(url);

           return  await Task.Run(() => Serializer.Deserialize<FeedMessage>(request.GetResponse().GetResponseStream()));
        }

    }
}
