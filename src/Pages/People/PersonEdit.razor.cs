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

namespace albumica.Pages.People
{
    public partial class PersonEdit
    {
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Parameter] public int Id { get; set; }
        private PersonCreateModel? _create;
        private PersonEditModel? _edit;
        private Person? _item;
        private HashSet<string> _names = new();
        private Dictionary<string, string>? _errors;
        private readonly IPeople _t = LocalizationFactory.People();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            var people = await Db.People.ToListAsync();

            if (Id == 0)
                _create = new();
            else
            {
                _item = people.FirstOrDefault(s => s.PersonId == Id);

                if (_item != null)
                    _edit = new(_item);
            }

            foreach (var person in people)
                if (person.PersonId != Id)
                    _names.Add(person.Name.ToUpper());

            StateHasChanged();
        }
        async Task CancelClicked() => await JS.NavigateBack();
        async Task SaveCreateClicked()
        {
            if (_create == null)
                return;

            _errors = _create.Validate(_t, _names);
            if (_errors != null)
                return;

            _item = new Person(_create.Name!);
            _item.FirstName = _create.FirstName;
            _item.LastName = _create.LastName;
            _item.DateOfBirth = _create.DateOfBirth;

            Db.People.Add(_item);
            await Db.SaveChangesAsync();
            await JS.NavigateBack();
        }
        async Task SaveEditClicked()
        {
            if (_edit == null || _item == null)
                return;

            _errors = _edit.Validate(_t, _names);
            if (_errors != null)
                return;

            _item.Name = _edit.Name!;
            _item.FirstName = _edit.FirstName;
            _item.LastName = _edit.LastName;
            _item.DateOfBirth = _edit.DateOfBirth;

            await Db.SaveChangesAsync();
            await JS.NavigateBack();
        }
    }
}