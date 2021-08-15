using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using albumica.Data;
using albumica.Models;
using albumica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace albumica.Pages.Import
{
    public partial class Location : IDisposable
    {
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
        private readonly IImport _t = LocalizationFactory.Import();
        private DotNetObjectReference<Location>? ThisRef;
        private IJSObjectReference? LocationService;

        private Dictionary<string, Country> Countries = new();
        private Dictionary<string, City> Cities = new();
        private Dictionary<string, Suburb> Suburbs = new();


        ExifTag[] CoordinateKeys = new ExifTag[] { ExifTag.GPSLatitudeRef, ExifTag.GPSLatitude, ExifTag.GPSLongitude };
        ExifTag[] AltitudeKeys = new ExifTag[] { ExifTag.GPSAltitude, ExifTag.GPSAltitudeRef };

        private bool Loading = true;
        private bool HasGpsCoordinates;
        private Data.Location CurrentLocation = new();
        private GeoCodeModel? CurrentGeoCode;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ThisRef = DotNetObjectReference.Create(this);
                LocationService = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/LocationService.js");

                await LoadLocations();

                await LocationService.InvokeVoidAsync("initialize", ThisRef);
                Loading = false;
            }
        }

        public void Dispose()
        {
            if (ThisRef != null)
                ThisRef.Dispose();
            if (LocationService != null)
                LocationService.DisposeAsync().GetAwaiter();
        }
        private async Task LoadLocations()
        {
            Countries = await Db.Countries
                .OrderBy(c => c.Name)
                .ToDictionaryAsync(c => c.Name);

            Cities = await Db.Cities
                .OrderBy(c => c.Name)
                .ToDictionaryAsync(c => c.Name);

            Suburbs = await Db.Suburbs
                .OrderBy(c => c.Name)
                .ToDictionaryAsync(c => c.Name);

            StateHasChanged();
        }
        private async Task ReverseGeoCode()
        {
            if (LocationService == null)
                return;

            var baseUri = $"https://nominatim.openstreetmap.org/reverse";
            var qs = new Dictionary<string, string?>
            {
                { "lat", CurrentLocation.Latitude.ToString() },
                { "lon", CurrentLocation.Longitude.ToString() },
                { "zoom", "14" },
                { "format", "json" },
                { "accept-language", C.Env.Locale },
            };

            var uri = QueryHelpers.AddQueryString(baseUri, qs);
            CurrentGeoCode = await LocationService.InvokeAsync<GeoCodeModel>("reverseGeoCode", uri);

            var newData = false;
            if (CurrentGeoCode != null && CurrentGeoCode.IsSuccess)
            {
                // Country
                if (!string.IsNullOrWhiteSpace(CurrentGeoCode.Country))
                {
                    if (Countries.ContainsKey(CurrentGeoCode.Country))
                        CurrentLocation.Country = Countries[CurrentGeoCode.Country];
                    else
                    {
                        var newCountry = new Country(CurrentGeoCode.Country, CurrentGeoCode.CountryCode!.ToUpper(), CurrentGeoCode.Country);
                        Db.Countries.Add(newCountry);
                        await Db.SaveChangesAsync();

                        CurrentLocation.Country = newCountry;
                        newData = true;
                    }
                    CurrentLocation.CountryId = CurrentLocation.Country.CountryId;
                }

                // City
                if (!string.IsNullOrWhiteSpace(CurrentGeoCode.City))
                {
                    if (Cities.ContainsKey(CurrentGeoCode.City))
                        CurrentLocation.City = Cities[CurrentGeoCode.City];
                    else
                    {
                        var newCity = new City(CurrentGeoCode.City, CurrentGeoCode.City);
                        newCity.CountryId = CurrentLocation.CountryId!.Value;
                        Db.Cities.Add(newCity);
                        await Db.SaveChangesAsync();

                        CurrentLocation.City = newCity;
                        newData = true;
                    }
                    CurrentLocation.CityId = CurrentLocation.City.CityId;
                }

                // Suburb
                if (!string.IsNullOrWhiteSpace(CurrentGeoCode.Suburb))
                {
                    if (Suburbs.ContainsKey(CurrentGeoCode.Suburb))
                        CurrentLocation.Suburb = Suburbs[CurrentGeoCode.Suburb];
                    else
                    {
                        var newSuburb = new Suburb(CurrentGeoCode.Suburb, CurrentGeoCode.Suburb);
                        newSuburb.CityId = CurrentLocation.CityId!.Value;
                        Db.Suburbs.Add(newSuburb);
                        await Db.SaveChangesAsync();

                        CurrentLocation.Suburb = newSuburb;
                        newData = true;
                    }
                    CurrentLocation.SuburbId = CurrentLocation.Suburb.SuburbId;
                }
            }
            else
            {
                //TODO: handle reverse geocode fail
            }

            if (newData)
                await LoadLocations();
        }
        public async Task ChangeImage(ImportImageModel model)
        {
            Loading = true;
            HasGpsCoordinates = false;
            CurrentLocation = new();
            StateHasChanged();

            var info = SixLabors.ImageSharp.Image.Identify(model.FullName);
            if (info == null)
                return; // TODO: this is video most likely

            var exif = info.Metadata.ExifProfile.Values.ToDictionary(m => m.Tag);

            if (CoordinateKeys.All(k => exif.ContainsKey(k)))
            {
                var lat = exif[ExifTag.GPSLatitude].GetValue() as Rational[];
                var lon = exif[ExifTag.GPSLongitude].GetValue() as Rational[];
                var latRef = exif[ExifTag.GPSLatitudeRef].GetValue();

                if (lat != null && lon != null && latRef != null)
                {
                    var longitude = lon[0].ToDouble() + (lon[1].ToDouble() / 60) + (lon[2].ToDouble() / 3600);
                    var latitude = lat[0].ToDouble() + (lat[1].ToDouble() / 60) + (lat[2].ToDouble() / 3600);
                    if (latRef.ToString()!.Equals("S", StringComparison.InvariantCultureIgnoreCase))
                        latitude *= -1;

                    CurrentLocation.Latitude = latitude;
                    CurrentLocation.Longitude = longitude;

                    await ReverseGeoCode();
                    HasGpsCoordinates = true;

                    // Extract altitude
                    // if (altitudeKeys.All(k => exif.ContainsKey(k)))
                    // {
                    //     var alt = (Rational)exif[ExifTag.GPSAltitude].GetValue();
                    //     var altRef = (byte)exif[ExifTag.GPSAltitudeRef].GetValue();

                    //     var result = alt.ToDouble();
                    //     if (altRef == 1)
                    //         result *= -1; // Below see level

                    //     // Use here
                    // }
                }
            }

            Loading = false;
            StateHasChanged();
        }
    }
}