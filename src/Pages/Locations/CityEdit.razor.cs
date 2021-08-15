using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using albumica.Data;
using albumica.Extensions;
using albumica.Models;
using albumica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace albumica.Pages.Locations
{
    public partial class CityEdit
    {
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Parameter] public int Id { get; set; }
        private Dictionary<int, string> _countries = null!;
        private List<City> _cities = null!;

        private CityCreateModel? _create;
        private CityEditModel? _edit;
        private City? _item;

        private Dictionary<string, string>? _errors;
        private readonly ILocations _t = LocalizationFactory.Locations();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            _countries = await Db.Countries
                .OrderBy(c => c.DisplayName)
                .ToDictionaryAsync(c => c.CountryId, c => c.DisplayName);

            _cities = await Db.Cities.ToListAsync();

            if (Id == 0)
                _create = new();
            else
            {
                _item = _cities.FirstOrDefault(s => s.CityId == Id);

                if (_item != null)
                    _edit = new(_item);
            }


            StateHasChanged();
        }
        async Task CancelClicked() => await JS.NavigateBack();
        async Task SaveCreateClicked()
        {
            if (_create == null)
                return;

            _errors = _create.Validate(_t, _cities);
            if (_errors != null)
                return;

            _item = new City(_create.Name!, _create.DisplayName!);
            _item.CountryId = _create.CountryId;

            Db.Cities.Add(_item);
            await Db.SaveChangesAsync();
            await JS.NavigateBack();
        }
        async Task SaveEditClicked()
        {
            if (_edit == null || _item == null)
                return;

            _errors = _edit.Validate(_t, _cities);
            if (_errors != null)
                return;

            _item.CountryId = _edit.CountryId;
            _item.Name = _edit.Name!;
            _item.DisplayName = _edit.DisplayName!;

            await Db.SaveChangesAsync();
            await JS.NavigateBack();
        }
    }
}