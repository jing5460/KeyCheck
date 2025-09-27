using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using PKeyToolWeb_NetCore.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PKeyToolWeb_NetCore
{
    public class WebFilter : ActionFilterAttribute
    {
        //
        // 摘要:
        //     Called before the action executes, after model binding is complete.
        //
        // 参数:
        //   context:
        //     The Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext.
        public override void OnActionExecuting(ActionExecutingContext context)
        {
           //自定义各类规则，例如拦截请求，获取请求信息等
        }

        //
        // 摘要:
        //     Called after the action executes, before the action result.
        //
        // 参数:
        //   context:
        //     The Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext.
        public override void OnActionExecuted(ActionExecutedContext context)
        {

        }

    }
}
