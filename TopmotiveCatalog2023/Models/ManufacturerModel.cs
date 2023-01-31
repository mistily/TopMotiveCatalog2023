using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopmotiveCatalog2023.Models
{
    [Table("manufacturer")]
    internal class ManufacturerModel : IModel
    {
        public ManufacturerModel() {
            Id = Guid.NewGuid();
            Description = String.Empty;
        }
        public ManufacturerModel(Guid id, string name, string desc, string phone, String imgpath, String? type)
        {
            Id = id;
            Description = desc;
            Contact_phone = phone;
            Picture = imgpath;
            Type = type;
        }
        [Column("id")]
        public Guid Id { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("contact_phone")]
        public string? Contact_phone { get; set; }

        [Column("picture")]
        public String? Picture { get; set; }

        [Column("type")]
        public String? Type { get; set; }

        [Column("short_code")]
        public String? ShortCode { get; set; }

        [Column("date_added")]
        public DateTime? DateAdded { get; }

        [Column("date_updated")]
        public DateTime? DateUpdated{ get; set; }

        public ICollection<VehicleModelsModel>? VehicleModels { get; set; }
        public ICollection<ArticleModel>? Articles { get; set; }
    }
}
