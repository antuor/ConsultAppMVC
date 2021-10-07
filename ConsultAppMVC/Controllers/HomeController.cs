using ConsultAppMVC.Data;
using ConsultAppMVC.Hubs;
using ConsultAppMVC.Infrastructure;
using ConsultAppMVC.Infrastructure.Repository;
using ConsultAppMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace ConsultAppMVC.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private IChatRepository _repo;
        public HomeController(IChatRepository repo, ApplicationDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public IActionResult Index([FromServices] ApplicationDbContext ctx)
        {
                var users = ctx.Users
                .Where(x => x.Id != User.GetUserId())
                .ToList();

                return View(users);
        }

        public IActionResult Private()
        {
            var chats = _repo.GetPrivateChats(GetUserId());

            return View(chats);
        }

        public IActionResult Protocol() => View();

        public async Task<IActionResult> CreatePrivateRoom(string userId)
        {
            int chatId = _repo.FindCommonRoom(GetUserId(), userId);
            if (chatId == -1)
            {
                var id = await _repo.CreatePrivateRoom(GetUserId(), userId);

                return RedirectToAction("Chat", new { id });
            }
            else return RedirectToAction("Chat", new { chatId });
            //return RedirectToAction("Find");
        }

        [HttpGet("{id}")]
        public IActionResult Chat(int id)
        {
            return View(_repo.GetChat(id));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom(string name)
        {
            await _repo.CreateRoom(name, GetUserId());
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> JoinRoom(int id)
        {
            await _repo.JoinRoom(id, GetUserId());

            return RedirectToAction("Chat", "Home", new { id = id });
        }

        public async Task<IActionResult> SendMessage(
            int roomId,
            string message,
            [FromServices] IHubContext<ChatHub> chat)
        {
            var Message = await _repo.CreateMessage(roomId, message, User.Identity.Name);

            await chat.Clients.Group(roomId.ToString())
                .SendAsync("RecieveMessage", new
                {
                    Text = Message.Text,
                    Name = Message.Name,
                    Timestamp = Message.Timestamp.ToString("dd/MM/yyyy hh:mm:ss")
                });

            return Ok();
        }

        public ActionResult ChatModal(int id)
        {
            //Chat c = _db.Chats.ToList().FirstOrDefault(com => com.Id == id);
            //if (id != null)
                return PartialView(id);
            //return StatusCode(404);
        }

        public ActionResult InChatModal(int id)
        {
            //Chat c = _db.Chats.ToList().FirstOrDefault(com => com.Id == id);
            //if (id != null)
            return PartialView(id);
            //return StatusCode(404);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(new { data = await _db.Users.Where(x => x.Id != User.GetUserId()).ToListAsync() });
        }

        [HttpGet]
        public async Task<IActionResult> GetConsults()
        {
            return Json(new { data = await _db.Chats.ToListAsync() });
        }

    }
}
