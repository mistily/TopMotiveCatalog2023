using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.Controllers
{
    internal class DBFiller
    {
        private IFiller _filler;
        public DBFiller()
        {
            Randomizer.Seed = new Random(3985309);
            this._filler = new ManufacturerController(); 
        }
        public DBFiller(IFiller flr)
        {
            Randomizer.Seed = new Random(3985309);
            this._filler = flr;
        }

        public void SetFiller(IFiller filler)
        {
            _filler = filler;
        }

        public IFiller getFiller()
        {
            return _filler;
        }
        public void fillGeneric()
        {
            try
            {
                if (!this.SeeIfEmpty())
                {
                    Console.WriteLine("Looks like the {0} table is not empty!", this._filler.ModelName);
                    Console.WriteLine("Do you wish to continue generating new objects in this table?(Y/N)");
                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    Char key = Convert.ToChar(keyInfo.Key);
                    if (key.ToString().ToLower() == "n")
                    {
                        return;
                    }
                }
                var result = this._filler.DoGenerate();
            }catch(Exception ex) {
                Console.WriteLine(ex.Message);
                
            }
        }

        public Boolean SeeIfEmpty()
        {
            return _filler.SeeIfEmpty();
        }


    }
}
