﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            // using Repo
            //CoverType coverType = new CoverType();
            //if (id == null)
            //{
            //    // Create
            //    return View(coverType);
            //}

            //// Edit
            //coverType = _unitOfWork.CoverType.Get(id.GetValueOrDefault());
            //if (coverType == null)
            //{
            //    return NotFound();
            //}
            //return View(coverType);

            // using Stored Procedures
            CoverType coverType = new CoverType();
            if (id == null)
            {
                // Create
                return View(coverType);
            }

            // Edit
            // using Dapper
            var parameter = new DynamicParameters();
            // Add parameter "@Id" from Stored Procedure, and pass id
            parameter.Add("@Id", id);
            // retrive from SP, and pass parameter
            coverType = _unitOfWork.SP_Call.OneRecord<CoverType>(SD.Proc_CoverType_Get, parameter);

            if (coverType == null)
            {
                return NotFound();
            }
            return View(coverType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            // using repo
            //if (ModelState.IsValid)
            //{
            //    if (coverType.Id == 0)
            //    {
            //        _unitOfWork.CoverType.Add(coverType);
            //    }
            //    else
            //    {
            //        _unitOfWork.CoverType.Update(coverType);
            //    }
            //    _unitOfWork.Save();
            //    return RedirectToAction(nameof(Index));
            //}
            
            // using Stored Procedures
            if (ModelState.IsValid)
            {
                // using Dapper
                var parameter = new DynamicParameters();
                // Add parameter @Name as coverType.Name
                parameter.Add("@Name", coverType.Name);

                if (coverType.Id == 0)
                {
                    // Execute SP for Create, pass parameter
                    _unitOfWork.SP_Call.Execute(SD.Proc_CoverType_Create, parameter);
                }
                else
                {
                    // If it's update add @Id parameter
                    parameter.Add("@Id", coverType.Id);
                    // Execute SP for Update, pass parameter
                    _unitOfWork.SP_Call.Execute(SD.Proc_CoverType_Update, parameter);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(coverType);
        }


        #region API CALLS

        // Using Api Calls for DataTables in View

        [HttpGet]
        public IActionResult GetAll()
        {
            // using CoverTypeRepository
            //var allObj = _unitOfWork.CoverType.GetAll();

            // using Stored procedures
            var allObj = _unitOfWork.SP_Call.List<CoverType>(SD.Proc_CoverType_GetAll, null);
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            // using Dapper
            var parameter = new DynamicParameters();
            // Add parameter "@Id" from Stored Procedure, and pass id
            parameter.Add("@Id", id);

            //var objFromDb = _unitOfWork.CoverType.Get(id);
            
            // retrive from SP, and pass parameter
            var objFromDb = _unitOfWork.SP_Call.OneRecord<CoverType>(SD.Proc_CoverType_Get,parameter);
            if (objFromDb == null)
            {
                // return error messages for Toaster-SweetAlert
                return Json(new { success = false, message = "Error while deleting" });
            }

            //_unitOfWork.CoverType.Remove(objFromDb);

            // Execute SP for Delete
            _unitOfWork.SP_Call.Execute(SD.Proc_CoverType_Delete, parameter);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion


    }
}
