using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopmotiveCatalog2023.DAL;

namespace TopmotiveCatalog2023.Models
{
    [Table("vehicle_types")]
    internal class VehicleTypesModel: IModel
    {
        public List<String> FuelTypes = new List<string>() { 
            "Petrol", 
            "Diesel", 
            "Bio diesel", 
            "Liquid petroleum gas", 
            "Ethanol or methanol", 
            "Electric", 
            "Hydrogen"
        };
    public VehicleTypesModel()
        {
            Id = Guid.NewGuid();
            Description= string.Empty;
        }
        public VehicleTypesModel(Guid id, string desc, ushort ccm, ushort kw, String fuelType, String bodyType, ushort doorCount, string notes, String imgpath, Guid? vehicle_model)
        {
            Id = id;
            Description = desc;
            CCM = ccm;
            KW = kw;
            FuelType = fuelType;
            BodyType = bodyType;
            DoorCount = doorCount;
            Notes = notes;
            Picture = imgpath;
            DateUpdated = DateTime.Now;
            VehicleModelId = vehicle_model;
        }
        [Column("id")]
        public Guid Id { get; set; }

        [Column("description")]
        public String Description { get; set; }

        [Column("CCM")]
        public ushort? CCM { get; set; }

        [Column("fuel_type")]
        public String? FuelType { get; set; }

        [Column("KW")]
        public uint? KW { get; set; }

        [Column("body_type")]
        public String? BodyType { get; set; }

        [Column("door_count")]
        public ushort? DoorCount { get; set; }

        [Column("notes")]
        public String? Notes { get; set; }

        [Column("picture")]
        public String? Picture { get; set; }

       
        [Column("date_added")]
        public DateTime? DateAdded { get;  }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }

        [Column("model_id")]
        public Guid? VehicleModelId { get; set; }

        public VehicleModelsModel? VehicleModel { get; set; }

        public ICollection<ArticleModel>? Articles { get; set; }

        public ICollection<VehicleTypesOfArticlesModel>? VehicleTypesOfArticles { get; set; }

        public ICollection<ProductGroupModel>? ProductGroups { get; set; }

        public ICollection<ProductGroupToVehicleTypeModel>? ProductGroupToVehicleTypes { get; set; }
    }
}
