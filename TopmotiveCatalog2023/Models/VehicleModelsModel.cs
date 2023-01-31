using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopmotiveCatalog2023.Models
{
    [Table("vehicle_models")]
    internal class VehicleModelsModel : IModel
    {
        public VehicleModelsModel() { 
            Id= Guid.NewGuid();
            Description = string.Empty;
        }

        public VehicleModelsModel(Guid id, string name, string desc, String imgpath, String notes, Guid manufacturer)
        {
            Id = id;
            Description = desc;
            Picture = imgpath;
            DateUpdated = DateTime.Now;
            Notes = notes;
            ManufacturerId = manufacturer;
        }

        [Column("id")]
        public Guid Id { get; set; }

        [Column("description")]
        public String Description { get; set; }

        [Column("manufacturer_id")]
        public Guid? ManufacturerId { get; set; }

        public ManufacturerModel? Manufacturer { get; set; }

        [Column("notes")]
        public String? Notes { get; set; }

        [Column("picture")]
        public String? Picture { get; set; }


        [Column("date_added")]
        public DateTime? DateAdded { get;  }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set;}

        public ICollection<VehicleTypesModel>? VehicleTypes { get; set; }
    }
}
