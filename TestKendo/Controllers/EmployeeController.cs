using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TestKendo.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace TestKendo.Controllers
{
    
    public class EmployeeController : Controller
    {
    public ActionResult EmployeeList([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                List<Employee> _emp = new List<Employee>();
                _emp.Add(new Employee(1, "Igor", "Test1"));
                _emp.Add(new Employee(2, "Ivan", "Test12"));
                _emp.Add(new Employee(3, "Andy", "Test123"));
                DataSourceResult result = _emp.ToDataSourceResult(request);
                return Json(result, System.Web.Mvc.JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, System.Web.Mvc.JsonRequestBehavior.AllowGet);

            }
        }
    }
}
