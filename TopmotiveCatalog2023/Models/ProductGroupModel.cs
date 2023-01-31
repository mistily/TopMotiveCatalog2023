using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopmotiveCatalog2023.Models
{
    [Table("product_group")]
    internal class ProductGroupModel : IModel
    {
        public ProductGroupModel()
        {
            Id= Guid.NewGuid();
            Description= string.Empty;
            DateUpdated = DateTime.Now;
        }
        public ProductGroupModel(Guid id, string desc, String imgpath)
        {
            Id = id;
            Description = desc;
            Picture = imgpath;
            DateUpdated = DateTime.Now;
        }
        [Column("id")]
        public Guid Id { get; set; }

        [Column("description")]
        public String Description { get; set; }

        [Column("picture")]
        public String? Picture { get; set; }

        [Column("date_added")]
        public DateTime? DateAdded { get; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }

        public ICollection<ArticleModel>? Articles { get; set; }

        public ICollection<VehicleTypesModel>? VehicleTypes { get; set; }

        public ICollection<ProductGroupToVehicleTypeModel>? ProductGroupToVehicleTypes { get; set; }
    }
}
