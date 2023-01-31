using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopmotiveCatalog2023.Models
{
    [Table("article")]
    internal class ArticleModel : IModel
    {
        private const string V = "date_added";
        private string? picture;

        public ArticleModel()
        {
            Id = Guid.NewGuid();
            Description = "Unnamed";
        }
        public ArticleModel(Guid id, String desc, float? price, String? imgpath, Guid? brand_id, String? currency_code, String? notes, Guid? product_group_id)
        {
            Id = id;
            Description = desc;
            Price = price;
            Picture = imgpath;
            BrandId = brand_id;
            ProductGroupId = product_group_id;
        }
        [Column("id")]
        public Guid Id { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("price")]
        public float? Price { get; set; }

        [Column("picture")]
        public string? Picture { get => picture; set => picture = value; }

        [Column(V)]
        public DateTime? DateAdded { get; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }
        [Column("brand_id")]
        public Guid? BrandId { get; set; }

        public ManufacturerModel? Brand { get; set; }

        [Column("product_group_id")]
        public Guid? ProductGroupId { get; set; }

        public ProductGroupModel? ProductGroup { get; set; }

        public ICollection<VehicleTypesOfArticlesModel>? VehicleTypesOfArticles { get; set; }

        public ICollection<VehicleTypesModel>? VehicleTypes { get; set; }
    }
}
