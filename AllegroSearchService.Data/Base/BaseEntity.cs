using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AllegroSearchService.Data.Base
{
    public abstract class BaseEntity<T> : IBaseEntity<T>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public virtual T Id { get; set; }

        object IBaseEntity.Id
        {
            get { return Id; }
            set => Id = (T)value;
        }

        [DataType(DataType.DateTime)]
        public DateTime CreateDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedDate { get; set; }

        public Guid? CreatedBy { get; set; }

        public Guid? ModifiedBy { get; set; }
    }


    public abstract class BaseEntityManualId<T> : BaseEntity<T>, IBaseEntity<T>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public override T Id { get; set; }
    }
}
