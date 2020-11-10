using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AllegroSearchService.Api.ViewModels;

namespace AllegroSearchService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        //public ActionResult EmployeeList([DataSourceRequest] DataSourceRequest request)
        //{
        //    try
        //    {
        //        List<Employee> _emp = new List<Employee>();
        //        _emp.Add(new Employee(1, "Bobb", "Ross"));
        //        _emp.Add(new Employee(2, "Pradeep", "Raj"));
        //        _emp.Add(new Employee(3, "Arun", "Kumar"));
        //        DataSourceResult result = _emp.ToDataSourceResult(request);
        //        return Json(result, System.Web.Mvc.JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.Message, System.Web.Mvc.JsonRequestBehavior.AllowGet);

        //    }
        //}
    }
}
