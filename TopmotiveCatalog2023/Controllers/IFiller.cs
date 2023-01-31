using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.Controllers
{
    internal interface IFiller {

        public String? ModelName { get; set; }
        public String? ModelType { get; set; }


        public object DoGenerate();
        public Dictionary<Guid, string> ListAll();

        public Dictionary<Guid, string>? GetOption(List<string?>? options = null);

        public Boolean SeeIfEmpty();

        public void AddNewFromConsole(List<object?> options);

        public void UpdateExistingFromConsole(List<object>? options=null, IModel? model=null);

        public void DeleteExisitngFromConsole(List<object>? options = null, IModel? model = null);

    }
}
