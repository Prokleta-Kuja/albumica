using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using albumica.Data;
using albumica.Models;
using albumica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace albumica.Pages.Locations
{
    public partial class Locations
    {
        [Inject] private AppDbContext Db { get; set; } = null!;

        private List<CountryModel> _countries = new();
        private List<CityModel> _cities = new();
        private List<Suburb> _suburbs = new();
        private readonly ILocations _t = LocalizationFactory.Locations();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _countries = await Db.CountryWithCounts.ToListAsync();

            _cities = await Db.Cities.Include(c => c.Country).Select(c => new CityModel(c, c.Suburbs!.Count)).ToListAsync();

            _suburbs = await Db.Suburbs.ToListAsync();
        }
    }
}