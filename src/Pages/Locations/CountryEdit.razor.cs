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
    public partial class CountryEdit
    {
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Parameter] public int Id { get; set; }
        private CountryCreateModel? _create;
        private CountryEditModel? _edit;
        private Country? _item;
        private HashSet<string> _names = new();
        private HashSet<string> _displayNames = new();
        private Dictionary<string, string>? _errors;
        private readonly ILocations _t = LocalizationFactory.Locations();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            var countries = await Db.Countries.ToListAsync();

            if (Id == 0)
                _create = new();
            else
            {
                _item = countries.FirstOrDefault(s => s.CountryId == Id);

                if (_item != null)
                    _edit = new(_item);
            }

            foreach (var country in countries)
                if (country.CountryId != Id)
                {
                    _names.Add(country.Name.ToUpper());
                    _displayNames.Add(country.DisplayName.ToUpper());
                }

            StateHasChanged();
        }
        async Task CancelClicked() => await JS.NavigateBack();
        async Task SaveCreateClicked()
        {
            if (_create == null)
                return;

            _errors = _create.Validate(_t, _names, _displayNames);
            if (_errors != null)
                return;

            _item = new Country(_create.Name!, _create.Code!, _create.DisplayName!);

            Db.Countries.Add(_item);
            await Db.SaveChangesAsync();
            await JS.NavigateBack();
        }
        async Task SaveEditClicked()
        {
            if (_edit == null || _item == null)
                return;

            _errors = _edit.Validate(_t, _names, _displayNames);
            if (_errors != null)
                return;

            _item.Name = _edit.Name!;
            _item.Code = _edit.Code!;
            _item.DisplayName = _edit.DisplayName!;

            await Db.SaveChangesAsync();
            await JS.NavigateBack();
        }
    }
}