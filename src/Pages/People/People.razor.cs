using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using albumica.Data;
using albumica.Models;
using albumica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace albumica.Pages.People
{
    public partial class People
    {
        [Inject] private AppDbContext Db { get; set; } = null!;

        private List<PersonModel> _people = new();
        private readonly IPeople _t = LocalizationFactory.People();
        private readonly Formats _f = LocalizationFactory.Formats();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _people = await Db.PersonWithCounts.ToListAsync(); ;
        }
    }
}