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
        private List<City> Cities = new();
        private List<Suburb> Suburbs = new();


        private bool Loading = true;
        public Data.Location CurrentLocation { get; set; } = new();
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
                .ToListAsync();

            Suburbs = await Db.Suburbs
                .OrderBy(c => c.Name)
                .ToListAsync();

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
                    CurrentLocation.City = Cities.FirstOrDefault(c => c.Name.Equals(CurrentGeoCode.City, StringComparison.InvariantCultureIgnoreCase) && c.CountryId == CurrentLocation.CountryId);
                    if (CurrentLocation.City == null)
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
                    CurrentLocation.Suburb = Suburbs.FirstOrDefault(c => c.Name.Equals(CurrentGeoCode.Suburb, StringComparison.InvariantCultureIgnoreCase) && c.CityId == CurrentLocation.CityId);
                    if (CurrentLocation.Suburb == null)
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
        public async Task Change(ImportModel model)
        {
            Loading = true;
            CurrentLocation = new();
            StateHasChanged();

            if (model.HasGpsData)
            {
                CurrentLocation.Latitude = model.Latitude!.Value;
                CurrentLocation.Longitude = model.Longitude!.Value;
                await ReverseGeoCode();
            }

            Loading = false;
            StateHasChanged();
        }
    }
}