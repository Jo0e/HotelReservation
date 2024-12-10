﻿using Infrastructures.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Models.Models;

namespace HotelReservation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContactUsController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public ContactUsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var contactMessage = unitOfWork.ContactUsRepository.Get();
            return View(contactMessage);
        }
        public IActionResult Details(int reqId)
        {
            var req = unitOfWork.ContactUsRepository.GetOne(where: c => c.Id == reqId);
            if (req.HelperId != null && req.HelperId != 0)
            {
                ViewBag.Comment = unitOfWork.CommentRepository.GetOne(where: e => e.Id == req.HelperId
                , include: [s => s.User]);
            }
            return View(req);
        }
        public IActionResult Respond(Models.Models.Message message)
        {
            message.MessageDateTime = DateTime.Now;
            if (ModelState.IsValid)
            {
                unitOfWork.MessageRepository.Create(message);
                unitOfWork.Complete();
                return RedirectToAction("Index");
            }

            return RedirectToAction("NotFound");
        }
        public IActionResult Delete(int reqId)
        {
            var toDelete = unitOfWork.ContactUsRepository.GetOne(where: o=>o.Id == reqId);
            if (toDelete != null)
            {
                unitOfWork.ContactUsRepository.Delete(toDelete);
                unitOfWork.Complete();
            }
            return RedirectToAction("Index");
        }
    }
}
