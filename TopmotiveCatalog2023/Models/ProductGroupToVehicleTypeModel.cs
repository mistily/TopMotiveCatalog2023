using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopmotiveCatalog2023.Models
{
    [Table("product_group_to_vehicle_type")]
    internal class ProductGroupToVehicleTypeModel : IModel
    {

        public ProductGroupToVehicleTypeModel()
        {
            VehicleType = new VehicleTypesModel();
            ProductGroup = new ProductGroupModel();
            ProductGroupId = ProductGroup.Id;
            VehicleType.Id = VehicleType.Id;
        }
        [Column("product_group_id")]
        public Guid ProductGroupId { get; set; }

        [Column("vehicle_type_id")]
        public Guid VehicleTypeId { get; set; }

        public VehicleTypesModel VehicleType { get; set; }
        public ProductGroupModel ProductGroup { get; set; }

        public String? Picture { get; set; }

        [Column("date_added")]
        public DateTime? DateAdded { get;  }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }
    }
}
