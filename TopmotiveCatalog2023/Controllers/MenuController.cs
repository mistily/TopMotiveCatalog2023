using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.Controllers
{
    internal class MenuController: Controller
    {
        readonly Dictionary<int, IFiller> Menu;

        public MenuController(Dictionary<int, IFiller> menu)
        {
            Menu = menu;
        }


        public MenuController(Action<String>? log)
        {
            Menu = new Dictionary<int, IFiller>()
            {
                { -1, new ManufacturerController() },
                { 1, new ModelController() },
                { 2, new VehicleTypeController() },
                { 3, new ProductGroupController() },
                { 4, new ArticleController() }
            };
            if(log!=null )
            {
                Log = log;
            }
        }
    }
}
