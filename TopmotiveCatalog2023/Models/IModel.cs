using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopmotiveCatalog2023.Models
{
    internal interface IModel
    {
        [Column("date_added")]
        public DateTime? DateAdded { get; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }
    }
}
