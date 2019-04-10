﻿using Licenta.Common.Entities;
using Licenta.Common.Models;
using Licenta.DataAccess;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Licenta.Controllers
{
    public class ConversationController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Conversation
        public ActionResult Index(string sortType)
        {
            if(sortType == null)
            {
                sortType = "Received";
            }
            var currentUser = User.Identity.GetUserId();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if(sortType == "Received")
                {
                    /*var conversations = from conv in _db.Conversations.Include("Product").Include("Sender")
                                        where conv.Product.UserId == currentUser
                                        select conv;
                    var conversations = from conv in _db.Conversations.Include("Product").Include("Sender")
                                        where conv.Product.UserId == currentUser
                                        select conv;
                    ViewBag.conversations = conversations;

                    var latestMessages = new List<Message>();
                    foreach (var conv in conversations)
                    {
                        var message = (from mess in _db.Messages.Include("Sender").Include("Receiver")
                                       where mess.ConversationId == conv.ConversationId
                                       orderby mess.Date descending
                                       select mess).First();
                        latestMessages.Add(message);
                    }*/

                    var conversations = (from c in _db.Conversations.Include("Product").Include("Sender")
                                        join m in _db.Messages.Include("Sender").Include("User")
                                        on c.ConversationId equals m.ConversationId
                                        where c.Product.UserId == currentUser
                                        select new { c, m } into x
                                        group x by new { x.c } into g
                                        select new
                                        { Conversation = g.Key.c,
                                          Message = g.Select(x => x.m).Where(y => y.ConversationId == g.Key.c.ConversationId).OrderByDescending(m => m.Date),
                                          MessageDate = g.Select(x => x.m).Max(x => x.Date)}).OrderByDescending(y => y.MessageDate);

                    var conversationsMes = new List<ConversationMessage>();
                    foreach(var conversation in conversations)
                    {
                        var convMes = new ConversationMessage { Conversation = conversation.Conversation ,
                                                               LatestMessage = conversation.Message.Where(m => m.ConversationId== conversation.Conversation.ConversationId).First()
                                                              };

                        convMes.LatestMessage.Content = MessageController.Decrypt(convMes.LatestMessage.Content);
                        conversationsMes.Add(convMes);
                    }
                    var model = new ConversationViewModel { Conversations = conversationsMes };

                    return View(model);
                }
                else
                if(sortType == "Sent")
                {
                    /*var conversations = from conv in _db.Conversations.Include("Product").Include("Sender")
                                        where conv.SenderId == currentUser
                                        select conv;
                    ViewBag.conversations = conversations;

                    var latestMessages = new List<Message>();
                    foreach (var conv in conversations)
                    {
                        var message = (from mess in _db.Messages.Include("Sender").Include("Receiver")
                                       where mess.ConversationId == conv.ConversationId
                                       orderby mess.Date descending
                                       select mess).First();
                        latestMessages.Add(message);
                    }*/

                    var conversations = (from c in _db.Conversations.Include("Product").Include("Sender")
                                         join m in _db.Messages.Include("Sender").Include("User")
                                         on c.ConversationId equals m.ConversationId
                                         where c.SenderId == currentUser
                                         select new { c, m } into x
                                         group x by new { x.c } into g
                                         select new
                                         {
                                             Conversation = g.Key.c,
                                             Message = g.Select(x => x.m).Where(y => y.ConversationId == g.Key.c.ConversationId).OrderByDescending(m => m.Date),
                                             MessageDate = g.Select(x => x.m).Max(x => x.Date)
                                         }).OrderByDescending(y => y.MessageDate);

                    var conversationsMes = new List<ConversationMessage>();
                    foreach (var conversation in conversations)
                    {
                        var convMes = new ConversationMessage
                        {
                            Conversation = conversation.Conversation,
                            LatestMessage = conversation.Message.Where(m => m.ConversationId == conversation.Conversation.ConversationId).First()
                        };
                        convMes.LatestMessage.Content = MessageController.Decrypt(convMes.LatestMessage.Content);
                        conversationsMes.Add(convMes);
                    }
                    var model = new ConversationViewModel { Conversations = conversationsMes };

                    return View(model);
                }

                return View();
            }

        }

        public ActionResult Show(int id)
        {
            Conversation conversation = _db.Conversations.Find(id);

            var messages = from msg in _db.Messages.Include("Sender").Include("Receiver")
                           where msg.ConversationId == id
                           orderby msg.Date
                           select msg;

            var messagesDecrypted = messages;
            foreach(var message in messagesDecrypted)
            {
                message.Content = MessageController.Decrypt(message.Content);
            };

            var model = new MessageViewModel
            {
                ConversationId = conversation.ConversationId,
                ProductId = conversation.ProductId,
                Product = conversation.Product,
                SenderId = conversation.SenderId,
                Sender = conversation.Sender,
                Messages = messagesDecrypted.ToList()
            };


            var currentUser = User.Identity.GetUserId();
            ViewBag.CurrentUser = currentUser;

            /*if ((conversation.Product.UserId == currentUser) || (conversation.SenderId == currentUser))
            {
                return View(model); 
            }*/
            //ViewBag.Received = true;
            if ((conversation.Product.UserId == currentUser) || (conversation.SenderId == currentUser))
            {
                var readMessages = messages.Where(m => (m.ReceiverId == currentUser) && (m.Read == false));
                if (readMessages.Count() > 0)
                {
                    foreach (Message messageRead in readMessages)
                    {
                        //MarkMessageAsRead(messageRead.MessageId);
                        messageRead.Read = true;
                    }
                    _db.SaveChanges();

                }
                /*try
                {
                    var readMessages = messages.Where(m => m.ReceiverId == currentUser);
                    foreach (var message in readMessages)
                        message.Read = true;

                    _db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }*/


                return View(model); 
            }
            /*else
            if (conversation.SenderId == currentUser)
            {
                //ViewBag.Received = false;
                var readMessages = messages.Where(m => m.ReceiverId == currentUser);
                foreach (var message in readMessages)
                    message.Read = true;

                return View(model);
            }*/
            else
            {
                return RedirectToAction("Index");
            }
        }

        /*[NonAction]
        public ActionResult MarkMessageAsRead(int id)
        {
            Message message = _db.Messages.Find(id);
            message.Read = true;
            _db.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());

        }
        */
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Conversation conversation = _db.Conversations.Find(id);

            _db.Conversations.Remove(conversation);
            _db.SaveChanges();
            TempData["message"] = "Conversatia a fost stearsa!";

            return RedirectToAction("Index");
        }
    }
}
