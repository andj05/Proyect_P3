

using Proyect_P3.Controllers;
using Proyect_P3.Models;
using System.Web;
using System.Web.Mvc;

namespace Proyect_P3.Filters
{
    public class VerificaSession : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var miUsario = (USER)HttpContext.Current.Session["Usuario"];
            if (miUsario == null)
            {
                if(filterContext.Controller is AccederController == false) 
                {
                    filterContext.HttpContext.Response.Redirect("~/Acceder/Login");
                }     
            }
            else
            {
                if (filterContext.Controller is AccederController == true)
                {
                    filterContext.HttpContext.Response.Redirect("~/Home/Index");
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}