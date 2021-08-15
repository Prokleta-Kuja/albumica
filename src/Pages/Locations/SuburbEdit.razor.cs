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
    public partial class SuburbEdit
    {
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Parameter] public int Id { get; set; }
        private Dictionary<int, string> _cities = null!;
        private List<Suburb> _suburbs = null!;

        private SuburbCreateModel? _create;
        private SuburbEditModel? _edit;
        private Suburb? _item;

        private Dictionary<string, string>? _errors;
        private readonly ILocations _t = LocalizationFactory.Locations();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            _cities = await Db.Cities
                .Include(c => c.Country)
                .OrderBy(c => c.DisplayName)
                .ToDictionaryAsync(c => c.CityId, c => $"{c.DisplayName}, {c.Country!.DisplayName}");

            _suburbs = await Db.Suburbs.ToListAsync();

            if (Id == 0)
                _create = new();
            else
            {
                _item = _suburbs.FirstOrDefault(s => s.SuburbId == Id);

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

            _errors = _create.Validate(_t, _suburbs);
            if (_errors != null)
                return;

            _item = new Suburb(_create.Name!, _create.DisplayName!);
            _item.CityId = _create.CityId;

            Db.Suburbs.Add(_item);
            await Db.SaveChangesAsync();
            await JS.NavigateBack();
        }
        async Task SaveEditClicked()
        {
            if (_edit == null || _item == null)
                return;

            _errors = _edit.Validate(_t, _suburbs);
            if (_errors != null)
                return;

            _item.CityId = _edit.CityId;
            _item.Name = _edit.Name!;
            _item.DisplayName = _edit.DisplayName!;

            await Db.SaveChangesAsync();
            await JS.NavigateBack();
        }
    }
}