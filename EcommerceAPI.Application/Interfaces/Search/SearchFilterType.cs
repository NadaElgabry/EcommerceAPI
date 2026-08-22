using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Search
{
    public enum SearchFilterType
    {
        Term,
        Terms,
        RangeGte,
        RangeLte,
        RangeGt,
        RangeLt
    }
}
