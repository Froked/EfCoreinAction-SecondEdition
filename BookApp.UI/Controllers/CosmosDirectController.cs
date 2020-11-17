﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApp.Infrastructure.LoggingServices;
using BookApp.Persistence.CosmosDb.Books;
using BookApp.ServiceLayer.CosmosDirect.Books;
using BookApp.ServiceLayer.CosmosDirect.Books.Services;
using BookApp.ServiceLayer.DefaultSql.Books;
using BookApp.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookApp.UI.Controllers
{
    public class CosmosDirectController : BaseTraceController
    {
        public async Task<IActionResult> Index(SortFilterPageOptions options, 
            [FromServices] CosmosDbContext context,
            [FromServices] BookAppSettings settings)
        {
            options.SetupRestOfDto(await context.CosmosDirectCountAsync(options, settings.CosmosDatabaseName));
            var bookList = (await context.CosmosDirectQueryAsync(options, settings.CosmosDatabaseName)).ToList();

            SetupTraceInfo();

            return View(new CosmosDirectBookListCombinedDto(options, bookList));
        }

        /// <summary>
        /// This provides the filter search dropdown content
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetFilterSearchContent(SortFilterPageOptions options, [FromServices] IBookFilterDropdownService service)
        {
            var traceIdent = HttpContext.TraceIdentifier;
            return Json(
                new TraceIndentGeneric<IEnumerable<DropdownTuple>>(
                    traceIdent,
                    service.GetFilterDropDownValues(
                        options.FilterBy)));
        }
    }
}