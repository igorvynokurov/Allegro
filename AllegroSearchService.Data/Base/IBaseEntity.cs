using System;
using System.Collections.Generic;
using System.Text;

namespace AllegroSearchService.Data.Base
{
    public interface IBaseEntity<T> : IBaseEntity
    {
        new T Id { get; set; }
    }

    public interface IBaseEntity
    {
        object Id { get; set; }
        DateTime CreateDate { get; set; }
        DateTime? ModifiedDate { get; set; }
        Guid? CreatedBy { get; set; }
        Guid? ModifiedBy { get; set; }
    }
}
