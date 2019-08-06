using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitRealtime;

namespace PublicTransport.Core.ViewModels
{

    public interface ILocation
    {

        double Longitude { get; }

        double Latitude { get; }
    }

    public class TransportaionVehicle : ReactiveObject, ILocation
    {
        public string Id { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Speed { get; set; }

        public string RouteId { get; set; }
    }
    public class MapViewModelBase : ReactiveObject
    {
        private readonly GTFSRealtimeFeedProvider _feedProvider = new GTFSRealtimeFeedProvider();

        private readonly string _vehiclePositionsUrl = "https://www.ztm.poznan.pl/pl/dla-deweloperow/getGtfsRtFile?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJ0ZXN0Mi56dG0ucG96bmFuLnBsIiwiY29kZSI6MSwibG9naW4iOiJtaFRvcm8iLCJ0aW1lc3RhbXAiOjE1MTM5NDQ4MTJ9.ND6_VN06FZxRfgVylJghAoKp4zZv6_yZVBu_1-yahlo&file=vehicle_positions.pb";
        private readonly string _tripUpdatesUrl = "https://www.ztm.poznan.pl/pl/dla-deweloperow/getGtfsRtFile/?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJ0ZXN0Mi56dG0ucG96bmFuLnBsIiwiY29kZSI6MSwibG9naW4iOiJtaFRvcm8iLCJ0aW1lc3RhbXAiOjE1MTM5NDQ4MTJ9.ND6_VN06FZxRfgVylJghAoKp4zZv6_yZVBu_1-yahlo&file=trip_updates.pb";

        private readonly SourceCache<TransportaionVehicle, string> _vehiclePositions = new SourceCache<TransportaionVehicle, string>(x => x.Id);
        private readonly ReadOnlyObservableCollection<TransportaionVehicle> _vehicleLocations;

        class CombinedInfo
        {
            public FeedMessage Trips { get; set; }
            public FeedMessage VehiclePositions { get; set; }
        }

        public MapViewModelBase()
        {
            var trips = _feedProvider.GetFeed(new Uri(_tripUpdatesUrl));
            var vehiclePositions = _feedProvider.GetFeed(new Uri(_vehiclePositionsUrl))
                .Do(x =>
                {

                });
            var tick = Observable.Interval(TimeSpan.FromSeconds(4));

            tick.Do(t =>
            {

            }).Subscribe();


            tick.SelectMany(t => vehiclePositions.WithLatestFrom(trips, (vehicleResult, tripsResult) =>
            {
                return new CombinedInfo
                {
                    Trips = tripsResult,
                    VehiclePositions = vehicleResult
                };
            }))
                .Retry()
                .SelectMany(x => Observable.FromAsync(() => CombineResults(x)))
                .Select(f => f.Entities.Select(x => new TransportaionVehicle
                {
                    Id = x.Vehicle.Vehicle.Id,
                    Latitude = x.Vehicle.Position.Latitude,
                    Longitude = x.Vehicle.Position.Longitude,
                    Speed = $"{x.Vehicle.Position.Speed}",
                    RouteId = $"{x.Vehicle.Trip.RouteId}"
                }))
                .Subscribe(vehicleLocations =>
                {
                    _vehiclePositions.AddOrUpdate(vehicleLocations);
                });


            _vehiclePositions.Connect()
                .ObserveOnDispatcher()
                .Bind(out _vehicleLocations)
                .DisposeMany()
                .Buffer(TimeSpan.FromSeconds(2))
                .Subscribe(x =>
                {
                    Debug.WriteLine($"{DateTime.Now} Changes {x.Count}");
                });
        }

        private async Task<FeedMessage> CombineResults(CombinedInfo combined)
        {
            return await Task.Run(() =>
            {
                var vehicleById = combined
                .VehiclePositions
                .Entities
                .Where(e => e?.Vehicle.Vehicle.Id != null)
                .ToDictionary(x => x.Vehicle.Vehicle.Id);

                foreach (var trip in combined.Trips.Entities.Where(x => x.TripUpdate?.Vehicle?.Id != null))
                {
                    FeedEntity entity = null;

                    if (vehicleById.TryGetValue(trip.TripUpdate.Vehicle.Id, out entity))
                    {
                        trip.Vehicle = entity.Vehicle;
                    }
                }


                return combined.Trips;
            });
        }

        public ReadOnlyObservableCollection<TransportaionVehicle> VehicleLocations { get { return _vehicleLocations; } }
    }
}
