using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopmotiveCatalog2023.Models
{
    [Table("vehicle_types_of_articles")]
    internal class VehicleTypesOfArticlesModel : IModel
    {
        public VehicleTypesOfArticlesModel()
        {
        }
        public VehicleTypesOfArticlesModel(Guid vti, Guid ai)
        {
            VehicleTypeId = vti;
            ArticleId = ai;
            VehicleType = new VehicleTypesModel();
            Article = new ArticleModel();
        }

        [Column("article_id")]
        public Guid ArticleId { get; set; }

        [Column("vehicle_type_id")]
        public Guid VehicleTypeId { get; set; }

        public VehicleTypesModel VehicleType { get; set; }
        public ArticleModel Article { get; set; }

        [Column("date_added")]
        public DateTime? DateAdded { get; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }
    }
}
